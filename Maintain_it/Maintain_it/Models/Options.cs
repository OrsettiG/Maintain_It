using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Models
{
    public enum Timeframe { Minutes, Hours, Days, Weeks, Months, Years }
    
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
    }
}
