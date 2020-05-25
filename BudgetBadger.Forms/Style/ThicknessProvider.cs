using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Style
{
    public class ThicknessProvider : INotifyPropertyChanged
    {
        public Thickness this[string key]
        {
            get
            {
                var splitKey = key.Split(',').ToList();

                if (Application.Current.Resources.TryGetValue(splitKey.ElementAtOrDefault(0), out object leftSize)
                    && Application.Current.Resources.TryGetValue(splitKey.ElementAtOrDefault(1), out object topSize)
                    && Application.Current.Resources.TryGetValue(splitKey.ElementAtOrDefault(2), out object rightSize)
                    && Application.Current.Resources.TryGetValue(splitKey.ElementAtOrDefault(3), out object bottomSize))
                {
                    double leftSizeDouble;
                    if (leftSize is OnIdiom<double> leftSizeIdiom)
                    {
                        leftSizeDouble = GetValue(leftSizeIdiom);
                    }
                    else
                    {
                        if (!double.TryParse(leftSize.ToString(), out leftSizeDouble))
                        {
                            throw new FormatException();
                        }
                    }

                        

                    double topSizeDouble;
                    if (topSize is OnIdiom<double> topSizeIdiom)
                    {
                        topSizeDouble = GetValue(topSizeIdiom);
                    }
                    else
                    {
                        if (!double.TryParse(topSize.ToString(), out topSizeDouble))
                        {
                            throw new FormatException();
                        }
                    }

                    double rightSizeDouble;
                    if (rightSize is OnIdiom<double> rightSizeIdiom)
                    {
                        rightSizeDouble = GetValue(rightSizeIdiom);
                    }
                    else
                    {
                        if (!double.TryParse(rightSize.ToString(), out rightSizeDouble))
                        {
                            throw new FormatException();
                        }
                    }

                    double bottomSizeDouble;
                    if (bottomSize is OnIdiom<double> bottomSizeIdiom)
                    {
                        bottomSizeDouble = GetValue(bottomSizeIdiom);
                    }
                    else
                    {
                        if (!double.TryParse(bottomSize.ToString(), out bottomSizeDouble))
                        {
                            throw new FormatException();
                        }
                    }

                    return new Thickness(leftSizeDouble, topSizeDouble, rightSizeDouble, bottomSizeDouble);
                }
                throw new FormatException();
            }
        }

        public static ThicknessProvider Instance { get; } = new ThicknessProvider();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        double GetValue(OnIdiom<double> idiom)
        {
            return idiom;
        }
    }
}
