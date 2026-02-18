using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Pure Flexo Logic Calculations.
/// Validates and Ports the legacy 'Shirin_Job' -> 'Plan_On_Roll' pipeline.
/// </summary>
public class FlexoCalculationService : IFlexoCalculationService
{
    private readonly IMachineProcessRepository _machineRepository;
    private readonly IToolMaterialRepository _materialRepository;
    private readonly ILogger<FlexoCalculationService> _logger;

    // Constants from Legacy System
    private const double PlateBearer = 15; // Gbl_Plate_Bearer
    private const double ColorStrip = 7;   // Gbl_ColorStrip
    private const double StandardGap = 3;  // Gbl_Standard_AC_Gap

    public FlexoCalculationService(
        IMachineProcessRepository machineRepository,
        IToolMaterialRepository materialRepository,
        ILogger<FlexoCalculationService> logger)
    {
        _machineRepository = machineRepository;
        _materialRepository = materialRepository;
        _logger = logger;
    }

    public async Task<Result<List<FlexoPlanResult>>> CalculateFlexoPlanAsync(FlexoPlanCalculationRequest request)
    {
        try
        {
            var plans = new List<FlexoPlanResult>();

            // CRITICAL FIX (Gap #3): Iterate through ALL machine-cylinder combinations
            // Legacy: Api_shiring_serviceController.cs Line 15467 - loops through Gbl_DT_Machine.Rows
            // With LEFT JOIN, machineGrid contains MULTIPLE rows per machine (one per cylinder)
            var machineGrid = await _machineRepository.GetMachineGridAsync("FLEXO");

            // FIX: Machine selection is OPTIONAL (Legacy: Line 1140-1147)
            // If MachineId == 0 (no machine selected), plan on ALL machines with cylinders
            var machineCylinderCombos = request.MachineId > 0
                ? machineGrid.Where(m => m.MachineID == request.MachineId).ToList()
                : machineGrid.ToList(); // ALL machines

            if (machineCylinderCombos.Count == 0)
            {
                string errorMsg = request.MachineId > 0
                    ? $"Machine ID {request.MachineId} not found."
                    : "No Flexo machines found in database.";
                return Result<List<FlexoPlanResult>>.Failure(errorMsg);
            }

            // FIX: Cylinder Validation - Skip machines without cylinder allocation
            // Legacy: Line 15377-15408 - validates Tool_Cylinder_Circumference_MM > 0
            machineCylinderCombos = machineCylinderCombos
                .Where(m => m.CylinderToolID != null && m.CylinderToolID > 0 && m.CylinderCircumferenceMM > 0)
                .ToList();

            if (machineCylinderCombos.Count == 0)
            {
                string errorMsg = request.MachineId > 0
                    ? $"Machine ID {request.MachineId} does not have any printing cylinders allocated."
                    : "No Flexo machines with printing cylinder allocation found.";
                return Result<List<FlexoPlanResult>>.Failure(errorMsg);
            }

            _logger.LogInformation($"[Planning] Found {machineCylinderCombos.Count} machine-cylinder combinations to evaluate.");

            var machineSlabs = await _machineRepository.GetMachineSlabsAsync(request.MachineId);

            // 2. Decide Calculation Strategy
            ReelDto? specificReel = null;
            if (request.PaperId > 0)
            {
                specificReel = await _materialRepository.GetReelByIdAsync(request.PaperId);
                if (specificReel == null)
                   return Result<List<FlexoPlanResult>>.Failure($"Paper ID {request.PaperId} not found in database.");
            }

            // LOOP THROUGH EACH MACHINE-CYLINDER COMBINATION (Legacy: for i = 0 to Gbl_DT_Machine.Rows.Count - 1)
            foreach (var machineWithCylinder in machineCylinderCombos)
            {
                // FIX: Grain Direction Filter (Gap #5)
                // Legacy: Line 2530-2551 - respects user's grain direction choice
                // Only calculate requested grain direction(s)
                // Legacy Line 2839-2845: Grain direction swaps Rect_H and Rect_L
                // Rect_H (→ UPS across roll width), Rect_L (→ UPS around cylinder)
                // "With Grain": Rect_H=Job_H, Rect_L=Job_L (no swap)
                // "Across Grain": Rect_H=Job_L, Rect_L=Job_H (swap H and L)
                List<(string Direction, double JobSizeW, double JobSizeH, double JobSizeL)> grainDirections = new();

                if (request.GrainDirection == "Both" || request.GrainDirection == "With Grain")
                {
                    grainDirections.Add(("With Grain", request.JobSizeW, request.JobSizeH, request.JobSizeL));
                }

                if (request.GrainDirection == "Both" || request.GrainDirection == "Across Grain")
                {
                    // CRITICAL: Swap H and L for "Across Grain" (Legacy Lines 2839-2845)
                    grainDirections.Add(("Across Grain", request.JobSizeW, request.JobSizeL, request.JobSizeH));
                }

                _logger.LogInformation($"[Grain Direction] User selected: {request.GrainDirection}, calculating {grainDirections.Count} grain variation(s)");

                foreach (var grainVariation in grainDirections)
                {
                    // Create modified request with swapped dimensions for grain direction
                    var modifiedRequest = new FlexoPlanCalculationRequest
                    {
                        JobSizeL = grainVariation.JobSizeL, // Swapped for "Across Grain"
                        JobSizeW = grainVariation.JobSizeW,
                        JobSizeH = grainVariation.JobSizeH, // Swapped for "Across Grain"
                        UpsAcross = request.UpsAcross,
                        UpsAround = request.UpsAround,
                        GapAcross = request.GapAcross,
                        GapAround = request.GapAround,
                        Bleed = request.Bleed,
                        PaperId = request.PaperId,
                        PaperRate = request.PaperRate,
                        PaperRateType = request.PaperRateType,
                        PaperUnit = request.PaperUnit,
                        MachineId = request.MachineId,
                        CylinderId = request.CylinderId,
                        FrontColors = request.FrontColors,
                        BackColors = request.BackColors,
                        SpecialFrontColors = request.SpecialFrontColors,
                        SpecialBackColors = request.SpecialBackColors,
                        CoatingType = request.CoatingType,
                        WindingDirectionId = request.WindingDirectionId,
                        CoreInnerDia = request.CoreInnerDia,
                        CoreOuterDia = request.CoreOuterDia,
                        LabelsPerRoll = request.LabelsPerRoll,
                        LabelType = request.LabelType,
                        FinishedFormat = request.FinishedFormat,
                        Quantity = request.Quantity,
                        PlateRate = request.PlateRate,
                        MakeReadyRate = request.MakeReadyRate,
                        CoatingRate = request.CoatingRate,
                        AdditionalOperations = request.AdditionalOperations,
                        ShadeCardRequired = request.ShadeCardRequired,
                        Orientation = request.Orientation,
                        WastageType = request.WastageType,
                        FlatWastageValue = request.FlatWastageValue,
                        CategoryId = request.CategoryId,
                        PlateBearer = request.PlateBearer,
                        ColorStrip = request.ColorStrip,
                        MakeReadyWastage = request.MakeReadyWastage,
                        GrainDirection = request.GrainDirection,
                        PlanInAvailableStock = request.PlanInAvailableStock,
                        PlanInSpecialSizePaper = request.PlanInSpecialSizePaper,
                        PlanInStandardSizePaper = request.PlanInStandardSizePaper,
                        PaperSuppliedByClient = request.PaperSuppliedByClient,
                        BackToBackPastingRequired = request.BackToBackPastingRequired,
                        PaperQuality = request.PaperQuality,
                        PaperGSM = request.PaperGSM,
                        PaperMill = request.PaperMill
                    };

                    // Strategy A: Specific Reel Selected
                    if (request.PaperId > 0)
                    {
                        await PlanOnRoll(modifiedRequest, machineWithCylinder, specificReel, machineSlabs, plans, grainVariation.Direction);
                    }
                    else
                    {
                        // Strategy B: No Reel Selected -> Suggest Options

                        // B1. Find from Standard Reels (PlanOnRoll)
                        // Apply Quality, GSM, Mill filters (matching legacy: Api_shiring_serviceController.cs line 200)
                        var foundReels = await _materialRepository.GetReelsAsync(
                            machineWithCylinder.MinSheetW ?? 0, 5000, 0, -14,
                            request.PaperQuality, request.PaperGSM, request.PaperMill);

                        // FIX: Planning Options - Filter reels based on user selections
                        // Legacy: Line 433-439, 613-634
                        if (request.PlanInAvailableStock)
                        {
                            // Filter to only papers with available stock
                            foundReels = foundReels.Where(r => r.StockQuantity > 0 || r.IsAvailable).ToList();
                            _logger.LogInformation($"[Planning Option] Plan in available stock: Filtered to {foundReels.Count} in-stock reels");
                        }

                        if (request.PlanInStandardSizePaper)
                        {
                            // Filter to only standard size papers
                            foundReels = foundReels.Where(r => r.IsStandardItem).ToList();
                            _logger.LogInformation($"[Planning Option] Plan in standard size: Filtered to {foundReels.Count} standard reels");
                        }

                        // Process each filtered reel
                        foreach (var reel in foundReels)
                        {
                            await PlanOnRoll(modifiedRequest, machineWithCylinder, reel, machineSlabs, plans, grainVariation.Direction);
                        }

                        // B2. Special Size Planning (PlanOnCylinder)
                        // Legacy: Line 619-634 - adds special size row when Check_Plan_In_Special_Size is true
                        if (request.PlanInSpecialSizePaper || request.CylinderId > 0)
                        {
                            _logger.LogInformation($"[Planning Option] Plan in special size: Generating custom size options");
                            await PlanOnCylinder(modifiedRequest, machineWithCylinder, machineSlabs, plans, null);
                        }
                    }
                }
            }

            if (plans.Count == 0)
            {
                string debugInfo = $"Machine ID: {request.MachineId}, Cylinders found: {machineCylinderCombos.Count}. ";
                debugInfo += "No valid plans generated for any cylinder combination.";

                return Result<List<FlexoPlanResult>>.Failure($"No valid plans generated. {debugInfo}");
            }

            return Result<List<FlexoPlanResult>>.Success(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating flexo plan");
            return Result<List<FlexoPlanResult>>.Failure($"Calculation failed: {ex.Message}");
        }
    }

    private async Task PlanOnRoll(FlexoPlanCalculationRequest request, MachineGridDto machine, ReelDto reel, List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto> slabs, List<FlexoPlanResult> plans, string grainDirection = "With Grain")
    {
        // Use request values directly (user can change Bearer & Gap during planning)
        // Legacy Line 507: Gbl_Plate_Bearer = value from frontend (0 means 0, not fallback!)
        // Legacy Line 448: Gbl_ColorStrip = value from frontend (column 15)
        double plateBearer = request.PlateBearer;
        double colorStrip = request.ColorStrip;
        double rollWidthEffective = (double)reel.SizeW - ((plateBearer * 2) + (colorStrip * 2) + request.GapAcross);
        double machineMin = (double)(machine.MinSheetW ?? 0);
        double machineMax = (double)(machine.MaxSheetW ?? 0);
        double realWidth = (double)reel.SizeW;

        _logger.LogInformation($"Checking Reel {reel.ItemID}: Width={realWidth}, Effective={rollWidthEffective}.");

        if (realWidth < machineMin || realWidth > machineMax) 
        {
             _logger.LogWarning($"Reel {reel.ItemID} skipped: Width {realWidth} outside limitations");
             return;
        }

        if (request.CylinderId > 0)
        {
             // If User Selected a Cylinder, we validate if the Job Repeat fits the Cylinder Circumference
             var cylinder = await _materialRepository.GetToolByIdAsync(request.CylinderId.Value);
             if (cylinder != null && cylinder.CircumferenceMM > 0)
             {
                 double jobRepeat = request.JobSizeL + request.GapAround; // Legacy: Gbl_Label_L goes around cylinder
                 double remainder = (double)cylinder.CircumferenceMM % jobRepeat;
                 
                 // Allow small tolerance
                 if (remainder > 1.0 && (jobRepeat - remainder) > 1.0) 
                 {
                     _logger.LogWarning($"[Planning] Repeat Mismatch Warning: Cylinder {cylinder.ToolCode} (Circ: {cylinder.CircumferenceMM}) vs Job Repeat {jobRepeat}. Remainder: {remainder}");
                 }
             }
        }

        List<CategoryWastageSettingDto> categoryWastageSettings = new List<CategoryWastageSettingDto>();
        if (request.WastageType == "Category & Process Wise Wastage" && request.CategoryId > 0)
        {
             categoryWastageSettings = await _machineRepository.GetCategoryWastageSettingsAsync(request.CategoryId);
        }

        // CRITICAL FIX: Use JobSizeH (height) for UPS across roll width
        // Legacy Line 15208: Gbl_UPS_H = RoundDown((Roll_Width + Gap) / Gbl_Label_H_With_Across)
        int upsAcross = CalculateUpsAcross(request.JobSizeH, request.GapAcross, rollWidthEffective, request.UpsAcross);

        if (upsAcross > 0)
        {
            _logger.LogInformation($"Plan Found on Roll! UpsAcross={upsAcross}");

            // CRITICAL FIX: Calculate AcrossGap based on actual layout
            // If only 1 label across, no gap needed
            double calculatedAcrossGap = upsAcross > 1 ? request.GapAcross : 0;
            double usedWidth = (upsAcross * request.JobSizeH) + ((upsAcross - 1) * calculatedAcrossGap);

            // CRITICAL FIX: Calculate UpsAround and AroundGap from Cylinder Circumference
            // Legacy: Lines 14896-14914 (Flexo-specific gap calculation)
            double cylinderCirc = (double)machine.CylinderCircumferenceMM;
            int calculatedUpsAround = 1; // Default
            double calculatedAroundGap = request.GapAround; // Fallback

            // CRITICAL FIX: Use JobSizeL (length) for UPS around cylinder
            // Legacy Line 14904: Gbl_UPS_L = RoundDown(Cylinder_Circumference / Gbl_Label_L_With_Around)
            if (cylinderCirc > 0 && request.JobSizeL > 0)
            {
                // STEP 1: Calculate UPS Around with initial gap
                double labelSizeWithGap = request.JobSizeL + request.GapAround;
                calculatedUpsAround = (int)Math.Floor(cylinderCirc / labelSizeWithGap);

                // Ensure at least 1 label fits
                if (calculatedUpsAround < 1) calculatedUpsAround = 1;

                // STEP 2: Apply gap > 1mm validation (Legacy Lines 14906-14914)
                if (request.GapAround == 0)
                {
                    // User didn't provide gap - calculate waste strip and apply only if > 1mm
                    double wasteStrip = Math.Round(cylinderCirc - (calculatedUpsAround * request.JobSizeL), 2);

                    if (wasteStrip > 1)
                    {
                        // Waste > 1mm: distribute evenly as gap
                        calculatedAroundGap = Math.Round(wasteStrip / calculatedUpsAround, 2);
                        _logger.LogInformation($"[Gap Auto-Calc] Waste={wasteStrip:F2}mm > 1mm, Applied Gap={calculatedAroundGap:F2}mm");
                    }
                    else
                    {
                        // Waste <= 1mm: keep gap as 0
                        calculatedAroundGap = 0;
                        _logger.LogInformation($"[Gap Auto-Calc] Waste={wasteStrip:F2}mm <= 1mm, Gap kept at 0");
                    }
                }
                else
                {
                    // User provided gap - always apply it
                    calculatedAroundGap = Math.Round((cylinderCirc - (calculatedUpsAround * request.JobSizeL)) / calculatedUpsAround, 2);
                    _logger.LogInformation($"[Gap User-Provided] Using calculated gap={calculatedAroundGap:F2}mm");
                }

                _logger.LogInformation($"[Cylinder Layout] Circumference={cylinderCirc}mm, UpsAround={calculatedUpsAround}, Final AroundGap={calculatedAroundGap:F2}mm");
            }

            var plan = new FlexoPlanResult
            {
                PaperId = reel.ItemID,
                PaperName = reel.ItemName,
                PaperWidth = (double)reel.SizeW,
                PaperLength = 0, // Will be set in CalculateCosting as TotalRunningMeter
                UpsAcross = upsAcross,
                UpsAround = calculatedUpsAround, // FIX: Use cylinder-based calculation
                CutSizeW = usedWidth,
                CutSizeH = request.JobSizeL + calculatedAroundGap, // Repeat length (Label_L_With_Around)
                MachineName = machine.MachineName ?? string.Empty,

                // CRITICAL FIX (Gap #4): Populate Cylinder Details from machine-cylinder combo
                // Legacy: Lines 16771+ (CylinderToolID, CylinderCircumferenceMM, etc.)
                CylinderCircumference = cylinderCirc,
                CylinderTeeth = machine.CylinderNoOfTeeth,
                ToolDescription = machine.CylinderToolID > 0
                    ? $"{machine.CylinderToolCode} (Circ: {machine.CylinderCircumferenceMM}mm, Teeth: {machine.CylinderNoOfTeeth})"
                    : string.Empty,

                // Gap #8: Gap & Wastage Strip - FIX: Use calculated gaps
                AcrossGap = calculatedAcrossGap,
                AroundGap = calculatedAroundGap,
                WastageStrip = (double)reel.SizeW - usedWidth,

                // Gap #10: Paper Breakdown
                PaperFaceGSM = reel.GSM,
                PaperReleaseGSM = reel.ReleaseGSM,
                PaperAdhesiveGSM = reel.AdhesiveGSM,
                PaperMill = reel.Manufecturer,
                PaperQuality = reel.Quality,
                GrainDirection = grainDirection, // Gap #9 & #5: Grain direction from parameter

                // Gap #14: Output Metadata
                PlanType = "Roll",
                OutputFormat = request.FinishedFormat,
                FrontColors = request.FrontColors,
                BackColors = request.BackColors,
                PrintingStyle = request.BackColors > 0 ? "Both Side" : "Single Side"
            };
            
            CalculateCosting(plan, request, machine, slabs, reel, categoryWastageSettings, (double)reel.GSM, (double)reel.EstimationRate);
            plans.Add(plan);
        }
    }

    private int CalculateUpsAcross(double jobSize, double gap, double effectiveWidth, int desiredUps = 0)
    {
        if (desiredUps > 0)
        {
             // Pre-Planned Logic override: check if desired ups fit
             double minimalWidth = (desiredUps * jobSize) + ((desiredUps - 1) * gap);
             // Use StandardGap buffer consistent with legacy logic
             if ((effectiveWidth + StandardGap) >= minimalWidth) return desiredUps;
             return 0; 
        }

        double labelSizeWithGap = jobSize + gap;
        int ups = 0;

        if (labelSizeWithGap <= 0) return 0;

        // Branch A: When only 1 label with gap fits (Legacy Lines 15190-15197)
        if (Math.Floor((effectiveWidth + StandardGap) / labelSizeWithGap) == 1)
        {
             // Try to fit more labels WITHOUT gap
             if (jobSize <= (effectiveWidth + StandardGap))
             {
                 ups = (int)Math.Floor((effectiveWidth + StandardGap) / jobSize);
             }
        }
        else
        {
             // Branch B: Multiple labels fit OR no labels fit (Legacy Lines 15208-15218)
             ups = (int)Math.Floor((effectiveWidth + StandardGap) / labelSizeWithGap);

             // CRITICAL: Try to fit ONE MORE UPS (Legacy optimization)
             if (ups >= 1)
             {
                 int nextUps = ups + 1;
                 double requiredWidth = (nextUps * jobSize) + (ups * gap);

                 if ((effectiveWidth + StandardGap) >= requiredWidth)
                 {
                     ups = nextUps;
                 }
             }
        }

        return ups;
    }

    private async Task PlanOnCylinder(
        FlexoPlanCalculationRequest request,
        MachineGridDto machine,
        List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto> slabs,
        List<FlexoPlanResult> plans,
        ReelDto? specificReel = null)
    {
        // Use request values directly (Legacy: Gbl_Plate_Bearer & Gbl_ColorStrip = values from frontend)
        double plateBearer = request.PlateBearer;
        double colorStrip = request.ColorStrip;
        double machineMaxRoll = (double)(machine.MaxSheetW ?? 0);
        double maxUsableCheck = machineMaxRoll - ((plateBearer * 2) + (colorStrip * 2)); 

        // CRITICAL FIX: Use JobSizeH (height goes across roll width, like legacy Gbl_Label_H)
        int maxUpsW = (int)Math.Floor(maxUsableCheck / request.JobSizeH);
        
        // GSM from specific reel or default
        double gsm = (specificReel != null) ? (double)specificReel.GSM : 0;
        
        string toolDesc = $"Cylinder ID: {request.CylinderId}";
        if (request.CylinderId > 0)
        {
             var cylinder = await _materialRepository.GetToolByIdAsync(request.CylinderId.Value);
             if (cylinder != null)
             {
                 toolDesc = $"{cylinder.ToolName} (Circ: {cylinder.CircumferenceMM}mm)";
             }
        }

        List<CategoryWastageSettingDto> categoryWastageSettings = new List<CategoryWastageSettingDto>();
        if (request.WastageType == "Category & Process Wise Wastage" && request.CategoryId > 0)
        {
             categoryWastageSettings = await _machineRepository.GetCategoryWastageSettingsAsync(request.CategoryId);
        }

        for (int i = maxUpsW; i >= 1; i--)
        {
            double requiredWidth = (i * request.JobSizeH) + ((i - 1) * request.GapAcross) + (plateBearer * 2) + (colorStrip * 2);
            
            if (requiredWidth >= (double)(machine.MinSheetW ?? 0) && requiredWidth <= machineMaxRoll)
            {
                 var plan = new FlexoPlanResult
                 {
                     PaperId = specificReel?.ItemID ?? 0, 
                     PaperName = specificReel?.ItemName ?? $"Special Size {requiredWidth}mm",
                     PaperWidth = requiredWidth,
                     UpsAcross = i,
                     UpsAround = 0, 
                     CutSizeW = (i * request.JobSizeH) + ((i - 1) * request.GapAcross),
                     MachineName = machine.MachineName ?? string.Empty,
                     ToolDescription = toolDesc
                 };

                 CalculateCosting(plan, request, machine, slabs, specificReel, categoryWastageSettings, gsm, (double)(specificReel?.EstimationRate ?? 0));
                 plans.Add(plan);
            }
        }
    }

    private void CalculateCosting(
        FlexoPlanResult plan,
        FlexoPlanCalculationRequest request,
        MachineGridDto machine,
        List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto> slabs,
        ReelDto? reel,
        List<CategoryWastageSettingDto> categoryWastageSettings,
        double gsm,
        double specificPaperRate = 0)
    {
        // FIX: TotalUps should include both Across and Around
        int totalUps = plan.UpsAcross * plan.UpsAround;
        plan.TotalUps = totalUps;

        // FIX: Prioritize User Input (MakeReadyWastage) > Machine Default
        // This resolves the build error 'name makeReadyWastage does not exist'
        double makeReadyWastage = request.MakeReadyWastage > 0 
                                  ? request.MakeReadyWastage 
                                  : (double)machine.MakeReadyWastageRunningMeter;

        if (totalUps == 0) return;

        // 1. CRITICAL FIX: Calculate Impressions FIRST (not running meters)
        // Legacy Line 15709: Printing_Impressions = Math.Ceiling((Gbl_Order_Quantity / Total_Ups))
        double printingImpressions = Math.Ceiling((double)request.Quantity / totalUps);

        // 2. Calculate Final Quantity (Legacy Line 15702)
        // FinalQuantityInPcs = Val(Printing_Impressions) * Val(Total_Ups)
        double finalQuantityInPcs = printingImpressions * totalUps;

        // 3. Calculate Required Running Meters (Legacy Lines 15717-15722)
        // Req_Running_Mtr = Math.Round((Gbl_Label_L_With_Around * (FinalQuantityInPcs / Gbl_UPS_H)) / 1000, 3)
        double reqRunningMeters = 0;

        if (request.Orientation == "PrePlannedSheetLabel")
        {
             double labelsPerMeter = (1000 / request.JobSizeL) * plan.UpsAcross; // Legacy: Label_L goes around cylinder
             if (labelsPerMeter > 0) reqRunningMeters = finalQuantityInPcs / labelsPerMeter;
        }
        else
        {
            // FLEXO FORMULA: Req_Running_Mtr = (Label_L_With_Around × (Final_Quantity / UPS_Across)) / 1000
            // Legacy Line 15721: Uses Gbl_Label_L_With_Around (length + around gap)
            double labelSizeWithAroundGap = request.JobSizeL + plan.AroundGap; // Label_L_With_Around

            if (plan.UpsAcross > 0)
            {
                reqRunningMeters = Math.Round((labelSizeWithAroundGap * (finalQuantityInPcs / plan.UpsAcross)) / 1000, 3);
            }

            _logger.LogInformation($"[Costing] Impressions={printingImpressions}, FinalQty={finalQuantityInPcs}, Label+Gap={labelSizeWithAroundGap:F2}mm, ReqMeters={reqRunningMeters:F3}");
        }

        // 2. Wastage Parameters & Slab Selection
        decimal ratePerRun = 0;
        double slabPlateCharges = 0;
        {
            _logger.LogWarning($"[Slab NOT Found] Using fallback wastage={makeReadyWastage}m. ReqMeters={reqRunningMeters:F2}");

            if (slabs.Count > 0)
            {
                foreach (var s in slabs.Take(3))
                {
                    _logger.LogInformation($"  - Available Slab: Range={s.RunningMeterRangeFrom}-{s.RunningMeterRangeTo}");
                }
            }
        }

        // 3. Detailed Wastage Calculation
        double wastageRunningMeters = 0;
        double processWastageMeters = 0;
        
        // A. Process Specific Wastage
        foreach (var op in request.AdditionalOperations)
        {
            double pWasteMeters = 0;
            if (op.WastagePercent > 0)
            {
                pWasteMeters = (reqRunningMeters * (op.WastagePercent / 100));
            }
            
            if (op.FlatWastage > pWasteMeters) pWasteMeters = op.FlatWastage;
            
            processWastageMeters += pWasteMeters;
        }

        // B. Machine/General Wastage based on WastageType
        if (request.WastageType == "Machine Default" || string.IsNullOrEmpty(request.WastageType))
        {
             wastageRunningMeters += makeReadyWastage; 
        }
        else if (request.WastageType == "Percentage")
        {
             wastageRunningMeters += (reqRunningMeters * request.FlatWastageValue / 100);
             wastageRunningMeters += makeReadyWastage;
        }
        else if (request.WastageType == "Flat") // Meter
        {
             wastageRunningMeters += request.FlatWastageValue + makeReadyWastage;
        }
        else if (request.WastageType == "Category & Process Wise Wastage")
        {
             double categoryWastage = CalculateCategoryWastage(categoryWastageSettings, reqRunningMeters, request.FrontColors, request.BackColors + request.SpecialFrontColors + request.SpecialBackColors);
             // Logic: Add Category Wastage to Process Wastage (Legacy Line 15825)
             wastageRunningMeters += categoryWastage + makeReadyWastage; 
        }
        
        // C. Shade Card
        if (request.ShadeCardRequired)
        {
             wastageRunningMeters += 400;
        }

        // D. Roll Change Wastage Logic
        double oneRollLength = (double?)reel?.AvgRollLength > 0 ? (double)reel.AvgRollLength : 
                               (double)machine.StandardRollLength > 0 ? (double)machine.StandardRollLength : 2000;
        
        double rollChangeWastagePerChange = (double)machine.RollChangeWastage > 0 ? (double)machine.RollChangeWastage : 10;
        
        double preRollTotal = reqRunningMeters + wastageRunningMeters + processWastageMeters;
        int numRollChanges = (int)Math.Max(0, Math.Ceiling(preRollTotal / oneRollLength) - 1);

        double totalRollChangeWastage = numRollChanges * rollChangeWastagePerChange;
        
        double totalMeters = preRollTotal + totalRollChangeWastage;

        // 4. Material Cost (Paper)
        double usedGsm = gsm > 0 ? gsm : 50;
        if (reel != null)
        {
             usedGsm = (double)(reel.GSM + reel.ReleaseGSM + reel.AdhesiveGSM);
             if (usedGsm <= 0) usedGsm = gsm;
        }

        // Calculate Square Meters (Legacy: Lines 16445-16448)
        double reqSquareMeter = (plan.PaperWidth / 1000.0) * reqRunningMeters;
        double wastageSquareMeter = (plan.PaperWidth / 1000.0) * wastageRunningMeters;
        double totalSquareMeter = (plan.PaperWidth / 1000.0) * totalMeters;
        // Label area = H (across) × L (around) - the two label dimensions
        double scrapSquareMeter = totalSquareMeter -
            ((request.JobSizeH / 1000.0) * (request.JobSizeL / 1000.0) * request.Quantity);

        plan.TotalPaperWeightKg = (totalMeters * plan.PaperWidth * usedGsm) / 1000000;

        // CRITICAL FIX: Use finalQuantityInPcs (Impressions × Total_UPS), not original order quantity
        // Legacy Line 15702: FinalQuantityInPcs = Val(Printing_Impressions) * Val(Total_Ups)
        plan.TotalQuantity = finalQuantityInPcs;

        // Populate Square Meter fields (Gap #7)
        plan.RequiredRunningMeter = reqRunningMeters;
        plan.TotalRunningMeter = totalMeters;
        plan.PaperLength = totalMeters; // Set PaperLength to total running meters
        plan.RequiredSquareMeter = reqSquareMeter;
        plan.TotalSquareMeter = totalSquareMeter;
        plan.WastageSquareMeter = wastageSquareMeter;
        plan.ScrapSquareMeter = scrapSquareMeter;

        // Gap #11: Populate Printing Impressions (already calculated at beginning)
        // Legacy Line 15709: Printing_Impressions = Math.Ceiling((Gbl_Order_Quantity / Total_Ups))
        plan.PrintingImpressions = printingImpressions;
        plan.ImpressionsToBeCharged = printingImpressions;

        // CRITICAL FIX (Gap #6): Calculate paper rate type and effective rate (needed for both paper cost and wastage cost)
        // Legacy: Lines 16454-16461
        double effectivePaperRate = specificPaperRate > 0 ? specificPaperRate : request.PaperRate;
        string paperRateType = reel?.EstimationUnit ?? request.PaperRateType;

        // FIX: Planning Option - Paper Supplied by Client
        // If client is supplying paper, exclude paper cost from calculation
        if (request.PaperSuppliedByClient)
        {
            plan.PaperCostTotal = 0;
            plan.PaperCostPer1000 = 0;
            _logger.LogInformation($"[Planning Option] Paper supplied by client: Paper cost = 0");
        }
        else
        {
            // Calculate paper cost based on rate type

            if (paperRateType.ToUpper() == "SQM" || paperRateType.ToUpper() == "SQUARE METER")
            {
                // SQM-based costing (Legacy: Line 16456)
                plan.PaperCostTotal = Math.Round(totalSquareMeter * effectivePaperRate, 2);
            }
            else if (paperRateType.ToUpper() == "KG")
            {
                // KG-based costing (Legacy: Line 16460)
                plan.PaperCostTotal = Math.Round(plan.TotalPaperWeightKg * effectivePaperRate, 2);
            }
            else if (paperRateType.ToUpper() == "RM" || paperRateType.ToUpper() == "RUNNING METER")
            {
                // Running Meter-based costing
                plan.PaperCostTotal = Math.Round(totalMeters * effectivePaperRate, 2);
            }
            else
            {
                // Default to KG if unknown
                plan.PaperCostTotal = Math.Round(plan.TotalPaperWeightKg * effectivePaperRate, 2);
            }
        }

        // 5. Machine Run Cost
        if (ratePerRun > 0)
        {
             plan.MachineRunCostTotal = (double)ratePerRun;
        }
        else
        {
             double speedPerMin = (double)machine.Speed > 0 ? (double)machine.Speed : 30; 
             
             double runMetricsMinutes = totalMeters / speedPerMin;
             double fixedTimeMinutes = (double)machine.MakeReadyTime + (double)machine.JobChangeOverTime;

             double rollChangeTimeMinutes = numRollChanges * (double)machine.RollChangeOverTime;

             double totalTimeMinutes = runMetricsMinutes + fixedTimeMinutes + rollChangeTimeMinutes;
             double totalTimeHours = totalTimeMinutes / 60;

             plan.MachineRunCostTotal = totalTimeHours * (double)machine.PerHourRate;
        }

        // 6. Plate Cost
        int totalColors = request.FrontColors + request.BackColors + request.SpecialFrontColors + request.SpecialBackColors;
        double effectivePlateRate = request.PlateRate > 0 ? request.PlateRate : slabPlateCharges;
        plan.PlateCostTotal = effectivePlateRate * totalColors;

        // 7. Additional Operations Total
        double additionalOpsTotal = 0;
        double totalRolls = request.LabelsPerRoll > 0 ? (double)request.Quantity / request.LabelsPerRoll : 0;
        // Fallback: If no LabelsPerRoll but we computed Roll Changes, maybe utilize that?
        // But 'Per Roll' charge usually applies to finished rolls. LabelsPerRoll is the key metric.

        foreach (var op in request.AdditionalOperations)
        {
            double opCost = CalculateOperationCost(op, request.Quantity, plan.TotalPaperWeightKg, plan.TotalQuantity, totalRolls);
            additionalOpsTotal += opCost;
        }
        plan.ConversionCostTotal = additionalOpsTotal;

        // FIX: Calculate MaterialCostTotal (Coating, Inks, Chemicals)
        double materialCost = 0;

        // Coating Cost
        if (!string.IsNullOrEmpty(request.CoatingType) && request.CoatingType != "None" && request.CoatingRate > 0)
        {
            // Apply coating rate to total square meters
            materialCost += totalSquareMeter * request.CoatingRate;
            _logger.LogInformation($"[Costing] Coating: {request.CoatingType}, Rate={request.CoatingRate}, SQM={totalSquareMeter:F2}, Cost={materialCost:F2}");
        }

        plan.MaterialCostTotal = Math.Round(materialCost, 2);

        // FIX: Calculate WastageCostTotal based on wastage paper cost
        double wastageKg = ((wastageRunningMeters + processWastageMeters + totalRollChangeWastage) * plan.PaperWidth * usedGsm) / 1000000;
        plan.TotalWastageKg = Math.Round(wastageKg, 2);

        // Wastage cost using same paper rate logic
        double wastageCost = 0;
        if (paperRateType.ToUpper() == "SQM" || paperRateType.ToUpper() == "SQUARE METER")
        {
            wastageCost = wastageSquareMeter * effectivePaperRate;
        }
        else if (paperRateType.ToUpper() == "KG")
        {
            wastageCost = wastageKg * effectivePaperRate;
        }
        else if (paperRateType.ToUpper() == "RM" || paperRateType.ToUpper() == "RUNNING METER")
        {
            wastageCost = (wastageRunningMeters + processWastageMeters + totalRollChangeWastage) * effectivePaperRate;
        }
        plan.WastageCostTotal = Math.Round(wastageCost, 2);

        // 8. Total Summary - FIX: Include MaterialCostTotal and WastageCostTotal
        plan.TotalCost = plan.PaperCostTotal + plan.MachineRunCostTotal + plan.PlateCostTotal + plan.MaterialCostTotal + plan.ConversionCostTotal + plan.WastageCostTotal;

        // Populate Detailed Breakdown for Transparency
        plan.MakeReadyWastageMeters = makeReadyWastage;
        plan.ProcessWastageMeters = processWastageMeters;
        plan.ProcessWastagePercent = processWastageMeters > 0 && reqRunningMeters > 0 ? Math.Round((processWastageMeters / reqRunningMeters) * 100, 2) : 0;
        plan.RollChangeWastageMeters = totalRollChangeWastage;

        // 9. Calculate Per-1000 Costs (for display in UI grids)
        if (request.Quantity > 0)
        {
            double quantityInThousands = request.Quantity / 1000.0;

            plan.PaperCostPer1000 = quantityInThousands > 0 ? Math.Round(plan.PaperCostTotal / quantityInThousands, 2) : 0;
            plan.MachineRunCostPer1000 = quantityInThousands > 0 ? Math.Round(plan.MachineRunCostTotal / quantityInThousands, 2) : 0;
            plan.PlateCostPer1000 = quantityInThousands > 0 ? Math.Round(plan.PlateCostTotal / quantityInThousands, 2) : 0;
            plan.MaterialCostPer1000 = quantityInThousands > 0 ? Math.Round(plan.MaterialCostTotal / quantityInThousands, 2) : 0;
            plan.ConversionCostPer1000 = quantityInThousands > 0 ? Math.Round(plan.ConversionCostTotal / quantityInThousands, 2) : 0;
            plan.WastageCostPer1000 = quantityInThousands > 0 ? Math.Round(plan.WastageCostTotal / quantityInThousands, 2) : 0;

            plan.UnitPrice = plan.TotalCost / request.Quantity;
            plan.UnitPrice1000 = plan.UnitPrice * 1000;
        }
    }

    private double CalculateOperationCost(OperationCostDto op, double quantity, double weightKg, double totalQty, double totalRolls)
    {
        double calculatedCost = 0;
        
        // Normalize RateType string
        string type = (op.RateType ?? "").Replace(" ", "").ToLower(); 

        switch (type)
        {
            case "fixed":
            case "flat":
                 calculatedCost = op.Rate;
                 break;
            case "perunit":
            case "unit":
            case "singleunit":
                 calculatedCost = op.Rate * quantity;
                 break;
            case "per1000":
            case "thousand":
            case "perthousand":
                 calculatedCost = op.Rate * (quantity / 1000);
                 break;
            case "perkg":
            case "kg":
                 calculatedCost = op.Rate * weightKg;
                 break;
            case "perroll":
            case "roll":
                 calculatedCost = op.Rate * totalRolls;
                 break;
            case "perhour":
            case "hour":
                 // Note: Time calculation for purely manual ops without standardized speed logic in request is ambiguous.
                 // We explicitly treat Rate as 'Cost per Hour' but default to 1 unit if no time provided.
                 // Ideally, AdditionalOperations should carry 'TimeInHours' if it's separate from Machine Run.
                 // Falling back to simple Rate addition or assuming it's a fixed cost masquerading as hourly rate in current scope.
                 calculatedCost = op.Rate; 
                 break;
            default:
                 // Default to Per Unit if unknown, or Fixed? Safer to assume Per Unit for operations.
                 calculatedCost = op.Rate * quantity;
                 break;
        }

        // Apply Minimum Charges
        if (op.MinimumCharges > 0 && calculatedCost < op.MinimumCharges)
        {
            calculatedCost = op.MinimumCharges;
        }

        // Add Setup Charges (Fixed one-time)
        if (op.SetupCharges > 0)
        {
            calculatedCost += op.SetupCharges;
        }

        return Math.Round(calculatedCost, 2);
    }

    private double CalculateCategoryWastage(List<CategoryWastageSettingDto> settings, double reqRunningMeters, int frontColors, int backColors)
    {
         if (settings == null || settings.Count == 0) return 0;
         if (frontColors <= 0 && backColors <= 0) return 0;

         double totalWastage = 0;

         // Logic ported from Legacy CategoryWiseWastage
         if (frontColors > 0 && backColors > 0 && frontColors == backColors)
         {
             // 1. Equal Colors Front & Back -> Try 'Both Side' setting
             var row = settings.FirstOrDefault(s => s.PrintingStyle == "Both Side" && s.NoOfColor == frontColors);
             if (row != null)
             {
                 totalWastage = GetWastageValue(row, reqRunningMeters);
                 return totalWastage;
             }
         }

         // 2. Separate Front / Back Calculation
         if (frontColors > 0)
         {
             var rowF = settings.FirstOrDefault(s => s.PrintingStyle == "Single Side" && s.NoOfColor == frontColors);
             if (rowF != null) totalWastage += GetWastageValue(rowF, reqRunningMeters);
         }
         
         if (backColors > 0)
         {
             var rowB = settings.FirstOrDefault(s => s.PrintingStyle == "Single Side" && s.NoOfColor == backColors);
             if (rowB != null) totalWastage += GetWastageValue(rowB, reqRunningMeters);
         }

         return totalWastage;
    }

    private double GetWastageValue(CategoryWastageSettingDto setting, double reqQty)
    {
         double percentQty = (reqQty * (double)setting.WastagePercentage) / 100;
         double flatQty = (double)setting.FlatWastage;
         
         return Math.Max(flatQty, percentQty);
    }


    public async Task<Result<bool>> ValidateMachineCapability(int machineId, double jobWidth, double jobHeight)
    {
         var machineGrid = await _machineRepository.GetMachineGridAsync("FLEXO");
         var machine = machineGrid.FirstOrDefault(m => m.MachineID == machineId);
         if (machine == null) return Result<bool>.Failure("Machine not found");

         if (jobWidth < (double?)machine.MinSheetW || jobWidth > (double?)machine.MaxSheetW)
             return Result<bool>.Success(false);

         return Result<bool>.Success(true);
    }
}
