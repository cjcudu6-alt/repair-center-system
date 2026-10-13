using RepairCenterSystem.Enums;
using RepairCenterSystem.Models;

namespace RepairCenterSystem.Events
{
    /// <summary>
    /// Delegate used to notify subscribers that an appliance changed its status.
    /// </summary>
    public delegate void StatusChangedHandler(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus);

    /// <summary>
    /// Delegate used to notify subscribers that an appliance is late for delivery.
    /// </summary>
    public delegate void DeliveryDelayedHandler(Appliance appliance, int daysLate);
}
