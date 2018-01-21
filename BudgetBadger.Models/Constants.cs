using System;
namespace BudgetBadger.Models
{
    public static class Constants
    {
        public static readonly EnvelopeGroup IncomeEnvelopeGroup = new EnvelopeGroup
        { 
            Id = new Guid("{b797f2af-bef2-4685-bbd6-73417414e6ce}"), 
            Description = "Income",
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope IncomeEnvelope = new Envelope
        { 
            Id = new Guid("{1bc8c32d-d04d-4079-90b6-c060e3e56e16}"), 
            Description = "Income", 
            Group = IncomeEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope BufferEnvelope = new Envelope 
        { 
            Id = new Guid("{d9e6e696-72c1-4d89-a1ab-4af24da2c72f}"), 
            Description = "Buffer", 
            Group = IncomeEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now 
        };
        public static readonly EnvelopeGroup SystemEnvelopeGroup = new EnvelopeGroup
        {
            Id = new Guid("{7a21e815-e95c-4785-b0a3-d65013a1196b}"),
            Description = "System",
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope IgnoredEnvelope = new Envelope 
        { 
            Id = new Guid("{cdf92e3b-3104-47b9-9a83-ca9e3e097c35}"), 
            Description = "Not Needed", 
            Group = SystemEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now 
        };
        public static readonly EnvelopeGroup DebtEnvelopeGroup = new EnvelopeGroup
        {
            Id = new Guid("{6c31199e-a5f8-448a-8af6-fd5bab73bcd7}"),
            Description = "Debt",
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope GenericDebtEnvelope = new Envelope
        {
            Id = new Guid("{094d0b91-6e66-42c0-b892-2205e9960fd2}"),
            Description = "Debt",
            IgnoreOverspend = true,
            Group = DebtEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Payee StartingBalancePayee = new Payee 
        { 
            Id = new Guid("{5c5d6f16-c8c0-4f1b-bdc1-f75494a63e8b}"), 
            Description = "Starting Balance",
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now 
        };
    }
}
