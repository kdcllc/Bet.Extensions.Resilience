#if NETSTANDARD2_0

using System.Data.SqlClient;

#else

using Microsoft.Data.SqlClient;

#endif

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Data.SqlClient
{
    public class SqlFailoverDbConnection : FailoverDbConnection
    {
        public SqlFailoverDbConnection(
            string connectionString,
            Guid correlationId,
            ISyncPolicy syncPolicy,
            IAsyncPolicy asyncPolicy,
            ILogger logger) : base(correlationId, syncPolicy, asyncPolicy, logger)
        {
            Connection = new SqlConnection(connectionString);
        }
    }
}
