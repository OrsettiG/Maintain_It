using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.Helpers
{
    public class Config
    {
        /// <summary>
        /// Xamarin.Android JobService Ids.
        /// </summary>
        public enum JobServiceIds { Notification = 100 }

        /// <summary>
        /// Common millisecond time intervals. This Enum is a regular <see cref="uint"/> and does not need to be cast.
        /// </summary>
        public enum MilliTimeIntervals { Minute = 6000, Hour = 360000, Day = 8640000 }

        /// <summary>
        /// Enum of long values that represents common tick amounts. You <i>must</i> cast this to a <see cref="long"/> before using it.
        /// </summary>
        public enum TickTimeIntervals : long { Minute = 60000000, Hour = 3600000000, Day = 86400000000 }

        /// <summary>
        /// Action options for Notifications. The Xamarin.Android code for notifications converts the text value of the enum into the action button text, so word these in the way the user should see them. Format should be ALL_UPPERCASE_WITH_UNDERSCORE_FOR_SPACES.
        /// </summary>
        public enum NotificationActions { REMIND_ME_LATER, DO_NOT_REMIND_ME }

        #region Constants
        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string NotificationIdKey = "notificationId";
        public const string MessageIdKey = "messageId";
        #endregion

        #region User Definable Settings
        private static int defaultReminders = 3;
        public static int DefaultReminders 
        { 
            get => defaultReminders; 
            set => defaultReminders = value; 
        }

        private static int defaultAdvanceNotice = 3;
        public static int DefaultAdvanceNotice 
        { 
            get => defaultAdvanceNotice; 
            set => defaultAdvanceNotice = value; 
        }

        private static Timeframe defaultNoticeTimeframe = Timeframe.Days;
        public static Timeframe DefaultNoticeTimeframe
        {
            get => defaultNoticeTimeframe;
            set => defaultNoticeTimeframe = value;
        }

        #endregion
    }
}
