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
        public enum JobServiceIds
        {
            Notification = 100, DatabaseManagement = 200, ServiceItemManagement = 201
        }

        /// <summary>
        /// Common millisecond time intervals. This Enum is a regular <see cref="uint"/> and does not need to be cast.
        /// </summary>
        public enum MilliTimeIntervals
        {
            Minute = 6000, Hour = 360000, Day = 8640000
        }

        /// <summary>
        /// Enum of long values that represents common tick amounts. You <i>must</i> cast this to a <see cref="long"/> before using it.
        /// </summary>
        public enum TickTimeIntervals : long
        {
            Minute = 60000000, Hour = 3600000000, Day = 86400000000
        }

        /// <summary>
        /// Action options for Notifications. The Xamarin.Android code for notifications converts the text value of the enum into the action button text, so word these in the way the user should see them. Format should be ALL_UPPERCASE_WITH_UNDERSCORE_FOR_SPACES.
        /// </summary>
        public enum NotificationActions
        {
            REMIND_ME_LATER, DO_NOT_REMIND_ME
        }

        /// <summary>
        /// User selectable options for the time of day they want notifications to be scheduled for.
        /// </summary>
        public enum NotificationReminderWindow
        {
            Morning, Afternoon, Evening
        }

        /// <summary>
        /// Edit state flag
        /// </summary>
        public enum EditState
        {
            Locked, Editing
        }

        #region Constants
        #region Notifications
        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string NotificationIdKey = "notificationId";
        public const string MessageIdKey = "messageId";
        public const int NotificationScanFrequencyWindowHours = 2;
        public const int NotificationScanFrequencyFlexWindowHours = 1;
        public const int NotificationScanFrequencyWindowMilliseconds = (int)MilliTimeIntervals.Hour * NotificationScanFrequencyWindowHours;
        public const int NotificationScanFrequencyFlexWindowMilliseconds = (int)MilliTimeIntervals.Hour * NotificationScanFrequencyFlexWindowHours;
        #endregion
        #region Font Icons
        public const string FontAwesomeSolid = "FA-Solid";
        #endregion
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

        private static NotificationReminderWindow notificationWindow = NotificationReminderWindow.Morning;
        public static NotificationReminderWindow NotificationWindow
        {
            get => notificationWindow;
            set => notificationWindow = value;
        }

        public static TimeSpan DefaultReminderTime
        {
            get
            {
                return NotificationWindow switch
                {
                    NotificationReminderWindow.Afternoon => new TimeSpan( 12, 0, 0 ),
                    NotificationReminderWindow.Evening => new TimeSpan( 17, 0, 0 ),
                    _ => new TimeSpan( 7, 0, 0 ),
                };
            }
        }

        public static DateTime DefaultServiceDateTime
        {
            get
            {
                return DateTime.Today.AddDays( 1 ).AddHours( DefaultReminderTime.Hours ).AddMinutes( DefaultReminderTime.Minutes );
            }
        }

        public enum AppResourceKeys
        {
            DarkPrimary,
            DarkSecondary,
            LightPrimary,
            LightSecondary,
            Accent1,
            Accent2,
            Test
        }

        #endregion
    }
}
