namespace RepairCenterSystem.Enums
{
    /// <summary>
    /// Life-cycle of an appliance inside the repair center.
    /// </summary>
    public enum RepairStatus
    {
        Received,     // appliance arrived at the center
        UnderRepair,  // technician started working on it
        Repaired,     // repair finished, waiting for the customer
        Delivered     // handed back to the customer
    }
}
