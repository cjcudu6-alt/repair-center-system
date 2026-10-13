using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RepairCenterSystem.Helpers
{
    /// <summary>
    /// Static utility class that validates user input before it reaches the models.
    /// </summary>
    public static class Validator
    {
        // Two or three capital letters, a dash, then 4 to 8 digits. Example: RF-10234
        private static readonly Regex SerialPattern = new Regex(@"^[A-Z]{2,3}-\d{4,8}$", RegexOptions.Compiled);

        public static bool IsValidSerialNumber(string serial)
        {
            return !string.IsNullOrWhiteSpace(serial) && SerialPattern.IsMatch(serial);
        }

        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= 2;
        }

        public static bool IsNonNegative(decimal value)
        {
            return value >= 0;
        }

        /// <summary>
        /// The expected delivery date must not be earlier than the received date.
        /// </summary>
        public static bool IsValidDeliveryDate(DateTime received, DateTime expected)
        {
            return expected.Date >= received.Date;
        }

        /// <summary>Parses a date in yyyy-MM-dd format.</summary>
        public static bool TryParseDate(string input, out DateTime date)
        {
            return DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out date);
        }
    }
}
