using System;
using BudgetBadger.Models;

namespace BudgetBadger.UnitTests.TestModels;

public class TestBudgets
{
    public static readonly Budget NewBudget = new Budget();

    public static readonly Budget ActiveBudget = new Budget
    {
        Id = Guid.NewGuid(),
        CreatedDateTime = DateTime.Now,
        ModifiedDateTime = DateTime.Now,
        Schedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now),
        Envelope = TestEnvelopes.ActiveEnvelope
    };
}