using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace MangoTube8
{
    class Utils
    {
        public static int ParseViewCount(string viewsText)
        {
            if (string.IsNullOrEmpty(viewsText))
            {
                return 0;
            }

            var numericText = viewsText.Replace(" views", "").Trim();

            if (numericText.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                numericText = numericText.Replace("M", "").Trim();
                double result;
                if (double.TryParse(numericText, out result))
                {
                    return (int)(result * 1000000);
                }
            }
            else if (numericText.EndsWith("K", StringComparison.OrdinalIgnoreCase))
            {
                numericText = numericText.Replace("K", "").Trim();
                double result;
                if (double.TryParse(numericText, out result))
                {
                    return (int)(result * 1000);
                }
            }

            int viewsCount;
            if (int.TryParse(numericText, out viewsCount))
            {
                return viewsCount;
            }

            return 0;
        }

        public static string ParseViewCountString(string viewsText)
        {
            if (string.IsNullOrEmpty(viewsText))
            {
                return "0 views";
            }

            var numericText = viewsText.Replace(" views", "").Trim();

            if (numericText.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                numericText = numericText.Replace("M", "").Trim();
                double result;
                if (double.TryParse(numericText, out result))
                {
                    return $"{result:F1}M views";
                }
            }

            else if (numericText.EndsWith("K", StringComparison.OrdinalIgnoreCase))
            {
                numericText = numericText.Replace("K", "").Trim();
                double result;
                if (double.TryParse(numericText, out result))
                {
                    return $"{result:F1}K views";
                }
            }

            int viewsCount;
            if (int.TryParse(numericText, out viewsCount))
            {
                return $"{viewsCount} views";
            }

            return "0 views";
        }

        public static string FormatViewCountWithCommas(int viewCount)
        {
            return viewCount.ToString("#,0", CultureInfo.InvariantCulture);
        }

        public static string AddCommasManually(int number)
        {
            string str = number.ToString();
            int length = str.Length;

            if (length <= 3)
                return str;

            StringBuilder builder = new StringBuilder();

            int count = 0;
            for (int i = length - 1; i >= 0; i--)
            {
                builder.Insert(0, str[i]);
                count++;

                if (count % 3 == 0 && i > 0)
                {
                    builder.Insert(0, ',');
                }
            }

            return builder.ToString();
        }



    }
}