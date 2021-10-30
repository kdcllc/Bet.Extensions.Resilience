#if NETSTANDARD2_0
using System.Data.SqlClient;

#else
using Microsoft.Data.SqlClient;

#endif
using Polly;

namespace Bet.Extensions.Resilience.Data.SqlClient
{
    public static class SqlPolicyExtensions
    {
        private static readonly Func<SqlException, bool> TransientSqlErrorsPredicate = (ex) =>
        {
            var codes = new int[]
            {
                (int)SqlHandledExceptions.DatabaseNotCurrentlyAvailable,
                (int)SqlHandledExceptions.ErrorProcessingRequest,
                (int)SqlHandledExceptions.ServiceCurrentlyBusy,
                (int)SqlHandledExceptions.NotEnoughResources
            };

            return codes.Contains(ex.Number);
        };

        private static readonly Func<SqlException, bool> TransactionSqlErrorsPredicate = (ex) =>
        {
            var codes = new int[]
            {
                (int)SqlHandledExceptions.SessionTerminatedLongTransaction,
                (int)SqlHandledExceptions.SessionTerminatedToManyLocks
            };

            return codes.Contains(ex.Number);
        };

        public static PolicyBuilder<TResult> HandleTransientSqlErrors<TResult>()
        {
            return Policy<TResult>.Handle<SqlException>(ex => TransientSqlErrorsPredicate(ex));
        }

        public static PolicyBuilder<TResult> HandleTransientSqlErrors<TResult>(SqlHandledExceptions exception)
        {
            return Policy<TResult>.Handle<SqlException>(ex => ex.Number == (int)exception);
        }

        public static PolicyBuilder HandleTransientSqlErrors()
        {
            return Policy.Handle<SqlException>(ex => TransientSqlErrorsPredicate(ex));
        }

        public static PolicyBuilder HandleTransientSqlErrors(SqlHandledExceptions exception)
        {
            return Policy.Handle<SqlException>(ex => ex.Number == (int)exception);
        }

        public static PolicyBuilder<TResult> HandleTransactionSqlErrors<TResult>()
        {
            return Policy<TResult>.Handle<SqlException>(ex => TransactionSqlErrorsPredicate(ex));
        }

        public static PolicyBuilder HandleTransactionSqlErrors()
        {
            return Policy.Handle<SqlException>(ex => TransactionSqlErrorsPredicate(ex));
        }
    }
}
