using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
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
        readonly IResourceContainer _resourceContainer;

        public PayeeLogic(IPayeeDataAccess payeeDataAccess,
            IAccountDataAccess accountDataAccess, 
            ITransactionDataAccess transactionDataAccess,
            IResourceContainer resourceContainer)
        {
            _payeeDataAccess = payeeDataAccess;
            _accountDataAccess = accountDataAccess;
            _transactionDataAccess = transactionDataAccess;
            _resourceContainer = resourceContainer;
        }

        async Task<Result> ValidateDeletePayeeAsync(Guid payeeId)
        {
            var errors = new List<string>();

            var payee = await _payeeDataAccess.ReadPayeeAsync(payeeId).ConfigureAwait(false);
            var populatePayee = await GetPopulatedPayee(payee).ConfigureAwait(false);

            if (!populatePayee.IsActive)
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeDeleteInactiveError")); 
            }

            if (populatePayee.IsAccount)
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeDeleteAccountError"));
            }

            if (populatePayee.Id == Constants.StartingBalancePayee.Id)
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeDeleteStartingBalanceError")); 
            }

            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(populatePayee.Id).ConfigureAwait(false);
            if (payeeTransactions.Any(t => t.IsActive && t.ServiceDate >= DateTime.Now))
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeDeleteFutureTransactionsError"));
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

                var payeesToReturn = (await Task.WhenAll(tasks)).ToList();
                payeesToReturn.Sort();

                result.Success = true;
				result.Data = payeesToReturn;
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

                var filteredPopulatedPayees = populatedPayees.Where(p => !p.IsAccount).ToList();
                filteredPopulatedPayees.Sort();

                result.Success = true;
                result.Data = filteredPopulatedPayees;
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

                var filteredPopulatedPayees = populatedPayees.Where(p => !p.IsAccount).ToList();
                filteredPopulatedPayees.Sort();

                result.Success = true;
                result.Data = filteredPopulatedPayees;
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
                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance
                                             && p.IsActive);

                var tasks = payees.Select(p => GetPopulatedPayee(p));

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);

                var filteredPopulatedPayees = populatedPayees.Where(p => !p.IsAccount).ToList();
                filteredPopulatedPayees.Sort();

                result.Success = true;
				result.Data = filteredPopulatedPayees;
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

        public Task<Result> ValidatePayeeAsync(Payee payee)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(payee.Description))
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeValidDescriptionError"));
            }

            return Task.FromResult<Result>(new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) });
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
