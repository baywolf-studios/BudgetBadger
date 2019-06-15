using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public static class StarSizerRatioHelpers
    {
        public static double GetColumnRatio(BindableObject obj)
        {
            return (double)obj.GetValue(ColumnRatioProperty);
        }
        public static void SetColumnRatio(BindableObject obj, double value)
        {
            obj.SetValue(ColumnRatioProperty, value);
        }

        public static readonly BindableProperty ColumnRatioProperty =
            BindableProperty.Create("ColumnRatio", typeof(double), typeof(StarSizerRatioHelpers), 1d, BindingMode.TwoWay);
        public static void OnColumnSizerChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }
    }

    public class DataGridRatioColumnSizer : GridColumnSizer
	{
		public DataGridRatioColumnSizer(SfDataGrid grid) : base(grid)
		{
		}
		protected override void SetStarWidthForColumns(double columnsWidth, IEnumerable<GridColumn> columns)
		{
			var removedColumn = new List<GridColumn>();
			var column = columns.ToList();
			var totalRemainingStarValue = columnsWidth;
			double removedWidth = 0;
			bool isRemoved;
			while (column.Count > 0)
			{
				isRemoved = false;
				removedWidth = 0;
				double columnsCount = 0;
				foreach (var data in column)
				{
					columnsCount += StarSizerRatioHelpers.GetColumnRatio(data);
				}
				double starWidth = Math.Floor((totalRemainingStarValue / columnsCount));
				var getColumn = column.First();

				//Calculate the ColumnSizer ratio for every column 
				starWidth *= StarSizerRatioHelpers.GetColumnRatio(getColumn);
				var columnSizer = DataGrid.GridColumnSizer;
				var method = columnSizer.GetType().GetRuntimeMethods().FirstOrDefault(x => x.Name == "SetColumnWidth");
				var width = method.Invoke(columnSizer, new object[] { getColumn, starWidth });
				double computeWidth = (double)width;

				if (starWidth != computeWidth && starWidth > 0)
				{
					isRemoved = true;
					column.Remove(getColumn);
					foreach (var remColumn in removedColumn)
					{
						if (!column.Contains(remColumn))
						{
							removedWidth += remColumn.ActualWidth;
							column.Add(remColumn);
						}
					}
					removedColumn.Clear();
					totalRemainingStarValue += removedWidth;
				}
				totalRemainingStarValue -= computeWidth;
				if (!isRemoved)
				{
					column.Remove(getColumn);
					if (!removedColumn.Contains(getColumn))
						removedColumn.Add(getColumn);
				}
			}
		}
	}
}
