﻿using System;
using BudgetBadger.Core.Models;
using Prism.Events;

namespace BudgetBadger.Forms.Events
{
    public class PayeeDeletedEvent : PubSubEvent<PayeeModel>
    {
    }
}
