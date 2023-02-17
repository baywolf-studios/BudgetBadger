using System;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Localization;

namespace BudgetBadger.Core.Models
{
	public record PayeeModel
	{
        public Guid Id { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
        public virtual string Group => !string.IsNullOrEmpty(Description) ? Description[0].ToString() : string.Empty;
    }

    public record AccountPayeeModel : PayeeModel
    {
        public override string Group => AppResources.PayeeTransferGroup;
    }

    public record StartingBalancePayeeModel : PayeeModel
    {
        public override string Group => AppResources.StartingBalancePayee;
        public StartingBalancePayeeModel()
        {
            Id = Constants.StartingBalancePayeeId;
            Description = AppResources.StartingBalancePayee;
            Notes = string.Empty;
        }
    }
}

