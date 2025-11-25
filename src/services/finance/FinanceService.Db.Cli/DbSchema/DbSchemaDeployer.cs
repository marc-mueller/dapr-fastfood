using FinanceService.Db.Cli.DbAccess;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace FinanceService.Db.Cli.DbSchema;

internal class DbSchemaDeployer
{
        private DacOperationStatus operationStatus;

       
        public Task<bool> DeployDatabase(string server, string database, string user, string password, string dacpacFile)
        {
            var csBuilder = new SqlConnectionStringBuilder();
            csBuilder.DataSource = server;
            csBuilder.InitialCatalog = database;
            if (!string.IsNullOrWhiteSpace(user))
            {
                csBuilder.UserID = user;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                csBuilder.Password = password;
            }

            return DeployDatabase(csBuilder.ConnectionString, dacpacFile);
        }

        public async Task<bool> DeployDatabase(string connectionString, string dacpacFile)
        {
            var dacService = NeedsAccessToken(connectionString) ? new DacServices(connectionString, new AzureAccessTokenProvider()) : new DacServices(connectionString);
            dacService.Message += MessageHandler;
            dacService.ProgressChanged += ProgressChangedHandler;
            dacService.Publish(DacPackage.Load(dacpacFile), (new SqlConnectionStringBuilder(connectionString)).InitialCatalog, new PublishOptions() { DeployOptions = new DacDeployOptions() { BlockOnPossibleDataLoss = false, GenerateSmartDefaults = true } });

            while (operationStatus == DacOperationStatus.Pending || operationStatus == DacOperationStatus.Running)
            {
                await Task.Delay(500).ConfigureAwait(false);
            }

            return operationStatus == DacOperationStatus.Completed;
        }

        private void ProgressChangedHandler(object sender, DacProgressEventArgs e)
        {
            operationStatus = e.Status;
        }

        private void MessageHandler(object sender, DacMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private bool NeedsAccessToken(string connectionString)
        {
            if (!(connectionString.Contains("Trusted_Connection=True", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("Integrated Security=True", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("Integrated Security=SSPI", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("UserID", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("User ID", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("UID", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                  connectionString.Contains("PWD", StringComparison.OrdinalIgnoreCase)
                ))
            {
                return true;
            }
            return false;
        }
    }