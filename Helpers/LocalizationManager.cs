using System;
using System.Globalization;
using System.Threading;

namespace Lottery.Helpers
{
    public static class LocalizationManager
    {
        public static CultureInfo CurrentCulture
            => Thread.CurrentThread.CurrentUICulture;

        public static void ChangeCulture(string cultureName)
        {
            CultureInfo culture = new CultureInfo(cultureName);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}