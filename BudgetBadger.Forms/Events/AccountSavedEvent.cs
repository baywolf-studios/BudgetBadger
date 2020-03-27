using System;
using BudgetBadger.Models;
using Prism.Events;

namespace BudgetBadger.Forms.Events
{
    public class AccountSavedEvent : PubSubEvent<Account>
    {
    }
}
