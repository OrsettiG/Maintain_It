using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maintain_it.ViewModels
{
    class CalendarViewModel
    {
        #region CONSTRUCTORS
        public CalendarViewModel()
        {
            _currentMonth = GetDatesThisMonth();
        }
        #endregion

        #region PROPERTIES

        #region Private
        private DateTime _now = DateTime.Now;
        private readonly List<DateTime> _currentMonth;
        #endregion

        #region Public
        public List<DateTime> CurrentMonth => _currentMonth;
        #endregion

        #endregion

        #region METHODS

        #region Private
        private List<DateTime> GetDatesThisMonth()
        {
            return Enumerable.Range( 1, DateTime.DaysInMonth( _now.Year, _now.Month ) ).Select( day => new DateTime( _now.Year, _now.Month, _now.Day ) ).ToList();
        }
        #endregion

        #endregion
    }
}
