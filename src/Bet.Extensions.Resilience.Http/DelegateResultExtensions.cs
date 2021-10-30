using Bet.Extensions.Resilience.Abstractions;

namespace Polly
{
    public static class DelegateResultExtensions
    {
        public static string GetMessage(this DelegateResult<HttpResponseMessage> delegateResult)
        {
            return delegateResult?.Exception?.GetExceptionMessages() ?? $"Failed with StatusCode: {delegateResult?.Result.StatusCode}";
        }
    }
}
