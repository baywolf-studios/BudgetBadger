﻿using System;
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

        public async Task<Result<int>> GetPayeesCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _payeeDataAccess.GetPayeesCountAsync();
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
                                             && p.IsActive).ToList();

                if (allPayees.Any(p => p.IsHidden && !p.IsDeleted && !p.IsStartingBalance))
                {
                    payees.Add(GetGenericHiddenPayee());
                }

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
                var test = ex.StackTrace;
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

        public async Task<Result<IReadOnlyList<Payee>>> GetPayeesForReportAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _payeeDataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance
                                             && p.IsActive).ToList();

                if (allPayees.Any(p => p.IsHidden && !p.IsDeleted && !p.IsStartingBalance))
                {
                    payees.Add(GetGenericHiddenPayee());
                }

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

        public async Task<Result<IReadOnlyList<Payee>>> GetHiddenPayeesAsync()
        {
            var result = new Result<IReadOnlyList<Payee>>();

            try
            {
                var allPayees = await _payeeDataAccess.ReadPayeesAsync().ConfigureAwait(false);

                var payees = allPayees.Where(p =>
                                             !p.IsStartingBalance
                                             && p.IsHidden
                                             && !p.IsDeleted);

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

        public async Task<Result> SoftDeletePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (payee.IsNew || payee.IsDeleted || payee.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeDeleteNotHiddenError"));
                }

                var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(id).ConfigureAwait(false);
                if (payeeTransactions.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeDeleteActiveTransactionsError"));
                }

                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);
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

        public async Task<Result> HidePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);

                // check for validation to Hide
                var errors = new List<string>();

                if (!payee.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeHideInactiveError"));
                }

                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);

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

        public async Task<Result> UnhidePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (payee.IsNew || payee.IsActive || payee.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("PayeeUnhideNotHiddenError"));
                }

                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);

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


        async Task<Result> ValidatePayeeAsync(Payee payee)
        {
            var errors = new List<string>();

            var account = await _accountDataAccess.ReadAccountAsync(payee.Id).ConfigureAwait(false);
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

            var payeeAccount = await _accountDataAccess.ReadAccountAsync(payee.Id).ConfigureAwait(false);

            payeeToPopulate.IsAccount = !payeeAccount.IsNew;

            if (string.IsNullOrEmpty(payeeToPopulate.Description))
            {
                payeeToPopulate.Group = String.Empty;
            }
            else if (payeeToPopulate.IsAccount)
            {
                payeeToPopulate.Group = _resourceContainer.GetResourceString("PayeeTransferGroup");
            }
            else if (payeeToPopulate.IsHidden)
            {
                payeeToPopulate.Group = _resourceContainer.GetResourceString("Hidden");
            }
            else
            {
                payeeToPopulate.Group = payeeToPopulate.Description[0].ToString().ToUpper();
            }

            return payeeToPopulate;
        }

        Payee GetStartingBalancePayee()
        {
            var startingBalancePayee = Constants.StartingBalancePayee.DeepCopy();

            startingBalancePayee.Description = _resourceContainer.GetResourceString(nameof(Constants.StartingBalancePayee));
            startingBalancePayee.Group = String.Empty;

            return startingBalancePayee;
        }

        Payee GetGenericHiddenPayee()
        {
            var hiddenPayee = Constants.GenericHiddenPayee.DeepCopy();

            hiddenPayee.Description = _resourceContainer.GetResourceString("Hidden");
            hiddenPayee.Group = _resourceContainer.GetResourceString("Hidden");

            return hiddenPayee;
        }
    }
}
