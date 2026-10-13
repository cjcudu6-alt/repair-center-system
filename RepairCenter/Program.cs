using System;
using RepairCenterSystem.Enums;
using RepairCenterSystem.Helpers;
using RepairCenterSystem.Models;
using RepairCenterSystem.Services;

namespace RepairCenterSystem
{
    /// <summary>
    /// Console entry point: a simple menu to manage the repair center.
    /// </summary>
    internal class Program
    {
        private static readonly RepairCenter Center = new RepairCenter();

        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            SubscribeToEvents();

            bool running = true;
            while (running)
            {
                ShowMenu();
                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": AddApplianceFlow(); break;
                        case "2": ListAppliances(); break;
                        case "3": ChangeStatusFlow("start"); break;
                        case "4": ChangeStatusFlow("complete"); break;
                        case "5": ChangeStatusFlow("deliver"); break;
                        case "6": ShowStatistics(); break;
                        case "7": CheckOverdue(); break;
                        case "8": LoadSampleData(); break;
                        case "0": running = false; break;
                        default: Console.WriteLine("Invalid choice."); break;
                    }
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    WriteColored("Error: " + ex.Message, ConsoleColor.Red);
                }
            }
        }

        // ---------- Event subscription ----------
        private static void SubscribeToEvents()
        {
            // Subscribe to the events using lambda expressions that match the delegate signatures
            Center.ApplianceStatusChanged += (appliance, oldStatus, newStatus) =>
            {
                WriteColored($"[EVENT] {appliance.SerialNumber}: {oldStatus} -> {newStatus}", ConsoleColor.Cyan);
                if (newStatus == RepairStatus.Repaired)
                    WriteColored($"[NOTIFY] Repair completed for {appliance.CustomerName}. Final cost: {appliance.CalculateRepairCost():0.00}$", ConsoleColor.Green);
            };

            Center.DeliveryDelayed += (appliance, daysLate) =>
            {
                WriteColored($"[ALERT] {appliance.SerialNumber} ({appliance.CustomerName}) is {daysLate} day(s) late!", ConsoleColor.Yellow);
            };
        }

        // ---------- Menu ----------
        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== Home Appliance Repair Center =====");
            Console.WriteLine("1) Add appliance");
            Console.WriteLine("2) List all appliances");
            Console.WriteLine("3) Start repair");
            Console.WriteLine("4) Complete repair");
            Console.WriteLine("5) Deliver to customer");
            Console.WriteLine("6) Statistics");
            Console.WriteLine("7) Check overdue deliveries");
            Console.WriteLine("8) Load sample data");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");
        }

        // ---------- Flows ----------
        private static void AddApplianceFlow()
        {
            Console.WriteLine("Type: 1) Refrigerator  2) Washing Machine  3) Air Conditioner");
            Console.Write("Choose type: ");
            string type = Console.ReadLine();

            string serial = Ask("Serial number (e.g. RF-10234): ").ToUpper();
            if (!Validator.IsValidSerialNumber(serial))
                throw new ArgumentException("Invalid serial number format.");

            string brand = Ask("Brand: ");
            string customer = Ask("Customer name: ");
            string fault = Ask("Fault description: ");

            DateTime received = DateTime.Today;
            DateTime expected = AskDate("Expected delivery date (yyyy-MM-dd): ");

            Appliance appliance;
            switch (type)
            {
                case "1":
                    var fridge = new Refrigerator(serial, brand, customer, fault, received, expected,
                                                  int.Parse(Ask("Capacity (liters): ")));
                    fridge.NeedsCompressorReplacement = AskYesNo("Needs compressor replacement?");
                    fridge.NeedsGasRefill = AskYesNo("Needs gas refill?");
                    appliance = fridge;
                    break;
                case "2":
                    var washer = new WashingMachine(serial, brand, customer, fault, received, expected,
                                                    double.Parse(Ask("Capacity (kg): ")));
                    washer.IsDrumDamaged = AskYesNo("Is the drum damaged?");
                    appliance = washer;
                    break;
                case "3":
                    var ac = new AirConditioner(serial, brand, customer, fault, received, expected,
                                                int.Parse(Ask("Capacity (BTU): ")));
                    ac.RequiresOnSiteVisit = AskYesNo("Requires on-site visit?");
                    ac.NeedsGasRefill = AskYesNo("Needs gas refill?");
                    appliance = ac;
                    break;
                default:
                    throw new ArgumentException("Unknown appliance type.");
            }

            appliance.LaborHours = double.Parse(Ask("Estimated labor hours: "));
            appliance.PartsCost = decimal.Parse(Ask("Parts cost ($): "));

            Center.AddAppliance(appliance);
            WriteColored("Appliance added successfully.", ConsoleColor.Green);
        }

        private static void ListAppliances()
        {
            if (Center.Appliances.Count == 0)
            {
                Console.WriteLine("No appliances registered.");
                return;
            }

            foreach (string line in Center.GetAllSummaries())
                Console.WriteLine(line);
        }

        private static void ChangeStatusFlow(string action)
        {
            string serial = Ask("Serial number: ").ToUpper();
            Appliance appliance = Center.FindBySerial(serial);
            if (appliance == null)
                throw new InvalidOperationException("Appliance not found.");

            switch (action)
            {
                case "start": appliance.StartRepair(); break;
                case "complete": appliance.CompleteRepair(); break;
                case "deliver": appliance.Deliver(); break;
            }
        }

        private static void ShowStatistics()
        {
            Console.WriteLine($"Under repair now        : {Appliance.UnderRepairCount}");
            Console.WriteLine($"Repaired / delivered    : {Appliance.RepairedCount}");
            Console.WriteLine($"Revenue (repaired jobs) : {Appliance.TotalRepairedRevenue:0.00}$");
            Console.WriteLine($"Total cost (all jobs)   : {Center.GetTotalRepairCost():0.00}$");
            Console.WriteLine();
            foreach (RepairStatus status in Enum.GetValues(typeof(RepairStatus)))
            {
                Console.WriteLine($"{status,-12}: {Appliance.GetCountByStatus(status)} appliance(s), " +
                                  $"{Center.GetTotalCostByStatus(status):0.00}$");
            }
        }

        private static void CheckOverdue()
        {
            DateTime today = DateTime.Today;
            string input = Ask("Check as of date (yyyy-MM-dd) or press Enter for today: ");
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (!Validator.TryParseDate(input, out today))
                    throw new ArgumentException("Invalid date format.");
            }

            int count = Center.CheckOverdueDeliveries(today);
            Console.WriteLine($"Overdue appliances: {count}");
        }

        private static void LoadSampleData()
        {
            DateTime today = DateTime.Today;

            var fridge = new Refrigerator("RF-10234", "Samsung", "Ahmad Ali", "Not cooling", today.AddDays(-10), today.AddDays(-3), 450);
            fridge.NeedsCompressorReplacement = true;
            fridge.LaborHours = 3;
            fridge.PartsCost = 35m;

            var washer = new WashingMachine("WM-20457", "LG", "Sara Hassan", "Drum does not spin", today.AddDays(-2), today.AddDays(5), 9);
            washer.IsDrumDamaged = true;
            washer.LaborHours = 2;
            washer.PartsCost = 20m;

            var ac = new AirConditioner("AC-30981", "Gree", "Omar Khaled", "Water leakage", today.AddDays(-1), today.AddDays(4), 18000);
            ac.RequiresOnSiteVisit = true;
            ac.NeedsGasRefill = true;
            ac.LaborHours = 2.5;
            ac.PartsCost = 15m;

            Center.AddAppliance(fridge);
            Center.AddAppliance(washer);
            Center.AddAppliance(ac);
            WriteColored("Sample data loaded (3 appliances).", ConsoleColor.Green);
        }

        // ---------- Input helpers ----------
        private static string Ask(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        private static bool AskYesNo(string prompt)
        {
            Console.Write(prompt + " (y/n): ");
            string answer = Console.ReadLine();
            return answer != null && answer.Trim().ToLower() == "y";
        }

        private static DateTime AskDate(string prompt)
        {
            if (!Validator.TryParseDate(Ask(prompt), out DateTime date))
                throw new ArgumentException("Invalid date format. Use yyyy-MM-dd.");
            return date;
        }

        private static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
