using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.Helpers
{
    internal class ByStepNumber : IComparer<Step>
    {
        int xStepNum, yStepNum;
        public int Compare( Step x, Step y )
        {
            xStepNum = x.StepNumber;
            yStepNum = y.StepNumber;

            return xStepNum.CompareTo( yStepNum );
        }
    }
}
