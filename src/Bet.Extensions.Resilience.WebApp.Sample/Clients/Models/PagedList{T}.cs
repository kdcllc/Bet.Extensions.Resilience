﻿using System.Collections.Generic;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients.Models
{
    public class PagedList<T>
    {
        public PagedList()
        {
            Items = new List<T>();
        }

        public int Total { get; set; }

        public List<T> Items { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
