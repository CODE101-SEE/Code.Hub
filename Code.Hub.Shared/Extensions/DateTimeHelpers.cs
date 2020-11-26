using System;
using System.Linq;

namespace Code.Hub.Shared.Extensions
{
    public static class DateTimeHelpers
    {
        public static double[] AllowedMinutes = { 0, 15, 30, 45 };
        public static int MaxAllowedHours = 24;

        public static double GetTimeFromString(string timeString)
        {
            var times = timeString.Split(":");

            if (times.Length < 1 || times.Length > 2)
                throw new Exception("Invalid format provided");

            double hours = Convert.ToDouble(times[0]);
            if (hours > 24)
                throw new Exception("Invalid hours provided");

            double minutes = 0;
            if (times.Length == 2)
                minutes = Convert.ToDouble(times[1]);

            return Math.Round((hours * 60 + minutes) / 60, 2); // Get minutes, then turn to hours and round to two decimal places
        }

        public static string GetFormattedTime(double time)
        {
            // Some system languages replace . with , and vice versa for numbers
            var stringHelper = time.ToString("00.00").Replace('.', ',');
            var hourHelper = stringHelper.Split(',');

            // Minutes are still represented as decimal part of an hour, not as actual minutes
            // We have to calculate how much minutes that actually is
            if (hourHelper.Length == 2 && hourHelper[1] != null)
            {
                // Example: Will take minutes as part of hour (.25) and turn them into minutes for diplay (.30, 30 minutes)
                var minutes = Math.Round((Double.Parse(hourHelper[1]) / 100 * 60), 2);
                hourHelper[1] = minutes.ToString("00");
            }

            // Will join strings to proper format (3.30 to 3:30)
            return string.Join(':', hourHelper);
        }

        public static double GetHoursFromString(string time, bool useMaxHours)
        {
            double? parsedTime = null;
            if (time.All(char.IsDigit))
                parsedTime = double.Parse(time);

            if (time.Contains(":"))
            {
                var times = time.Split(":");
                if (times.Length != 2)
                    throw new Exception("Invalid format provided");

                var hours = Convert.ToDouble(times[0]);
                var minutes = Convert.ToDouble(times[1]);
                if (!AllowedMinutes.Contains(minutes))
                    throw new Exception("Invalid minutes provided");

                parsedTime = Math.Round((hours * 60 + minutes) / 60, 2); // Get minutes, then turn to hours and round to two decimal places
            }

            if (!parsedTime.HasValue)
                throw new Exception("Invalid format provided");

            if (useMaxHours && parsedTime > MaxAllowedHours)
                throw new Exception($"You are limited to using {MaxAllowedHours}");

            return parsedTime.Value;
        }

    }
}
