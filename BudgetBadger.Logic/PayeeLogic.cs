using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

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

            PayeeDataAccess.CreatePayeeAsync(Constants.StartingBalancePayee);
        }

        public async Task<Result> DeletePayeeAsync(Payee payee)
        {
            var result = new Result();

            payee.DeletedDateTime = DateTime.Now;

            await PayeeDataAccess.UpdatePayeeAsync(payee);
            result.Success = true;

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
                var payees = await PayeeDataAccess.ReadPayeesAsync();

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

        public IEnumerable<GroupedList<Payee>> GroupPayees(IEnumerable<Payee> payees)
        {
            var groupedPayees = new List<GroupedList<Payee>>();
            var deletedPayees = payees.Where(p => p.DeletedDateTime != null);
            var activePayees = payees.Where(p => p.DeletedDateTime == null);

            var temp = activePayees.GroupBy(a => a.Description[0].ToString());
            foreach (var tempGroup in temp)
            {
                var groupedList = new GroupedList<Payee>(tempGroup.Key, tempGroup.Key);
                groupedList.AddRange(tempGroup);
                groupedPayees.Add(groupedList);
            }

            var includeDeleted = false; //will get this from settings dataaccess
            if (includeDeleted)
            {
                var deletedGroupedList = new GroupedList<Payee>("Deleted", "Del");
                deletedGroupedList.AddRange(deletedPayees);
                groupedPayees.Add(deletedGroupedList);
            }

            return groupedPayees;
        }

        public async Task<Result<Payee>> UpsertPayeeAsync(Payee payee)
        {
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
