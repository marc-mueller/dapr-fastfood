using System.CommandLine;
using FinanceService.Db.Cli.Commands;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("FinanceService Database CLI Tool");
        
        var deployCommand = DeployDbSchemaCommand.Create();
        rootCommand.Subcommands.Add(deployCommand);

        return rootCommand.Parse(args).Invoke();
    }
}