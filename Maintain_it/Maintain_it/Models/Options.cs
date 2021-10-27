using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Models
{
    public enum Timeframe { MINUTES, HOURS, DAYS, WEEKS, MONTHS, YEARS }
    
    public static class Options
    {
        public static List<Timeframe> timeframes = new List<Timeframe>()
        {
            Timeframe.MINUTES,
            Timeframe.HOURS,
            Timeframe.DAYS,
            Timeframe.WEEKS,
            Timeframe.MONTHS,
            Timeframe.YEARS
        };
    }
}
