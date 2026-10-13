using System;

namespace RepairCenterSystem.Models
{
    /// <summary>
    /// Refrigerator: cost depends on compressor replacement and gas refill.
    /// </summary>
    public class Refrigerator : Appliance
    {
        private const decimal CompressorFee = 120m;
        private const decimal GasRefillFee = 40m;

        private readonly int _capacityLiters;
        private bool _needsCompressorReplacement;
        private bool _needsGasRefill;

        public Refrigerator(string serialNumber, string brand, string customerName, string faultDescription,
                            DateTime receivedDate, DateTime expectedDeliveryDate, int capacityLiters)
            : base(serialNumber, brand, customerName, faultDescription, receivedDate, expectedDeliveryDate)
        {
            if (capacityLiters <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");
            _capacityLiters = capacityLiters;
        }

        public int CapacityLiters => _capacityLiters;

        public bool NeedsCompressorReplacement
        {
            get => _needsCompressorReplacement;
            set => _needsCompressorReplacement = value;
        }

        public bool NeedsGasRefill
        {
            get => _needsGasRefill;
            set => _needsGasRefill = value;
        }

        public override string ApplianceType => "Refrigerator";

        public override decimal CalculateRepairCost()
        {
            decimal cost = LaborCost + PartsCost;
            if (_needsCompressorReplacement) cost += CompressorFee;
            if (_needsGasRefill) cost += GasRefillFee;
            return cost;
        }
    }
}
