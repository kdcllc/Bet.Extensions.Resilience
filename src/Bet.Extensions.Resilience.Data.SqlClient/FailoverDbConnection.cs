using System.Data;
using System.Data.Common;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Data.SqlClient
{
    public abstract class FailoverDbConnection : DbConnection
    {
#pragma warning disable SA1306 // Field names should begin with lower-case letter
#pragma warning disable SA1401 // Fields should be private
        protected DbConnection Connection;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1306 // Field names should begin with lower-case letter

        private readonly ILogger _logger;
        private readonly Guid _correlationId;
        private readonly ISyncPolicy _syncPolicy;
        private readonly IAsyncPolicy _asyncPolicy;

        private bool _disposed;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        protected FailoverDbConnection(
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            Guid correlationId,
            ISyncPolicy syncPolicy,
            IAsyncPolicy asyncPolicy,
            ILogger logger)
        {
            _correlationId = correlationId;
            _syncPolicy = syncPolicy ?? throw new ArgumentNullException(nameof(syncPolicy));
            _asyncPolicy = asyncPolicy ?? throw new ArgumentNullException(nameof(asyncPolicy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        ~FailoverDbConnection() => Dispose(false);

        public override string ConnectionString
        {
            get => Connection.ConnectionString;
            set => Connection.ConnectionString = value;
        }

        public override string Database => Connection.Database;

        public override string DataSource => Connection.DataSource;

        public override string ServerVersion => Connection.ServerVersion;

        public override ConnectionState State => Connection.State;

        public override void ChangeDatabase(string databaseName)
        {
            _logger.LogDebug("Changing database to {databaseName} - CorrelationId: {correlationId}", databaseName, _correlationId);
            Connection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            if (Connection.State == ConnectionState.Open)
            {
                _logger.LogDebug("Closing database connection to {connectionString} - CorrelationId.", Connection.ConnectionString, _correlationId);
                Connection.Close();
            }
        }

        public override void Open()
        {
            _syncPolicy.Execute(() =>
            {
                _logger.LogDebug("Opening database connection to {connectionString}. - CorrelationId.", Connection.ConnectionString, _correlationId);

                if (Connection.State != ConnectionState.Closed && Connection.State != ConnectionState.Open)
                {
                    Connection.Close();
                }

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
            });
        }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return _asyncPolicy.ExecuteAsync(
               async (token) =>
               {
                   if (Connection.State != ConnectionState.Closed && Connection.State != ConnectionState.Open)
                   {
#if NETSTANDARD2_0
                       Connection.Close();
#elif NETSTANARD2_1
                       await _connection.CloseAsync().ConfigureAwait(false);
#endif
                   }

                   if (Connection.State == ConnectionState.Closed)
                   {
                       await Connection.OpenAsync(token).ConfigureAwait(false);
                   }
               },
               cancellationToken);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            _logger.LogDebug("Beginning database transaction with IsolationLevel = {isolationLevel}.", isolationLevel, _correlationId);
            return Connection.BeginTransaction();
        }

        protected override DbCommand CreateDbCommand()
        {
            _logger.LogDebug("Creating database command - CorrelationId: {correlationId}", _correlationId);
            return new FailoverDbCommand(_correlationId, Connection.CreateCommand(), _syncPolicy, _asyncPolicy, _logger);
        }

        protected override void Dispose(bool Disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (Disposing)
            {
                // No managed resources to release.
            }

            // Release unmanaged resources.
            Connection?.Dispose();

            // Do not release logger.  Its lifetime is controlled by caller.
            _disposed = true;
        }
    }
}
