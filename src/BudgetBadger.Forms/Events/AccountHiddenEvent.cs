using System;
using BudgetBadger.Models;
using Prism.Events;

namespace BudgetBadger.Forms.Events
{
    public class AccountHiddenEvent : PubSubEvent<Account>
    {
    }
}
