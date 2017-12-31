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

        public PayeeLogic(IPayeeDataAccess payeeDataAccess)
        {
            PayeeDataAccess = payeeDataAccess;
        }

        public async Task<Result> DeletePayeeAsync(Payee payee)
        {
            var result = new Result();

            payee.DeletedDateTime = DateTime.Now;

            await PayeeDataAccess.UpdatePayeeAsync(payee);
            result.Success = true;

            return result;
        }

        public Task<Result<Payee>> GetPayeeAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<Payee>>> GetPayeesAsync()
        {
            var result = new Result<IEnumerable<Payee>>();

            try
            {
                var payees = await PayeeDataAccess.ReadPayeesAsync();

                result.Success = true;
                result.Data = payees;
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

        public IEnumerable<GroupedList<Payee>> GroupPayees(IEnumerable<Payee> payees, bool includeDeleted = false)
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

            var deletedGroupedList = new GroupedList<Payee>("Deleted", "Del");

            if (includeDeleted)
            {
                deletedGroupedList.AddRange(deletedPayees);
            }

            groupedPayees.Add(deletedGroupedList);

            return groupedPayees;
        }

        public async Task<Result<Payee>> UpsertPayeeAsync(Payee payee)
        {
            var result = new Result<Payee>();
            var newPayee = payee.DeepCopy();

            if (newPayee.CreatedDateTime == null)
            {
                newPayee.CreatedDateTime = DateTime.Now;
                newPayee.ModifiedDateTime = DateTime.Now;
                try
                {
                    await PayeeDataAccess.CreatePayeeAsync(newPayee);
                    result.Success = true;
                    result.Data = newPayee;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }
            else
            {
                newPayee.ModifiedDateTime = DateTime.Now;
                try
                {
                    await PayeeDataAccess.UpdatePayeeAsync(newPayee);
                    result.Success = true;
                    result.Data = newPayee;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }

            }

            return result;
        }
    }
}
