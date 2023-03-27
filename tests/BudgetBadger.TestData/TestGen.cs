using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.Core.Utilities;
using BudgetBadger.DataAccess.Dtos;

namespace BudgetBadger.TestData;
public static class TestGen
{
    private static Random s_rnd = new();
    public static string RndString() => Guid.NewGuid().ToString("n")[..8];
    public static decimal RndDecimal() => new(s_rnd.NextDouble() * s_rnd.Next(-10000, 10000));
    public static bool RndBool() => s_rnd.Next() > (int.MaxValue / 2);
    public static DateTime RndDate() => new(s_rnd.Next(1, 9999), s_rnd.Next(1, 12), s_rnd.Next(1, 28));
    public static Guid RndGuid() => Guid.NewGuid();
    public const string EmptyString = "";
    public const string WhiteSpaceString = "      ";

    public static void SetRandomGeneratorSeed(int seed)
    {
        s_rnd = new(seed);
    }

    public static AccountDto AccountDto() => new AccountDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        OnBudget = RndBool(),
        ModifiedDateTime = RndDate()
    };

    public static AccountDto BudgetAccountDto() => new AccountDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        OnBudget = true,
        ModifiedDateTime = RndDate()
    };

    public static AccountDto ReportingAccountDto() => new AccountDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        OnBudget = false,
        ModifiedDateTime = RndDate()
    };

    public static AccountDto HiddenAccountDto() => new AccountDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        OnBudget = false,
        ModifiedDateTime = RndDate(),
        Hidden = true
    };

    public static AccountDto DeletedAccountDto() => new AccountDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        OnBudget = RndBool(),
        ModifiedDateTime = RndDate(),
        Hidden = true,
        Deleted = true
    };

    public static PayeeDto PayeeDto() => new PayeeDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        ModifiedDateTime = RndDate()
    };

    public static PayeeDto HiddenPayeeDto() => new PayeeDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        ModifiedDateTime = RndDate(),
        Hidden = true
    };

    public static PayeeDto DeletedPayeeDto() => new PayeeDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        ModifiedDateTime = RndDate(),
        Hidden = true,
        Deleted = true
    };

    public static PayeeDto StartingBalancePayeeDto => new PayeeDto
    {
        Id = Constants.StartingBalancePayeeId,
        Description = nameof(AppResources.StartingBalancePayee),
        ModifiedDateTime = RndDate()
    };

    public static EnvelopeGroupDto EnvelopeGroupDto() => new()
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        ModifiedDateTime = RndDate()
    };

    public static EnvelopeGroupDto SystemEnvelopeGroupDto => new()
    {
        Id = Constants.SystemEnvelopeGroupId,
        Description = nameof(AppResources.SystemEnvelopeGroup),
        ModifiedDateTime = RndDate()
    };

    public static EnvelopeGroupDto IncomeEnvelopeGroupDto => new()
    {
        Id = Constants.IncomeEnvelopeGroupId,
        Description = nameof(AppResources.IncomeEnvelopeGroup),
        ModifiedDateTime = RndDate()
    };

    public static EnvelopeGroupDto DebtEnvelopeGroupDto => new()
    {
        Id = Constants.DebtEnvelopeGroupId,
        Description = nameof(AppResources.DebtEnvelopeGroup),
        ModifiedDateTime = RndDate()
    };

    public static EnvelopeGroupDto HiddenEnvelopeGroupDto() => new EnvelopeGroupDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        ModifiedDateTime = RndDate(),
        Hidden = true
    };

    public static EnvelopeGroupDto DeletedEnvelopeGroupDto() => new EnvelopeGroupDto
    {
        Id = RndGuid(),
        Description = RndString(),
        Notes = RndString(),
        ModifiedDateTime = RndDate(),
        Hidden = true,
        Deleted = true
    };

    public static (EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) EnvelopeDto()
    {
        var envelopeGroupDto = EnvelopeGroupDto();
        return (
            new EnvelopeDto
            {
                Id = RndGuid(),
                Description = RndString(),
                EnvelopGroupId = envelopeGroupDto.Id,
                ModifiedDateTime = RndDate()
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
                Id = RndGuid(),
                Description = RndString(),
                EnvelopGroupId = envelopeGroupDto.Id,
                ModifiedDateTime = RndDate(),
                Hidden = true
            },
            envelopeGroupDto
        );
    }

    public static (EnvelopeDto softDeletedEnvelopeDto, EnvelopeGroupDto softDeletedEnvelopeGroupDto) SoftDeletedEnvelopeDto()
    {
        var envelopeGroupDto = DeletedEnvelopeGroupDto();
        return (
            new EnvelopeDto
            {
                Id = RndGuid(),
                Description = RndString(),
                EnvelopGroupId = envelopeGroupDto.Id,
                ModifiedDateTime = RndDate(),
                Hidden = true,
                Deleted = true
            },
            envelopeGroupDto
        );
    }

    public static BudgetPeriodDto BudgetPeriodDto()
    {
        var now = RndDate();
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
        var (envelopeDto, envelopeGroupDto) = EnvelopeDto();
        var budgetPeriod = BudgetPeriodDto();
        return (
            new BudgetDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                IgnoreOverspend = RndBool(),
                EnvelopeId = envelopeDto.Id,
                BudgetPeriodId = budgetPeriod.Id,
                ModifiedDateTime = RndDate()
            },
            budgetPeriod,
            envelopeDto,
            envelopeGroupDto
        );
    }

    public static (BudgetDto hiddenBudgetDto, BudgetPeriodDto budgetPeriodDto, EnvelopeDto hiddenEnvelopeDto, EnvelopeGroupDto hiddenEnvelopeGroupDto) HiddenBudgetDto()
    {
        var (hiddenEnvelopeDto, hiddenEnvelopeGroupDto) = HiddenEnvelopeDto();
        var budgetPeriod = BudgetPeriodDto();
        return (
            new BudgetDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                IgnoreOverspend = RndBool(),
                EnvelopeId = hiddenEnvelopeDto.Id,
                BudgetPeriodId = budgetPeriod.Id,
                ModifiedDateTime = RndDate()
            },
            budgetPeriod,
            hiddenEnvelopeDto,
            hiddenEnvelopeGroupDto
        );
    }

    public static (BudgetDto softDeletedBudgetDto, BudgetPeriodDto budgetPeriodDto, EnvelopeDto softDeletedEnvelopeDto, EnvelopeGroupDto softDeletedEnvelopeGroupDto) SoftDeletedBudgetDto()
    {
        var (softDeletedEnvelopeDto, softDeletedEnvelopeGroupDto) = SoftDeletedEnvelopeDto();
        var budgetPeriod = BudgetPeriodDto();
        return (
            new BudgetDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                IgnoreOverspend = RndBool(),
                EnvelopeId = softDeletedEnvelopeDto.Id,
                BudgetPeriodId = budgetPeriod.Id,
                ModifiedDateTime = RndDate()
            },
            budgetPeriod,
            softDeletedEnvelopeDto,
            softDeletedEnvelopeGroupDto
        );
    }

    public static (TransactionDto transactionDto, PayeeDto payeeDto, AccountDto accountDto, EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) TransactionDto()
    {
        var (envelopeDto, envelopeGroupDto) = EnvelopeDto();
        var payee = PayeeDto();
        var account = AccountDto();

        return (
            new TransactionDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = envelopeDto.Id,
                Posted = RndBool(),
                Reconciled = RndBool(),
                ServiceDate = RndDate(),
                ModifiedDateTime = RndDate()
            },
            payee,
            account,
            envelopeDto,
            envelopeGroupDto
        );
    }

    public static (TransactionDto transactionDto, PayeeDto payeeDto, AccountDto accountDto, EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) PendingTransactionDto()
    {
        var (envelopeDto, envelopeGroupDto) = EnvelopeDto();
        var payee = PayeeDto();
        var account = AccountDto();

        return (
            new TransactionDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = envelopeDto.Id,
                Posted = false,
                Reconciled = false,
                ServiceDate = RndDate(),
                ModifiedDateTime = RndDate()
            },
            payee,
            account,
            envelopeDto,
            envelopeGroupDto
        );
    }

    public static (TransactionDto transactionDto, PayeeDto payeeDto, AccountDto accountDto, EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) PostedTransactionDto()
    {
        var (envelopeDto, envelopeGroupDto) = EnvelopeDto();
        var payee = PayeeDto();
        var account = AccountDto();

        return (
            new TransactionDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = envelopeDto.Id,
                Posted = true,
                Reconciled = false,
                ServiceDate = RndDate(),
                ModifiedDateTime = RndDate()
            },
            payee,
            account,
            envelopeDto,
            envelopeGroupDto
        );
    }

    public static (TransactionDto transactionDto, PayeeDto payeeDto, AccountDto accountDto, EnvelopeDto envelopeDto, EnvelopeGroupDto envelopeGroupDto) ReconciledTransactionDto()
    {
        var (envelopeDto, envelopeGroupDto) = EnvelopeDto();
        var payee = PayeeDto();
        var account = AccountDto();

        return (
            new TransactionDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = envelopeDto.Id,
                Posted = true,
                Reconciled = true,
                ServiceDate = RndDate(),
                ModifiedDateTime = RndDate()
            },
            payee,
            account,
            envelopeDto,
            envelopeGroupDto
        );
    }

    public static (TransactionDto softDeletedTransactionDto, PayeeDto softDeletedPayeeDto, AccountDto softDeletedAccountDto, EnvelopeDto softDeletedEnvelopeDto, EnvelopeGroupDto softDeletedEnvelopeGroupDto) SoftDeletedTransactionDto()
    {
        var (softDeletedEnvelopeDto, softDeletedEnvelopeGroupDto) = SoftDeletedEnvelopeDto();
        var payee = DeletedPayeeDto();
        var account = DeletedAccountDto();

        return (
            new TransactionDto
            {
                Id = RndGuid(),
                Amount = RndDecimal(),
                PayeeId = payee.Id,
                AccountId = account.Id,
                EnvelopeId = softDeletedEnvelopeDto.Id,
                Posted = RndBool(),
                Reconciled = RndBool(),
                ServiceDate = RndDate(),
                ModifiedDateTime = RndDate(),
                Deleted = true
            },
            payee,
            account,
            softDeletedEnvelopeDto,
            softDeletedEnvelopeGroupDto
        );
    }
}

