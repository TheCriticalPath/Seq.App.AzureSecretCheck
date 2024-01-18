using SEQ.App.AzureSecretCheck;
using Seq.Apps;
using Serilog;
using Serilog.Events;
using System.IO.Pipes;
using Newtonsoft.Json.Linq;
using Serilog.Formatting.Compact.Reader;
using Microsoft.Extensions.Configuration;

// See https://aka.ms/new-console-template for more information
// https://github.com/janpieterz/seq-input-certificatecheck/blob/main/Terminal/Program.cs
var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();
config.Reload();
Serilog.Core.Logger generalLog = new Serilog.LoggerConfiguration().WriteTo.Console(outputTemplate: "[MAIN][{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}").CreateLogger();
Serilog.Core.Logger selfLog = new Serilog.LoggerConfiguration().WriteTo.Console(outputTemplate: "[\x1b[43;34mSELF\x1b[0m][{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}").CreateLogger();
TestHost testHost = new TestHost { App = new App("1", "test", new Dictionary<string, string>() { }, ""), Logger = selfLog };

AnonymousPipeServerStream hostPipe = new AnonymousPipeServerStream(PipeDirection.In);
CancellationTokenSource cts = new CancellationTokenSource();
StreamReader streamLogReader = new StreamReader(hostPipe);
Task hostTask = RunSimulatedHost(testHost, config, hostPipe.GetClientHandleAsString(), cts.Token);

#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
Task<string> exitTask = Task.Run<string>(Console.In.ReadLine);
#pragma warning restore CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).

while (!cts.IsCancellationRequested)
{
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
    Task<string> newEventTask = streamLogReader.ReadLineAsync();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    Task finished = await Task.WhenAny(newEventTask, exitTask);
    if (finished == exitTask)
    {
        cts.Cancel();
    }
    else // newEventTask
    {
        try
        {
            string? sEvent = await newEventTask;
            JObject jEvent = JObject.Parse(sEvent);
            LogEvent lEvent = LogEventReader.ReadFromJObject(jEvent);
            generalLog.Write(lEvent);
        }
        catch (Exception ex)
        {
            string sEvent = "";
            try
            {
                sEvent = await newEventTask;
            }
            catch { }
            selfLog.ForContext("Event", sEvent).Fatal(ex, "HOST: Could not read event.");
        }
    }
}
await Task.WhenAll(hostTask);


async Task RunSimulatedHost(TestHost testHost, IConfigurationRoot configuration, string pipeHandle, CancellationToken cancellationToken)
{
    AnonymousPipeClientStream clientPipe = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandle);
    using (StreamWriter writer = new StreamWriter(clientPipe))
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var appObjectIds = configuration["AzureAppObjectIds"].Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        AzureSecretCheckInput runner = new AzureSecretCheckInput
        {
            AppObjectIds = string.Join(Environment.NewLine, appObjectIds),
            ClientId = configuration["AzureClientId"],
            TenantId = configuration["AzureTenantId"],
            ClientSecret = configuration["AzureClientSecret"],
            GraphScopes = configuration["GraphScopes"],
            IntervalSeconds = 5
        }; 
        

        runner.Attach(testHost);
        runner.Start(writer);
        try
        {
            await Task.Delay(-1, cancellationToken);
        }
        catch (TaskCanceledException) { }
        runner.Stop();
    }
}

public class TestHost : IAppHost
{
    public App? App { get; set; }
    public Host? Host { get; set; }
    public ILogger? Logger { get; set; }
    public string? StoragePath { get; set; }
}
