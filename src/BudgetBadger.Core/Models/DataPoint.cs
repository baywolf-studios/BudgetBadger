using System;
namespace BudgetBadger.Core.Models
{
    public class DataPoint<X, Y> : ObservableBase
    {
        string xLabel;
        public string XLabel
        {
            get => xLabel;
            set => SetProperty(ref xLabel, value);
        }

        X xValue;
        public X XValue 
        {
            get => xValue;
            set => SetProperty(ref xValue, value);
        }

        string yLabel;
        public string YLabel
        {
            get => yLabel;
            set => SetProperty(ref yLabel, value);
        }

        Y yValue;
        public Y YValue
        {
            get => yValue;
            set => SetProperty(ref yValue, value);
        }
    }
}
