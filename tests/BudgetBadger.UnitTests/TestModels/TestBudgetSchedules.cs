﻿using System;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestBudgetSchedules
    {
        public static BudgetSchedule GetBudgetScheduleFromDate(DateTime date)
        {
            var selectedSchedule = new BudgetSchedule();
            selectedSchedule.BeginDate = new DateTime(date.Year, date.Month, 1);
            selectedSchedule.EndDate = selectedSchedule.BeginDate.AddMonths(1).AddTicks(-1);
            selectedSchedule.Id = selectedSchedule.BeginDate.ToGuid();

            return selectedSchedule;
        }
    }
}
