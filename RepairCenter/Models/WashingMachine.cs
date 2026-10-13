using System;

namespace RepairCenterSystem.Models
{
    /// <summary>
    /// Washing machine: cost depends on drum damage and machine capacity.
    /// </summary>
    public class WashingMachine : Appliance
    {
        private const decimal DrumRepairFee = 60m;
        private const decimal LargeCapacitySurcharge = 15m;
        private const double LargeCapacityKg = 8.0;

        private readonly double _capacityKg;
        private bool _isDrumDamaged;

        public WashingMachine(string serialNumber, string brand, string customerName, string faultDescription,
                              DateTime receivedDate, DateTime expectedDeliveryDate, double capacityKg)
            : base(serialNumber, brand, customerName, faultDescription, receivedDate, expectedDeliveryDate)
        {
            if (capacityKg <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");
            _capacityKg = capacityKg;
        }

        public double CapacityKg => _capacityKg;

        public bool IsDrumDamaged
        {
            get => _isDrumDamaged;
            set => _isDrumDamaged = value;
        }

        public override string ApplianceType => "Washing Machine";

        public override decimal CalculateRepairCost()
        {
            decimal cost = LaborCost + PartsCost;
            if (_isDrumDamaged) cost += DrumRepairFee;
            if (_capacityKg > LargeCapacityKg) cost += LargeCapacitySurcharge;
            return cost;
        }
    }
}
