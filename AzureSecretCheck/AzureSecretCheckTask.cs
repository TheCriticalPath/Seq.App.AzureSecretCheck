using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Seq.Apps;

namespace SEQ.App.AzureSecretCheck
{
    internal class AzureSecretCheckTask : IDisposable
    {
        readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        readonly Task _azureSecretCheckTask;
        readonly string _appObjectId;
        public AzureSecretCheckTask(AzureSecretValidityCheck secretCheck
                                   , string appObjectId
                                   , TimeSpan interval
                                   , AzureSecretCheckReporter reporter
                                   , ILogger diagnosticLog)
        {
            if (secretCheck == null) throw new ArgumentNullException(nameof(secretCheck));
            if (reporter == null) throw new ArgumentNullException(nameof(reporter));
            _appObjectId = appObjectId;
            _azureSecretCheckTask = Task.Run(() => Run(secretCheck, appObjectId, interval, reporter, diagnosticLog, _cancel.Token), _cancel.Token);

        }

        private static async Task Run(AzureSecretValidityCheck secretCheck, string appObjectId, TimeSpan interval, AzureSecretCheckReporter reporter, ILogger diagnosticLog, CancellationToken cancel)
        {
            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    var sw = Stopwatch.StartNew();

                    var result = await secretCheck.CheckNow(cancel, diagnosticLog).ConfigureAwait(false);
                    reporter.Report(result);
                    sw.Stop();
                    var total = sw.Elapsed.TotalMilliseconds;

                    if (total < interval.TotalMilliseconds)
                    {
                        var delay = (int)(interval.TotalMilliseconds - total);
                        await Task.Delay(delay, cancel).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Unloading
            }
            catch (Exception ex)
            {
                diagnosticLog.Fatal(ex, "The health check task threw an unhandled exception");
            }

        }

        public void Dispose()
        {
            _cancel.Dispose();
            _azureSecretCheckTask.Dispose();
        }

        public void Stop()
        {
            _cancel.Cancel();
            _azureSecretCheckTask.Wait();
        }
    }
}