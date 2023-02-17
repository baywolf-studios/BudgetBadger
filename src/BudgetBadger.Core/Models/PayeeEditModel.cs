using System;
using BudgetBadger.Core.Dtos;

namespace BudgetBadger.Core.Models
{
	public record PayeeEditModel
    {
        public Guid Id { get; init; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public bool Hidden { get; init; }
        public string Group => !string.IsNullOrEmpty(Description) ? Description[0].ToString() : string.Empty;
    }
}

