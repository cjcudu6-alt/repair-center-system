using System;
using System.Collections.Generic;
using RepairCenterSystem.Enums;
using RepairCenterSystem.Events;
using RepairCenterSystem.Helpers;
using RepairCenterSystem.Interfaces;

namespace RepairCenterSystem.Models
{
    /// <summary>
    /// Abstract base class for all appliances. Implements IRepairable and holds the shared data.
    /// Each concrete appliance must define its own repair cost rule.
    /// </summary>
    public abstract class Appliance : IRepairable
    {
        // ---------- Static members (statistics shared by all appliances) ----------
        private static readonly Dictionary<RepairStatus, int> _countByStatus = new Dictionary<RepairStatus, int>
        {
            { RepairStatus.Received, 0 },
            { RepairStatus.UnderRepair, 0 },
            { RepairStatus.Repaired, 0 },
            { RepairStatus.Delivered, 0 }
        };
        private static decimal _totalRepairedRevenue;

        /// <summary>Number of appliances still being repaired.</summary>
        public static int UnderRepairCount => _countByStatus[RepairStatus.UnderRepair];

        /// <summary>Number of appliances whose repair is finished (repaired or delivered).</summary>
        public static int RepairedCount => _countByStatus[RepairStatus.Repaired] + _countByStatus[RepairStatus.Delivered];

        /// <summary>Total cost of all appliances whose repair has been completed.</summary>
        public static decimal TotalRepairedRevenue => _totalRepairedRevenue;

        /// <summary>Returns how many appliances are currently in the given status.</summary>
        public static int GetCountByStatus(RepairStatus status) => _countByStatus[status];

        // ---------- Constants ----------
        protected const decimal HourlyLaborRate = 10m;

        // ---------- Private fields ----------
        private readonly string _serialNumber;
        private readonly string _brand;
        private readonly DateTime _receivedDate;
        private readonly DateTime _expectedDeliveryDate;
        private string _customerName;
        private string _faultDescription;
        private decimal _partsCost;
        private double _laborHours;
        private RepairStatus _status;

        // ---------- Event ----------
        /// <summary>Raised whenever the status of this appliance changes.</summary>
        public event StatusChangedHandler StatusChanged;

        // ---------- Constructor ----------
        protected Appliance(string serialNumber, string brand, string customerName,
                            string faultDescription, DateTime receivedDate, DateTime expectedDeliveryDate)
        {
            if (!Validator.IsValidSerialNumber(serialNumber))
                throw new ArgumentException("Invalid serial number. Expected format like RF-10234.");
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand is required.");
            if (!Validator.IsValidDeliveryDate(receivedDate, expectedDeliveryDate))
                throw new ArgumentException("Expected delivery date cannot be before the received date.");

            _serialNumber = serialNumber;
            _brand = brand.Trim();
            _receivedDate = receivedDate;
            _expectedDeliveryDate = expectedDeliveryDate;
            CustomerName = customerName;        // goes through validation in the property
            FaultDescription = faultDescription;
            _status = RepairStatus.Received;

            _countByStatus[RepairStatus.Received]++;
        }

        // ---------- Properties ----------
        /// <summary>Read-only: cannot change after the object is created.</summary>
        public string SerialNumber => _serialNumber;
        public string Brand => _brand;
        public DateTime ReceivedDate => _receivedDate;
        public DateTime ExpectedDeliveryDate => _expectedDeliveryDate;

        /// <summary>Current status. Can only be changed through StartRepair / CompleteRepair / Deliver.</summary>
        public RepairStatus Status => _status;

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (!Validator.IsValidName(value))
                    throw new ArgumentException("Customer name must contain at least 2 characters.");
                _customerName = value.Trim();
            }
        }

        public string FaultDescription
        {
            get => _faultDescription;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Fault description is required.");
                _faultDescription = value.Trim();
            }
        }

        public decimal PartsCost
        {
            get => _partsCost;
            set
            {
                if (!Validator.IsNonNegative(value))
                    throw new ArgumentException("Parts cost cannot be negative.");
                EnsureNotFinished();
                _partsCost = value;
            }
        }

        public double LaborHours
        {
            get => _laborHours;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Labor hours cannot be negative.");
                EnsureNotFinished();
                _laborHours = value;
            }
        }

        /// <summary>Labor cost shared by all appliance types.</summary>
        protected decimal LaborCost => (decimal)_laborHours * HourlyLaborRate;

        // ---------- Abstract members ----------
        /// <summary>Each appliance type calculates its repair cost in its own way.</summary>
        public abstract decimal CalculateRepairCost();

        /// <summary>Short name of the appliance type (used in reports).</summary>
        public abstract string ApplianceType { get; }

        // ---------- Repair workflow ----------
        public void StartRepair()
        {
            if (_status != RepairStatus.Received)
                throw new InvalidOperationException("Repair can only start for a newly received appliance.");
            ChangeStatus(RepairStatus.UnderRepair);
        }

        public void CompleteRepair()
        {
            if (_status != RepairStatus.UnderRepair)
                throw new InvalidOperationException("Only an appliance under repair can be completed.");
            _totalRepairedRevenue += CalculateRepairCost();
            ChangeStatus(RepairStatus.Repaired);
        }

        public void Deliver()
        {
            if (_status != RepairStatus.Repaired)
                throw new InvalidOperationException("Only a repaired appliance can be delivered.");
            ChangeStatus(RepairStatus.Delivered);
        }

        /// <summary>True when the delivery date has passed and the customer has not received the appliance.</summary>
        public bool IsOverdue(DateTime today)
        {
            return _status != RepairStatus.Delivered && today.Date > _expectedDeliveryDate.Date;
        }

        public string GetSummary()
        {
            return $"[{ApplianceType}] {_serialNumber} | {_brand} | Customer: {_customerName} | " +
                   $"Status: {_status} | Due: {_expectedDeliveryDate:yyyy-MM-dd} | Cost: {CalculateRepairCost():0.00}$";
        }

        // ---------- Helpers ----------
        private void ChangeStatus(RepairStatus newStatus)
        {
            RepairStatus oldStatus = _status;
            _status = newStatus;

            _countByStatus[oldStatus]--;
            _countByStatus[newStatus]++;

            // Notify subscribers (null-safe invocation)
            StatusChanged?.Invoke(this, oldStatus, newStatus);
        }

        private void EnsureNotFinished()
        {
            if (_status == RepairStatus.Repaired || _status == RepairStatus.Delivered)
                throw new InvalidOperationException("Cost data cannot be modified after the repair is completed.");
        }
    }
}
