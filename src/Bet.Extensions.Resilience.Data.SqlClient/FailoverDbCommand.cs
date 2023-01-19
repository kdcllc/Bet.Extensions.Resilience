using System.Data;
using System.Data.Common;
using System.Text;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Data.SqlClient;

public class FailoverDbCommand : DbCommand
{
    private readonly ILogger _logger;
    private readonly Guid _correlationId;
    private readonly ISyncPolicy _syncPolicy;
    private readonly IAsyncPolicy _asyncPolicy;
    private bool _disposed;
    private DbCommand _command;

    public FailoverDbCommand(
        Guid correlationId,
        DbCommand command,
        ISyncPolicy syncPolicy,
        IAsyncPolicy asyncPolicy,
        ILogger logger)
    {
        _correlationId = correlationId;
        _command = command;
        _syncPolicy = syncPolicy;
        _asyncPolicy = asyncPolicy;
        _logger = logger;
    }

    ~FailoverDbCommand() => Dispose(false);

    public override string CommandText
    {
        get => _command.CommandText;
        set => _command.CommandText = value;
    }

    public override int CommandTimeout
    {
        get => _command.CommandTimeout;
        set => _command.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => _command.CommandType;
        set => _command.CommandType = value;
    }

    public override bool DesignTimeVisible
    {
        get => _command.DesignTimeVisible;
        set => _command.DesignTimeVisible = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => _command.UpdatedRowSource;
        set => _command.UpdatedRowSource = value;
    }

    protected override DbConnection DbConnection
    {
        get => _command.Connection;
        set => _command.Connection = value;
    }

    protected override DbParameterCollection DbParameterCollection => _command.Parameters;

    protected override DbTransaction DbTransaction
    {
        get => _command.Transaction;
        set => _command.Transaction = value;
    }

    public override void Cancel()
    {
        _logger.LogInformation("Canceling database command. {correlationId}", _correlationId);
        _command.Cancel();
    }

    public override int ExecuteNonQuery()
    {
        return _syncPolicy.Execute(() =>
        {
            LogBeforeExecute();

            var result = _command.ExecuteNonQuery();

            LogAfterExecuted();

            return result;
        });
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return _asyncPolicy.ExecuteAsync(
            token =>
            {
                LogBeforeExecute();

                var result = _command.ExecuteNonQueryAsync(token);

                LogAfterExecuted();

                return result;
            }, cancellationToken);
    }

    public override object ExecuteScalar()
    {
        return _syncPolicy.Execute(
           () =>
           {
               LogBeforeExecute();
               var result = _command.ExecuteScalar();

               return result;
           });
    }

    public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return _asyncPolicy.ExecuteAsync(
            token =>
            {
                LogBeforeExecute();

                var result = _command.ExecuteScalarAsync(token);

                return result;
            }, cancellationToken);
    }

    public override void Prepare()
    {
        _logger.LogInformation("Preparing database command. {correlationId}", _correlationId);
        _command.Prepare();
    }

    protected override DbParameter CreateDbParameter()
    {
        return _command.CreateParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        LogBeforeExecute();
        return _command.ExecuteReader(behavior);
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
        _command?.Dispose();

        // Do not release logger.  Its lifetime is controlled by caller.
        _disposed = true;
    }

    private void LogBeforeExecute()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("CorrelationId: ").Append(_correlationId).AppendLine();
            stringBuilder.Append("Database command type = ").Append(_command.CommandType).AppendLine();
            stringBuilder.Append("Database command text = ").Append(_command.CommandText).AppendLine(".");

            foreach (IDataParameter parameter in _command.Parameters)
            {
                if ((parameter.Direction == ParameterDirection.Output)
                    || (parameter.Direction == ParameterDirection.ReturnValue))
                {
                    continue;
                }

                stringBuilder.Append("Database command parameter ").Append(parameter.ParameterName).Append(" = ").Append(parameter.Value).AppendLine(".");
            }

            _logger.LogDebug(stringBuilder.ToString());
        }
    }

    private void LogAfterExecuted()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("CorrelationId: ").Append(_correlationId).AppendLine();

            foreach (IDataParameter parameter in _command.Parameters)
            {
                if (parameter.Direction == ParameterDirection.Input)
                {
                    continue;
                }

                stringBuilder.Append("Database command parameter ").Append(parameter.ParameterName).Append(" = ").Append(parameter.Value).AppendLine(".");
            }

            _logger.LogDebug(stringBuilder.ToString());
        }
    }
}
