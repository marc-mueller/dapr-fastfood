using System.CommandLine;
using FinanceService.Db.Cli.DbSchema;

namespace FinanceService.Db.Cli.Commands;

internal static class DeployDbSchemaCommand
{
    public static Command Create()
    {
        var command = new Command("deploy", "Deploy the database schema with DacPac");
        
        var connectionStringOption = new Option<string?>("--connectionstring", "-c")
        {
            Description = "Connection String"
        };
        
        var serverOption = new Option<string?>("--server", "-s")
        {
            Description = "Database Server"
        };
        
        var databaseOption = new Option<string?>("--database", "-d")
        {
            Description = "Database"
        };
        
        var userOption = new Option<string?>("--user", "-u")
        {
            Description = "User"
        };
        
        var passwordOption = new Option<string?>("--password", "-p")
        {
            Description = "Password"
        };
        
        var dacpacFileOption = new Option<string>("--dacpacfile", "-f")
        {
            Description = "DacPac File"
        };
        dacpacFileOption.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<string>() == null)
            {
                result.AddError("--dacpacfile is required");
            }
        });

        command.Options.Add(connectionStringOption);
        command.Options.Add(serverOption);
        command.Options.Add(databaseOption);
        command.Options.Add(userOption);
        command.Options.Add(passwordOption);
        command.Options.Add(dacpacFileOption);

        command.SetAction(async parseResult =>
        {
            var connectionString = parseResult.GetValue(connectionStringOption);
            var server = parseResult.GetValue(serverOption);
            var database = parseResult.GetValue(databaseOption);
            var user = parseResult.GetValue(userOption);
            var password = parseResult.GetValue(passwordOption);
            var dacpacFile = parseResult.GetValue(dacpacFileOption);
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                var result = await HandleDbDeployment(connectionString!, dacpacFile!);
                Environment.ExitCode = result;
            }
            else if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(database))
            {
                var result = await HandleDbDeployment(server!, database!, user, password, dacpacFile!);
                Environment.ExitCode = result;
            }
            else
            {
                Console.WriteLine("Error: Either provide --connectionstring or both --server and --database");
                Environment.ExitCode = 1;
            }
        });
        
        return command;
    }

    private static async Task<int> HandleDbDeployment(string server, string database, string? user, string? password, string dacpacFile)
    {
        var deployer = new DbSchemaDeployer();
        var result = await deployer.DeployDatabase(server, database, user ?? string.Empty, password ?? string.Empty, dacpacFile).ConfigureAwait(false);
        return result ? 0 : 1;
    }

    private static async Task<int> HandleDbDeployment(string connectionString, string dacpacFile)
    {
        var deployer = new DbSchemaDeployer();
        var result = await deployer.DeployDatabase(connectionString, dacpacFile).ConfigureAwait(false);
        return result ? 0 : 1;
    }
}