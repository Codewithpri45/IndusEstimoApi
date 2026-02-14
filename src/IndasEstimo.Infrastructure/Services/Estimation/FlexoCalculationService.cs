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

            // 1. Fetch Machine Details and Slabs
            var machineGrid = await _machineRepository.GetMachineGridAsync("FLEXO"); 
            var selectedMachine = machineGrid.FirstOrDefault(m => m.MachineID == request.MachineId);
            
            if (selectedMachine == null)
                return Result<List<FlexoPlanResult>>.Failure($"Machine ID {request.MachineId} not found.");

            var machineSlabs = await _machineRepository.GetMachineSlabsAsync(selectedMachine.MachineID);

            // 2. Decide Calculation Strategy
            ReelDto? specificReel = null;
            if (request.PaperId > 0)
            {
                specificReel = await _materialRepository.GetReelByIdAsync(request.PaperId);
                // Note: Not failing here if null yet, handled in branches
            }

            if (request.CylinderId > 0)
            {
               if (request.PaperId > 0 && specificReel == null)
                   return Result<List<FlexoPlanResult>>.Failure($"Paper ID {request.PaperId} not found in database.");

               await PlanOnCylinder(request, selectedMachine, machineSlabs, plans, specificReel);
            }
            else
            {
               var reels = new List<ReelDto>();
               if (request.PaperId > 0)
               {
                   if (specificReel == null)
                       return Result<List<FlexoPlanResult>>.Failure($"Paper ID {request.PaperId} not found.");
                   reels.Add(specificReel);
               }
               else
               {
                   var foundReels = await _materialRepository.GetReelsAsync(selectedMachine.MinSheetW ?? 0, 5000, 0, -14); 
                   reels.AddRange(foundReels);
               }

               if (reels.Count == 0)
                    return Result<List<FlexoPlanResult>>.Failure($"No suitable paper reels found for Machine {selectedMachine.MachineName}.");

               foreach (var reel in reels)
               {
                   PlanOnRoll(request, selectedMachine, reel, machineSlabs, plans);
               }
            }

            if (plans.Count == 0)
            {
                string debugInfo = $"Machine: {selectedMachine.MachineName} (Limits: {selectedMachine.MinSheetW ?? 0}-{selectedMachine.MaxSheetW ?? 0} mm). ";
                if (request.CylinderId > 0) 
                    debugInfo += "PlanOnCylinder Strategy: Calculated widths did not fit within machine limits.";
                else 
                    debugInfo += "PlanOnRoll Strategy: No available reels fit within limits or gap logic.";
                
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

    private void PlanOnRoll(FlexoPlanCalculationRequest request, MachineGridDto machine, ReelDto reel, List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto> slabs, List<FlexoPlanResult> plans)
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

        int upsAcross = CalculateUpsAcross(request.JobSizeW, request.GapAcross, rollWidthEffective);
        
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
                UpsAround = 0, 
                CutSizeW = usedWidth,
                MachineName = machine.MachineName ?? string.Empty,
            };
            
            CalculateCosting(plan, request, machine, slabs, (double)reel.GSM, (double)reel.EstimationRate);
            plans.Add(plan);
        }
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
                     ToolDescription = $"Cylinder ID: {request.CylinderId}"
                 };

                 CalculateCosting(plan, request, machine, slabs, gsm, (double)(specificReel?.EstimationRate ?? 0));
                 plans.Add(plan);
            }
        }
    }

    private int CalculateUpsAcross(double jobSize, double gap, double effectiveWidth)
    {
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

    private void CalculateCosting(
        FlexoPlanResult plan, 
        FlexoPlanCalculationRequest request, 
        MachineGridDto machine,
        List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto> slabs,
        double gsm,
        double specificPaperRate = 0)
    {
        double totalUps = plan.UpsAcross; // * UpsAround (UpsAround is traditionally 1 for Flexo roll planning context here)
        if (totalUps == 0) return;

        // 1. Calculate Running Meters for the JOB only
        double labelsPerMeter = (1000 / (request.JobSizeH + request.GapAround)) * plan.UpsAcross;
        if (labelsPerMeter <= 0) return;

        double jobRunningMeters = request.Quantity / labelsPerMeter;
        
        // 2. Wastage Parameters (Strict Legacy Fallbacks)
        double makeReadyWastage = 50; 
        double processWastagePer = 2.0; 
        double rollChangeWastage = 10; 
        decimal ratePerRun = 0; // Will be calculated based on time
        double slabPlateCharges = 0;
        
        // Find Slab based on Job Running Meters
        var slab = slabs.FirstOrDefault(s => jobRunningMeters >= (double)s.RunningMeterRangeFrom && jobRunningMeters <= (double)s.RunningMeterRangeTo);

        if (slab != null)
        {
            makeReadyWastage = (double)slab.Wastage; // Legacy "Wastage" column is usually MakeReady Wastage (Meters)
            // Process Wastage usually comes from Machine Master or Global Config, but if Slab has it, use it.
            // Assuming Slab Rate is Fixed Rate for this Quantity Range (Legacy behavior for some clients) OR Rate per 1000.
            ratePerRun = slab.Rate;
            slabPlateCharges = (double)slab.PlateCharges;
        }

        // 3. Detailed Wastage Calculation
        double processWastageMeters = jobRunningMeters * (processWastagePer / 100);
        // Approximation: 1 roll change every 1000 meters or 200kg? Legacy usually has a fixed 'Avg Roll Length'
        int numRollChanges = (int)(jobRunningMeters / 2000); // Assuming 2000m roll length as standard placeholder if not in DB
        double totalRollChangeWastage = numRollChanges * rollChangeWastage;

        double totalMeters = jobRunningMeters + makeReadyWastage + processWastageMeters + totalRollChangeWastage;

        // 4. Material Cost (Paper)
        // Formula: (TotalMeters * Width_mm * GSM) / 1,000,000
        double usedGsm = gsm > 0 ? gsm : 100; // Fallback
        plan.TotalPaperWeightKg = (totalMeters * plan.PaperWidth * usedGsm) / 1000000; 
        plan.TotalQuantity = request.Quantity;
        
        double effectivePaperRate = specificPaperRate > 0 ? specificPaperRate : request.PaperRate;
        plan.PaperCostTotal = plan.TotalPaperWeightKg * effectivePaperRate;

        // 5. Machine Run Cost
        // Legacy: RunCost = (TotalRunningMeters / Speed) * HourlyRate
        // OR Slab Rate (Flat Cost for range).
        // If Slab Rate > 0, we use it as the TOTAL machine cost for this quantity bracket (common in label industry).
        // If Slab Rate is 0, we fallback to Hourly Calculation.
        
        if (ratePerRun > 0)
        {
             plan.MachineRunCostTotal = (double)ratePerRun;
        }
        else
        {
             // Hourly Calculation
             double speedPerMin = 30; // 30 meters per min default
             double runTimeMinutes = totalMeters / speedPerMin;
             double runTimeHours = runTimeMinutes / 60;
             plan.MachineRunCostTotal = runTimeHours * (double)machine.PerHourRate;
        }

        // 6. Plate Cost
        int totalColors = request.FrontColors + request.BackColors + request.SpecialFrontColors + request.SpecialBackColors;
        double effectivePlateRate = request.PlateRate > 0 ? request.PlateRate : slabPlateCharges;
        plan.PlateCostTotal = effectivePlateRate * totalColors;

        // 7. Additional Operations (Conversion)
        double additionalOpsTotal = 0;
        foreach (var op in request.AdditionalOperations)
        {
            if (op.RateType == "PerHour")
            {
                 // Ops Time often = Machine Run Time
                 // additionalOpsTotal += (totalMeters / OpSpeed) * Rate
                 additionalOpsTotal += op.Rate; // Simplified
            }
            else // PerUnit
            {
                additionalOpsTotal += op.Rate * request.Quantity;
            }
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
