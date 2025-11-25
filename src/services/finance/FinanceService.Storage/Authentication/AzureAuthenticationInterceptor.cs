using System.Data.Common;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceService.Storage.Authentication;

public class AzureAuthenticationInterceptor : DbConnectionInterceptor
    {
        private const string AzureDatabaseResourceIdentifier = "https://database.windows.net/.default";
        private readonly TokenCredential _tokenCredential;

        public AzureAuthenticationInterceptor()
        {
            _tokenCredential = new DefaultAzureCredential();
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            await AddAccessToken(connection, cancellationToken).ConfigureAwait(false);
            return result;
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            return ConnectionOpeningAsync(connection, eventData, result).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task AddAccessToken(DbConnection connection, CancellationToken cancellationToken)
        {
            if (connection is SqlConnection sqlConnection && !ContainsSensitiveInfo(sqlConnection.ConnectionString))
            {
                var accessToken = await _tokenCredential.GetTokenAsync(
                    new TokenRequestContext(new[] { AzureDatabaseResourceIdentifier }),
                    cancellationToken).ConfigureAwait(false);

                sqlConnection.AccessToken = accessToken.Token;
            }
        }

        private static bool ContainsSensitiveInfo(string connectionString)
        {
            return connectionString.Contains("Trusted_Connection=True", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("Integrated Security=True", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("Integrated Security=SSPI", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("UserID", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("User ID", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("UID", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("PWD", StringComparison.OrdinalIgnoreCase);
        }
    }