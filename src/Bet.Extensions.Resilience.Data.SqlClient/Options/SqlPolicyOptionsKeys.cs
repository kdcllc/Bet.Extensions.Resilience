namespace Bet.Extensions.Resilience.Data.SqlClient.Options;

public static class SqlPolicyOptionsKeys
{
    public const string DefaultHttpPolicy = "SqlPolicies";

    public const string SqlAdvancedCircuitBreakerPolicy = nameof(SqlAdvancedCircuitBreakerPolicy);
    public const string SqlBulkheadPolicy = nameof(SqlBulkheadPolicy);
    public const string SqlCircuitBreakerPolicy = nameof(SqlCircuitBreakerPolicy);
    public const string SqlFallbackPolicy = nameof(SqlFallbackPolicy);
    public const string SqlRetryJitterPolicy = nameof(SqlRetryJitterPolicy);
    public const string SqlRetryPolicy = nameof(SqlRetryPolicy);
    public const string SqlTimeoutPolicy = nameof(SqlTimeoutPolicy);
}
