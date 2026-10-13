using RepairCenterSystem.Enums;

namespace RepairCenterSystem.Interfaces
{
    /// <summary>
    /// Core operations shared by every appliance handled by the repair center.
    /// </summary>
    public interface IRepairable
    {
        string SerialNumber { get; }
        RepairStatus Status { get; }

        decimal CalculateRepairCost();
        void StartRepair();
        void CompleteRepair();
        void Deliver();
        string GetSummary();
    }
}
