﻿using EntityEquity.Common;
using EntityEquity.Data;
namespace EntityEquity.Models
{
    public class FinalizeOrderReturnModel
    {
        public Order Order { get; set; }
        public CreditCardResult Result { get; set; }
        public bool MustShip { get; set; }
        public bool PaidMustShip
        {
            get
            {
                return Result.Successful && MustShip;
            }
        }
    }
}
