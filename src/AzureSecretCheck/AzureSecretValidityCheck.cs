using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Seq.Apps;

namespace Seq.App.AzureSecretCheck

{
    public class AzureSecretValidityCheck
    {
        private readonly string _title;
        private readonly string _appObjectId;
        private readonly int _validityDays;
        private readonly Settings _settings;
        const string OutcomeSucceeded = "succeeded", OutcomeFailed = "failed";
        public AzureSecretValidityCheck(string title, string appObjectId, int validityDays, Settings settings)
        {
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _appObjectId = appObjectId ?? throw new ArgumentNullException(nameof(appObjectId));
            _validityDays = validityDays;
            _settings = settings;

        }

        public async Task<List<AzureSecretCheckResult>> CheckNow(CancellationToken cancel, ILogger diagnosticLog)
        {
            List<AzureSecretCheckResult> results = new List<AzureSecretCheckResult>();

            string outcome = string.Empty;
            var utcTimestamp = DateTime.UtcNow;
            AzureApplication azureApplication = null;
            AzureSecretCheckResult azureSecretCheckResult = null;
            DateTimeOffset? expirationDate = null;
            AzureSecretCheckResultKey azureSecretCheckKeyResult = null;
            bool keyIsValid = true;
            bool passwordIsValid = true;
            try
            {
                GraphHelper.InitializeGraphForUserAuth(_settings);
                azureApplication = await GraphHelper.GetAzureApplication(_appObjectId);
                var keys = await azureApplication.GetAllKeys();

                // Handle Keys
                int i = 0;
                foreach (var key in keys)
                {
                    expirationDate = await azureApplication.GetKeyExpiration(i);
                    if (expirationDate.HasValue && expirationDate < DateTime.UtcNow.AddDays(_validityDays))
                    {
                        outcome = OutcomeFailed;
                    }
                    else
                    {
                        outcome = OutcomeSucceeded;
                    }
                    var level = outcome == OutcomeFailed ? "Error" : null;

                    results.Add(new AzureSecretCheckResult(utcTimestamp
                                 , azureApplication.AppId
                                 , azureApplication.AppObjectId
                                 , azureApplication.DisplayName
                                 , azureApplication.Description
                                 , azureApplication.DisabledByMicrosoftStatus
                                 , azureApplication.Tags
                                 , azureApplication.CreatedDateTime
                                 , expirationDate
                                 , key.Description
                                 , outcome
                                 , level
                                 , "Certificate"
                                 ));
                    i++;
                }

            }
            catch (Exception exception)
            {
                diagnosticLog.Error(exception, "Something went wrong while checking secrets for {AppObjectId}", _appObjectId);
                outcome = OutcomeFailed;
            }


            return results;

        }
    }
}