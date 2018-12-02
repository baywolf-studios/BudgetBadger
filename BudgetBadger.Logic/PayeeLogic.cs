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

        async Task<Result> ValidateDeletePayeeAsync(Guid payeeId)
        {
            var errors = new List<string>();

            var payee = await _payeeDataAccess.ReadPayeeAsync(payeeId).ConfigureAwait(false);
            var populatePayee = await GetPopulatedPayee(payee).ConfigureAwait(false);

            if (!populatePayee.IsActive)
            {
                errors.Add("Cannot delete an inactive payee"); 
            }

            if (populatePayee.IsAccount)
            {
                errors.Add("Cannot delete an account payee");
            }

            if (populatePayee.Id == Constants.StartingBalancePayee.Id)
            {
                errors.Add("Cannot delete the starting balance payee"); 
            }

            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(populatePayee.Id).ConfigureAwait(false);
            if (payeeTransactions.Any(t => t.IsActive && t.ServiceDate >= DateTime.Now))
            {
                errors.Add("Cannot delete a payee with future transactions on it");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        public async Task<Result> DeletePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var validationResult = await ValidateDeletePayeeAsync(id).ConfigureAwait(false);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);
				payee.ModifiedDateTime = DateTime.Now;
                payee.DeletedDateTime = DateTime.Now;

                await _payeeDataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> UndoDeletePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);
                payee.ModifiedDateTime = DateTime.Now;
                payee.DeletedDateTime = null;

                await _payeeDataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);
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
                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);
                var populatedPayee = await GetPopulatedPayee(payee).ConfigureAwait(false);
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
                var allPayees = await _payeeDataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance
                                             && p.IsActive);

                var tasks = payees.Select(GetPopulatedPayee);

                result.Success = true;
				result.Data = OrderPayees(await Task.WhenAll(tasks));
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Payee>>> GetDeletedPayeesAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _payeeDataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance
                                             && p.IsDeleted);

                var tasks = payees.Select(GetPopulatedPayee);

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);

                var filteredPopulatedPayees = populatedPayees.Where(p => !p.IsAccount);

                result.Success = true;
                result.Data = OrderPayees(filteredPopulatedPayees);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesForReportAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _payeeDataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance
                                             && p.IsActive);

                var tasks = payees.Select(p => GetPopulatedPayee(p));

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);

                var filteredPopulatedPayees = populatedPayees.Where(p => !p.IsAccount);

                result.Success = true;
                result.Data = OrderPayees(filteredPopulatedPayees);
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
                var allPayees = await _payeeDataAccess.ReadPayeesAsync().ConfigureAwait(false);

                // ugly hardcoded to remove the starting balance payee.
                // may move to a "Payee Group" type setup
                var payees = allPayees.Where(p => p.IsActive && !p.IsStartingBalance).ToList();


                var tasks = payees.Select(GetPopulatedPayee);

                var payeesTemp = await Task.WhenAll(tasks).ConfigureAwait(false);

                result.Success = true;
				result.Data = OrderPayees(payeesTemp.Where(p => !p.IsAccount));
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public bool FilterPayee(Payee payee, string searchText)
        {
            if (payee != null)
            {
                return payee.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }
        
		public IReadOnlyList<Payee> OrderPayees(IEnumerable<Payee> payees)
        {
            return payees.OrderByDescending(p => p.IsAccount).ThenBy(p => p.Group).ThenBy(a => a.Description).ToList();
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
            var validationResult = await ValidatePayeeAsync(payee).ConfigureAwait(false);
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
                    await _payeeDataAccess.CreatePayeeAsync(payeeToUpsert).ConfigureAwait(false);
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
                    await _payeeDataAccess.UpdatePayeeAsync(payeeToUpsert).ConfigureAwait(false);
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

        async Task<Payee> GetPopulatedPayee(Payee payee)
        {
            var payeeToPopulate = payee.DeepCopy();

            var payeeAccount = await _accountDataAccess.ReadAccountAsync(payee.Id).ConfigureAwait(false);

            payeeToPopulate.IsAccount = !payeeAccount.IsNew;

            return payeeToPopulate;
        }
    }
}
