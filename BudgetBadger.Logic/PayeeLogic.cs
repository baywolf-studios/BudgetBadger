using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Logic
{
    public class PayeeLogic : IPayeeLogic
    {
        readonly IPayeeDataAccess _payeeDataAccess;
        readonly IAccountDataAccess _accountDataAccess;
        readonly ITransactionDataAccess _transactionDataAccess;

        public PayeeLogic(IPayeeDataAccess payeeDataAccess, IAccountDataAccess accountDataAccess, 
                          ITransactionDataAccess transactionDataAccess)
        {
            _payeeDataAccess = payeeDataAccess;
            _accountDataAccess = accountDataAccess;
            _transactionDataAccess = transactionDataAccess;
        }

        async Task<Result> ValidateDeletePayeeAsync(Payee payee)
        {
            if (!payee.IsActive)
            {
                return new Result { Success = false, Message = "Cannot delete an inactive payee" }; 
            }

            if (payee.IsAccount)
            {
                return new Result { Success = false, Message = "Cannot delete an account payee" };
            }

            if (payee.Id == Constants.StartingBalancePayee.Id)
            {
                return new Result { Success = false, Message = "Cannot delete the starting balance payee" }; 
            }

            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(payee.Id);
            if (payeeTransactions.Any(t => t.IsActive && t.ServiceDate >= DateTime.Now))
            {
                return new Result { Success = false, Message = "Cannot delete a payee with future transactions on it" };
            }

            return new Result { Success = true };
        }

        public async Task<Result> DeletePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var payee = await _payeeDataAccess.ReadPayeeAsync(id);
                var populatePayee = await GetPopulatedPayee(payee);

                var validationResult = await ValidateDeletePayeeAsync(populatePayee);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                payee.DeletedDateTime = DateTime.Now;

                await _payeeDataAccess.UpdatePayeeAsync(payee);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Payee>> GetPayeeAsync(Guid id)
        {
            var result = new Result<Payee>();

            try
            {
                var payee = await _payeeDataAccess.ReadPayeeAsync(id);
                var populatedPayee = await GetPopulatedPayee(payee);
                result.Success = true;
                result.Data = populatedPayee;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _payeeDataAccess.ReadPayeesAsync();

                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance()
                                             && p.IsActive);

                var tasks = payees.Select(p => GetPopulatedPayee(p));

                result.Success = true;
                result.Data = await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _payeeDataAccess.ReadPayeesAsync();

                // ugly hardcoded to remove the starting balance payee.
                // may move to a "Payee Group" type setup
                var payees = allPayees.Where(p => p.IsActive && !p.IsStartingBalance())
                                      .ToList();

                var includeDeleted = false; //will get this from settings dataaccess
                if (includeDeleted)
                {
                    payees.AddRange(allPayees.Where(p => p.IsDeleted));
                }

                var tasks = payees.Select(p => GetPopulatedPayee(p));

                var payeesTemp = await Task.WhenAll(tasks);

                result.Success = true;
                result.Data = payeesTemp.Where(p => !p.IsAccount).ToList();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public IReadOnlyList<Payee> SearchPayees(IEnumerable<Payee> payees, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return payees.ToList();
            }

            return payees.Where(a => a.Description.ToLower().Contains(searchText.ToLower())).ToList();
        }

        public IReadOnlyList<IGrouping<string, Payee>> GroupPayees(IEnumerable<Payee> payees)
        {
            var groupedPayees = payees.GroupBy(p =>
            {
                if (p.IsAccount)
                {
                    return "Transfer";
                }
                else if (p.DeletedDateTime.HasValue)
                {
                    return "Deleted";
                }
                else
                {
                    return p.Description[0].ToString();
                }
            });

            return groupedPayees.ToList();
        }

        public Task<Result> ValidatePayeeAsync(Payee payee)
        {
            if (!payee.IsValid())
            {
                return Task.FromResult(payee.Validate());
            }

            return Task.FromResult(new Result { Success = true });
        }

        public async Task<Result<Payee>> SavePayeeAsync(Payee payee)
        {
            var validationResult = await ValidatePayeeAsync(payee);
            if (!validationResult.Success)
            {
                return validationResult.ToResult<Payee>();
            }

            var result = new Result<Payee>();
            var payeeToUpsert = payee.DeepCopy();
            var dateTimeNow = DateTime.Now;

            if (payeeToUpsert.IsNew)
            {
                payeeToUpsert.Id = Guid.NewGuid();
                payeeToUpsert.CreatedDateTime = dateTimeNow;
                payeeToUpsert.ModifiedDateTime = dateTimeNow;
                try
                {
                    await _payeeDataAccess.CreatePayeeAsync(payeeToUpsert);
                    result.Success = true;
                    result.Data = payeeToUpsert;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }
            else
            {
                payeeToUpsert.ModifiedDateTime = dateTimeNow;
                try
                {
                    await _payeeDataAccess.UpdatePayeeAsync(payeeToUpsert);
                    result.Success = true;
                    result.Data = payeeToUpsert;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }

            }

            return result;
        }

        private async Task<Payee> GetPopulatedPayee(Payee payee)
        {
            var payeeToPopulate = payee.DeepCopy();

            var payeeAccount = await _accountDataAccess.ReadAccountAsync(payee.Id);

            payeeToPopulate.IsAccount = payeeAccount.IsActive;

            return payeeToPopulate;
        }
    }
}
