using System.Diagnostics;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.WebApp.Sample.Clients;
using Bet.Extensions.Resilience.WebApp.Sample.Models;

using Microsoft.AspNetCore.Mvc;

using Polly.CircuitBreaker;

namespace Bet.Extensions.Resilience.WebApp.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IThrowClient _throwClient;
        private readonly IBibleClient _bibleClient;
        private readonly IServiceProvider _provider;
        private readonly PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions> _policyBucket;

        public HomeController(
            IThrowClient throwClient,
            IBibleClient bibleClient,
            IServiceProvider provider,
            PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions> policyBucket)
        {
            _throwClient = throwClient ?? throw new ArgumentNullException(nameof(throwClient));
            _bibleClient = bibleClient ?? throw new ArgumentNullException(nameof(bibleClient));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _policyBucket = policyBucket ?? throw new ArgumentNullException(nameof(policyBucket));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new BibleQuoteModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BibleQuoteModel model, CancellationToken cancellationToken)
        {
            // testing the configuration of policies
            var policy = _policyBucket.GetPolicy(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy);

            var services = _provider.GetServices<PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions>>();

            var result = await _bibleClient.GetQuoteAsync(model.Search, cancellationToken);

            model.Result = result;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Status(CancellationToken cancellationToken)
        {
            var result = await _throwClient.GetStatusAsync(cancellationToken);
            ViewBag.Status = result;
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
