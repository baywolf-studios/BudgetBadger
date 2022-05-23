using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Core.Logic
{
    public class PayeeLogic : IPayeeLogic
    {
        readonly IDataAccess _dataAccess;
        readonly IResourceContainer _resourceContainer;

        public PayeeLogic(IDataAccess dataAccess, 
            IResourceContainer resourceContainer)
        {
            _dataAccess = dataAccess;
            _resourceContainer = resourceContainer;
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
                    await _dataAccess.CreatePayeeAsync(payeeToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = await GetPopulatedPayee(payeeToUpsert).ConfigureAwait(false);
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
                    await _dataAccess.UpdatePayeeAsync(payeeToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = await GetPopulatedPayee(payeeToUpsert).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }

            }

            return result;
        }

        public async Task<Result<int>> GetPayeesCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _dataAccess.GetPayeesCountAsync();
                result.Success = true;
                result.Data = count;
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
                var payee = await _dataAccess.ReadPayeeAsync(id).ConfigureAwait(false);
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

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _dataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var tasks = allPayees.Select(GetPopulatedPayee);

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);
                var payeesToReturn = populatedPayees.Where(p => FilterPayee(p, FilterType.Standard)).ToList();

                if (populatedPayees.Any(p => FilterPayee(p, FilterType.Hidden)))
                {
                    payeesToReturn.Add(GetGenericHiddenPayee());
                }

                result.Success = true;
                result.Data = payeesToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                var test = ex.StackTrace;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _dataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var tasks = allPayees.Select(GetPopulatedPayee);

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);
                var payeesToReturn = populatedPayees.Where(p => FilterPayee(p, FilterType.Selection)).ToList();

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

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesForReportAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _dataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var tasks = allPayees.Select(GetPopulatedPayee);

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);
                var payeesToReturn = populatedPayees.Where(p => FilterPayee(p, FilterType.Report)).ToList();

                if (populatedPayees.Any(p => FilterPayee(p, FilterType.Hidden)))
                {
                    payeesToReturn.Add(GetGenericHiddenPayee());
                }

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

        public async Task<Result<IReadOnlyList<Payee>>> GetHiddenPayeesAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _dataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var tasks = allPayees.Select(GetPopulatedPayee);

                var populatedPayees = await Task.WhenAll(tasks).ConfigureAwait(false);
                var payeesToReturn = populatedPayees.Where(p => FilterPayee(p, FilterType.Hidden)).ToList();

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

        public async Task<Result<Payee>> SoftDeletePayeeAsync(Guid id)
        {
            var result = new Result<Payee>();

            try
            {
                var payee = await _dataAccess.ReadPayeeAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (payee.IsNew || payee.IsDeleted || payee.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeDeleteNotHiddenError"));
                }

                var payeeTransactions = await _dataAccess.ReadPayeeTransactionsAsync(id).ConfigureAwait(false);
                if (payeeTransactions.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeDeleteActiveTransactionsError"));
                }

                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);
                if (payee.IsStartingBalance || account.IsActive || payee.IsGenericHiddenPayee)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeDeleteSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                payee.DeletedDateTime = DateTime.Now;
                payee.ModifiedDateTime = DateTime.Now;

                await _dataAccess.UpdatePayeeAsync(payee);

                result.Success = true;
                result.Data = await GetPopulatedPayee(payee).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Payee>> HidePayeeAsync(Guid id)
        {
            var result = new Result<Payee>();

            try
            {
                var payee = await _dataAccess.ReadPayeeAsync(id).ConfigureAwait(false);

                // check for validation to Hide
                var errors = new List<string>();

                if (!payee.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeHideInactiveError"));
                }

                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                if (payee.IsStartingBalance || account.IsActive || payee.IsGenericHiddenPayee)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeHideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                payee.HiddenDateTime = DateTime.Now;
                payee.ModifiedDateTime = DateTime.Now;

                await _dataAccess.UpdatePayeeAsync(payee);

                result.Success = true;
                result.Data = await GetPopulatedPayee(payee).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Payee>> UnhidePayeeAsync(Guid id)
        {
            var result = new Result<Payee>();

            try
            {
                var payee = await _dataAccess.ReadPayeeAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (payee.IsNew || payee.IsActive || payee.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeUnhideNotHiddenError"));
                }

                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                if (payee.IsStartingBalance || account.IsActive || payee.IsGenericHiddenPayee)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeUnhideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                payee.HiddenDateTime = null;
                payee.ModifiedDateTime = DateTime.Now;

                await _dataAccess.UpdatePayeeAsync(payee);

                result.Success = true;
                result.Data = await GetPopulatedPayee(payee).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            if (payee != null)
            {
                return payee.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public bool FilterPayee(Payee payee, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Standard:
                case FilterType.Report:
                    return payee.IsActive && !payee.IsStartingBalance && !payee.IsAccount;
                case FilterType.Selection:
                    return payee.IsActive && !payee.IsStartingBalance;
                case FilterType.Hidden:
                    return payee.IsHidden && !payee.IsDeleted && !payee.IsGenericHiddenPayee && !payee.IsStartingBalance && !payee.IsAccount;
                case FilterType.All:
                default:
                    return true;
            }
        }

        async Task<Result> ValidatePayeeAsync(Payee payee)
        {
            var errors = new List<string>();

            var account = await _dataAccess.ReadAccountAsync(payee.Id).ConfigureAwait(false);
            if (payee.IsStartingBalance || account.IsActive || payee.IsGenericHiddenPayee)
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeSaveSystemError"));
            }

            if (string.IsNullOrEmpty(payee.Description))
            {
                errors.Add(_resourceContainer.GetResourceString("PayeeValidDescriptionError"));
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        async Task<Payee> GetPopulatedPayee(Payee payee)
        {
            if (payee.IsStartingBalance)
            {
                return GetStartingBalancePayee();
            }

            if( payee.IsGenericHiddenPayee)
            {
                return GetGenericHiddenPayee();
            }

            var payeeToPopulate = payee.DeepCopy();

            var payeeAccount = await _dataAccess.ReadAccountAsync(payee.Id).ConfigureAwait(false);

            payeeToPopulate.IsAccount = !payeeAccount.IsNew;

            payeeToPopulate.TranslatePayee(_resourceContainer);

            return payeeToPopulate;
        }

        Payee GetStartingBalancePayee()
        {
            var startingBalancePayee = Constants.StartingBalancePayee.DeepCopy();

            startingBalancePayee.TranslatePayee(_resourceContainer);

            return startingBalancePayee;
        }

        Payee GetGenericHiddenPayee()
        {
            var hiddenPayee = Constants.GenericHiddenPayee.DeepCopy();

            hiddenPayee.TranslatePayee(_resourceContainer);

            return hiddenPayee;
        }
    }
}
