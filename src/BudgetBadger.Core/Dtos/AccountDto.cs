﻿using System;

namespace BudgetBadger.Core.Dtos
{
	public record AccountDto
	{
        public Guid Id { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
        public bool OnBudget { get; init; }
        public bool Hidden { get; init; }
        public bool Deleted { get; init; }
        public DateTime ModifiedDateTime { get; init; }
    }
}
