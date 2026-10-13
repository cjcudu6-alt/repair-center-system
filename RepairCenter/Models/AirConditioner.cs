using System;

namespace RepairCenterSystem.Models
{
    /// <summary>
    /// Air conditioner: labor is more expensive (work at height) and an on-site visit may be required.
    /// </summary>
    public class AirConditioner : Appliance
    {
        private const decimal HeightWorkMultiplier = 1.2m;
        private const decimal OnSiteVisitFee = 25m;
        private const decimal GasRefillFee = 50m;

        private readonly int _capacityBtu;
        private bool _requiresOnSiteVisit;
        private bool _needsGasRefill;

        public AirConditioner(string serialNumber, string brand, string customerName, string faultDescription,
                              DateTime receivedDate, DateTime expectedDeliveryDate, int capacityBtu)
            : base(serialNumber, brand, customerName, faultDescription, receivedDate, expectedDeliveryDate)
        {
            if (capacityBtu <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");
            _capacityBtu = capacityBtu;
        }

        public int CapacityBtu => _capacityBtu;

        public bool RequiresOnSiteVisit
        {
            get => _requiresOnSiteVisit;
            set => _requiresOnSiteVisit = value;
        }

        public bool NeedsGasRefill
        {
            get => _needsGasRefill;
            set => _needsGasRefill = value;
        }

        public override string ApplianceType => "Air Conditioner";

        public override decimal CalculateRepairCost()
        {
            decimal cost = (LaborCost * HeightWorkMultiplier) + PartsCost;
            if (_requiresOnSiteVisit) cost += OnSiteVisitFee;
            if (_needsGasRefill) cost += GasRefillFee;
            return cost;
        }
    }
}
