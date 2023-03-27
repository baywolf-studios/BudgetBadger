using System;
using BudgetBadger.Core.Localization;

namespace BudgetBadger.Core.Models
{
    public static class Constants
    {
        public static readonly Guid GenericHiddenPayeeId = new Guid("{f8d9fbfe-973e-452d-95eb-d4b876ebceda}");
        public static readonly Guid GenericHiddenAccountId = new Guid("{f8d9fbfe-973e-452d-95eb-d4b876ebceda}");
        public static readonly Guid GenericHiddenEnvelopeGroupId = new Guid("{f8d9fbfe-973e-452d-95eb-d4b876ebceda}");

        public static readonly Guid StartingBalancePayeeId = new Guid("{5c5d6f16-c8c0-4f1b-bdc1-f75494a63e8b}");

        public static readonly Guid DebtEnvelopeGroupId = new Guid("{6c31199e-a5f8-448a-8af6-fd5bab73bcd7}");
        public static readonly Guid IncomeEnvelopeGroupId = new Guid("{b797f2af-bef2-4685-bbd6-73417414e6ce}");
        public static readonly Guid SystemEnvelopeGroupId = new Guid("{7a21e815-e95c-4785-b0a3-d65013a1196b}");
        public static readonly Guid MonthlyBillsEnvelopGroupId = new Guid("{f3d90935-bb10-4cf7-ae4b-fa7ca041a6b1}");
        public static readonly Guid EverydayExpensesEnvelopeGroupId = new Guid("{ce3bc99c-610b-413c-a06a-5888ef596cf1}");
        public static readonly Guid SavingsGoalsEnvelopeGroupdId = new Guid("{0f3e250f-db63-4c5e-8090-f1dca882fb53}");

        public static readonly Guid IncomeEnvelopeId = new Guid("{1bc8c32d-d04d-4079-90b6-c060e3e56e16}");
        public static readonly Guid IgnoredEnvelopeId = new Guid("{cdf92e3b-3104-47b9-9a83-ca9e3e097c35}");

        public static readonly PayeeModel StartingBalancePayee = new PayeeModel
        {
            Id = StartingBalancePayeeId,
            Description = nameof(AppResources.StartingBalancePayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly EnvelopeGroupModel IncomeEnvelopeGroup = new EnvelopeGroupModel
        {
            Id = IncomeEnvelopeGroupId,
            Description = nameof(IncomeEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope IncomeEnvelope = new Envelope
        {
            Id = IncomeEnvelopeId,
            Description = nameof(IncomeEnvelope),
            Group = IncomeEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope BufferEnvelope = new Envelope
        {
            Id = new Guid("{d9e6e696-72c1-4d89-a1ab-4af24da2c72f}"),
            Description = nameof(BufferEnvelope),
            Group = IncomeEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly EnvelopeGroupModel SystemEnvelopeGroup = new EnvelopeGroupModel
        {
            Id = SystemEnvelopeGroupId,
            Description = nameof(SystemEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope IgnoredEnvelope = new Envelope
        {
            Id = IgnoredEnvelopeId,
            Description = nameof(IgnoredEnvelope),
            Group = SystemEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly EnvelopeGroupModel DebtEnvelopeGroup = new EnvelopeGroupModel
        {
            Id = DebtEnvelopeGroupId,
            Description = nameof(DebtEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
        public static readonly Envelope GenericDebtEnvelope = new Envelope
        {
            Id = new Guid("{094d0b91-6e66-42c0-b892-2205e9960fd2}"),
            Description = nameof(GenericDebtEnvelope),
            IgnoreOverspend = true,
            Group = DebtEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };
    }
}
