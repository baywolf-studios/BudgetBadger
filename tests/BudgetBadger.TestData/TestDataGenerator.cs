using System;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.Core.Utilities;

namespace BudgetBadger.TestData;
public static class TestDataGenerator
{
    private static Random rnd = new Random();
    public static string RandomName() => Guid.NewGuid().ToString("n").Substring(0, 8);
    public static decimal RandomDecimal() => new decimal(rnd.NextDouble());
    public static bool RandomBoolean() => rnd.Next() > (Int32.MaxValue / 2);

    public static AccountDto AccountDto() => new AccountDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now
    };

    public static AccountDto HiddenAccountDto() => new AccountDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now,
        Hidden = true
    };

    public static AccountDto SoftDeletedAccountDto() => new AccountDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now,
        Hidden = true,
        Deleted = true
    };

    public static PayeeDto PayeeDto() => new PayeeDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now
    };

    public static PayeeDto HiddenPayeeDto() => new PayeeDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now,
        Hidden = true
    };

    public static PayeeDto SoftDeletedPayeeDto() => new PayeeDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now,
        Hidden = true,
        Deleted = true
    };

    public static PayeeDto StartingBalancePayeeDto() => new PayeeDto
    {
        Id = Constants.StartingBalancePayeeId,
        Description = nameof(AppResources.StartingBalancePayee),
        ModifiedDateTime = DateTime.Now
    };

    public static EnvelopeGroupDto EnvelopeGroupDto() => new EnvelopeGroupDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now
    };

    public static EnvelopeGroupDto HiddenEnvelopeGroupDto() => new EnvelopeGroupDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now,
        Hidden = true
    };

    public static EnvelopeGroupDto SoftDeletedEnvelopeGroupDto() => new EnvelopeGroupDto
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        ModifiedDateTime = DateTime.Now,
        Hidden = true,
        Deleted = true
    };

    public static (EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) EnvelopeDto()
    {
        var envelopeGroupDto = EnvelopeGroupDto();
        return (
            new EnvelopeDto
            {
                Id = Guid.NewGuid(),
                Description = RandomName(),
                EnvelopGroupId = envelopeGroupDto.Id,
                ModifiedDateTime = DateTime.Now
            },
            envelopeGroupDto
        );
    }

    public static (EnvelopeDto hiddenEnvelopeDto, EnvelopeGroupDto hiddenEnvelopeGroupDto) HiddenEnvelopeDto()
    {
        var envelopeGroupDto = HiddenEnvelopeGroupDto();
        return (
            new EnvelopeDto
            {
                Id = Guid.NewGuid(),
                Description = RandomName(),
                EnvelopGroupId = envelopeGroupDto.Id,
                ModifiedDateTime = DateTime.Now,
                Hidden = true
            },
            envelopeGroupDto
        );
    }

    public static (EnvelopeDto softDeletedEnvelopeDto, EnvelopeGroupDto softDeletedEnvelopeGroupDto) SoftDeletedEnvelopeDto()
    {
        var envelopeGroupDto = SoftDeletedEnvelopeGroupDto();
        return (
            new EnvelopeDto
            {
                Id = Guid.NewGuid(),
                Description = RandomName(),
                EnvelopGroupId = envelopeGroupDto.Id,
                ModifiedDateTime = DateTime.Now,
                Hidden = true,
                Deleted = true
            },
            envelopeGroupDto
        );
    }

    public static BudgetPeriodDto BudgetPeriodDto()
    {
        var now = DateTime.Now;
        return new BudgetPeriodDto
        {
            Id = now.ToGuid(),
            BeginDate = now,
            EndDate = now.AddDays(30),
            ModifiedDateTime = now
        };
    }

    public static (BudgetDto budgetDto, BudgetPeriodDto budgetPeriodDto, EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) BudgetDto()
    {
        var envelope = EnvelopeDto();
        var budgetPeriod = BudgetPeriodDto();
        return (
            new BudgetDto
            {
                Id = Guid.NewGuid(),
                Amount = RandomDecimal(),
                IgnoreOverspend = RandomBoolean(),
                EnvelopeId = envelope.envelopeDto.Id,
                BudgetPeriodId = budgetPeriod.Id,
                ModifiedDateTime = DateTime.Now
            },
            budgetPeriod,
            envelope.envelopeDto,
            envelope.envelopeGroupDto
        );
    }

    public static (BudgetDto hiddenBudgetDto, BudgetPeriodDto budgetPeriodDto, EnvelopeDto hiddenEnvelopeDto, EnvelopeGroupDto hiddenEnvelopeGroupDto) HiddenBudgetDto()
    {
        var envelope = HiddenEnvelopeDto();
        var budgetPeriod = BudgetPeriodDto();
        return (
            new BudgetDto
            {
                Id = Guid.NewGuid(),
                Amount = RandomDecimal(),
                IgnoreOverspend = RandomBoolean(),
                EnvelopeId = envelope.hiddenEnvelopeDto.Id,
                BudgetPeriodId = budgetPeriod.Id,
                ModifiedDateTime = DateTime.Now
            },
            budgetPeriod,
            envelope.hiddenEnvelopeDto,
            envelope.hiddenEnvelopeGroupDto
        );
    }

    public static (BudgetDto softDeletedBudgetDto, BudgetPeriodDto budgetPeriodDto, EnvelopeDto softDeletedEnvelopeDto, EnvelopeGroupDto softDeletedEnvelopeGroupDto) SoftDeletedBudgetDto()
    {
        var envelope = SoftDeletedEnvelopeDto();
        var budgetPeriod = BudgetPeriodDto();
        return (
            new BudgetDto
            {
                Id = Guid.NewGuid(),
                Amount = RandomDecimal(),
                IgnoreOverspend = RandomBoolean(),
                EnvelopeId = envelope.softDeletedEnvelopeDto.Id,
                BudgetPeriodId = budgetPeriod.Id,
                ModifiedDateTime = DateTime.Now
            },
            budgetPeriod,
            envelope.softDeletedEnvelopeDto,
            envelope.softDeletedEnvelopeGroupDto
        );
    }

    public static (TransactionDto transactionDto, PayeeDto payeeDto, AccountDto accountDto, EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) TransactionDto()
    {
        var envelope = EnvelopeDto();
        var payee = PayeeDto();
        var account = AccountDto();

        return (
            new TransactionDto
            {
                Id = Guid.NewGuid(),
                Amount = RandomDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = envelope.envelopeDto.Id,
                Posted = RandomBoolean(),
                Reconciled = RandomBoolean(),
                ServiceDate = DateTime.Now,
                ModifiedDateTime = DateTime.Now
            },
            payee,
            account,
            envelope.envelopeDto,
            envelope.envelopeGroupDto
        );
    }

    public static (TransactionDto softDeletedTransactionDto, PayeeDto softDeletedPayeeDto, AccountDto softDeletedAccountDto, EnvelopeDto softDeletedEnvelopeDto, EnvelopeGroupDto softDeletedEnvelopeGroupDto) SoftDeletedTransactionDto()
    {
        var envelope = SoftDeletedEnvelopeDto();
        var payee = SoftDeletedPayeeDto();
        var account = SoftDeletedAccountDto();

        return (
            new TransactionDto
            {
                Id = Guid.NewGuid(),
                Amount = RandomDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = envelope.softDeletedEnvelopeDto.Id,
                Posted = RandomBoolean(),
                Reconciled = RandomBoolean(),
                ServiceDate = DateTime.Now,
                ModifiedDateTime = DateTime.Now
            },
            payee,
            account,
            envelope.softDeletedEnvelopeDto,
            envelope.softDeletedEnvelopeGroupDto
        );
    }

    public static PayeeEditModel NewPayeeEditModel() => new PayeeEditModel
    {
        Description = RandomName()
    };

    public static PayeeEditModel ActivePayeeEditModel() => new PayeeEditModel
    {
        Id = Guid.NewGuid(),
        Description = RandomName()
    };

    public static PayeeEditModel HiddenPayeeEditModel() => new PayeeEditModel
    {
        Id = Guid.NewGuid(),
        Description = RandomName(),
        Hidden = true
    };
}

