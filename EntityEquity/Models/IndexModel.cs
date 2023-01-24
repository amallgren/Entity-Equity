﻿using EntityEquity.Data;
using Microsoft.AspNetCore.Identity;

namespace EntityEquity.Models
{
    public class IndexModel
    {
        public string BaseAddress { get; set; }
        public List<Property>? Properties { get; set; }
        public List<EquityShare>? EquityShares { get; set; }
        public string UserId { get; set; }
        public IndexModel(string baseAddress, string userId)
        {
            BaseAddress = baseAddress;
            UserId = userId;
        }
    }
}
