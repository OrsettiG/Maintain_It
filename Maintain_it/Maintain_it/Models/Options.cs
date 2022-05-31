using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Models
{
    public enum Timeframe { None, Minutes, Hours, Days, Weeks, Months, Years }
    public enum TagType { General, Step, Retailer, ShoppingList }
    public enum TimeInMinutes { None = 0, Minutes = 1, Hours = 60, Days = 1440, Weeks = 10080, Months = 43800, Years = 525600 }

    public static class Options
    {
        public static List<Timeframe> timeframes = new List<Timeframe>()
        {
            Timeframe.None,
            Timeframe.Minutes,
            Timeframe.Hours,
            Timeframe.Days,
            Timeframe.Weeks,
            Timeframe.Months,
            Timeframe.Years
        };

        public static List<TagType> tagTypes = new List<TagType>()
        {
            TagType.General,
            TagType.Step,
            TagType.Retailer,
            TagType.ShoppingList
        };
    }
}
