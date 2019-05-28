﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Bet.Extensions.Resilience.SampleWebApp.Clients.Models;

namespace Bet.Extensions.Resilience.SampleWebApp.Clients
{
    public interface IChavahClient
    {
        Task<IEnumerable<Song>> GetPopular(int count);
    }
}
