using System;
using System.Windows.Controls.Primitives;

namespace DuplicateFinder.Utilities
{
    class ScanningProgress : IProgress<int>
    {
        public int Divisor { get; set; }

        public int Addend { get; set; }

        public Action<double> Action { get; set; }

        public ScanningProgress(Action<double> action, int addend = 0, int divisor = 1)
        {
            Divisor = divisor;
            Addend = addend;
            Action = action;
        }

        /// <summary>
        /// Reports the sum of the value and the Addend divided by the Divisor
        /// as a percentage (divided by 100). Any value who's final division result 
        /// is greater than 1 will be reported as a value of 1 (100%).
        /// </summary>
        /// <param name="value">The int value of the progress value.</param>
        public void Report(int value)
        {
            if (Divisor == 0)
            {
                throw new DivideByZeroException();
            }

            double reportValue = (double)(value / Divisor) + Addend;

            if (reportValue > 100)
            {
                reportValue = 100;
            }

            Action(reportValue);
        }
    }
}
