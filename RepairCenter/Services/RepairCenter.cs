using System;
using System.Collections.Generic;
using System.Linq;
using RepairCenterSystem.Enums;
using RepairCenterSystem.Events;
using RepairCenterSystem.Models;

namespace RepairCenterSystem.Services
{
    /// <summary>
    /// Main class: holds a list of Appliance objects and processes them polymorphically.
    /// </summary>
    public class RepairCenter
    {
        private readonly List<Appliance> _appliances = new List<Appliance>();

        /// <summary>Forwarded from any appliance when its status changes.</summary>
        public event StatusChangedHandler ApplianceStatusChanged;

        /// <summary>Raised when an appliance is late for delivery.</summary>
        public event DeliveryDelayedHandler DeliveryDelayed;

        public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();

        public void AddAppliance(Appliance appliance)
        {
            if (appliance == null)
                throw new ArgumentNullException(nameof(appliance));
            if (FindBySerial(appliance.SerialNumber) != null)
                throw new InvalidOperationException("An appliance with this serial number already exists.");

            appliance.StatusChanged += OnApplianceStatusChanged;
            _appliances.Add(appliance);
        }

        public Appliance FindBySerial(string serial)
        {
            return _appliances.FirstOrDefault(a => a.SerialNumber.Equals(serial, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Appliance> GetByStatus(RepairStatus status)
        {
            return _appliances.Where(a => a.Status == status);
        }

        /// <summary>Polymorphism: each appliance calculates its own cost.</summary>
        public decimal GetTotalRepairCost()
        {
            decimal total = 0;
            foreach (Appliance appliance in _appliances)
                total += appliance.CalculateRepairCost();
            return total;
        }

        public decimal GetTotalCostByStatus(RepairStatus status)
        {
            return GetByStatus(status).Sum(a => a.CalculateRepairCost());
        }

        /// <summary>Returns one summary line per appliance (polymorphic call to GetSummary).</summary>
        public IEnumerable<string> GetAllSummaries()
        {
            foreach (Appliance appliance in _appliances)
                yield return appliance.GetSummary();
        }

        /// <summary>Checks all appliances and raises DeliveryDelayed for each late one.</summary>
        public int CheckOverdueDeliveries(DateTime today)
        {
            int count = 0;
            foreach (Appliance appliance in _appliances)
            {
                if (appliance.IsOverdue(today))
                {
                    int daysLate = (today.Date - appliance.ExpectedDeliveryDate.Date).Days;
                    DeliveryDelayed?.Invoke(appliance, daysLate);
                    count++;
                }
            }
            return count;
        }

        private void OnApplianceStatusChanged(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus)
        {
            ApplianceStatusChanged?.Invoke(appliance, oldStatus, newStatus);
        }
    }
}
