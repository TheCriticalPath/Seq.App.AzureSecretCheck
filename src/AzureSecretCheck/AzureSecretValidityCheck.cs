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
            bool objectIsValid = true;
            try
            {
                GraphHelper.InitializeGraphForUserAuth(_settings);
                azureApplication = await GraphHelper.GetAzureApplication(_appObjectId);
                var keys = await azureApplication.GetAllKeys();
                var passwords = await azureApplication.GetAllPasswords();
                // Handle Keys
                int i = 0;
                foreach (var key in keys)
                {
                    expirationDate = await azureApplication.GetKeyExpiration(i);
                    var ageInDays = key.StartDateTime.HasValue ? Convert.ToInt32(Math.Floor((DateTime.UtcNow - key.StartDateTime.Value).TotalDays)) : -1;
                    if (expirationDate.HasValue && expirationDate < DateTime.UtcNow.AddDays(_validityDays))
                    {
                        outcome = OutcomeFailed;
                        objectIsValid = false;
                    }
                    else
                    {
                        outcome = OutcomeSucceeded;
                    }
                    var level = outcome == OutcomeFailed ? "Error" : null;

                    results.Add(new AzureSecretCheckResult(utcTimestamp
                                 , azureApplication.AppId
                                 , azureApplication.AppIdUrl
                                 , azureApplication.AppObjectId
                                 , azureApplication.DisplayName
                                 , azureApplication.Description
                                 , azureApplication.DisabledByMicrosoftStatus
                                 , azureApplication.Tags
                                 , azureApplication.CreatedDateTime
                                 , expirationDate
                                 , azureApplication.Owners
                                 , key.Description
                                 , azureApplication.Notes
                                 , outcome
                                 , level
                                 , "Certificate"
                                 , objectIsValid
                                 , ageInDays
                                 ));
                    i++;
                }
                i = 0;
                foreach (var pass in passwords)
                {
                    expirationDate = await azureApplication.GetPasswordExpiration(i);
                    var ageInDays = pass.StartDateTime.HasValue ? Convert.ToInt32(Math.Floor((DateTime.UtcNow - pass.StartDateTime.Value).TotalDays)) : -1;

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
                                 , azureApplication.AppIdUrl
                                 , azureApplication.AppObjectId
                                 , azureApplication.DisplayName
                                 , azureApplication.Description
                                 , azureApplication.DisabledByMicrosoftStatus
                                 , azureApplication.Tags
                                 , azureApplication.CreatedDateTime
                                 , expirationDate
                                 , azureApplication.Owners
                                 , pass.Description
                                 , azureApplication.Notes
                                 , outcome
                                 , level
                                 , "Password"
                                 , objectIsValid
                                 , ageInDays
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