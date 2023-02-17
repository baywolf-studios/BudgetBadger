﻿using System;

namespace BudgetBadger.Core.Dtos
{
	public record BudgetPeriodDto
    {
        public Guid Id { get; init; }
        public DateTime BeginDate { get; init; }
        public DateTime EndDate { get; init; }
        public DateTime ModifiedDateTime { get; init; }
    }
}
