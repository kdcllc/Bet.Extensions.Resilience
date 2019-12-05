using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.WebApp.Sample.Clients;
using Bet.Extensions.Resilience.WebApp.Sample.Clients.Models;

using Microsoft.AspNetCore.Mvc;

namespace Bet.Extensions.Resilience.WebApp.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly IChavahClient _chavahClient;

        public SongsController(IChavahClient chavahClient)
        {
            _chavahClient = chavahClient ?? throw new ArgumentNullException(nameof(chavahClient));
        }

        [HttpGet]
        public async Task<IEnumerable<Song>> Get(int count = 3)
        {
            var result = await _chavahClient.GetPopular(count);

            return result;
        }
    }
}
