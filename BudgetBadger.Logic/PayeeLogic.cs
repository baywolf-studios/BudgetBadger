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
        readonly IPayeeDataAccess PayeeDataAccess;
        readonly IAccountDataAccess AccountDataAccess;

        public PayeeLogic(IPayeeDataAccess payeeDataAccess, IAccountDataAccess accountDataAccess)
        {
            PayeeDataAccess = payeeDataAccess;
            AccountDataAccess = accountDataAccess;
        }

        public async Task<Result> DeletePayeeAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var payee = await PayeeDataAccess.ReadPayeeAsync(id);
                payee.DeletedDateTime = DateTime.Now;

                await PayeeDataAccess.UpdatePayeeAsync(payee);
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
                var payee = await PayeeDataAccess.ReadPayeeAsync(id);
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

        public async Task<Result<IEnumerable<Payee>>> GetPayeesAsync()
        {
            var result = new Result<IEnumerable<Payee>>();

            try
            {
                var allPayees = await PayeeDataAccess.ReadPayeesAsync();

                // ugly hardcoded to remove the starting balance payee.
                // may move to a "Payee Group" type setup
                var payees = allPayees.Where(p => p.DeletedDateTime == null && p.Description != "Starting Balance").ToList();

                var includeDeleted = false; //will get this from settings dataaccess
                if (includeDeleted)
                {
                    payees.AddRange(allPayees.Where(p => p.DeletedDateTime.HasValue));
                }

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

        public IEnumerable<Payee> SearchPayees(IEnumerable<Payee> payees, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return payees;
            }

            return payees.Where(a => a.Description.ToLower().Contains(searchText.ToLower()));
        }

        public ILookup<string, Payee> GroupPayees(IEnumerable<Payee> payees)
        {
            var groupedPayees = payees.ToLookup(p =>
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

            return groupedPayees;
        }

        public async Task<Result<Payee>> SavePayeeAsync(Payee payee)
        {
            if (!payee.IsValid())
            {
                return payee.Validate().ToResult<Payee>();
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
                    await PayeeDataAccess.CreatePayeeAsync(payeeToUpsert);
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
                    await PayeeDataAccess.UpdatePayeeAsync(payeeToUpsert);
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

            var payeeAccount = await AccountDataAccess.ReadAccountAsync(payee.Id);

            payeeToPopulate.IsAccount = payeeAccount.Exists;

            return payeeToPopulate;
        }
    }
}
