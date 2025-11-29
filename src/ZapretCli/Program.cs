using System.CommandLine;
using ZapretCli;
using ZapretCli.Configuration;
using ZapretCli.Models;
using ZapretCli.Services;

var baseDirectory = AppContext.BaseDirectory;
var app = new CliApplication(
    new ConfigLoader(baseDirectory),
    new ZapretProcessRunner(),
    new StrategyRepository(),
    new FileLastStateStore(baseDirectory),
    new JsonPrinter());

var rootCommand = app.BuildRootCommand();
return await rootCommand.InvokeAsync(args);
