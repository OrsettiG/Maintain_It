using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public class Config
    {
        public enum JobServiceIds { Notification = 100 }
        public enum MilliTimeIntervals { Minute = 6000, Hour = 360000, Day = 8640000 }
        public enum NotificationActions { REMIND_ME_LATER, DO_NOT_REMIND_ME }

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string IdKey = "id";

        private static int maxReminders = 3;
        public static int MaxReminders { get => maxReminders; set => maxReminders = value; }
    }
}
