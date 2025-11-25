using Azure.Core;
using Azure.Identity;
using Microsoft.SqlServer.Dac;

namespace FinanceService.Db.Cli.DbAccess;

internal class AzureAccessTokenProvider : IUniversalAuthProvider
{
    private const string AzureDatabaseResourceIdentifier = "https://database.windows.net/.default";

    private readonly TokenCredential _tokenCredential;

    public AzureAccessTokenProvider() : this(new DefaultAzureCredential())
    {
    }

    public AzureAccessTokenProvider(string tenantId, string clientId, string clientSecret)
        : this(new ClientSecretCredential(tenantId, clientId, clientSecret))
    {
    }

    public AzureAccessTokenProvider(TokenCredential tokenCredential)
    {
        _tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
    }

    public string GetValidAccessToken()
    {
        return _tokenCredential.GetToken(new TokenRequestContext(scopes: new string[] { AzureDatabaseResourceIdentifier }) { }, CancellationToken.None).Token;
    }
}