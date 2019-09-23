using System.Net.Http;

namespace Polly
{
    public static class DelegateResultExtensions
    {
        public static string GetMessage(this DelegateResult<HttpResponseMessage> delegateResult)
        {
            var ex = delegateResult?.Exception?.Message;
            return ex != null
                ? $"Exception: {ex}"
                : $"Failed with StatusCode {delegateResult?.Result?.StatusCode}";
        }
    }
}
