using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models;
using BudgetBadger.Core.Converters;

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

        public async Task<Result<Guid>> SavePayeeAsync(PayeeEditModel payee)
        {
            try
            {
                var validationErrors = new List<string>();
                if (string.IsNullOrEmpty(payee.Description))
                {
                    validationErrors.Add(AppResources.PayeeValidDescriptionError);
                }
                if (payee.Id == Constants.StartingBalancePayeeId)
                {
                    validationErrors.Add(AppResources.PayeeSaveSystemError);
                }
                var accounts = await _dataAccess.ReadAccountDtosAsync(new List<Guid> { payee.Id });
                if (accounts.Any())
                {
                    validationErrors.Add(AppResources.PayeeSaveSystemError);
                }
                if (validationErrors.Any())
                {
                    return Result.Fail<Guid>(string.Join(Environment.NewLine, validationErrors));
                }

                var payeeDto = PayeeEditModelConverter.Convert(payee);

                var existingPayeeDtos = await _dataAccess.ReadPayeeDtosAsync(new List<Guid> { payeeDto.Id });

                if (existingPayeeDtos.Any())
                {
                    await _dataAccess.UpdatePayeeDtoAsync(payeeDto);
                }
                else
                {
                    await _dataAccess.CreatePayeeDtoAsync(payeeDto);
                }

                return Result.Ok<Guid>(payee.Id);
            }
            catch (Exception ex)
            {
                return Result.Fail<Guid>(ex.Message);
            }
        }

        public async Task<Result<int>> GetHiddenPayeesCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var payees = await _dataAccess.ReadPayeeDtosAsync();
                var hiddenPayees = payees.Where(p => p.Hidden && !p.Deleted);
                var hiddenAccountPayees = await _dataAccess.ReadAccountDtosAsync(hiddenPayees.Select(p => p.Id));
                result.Success = true;
                result.Data = hiddenPayees.Where(p => !hiddenAccountPayees.Any(a => a.Id == p.Id)).Count();
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

        public async Task<Result<IReadOnlyList<PayeeEditModel>>> GetPayees2Async(IEnumerable<Guid> payeeIds = null)
        {
            try
            {
                var allPayees = await _dataAccess.ReadPayeeDtosAsync(payeeIds).ConfigureAwait(false);
                var payeeAccounts = await _dataAccess.ReadAccountDtosAsync(allPayees.Select(p => p.Id));

                var payeesToReturn = allPayees.AsParallel()
                    .Where(p => !p.Hidden
                             && !p.Deleted
                             && p.Id != Constants.StartingBalancePayeeId
                             && !payeeAccounts.Any(a => a.Id == p.Id))
                    .Select(p => PayeeEditModelConverter.Convert(p));

                return Result.Ok<IReadOnlyList<PayeeEditModel>>(payeesToReturn.ToList().AsReadOnly());
            }
            catch (Exception ex)
            {
                return Result.Fail<IReadOnlyList<PayeeEditModel>>(ex.Message);
            }
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

        public async Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesForSelection2Async(IEnumerable<Guid> payeeIds = null)
        {
            try
            {
                var allPayees = await _dataAccess.ReadPayeeDtosAsync(payeeIds).ConfigureAwait(false);
                var payeeAccounts = await _dataAccess.ReadAccountDtosAsync(allPayees.Select(p => p.Id));

                var payeesToReturn = allPayees.AsParallel()
                    .Where(p => !p.Hidden
                             && !p.Deleted
                             && p.Id != Constants.StartingBalancePayeeId)
                    .Select(p =>
                    {
                        var account = payeeAccounts.FirstOrDefault(a => a.Id == p.Id);
                        if (account is null)
                        {
                            return PayeeModelConverter.Convert(p);
                        }
                        else
                        {
                            return PayeeModelConverter.Convert(account);
                        }
                    });

                return Result.Ok<IReadOnlyList<PayeeModel>>(payeesToReturn.ToList().AsReadOnly());
            }
            catch (Exception ex)
            {
                return Result.Fail<IReadOnlyList<PayeeModel>>(ex.Message);
            }
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


        public async Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesForReport2Async(IEnumerable<Guid> payeeIds = null)
        {
            try
            {
                var allPayees = await _dataAccess.ReadPayeeDtosAsync(payeeIds).ConfigureAwait(false);
                var payeeAccounts = await _dataAccess.ReadAccountDtosAsync(allPayees.Select(p => p.Id));

                var payeesToReturn = allPayees.AsParallel()
                    .Where(p => !p.Hidden
                             && !p.Deleted
                             && p.Id != Constants.StartingBalancePayeeId
                             && !payeeAccounts.Any(a => a.Id == p.Id))
                    .Select(p => PayeeModelConverter.Convert(p));

                return Result.Ok<IReadOnlyList<PayeeModel>>(payeesToReturn.ToList().AsReadOnly());
            }
            catch (Exception ex)
            {
                return Result.Fail<IReadOnlyList<PayeeModel>>(ex.Message);
            }
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

        public async Task<Result<IReadOnlyList<PayeeEditModel>>> GetHiddenPayees2Async(IEnumerable<Guid> payeeIds)
        {
            try
            {
                var allPayees = await _dataAccess.ReadPayeeDtosAsync(payeeIds).ConfigureAwait(false);
                var payeeAccounts = await _dataAccess.ReadAccountDtosAsync(allPayees.Select(p => p.Id));

                var payeesToReturn = allPayees.AsParallel()
                    .Where(p => p.Hidden
                             && !p.Deleted
                             && p.Id != Constants.StartingBalancePayeeId
                             && !payeeAccounts.Any(a => a.Id == p.Id))
                    .Select(p => PayeeEditModelConverter.Convert(p));

                return Result.Ok<IReadOnlyList<PayeeEditModel>>(payeesToReturn.ToList().AsReadOnly());
            }
            catch (Exception ex)
            {
                return Result.Fail<IReadOnlyList<PayeeEditModel>>(ex.Message);
            }
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

        public Task<Result> SoftDeletePayee2Async(Guid payeeId)
        {
            throw new NotImplementedException();
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

        public Task<Result> HidePayee2Async(Guid payeeId)
        {
            throw new NotImplementedException();
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

        public Task<Result> UnhidePayee2Async(Guid payeeId)
        {
            throw new NotImplementedException();
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
