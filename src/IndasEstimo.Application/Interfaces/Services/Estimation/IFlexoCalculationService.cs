using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for pure Flexo logic calculation.
/// Encapsulates the core algorithms for Roll/Cylinder estimation.
/// </summary>
public interface IFlexoCalculationService
{
    /// <summary>
    /// Calculates optimal Flexo plan based on Job, Material, and Machine constraints.
    /// Equivalent to legacy 'Shirin_Job' -> 'Plan_On_Roll' pipeline.
    /// </summary>
    Task<Result<List<FlexoPlanResult>>> CalculateFlexoPlanAsync(FlexoPlanCalculationRequest request);

    /// <summary>
    /// Validates if a job can fit on a specific machine/cylinder combination.
    /// Used for pre-filtering machines.
    /// </summary>
    Task<Result<bool>> ValidateMachineCapability(int machineId, double jobWidth, double jobHeight);
}
