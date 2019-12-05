using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.WebApp.Sample.Clients.Models;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients
{
    public class ChavahClient : IChavahClient
    {
        private readonly HttpClient _httpClient;
        private ChavahClientOptions _options;

        public ChavahClient(
            HttpClient httpClient,
            IOptionsMonitor<ChavahClientOptions> optionsMonitor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _options = optionsMonitor.Get(nameof(ChavahClient));

            optionsMonitor.OnChange((newValues) =>
            {
                _options = newValues;
            });
        }

        public async Task<IEnumerable<Song>> GetPopular(int count)
        {
            var response = await _httpClient.GetAsync($"api/songs/getpopular?count={count}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Song>>(json);
        }
    }
}
