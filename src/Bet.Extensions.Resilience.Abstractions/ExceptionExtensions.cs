using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    public static class ExceptionExtensions
    {
        public static string GetExceptionMessages(this Exception ex)
        {
            var baseException = ex.GetBaseException();
            if (baseException == ex)
            {
                return ex.Message;
            }

            return ex.Message + $"{Environment.NewLine}Inner Exception: {baseException.Message}";
        }

        public static string GetExceptionMessages<TResult>(this DelegateResult<TResult> result)
        {
            return result?.Exception?.Message ?? "reason missing";
        }
    }
}
