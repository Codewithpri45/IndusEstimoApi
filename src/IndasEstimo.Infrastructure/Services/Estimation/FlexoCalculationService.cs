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

            // Filter by requested MachineID - this returns ALL cylinders for that machine
            var machineCylinderCombos = machineGrid
                .Where(m => m.MachineID == request.MachineId)
                .ToList();

            if (machineCylinderCombos.Count == 0)
                return Result<List<FlexoPlanResult>>.Failure($"Machine ID {request.MachineId} not found.");

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
                // Gap #5: Grain Direction Variations - Legacy calls calculation twice (With Grain & Across Grain)
                // Legacy: Lines where dimensions are swapped for grain direction trials
                var grainDirections = new[]
                {
                    new { Direction = "With Grain", JobSizeW = request.JobSizeW, JobSizeH = request.JobSizeH },
                    new { Direction = "Across Grain", JobSizeW = request.JobSizeH, JobSizeH = request.JobSizeW }
                };

                foreach (var grainVariation in grainDirections)
                {
                    // Create modified request with swapped dimensions for grain direction
                    var modifiedRequest = new FlexoPlanCalculationRequest
                    {
                        JobSizeL = request.JobSizeL,
                        JobSizeW = grainVariation.JobSizeW,
                        JobSizeH = grainVariation.JobSizeH,
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
                        CategoryId = request.CategoryId
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
                        var foundReels = await _materialRepository.GetReelsAsync(
                            machineWithCylinder.MinSheetW ?? 0, 5000, 0, -14);

                        foreach (var reel in foundReels)
                        {
                            await PlanOnRoll(modifiedRequest, machineWithCylinder, reel, machineSlabs, plans, grainVariation.Direction);
                        }

                        // B2. If Cylinder Selected, ALSO Suggest Optimal Special Sizes
                        if (request.CylinderId > 0)
                        {
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
        double rollWidthEffective = (double)reel.SizeW - ((PlateBearer * 2) + (ColorStrip * 2) + StandardGap);
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
                 double jobRepeat = request.JobSizeH + request.GapAround;
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

        int upsAcross = CalculateUpsAcross(request.JobSizeW, request.GapAcross, rollWidthEffective, request.UpsAcross);
        
        if (upsAcross > 0)
        {
            _logger.LogInformation($"Plan Found on Roll! UpsAcross={upsAcross}");
            
            double usedWidth = (upsAcross * request.JobSizeW) + ((upsAcross - 1) * request.GapAcross);

            var plan = new FlexoPlanResult
            {
                PaperId = reel.ItemID,
                PaperName = reel.ItemName,
                PaperWidth = (double)reel.SizeW,
                UpsAcross = upsAcross,
                UpsAround = (int)Math.Floor(1000 / (request.JobSizeH + request.GapAround)), // Standard Rows per Meter logic for Flexo Roll
                CutSizeW = usedWidth,
                MachineName = machine.MachineName ?? string.Empty,

                // CRITICAL FIX (Gap #4): Populate Cylinder Details from machine-cylinder combo
                // Legacy: Lines 16771+ (CylinderToolID, CylinderCircumferenceMM, etc.)
                CylinderCircumference = (double)machine.CylinderCircumferenceMM,
                CylinderTeeth = machine.CylinderNoOfTeeth,
                ToolDescription = machine.CylinderToolID > 0
                    ? $"{machine.CylinderToolCode} (Circ: {machine.CylinderCircumferenceMM}mm, Teeth: {machine.CylinderNoOfTeeth})"
                    : string.Empty,

                // Gap #8: Gap & Wastage Strip
                AcrossGap = request.GapAcross,
                AroundGap = request.GapAround,
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

        if (Math.Floor((effectiveWidth + StandardGap) / labelSizeWithGap) == 1)
        {
             if (jobSize <= (effectiveWidth + StandardGap))
             {
                 ups = (int)Math.Floor((effectiveWidth + StandardGap) / jobSize);
             }
        }
        else if (labelSizeWithGap <= effectiveWidth)
        {
             ups = (int)Math.Floor((effectiveWidth + StandardGap) / labelSizeWithGap);
             
             int nextUps = ups + 1;
             double requiredWidth = (nextUps * jobSize) + ((nextUps - 1) * gap);
             
             if ((effectiveWidth + StandardGap) >= requiredWidth)
             {
                 ups = nextUps;
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
        double machineMaxRoll = (double)(machine.MaxSheetW ?? 0);
        double maxUsableCheck = machineMaxRoll - ((PlateBearer * 2) + (ColorStrip * 2)); 

        int maxUpsW = (int)Math.Floor(maxUsableCheck / request.JobSizeW); 
        
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
            double requiredWidth = (i * request.JobSizeW) + ((i - 1) * request.GapAcross) + (PlateBearer * 2) + (ColorStrip * 2);
            
            if (requiredWidth >= (double)(machine.MinSheetW ?? 0) && requiredWidth <= machineMaxRoll)
            {
                 var plan = new FlexoPlanResult
                 {
                     PaperId = specificReel?.ItemID ?? 0, 
                     PaperName = specificReel?.ItemName ?? $"Special Size {requiredWidth}mm",
                     PaperWidth = requiredWidth,
                     UpsAcross = i,
                     UpsAround = 0, 
                     CutSizeW = (i * request.JobSizeW) + ((i - 1) * request.GapAcross),
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
        double totalUps = plan.UpsAcross; // * UpsAround (UpsAround is traditionally 1 for Flexo roll planning context here)
        if (totalUps == 0) return;

        // 1. Calculate Required Running Meters
        double reqRunningMeters = 0;
        
        if (request.Orientation == "PrePlannedSheetLabel")
        {
             double labelsPerMeter = (1000 / request.JobSizeH) * plan.UpsAcross; 
             if (labelsPerMeter > 0) reqRunningMeters = request.Quantity / labelsPerMeter;
        }
        else
        {
            double repeat = request.JobSizeH + request.GapAround;
            if (repeat <= 0) repeat = 1;
            double labelsPerMeter = (1000 / repeat) * plan.UpsAcross;
            if (labelsPerMeter > 0) reqRunningMeters = request.Quantity / labelsPerMeter;
        }

        // 2. Wastage Parameters & Slab Selection
        decimal ratePerRun = 0; 
        double slabPlateCharges = 0;
        double makeReadyWastage = 50; // Fallback

        // Find Slab based on Job Metrics
        var slab = slabs.FirstOrDefault(s => reqRunningMeters >= (double)s.RunningMeterRangeFrom && reqRunningMeters <= (double)s.RunningMeterRangeTo);

        if (slab != null)
        {
            makeReadyWastage = (double)slab.Wastage; 
            ratePerRun = slab.Rate;
            slabPlateCharges = (double)slab.PlateCharges;
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
        double scrapSquareMeter = totalSquareMeter -
            ((request.JobSizeH / 1000.0) * (request.JobSizeW / 1000.0) * request.Quantity);

        plan.TotalPaperWeightKg = (totalMeters * plan.PaperWidth * usedGsm) / 1000000;
        plan.TotalQuantity = request.Quantity;

        // Populate Square Meter fields (Gap #7)
        plan.RequiredRunningMeter = reqRunningMeters;
        plan.TotalRunningMeter = totalMeters;
        plan.RequiredSquareMeter = reqSquareMeter;
        plan.TotalSquareMeter = totalSquareMeter;
        plan.WastageSquareMeter = wastageSquareMeter;
        plan.ScrapSquareMeter = scrapSquareMeter;

        // Gap #11: Printing Impressions Calculation
        // For Flexo: 1 impression = 1 cylinder rotation = cylinder circumference in meters
        double repeatLength = request.JobSizeH + request.GapAround; // mm
        if (repeatLength > 0)
        {
            // Total impressions = total meters / repeat length (in meters)
            plan.PrintingImpressions = Math.Round((totalMeters * 1000) / repeatLength, 0);
            plan.ImpressionsToBeCharged = Math.Round((reqRunningMeters * 1000) / repeatLength, 0);
        }
        else
        {
            plan.PrintingImpressions = 0;
            plan.ImpressionsToBeCharged = 0;
        }

        // CRITICAL FIX (Gap #6): Paper Cost based on Rate Type
        // Legacy: Lines 16454-16461
        double effectivePaperRate = specificPaperRate > 0 ? specificPaperRate : request.PaperRate;
        string paperRateType = reel?.EstimationUnit ?? request.PaperRateType;

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

        // 8. Total Summary
        plan.TotalCost = plan.PaperCostTotal + plan.MachineRunCostTotal + plan.PlateCostTotal + plan.ConversionCostTotal;
        
        // Populate Detailed Breakdown for Transparency
        plan.MakeReadyWastageMeters = makeReadyWastage;
        plan.ProcessWastageMeters = processWastageMeters;
        plan.RollChangeWastageMeters = totalRollChangeWastage;
        
        if (request.Quantity > 0)
        {
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
