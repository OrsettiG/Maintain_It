using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Models
{
    public enum Timeframe { Minutes, Hours, Days, Weeks, Months, Years }
    public enum TagType { General, Step, Retailer, ShoppingList }

    public static class Options
    {
        public static List<Timeframe> timeframes = new List<Timeframe>()
        {
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
