using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public class Config
    {
        public enum JobServiceIds { Notification = 100 }
        public enum MilliTimeIntervals { Minute = 6000, Hour = 360000, Day = 8640000 }

        public const string REMIND_LATER = "REMIND_LATER";
        public const string DO_NOT_REMIND = "DO_NOT_REMIND";
        
    }
}
