using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.DataAccess;
using BudgetBadger.Core.Localization;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Logic
{
    public interface IPayeeLogic
    {
        Task<ItemsResponse<Payee>> SearchPayeesAsync(bool? hidden = null, bool? isAccount = null, bool? isStartingBalance = null);
        Task<DataResponse<PayeeId>> CreatePayeeAsync(string description, string notes, bool hidden);
        Task<DataResponse<Payee>> ReadPayeeAsync(PayeeId payeeId);
        Task<Response> UpdatePayeeAsync(PayeeId payeeId, string description, string notes, bool hidden);
        Task<Response> DeletePayeeAsync(PayeeId payeeId);
    }

    public class PayeeLogic : IPayeeLogic
    {
        readonly IDataAccess _dataAccess;

        public PayeeLogic(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<ItemsResponse<Payee>> SearchPayeesAsync(bool? hidden = null, bool? isAccount = null, bool? isStartingBalance = null)
        {
            try
            {
                // get all non-deleted payeeDtos
                IEnumerable<PayeeDto> payeeDtos = await _dataAccess.ReadPayeeDtosAsync().ConfigureAwait(false);
                payeeDtos = payeeDtos.AsParallel().Where(p => !p.Deleted).ToList();
                var payeeIds = new HashSet<Guid>(payeeDtos.AsParallel().Select(p => p.Id));

                // get all non-deleted accountDtos from the payeeIds
                IEnumerable<AccountDto> accountDtos = await _dataAccess.ReadAccountDtosAsync(payeeIds).ConfigureAwait(false);
                accountDtos = accountDtos.AsParallel().Where(a => !a.Deleted).ToList();
                var accountIds = new HashSet<Guid>(accountDtos.AsParallel().Select(a => a.Id));

                // remove any accountDtos from the payeeDtos based on Id
                payeeDtos = payeeDtos.AsParallel().Where(p => !accountIds.Contains(p.Id));
                accountDtos = accountDtos.AsParallel();

                if (isAccount == true)
                {
                    payeeDtos = Enumerable.Empty<PayeeDto>();
                }
                else if (isAccount == false)
                {
                    accountDtos = Enumerable.Empty<AccountDto>();
                }

                if (isStartingBalance == true)
                {
                    payeeDtos = payeeDtos.Where(p => p.Id == Core.Models.Constants.StartingBalancePayeeId);
                    accountDtos = Enumerable.Empty<AccountDto>();
                }
                else if (isStartingBalance == false)
                {
                    payeeDtos = payeeDtos.Where(p => p.Id != Core.Models.Constants.StartingBalancePayeeId);
                }

                if (hidden == true)
                {
                    payeeDtos = payeeDtos.Where(p => p.Hidden);
                    accountDtos = accountDtos.Where(a => a.Hidden);
                }
                else if (hidden == false)
                {
                    payeeDtos = payeeDtos.Where(p => !p.Hidden);
                    accountDtos = accountDtos.Where(a => !a.Hidden);
                }

                var payees = payeeDtos
                    .Select(p => PayeeConverter.Convert(p))
                    .Concat(accountDtos.Select(a => PayeeConverter.Convert(a)))
                    .ToList();

                return ItemsResponse.OK(payees, payees.Count);
            }
            catch (Exception ex)
            {
                return ItemsResponse.InternalError<Payee>(ex.Message);
            }
        }

        public async Task<DataResponse<PayeeId>> CreatePayeeAsync(string description, string notes, bool hidden)
        {
            try
            {
                if (string.IsNullOrEmpty(description))
                {
                    return DataResponse.BadRequest<PayeeId>(AppResources.PayeeValidDescriptionError);
                }

                var payeeId = new PayeeId();
                var payeeDto = new PayeeDto()
                {
                    Id = payeeId,
                    Description = description,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = DateTime.Now
                };

                await _dataAccess.CreatePayeeDtoAsync(payeeDto).ConfigureAwait(false); ;

                return DataResponse.OK(payeeId);
            }
            catch (Exception ex)
            {
                return DataResponse.InternalError<PayeeId>(ex.Message);
            }
        }

        public async Task<DataResponse<Payee>> ReadPayeeAsync(PayeeId payeeId)
        {
            var payeeDtos = await _dataAccess.ReadPayeeDtosAsync(new List<Guid>() { payeeId }).ConfigureAwait(false);
            var payeeDto = payeeDtos.FirstOrDefault();
            if (payeeDto == null)
            {
                return DataResponse.NotFound<Payee>(AppResources.NotFoundError);
            }
            else if (payeeDto.Deleted)
            {
                return DataResponse.Gone<Payee>(AppResources.GoneError);
            }

            var accountDtos = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { payeeId }).ConfigureAwait(false); ;
            var accountDto = accountDtos.FirstOrDefault();
            if (accountDto != null)
            {
                return DataResponse.OK(PayeeConverter.Convert(accountDto));
            }

            return DataResponse.OK(PayeeConverter.Convert(payeeDto));
        }

        public async Task<Response> UpdatePayeeAsync(PayeeId payeeId, string description, string notes, bool hidden)
        {
            try
            {
                if (payeeId == Core.Models.Constants.StartingBalancePayeeId)
                {
                    return Response.Forbidden(AppResources.PayeeSaveSystemError);
                }

                if (payeeId == Guid.Empty)
                {
                    return Response.BadRequest(AppResources.PayeeSaveSystemError);
                }

                if (string.IsNullOrEmpty(description))
                {
                    return Response.BadRequest(AppResources.PayeeValidDescriptionError);
                }

                var payeeDtos = await _dataAccess.ReadPayeeDtosAsync(new List<Guid>() { payeeId }).ConfigureAwait(false);
                var payeeDto = payeeDtos.FirstOrDefault();
                if (payeeDto == null)
                {
                    return Response.NotFound(AppResources.TransactionValidPayeeExistError);
                }
                if (payeeDto.Deleted)
                {
                    return Response.Gone(AppResources.TransactionValidPayeeDeletedError); // use better strings
                }

                var accountDtos = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { payeeId }).ConfigureAwait(false);
                var accountDto = accountDtos.FirstOrDefault();
                if (accountDto != null)
                {
                    return Response.Conflict(string.Empty);
                }

                await _dataAccess.UpdatePayeeDtoAsync(new PayeeDto()
                {
                    Id = payeeId,
                    Description = description,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = DateTime.Now
                }).ConfigureAwait(false); ;

                return Response.OK();
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        public async Task<Response> DeletePayeeAsync(PayeeId payeeId)
        {
            try
            {
                if (payeeId == Core.Models.Constants.StartingBalancePayeeId)
                {
                    return Response.Forbidden(AppResources.PayeeDeleteStartingBalanceError);
                }

                if (payeeId == Guid.Empty)
                {
                    return Response.BadRequest(AppResources.PayeeDeleteNotHiddenError);
                }

                var payees = await _dataAccess.ReadPayeeDtosAsync(new List<Guid>() { payeeId }).ConfigureAwait(false);
                var payeeDto = payees.FirstOrDefault();
                if (payeeDto == null)
                {
                    return Response.NotFound(AppResources.PayeeDeleteNotHiddenError);
                }
                else if (payeeDto.Deleted)
                {
                    return Response.Gone(AppResources.PayeeDeleteNotHiddenError);
                }
                else if (!payeeDto.Hidden)
                {
                    return Response.Conflict(AppResources.PayeeDeleteInactiveError);
                }

                var payeeTransactionDtos = await _dataAccess.ReadTransactionDtosAsync(payeeIds: new List<Guid>() { payeeId }).ConfigureAwait(false);
                if (payeeTransactionDtos.Any(t => !t.Deleted))
                {
                    return Response.Conflict(AppResources.PayeeDeleteActiveTransactionsError);
                }

                var accounts = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { payeeId }).ConfigureAwait(false);
                var accountDto = accounts.FirstOrDefault();
                if (accountDto != null)
                {
                    return Response.Conflict(AppResources.PayeeDeleteAccountError);
                }

                await _dataAccess.UpdatePayeeDtoAsync(payeeDto with { Deleted = true, ModifiedDateTime = DateTime.Now }).ConfigureAwait(false);

                return Response.OK();
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }
    }
}
