using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Seq.Apps;

namespace SEQ.App.AzureSecretCheck

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

        public async Task<AzureSecretCheckResult> CheckNow(CancellationToken cancel, ILogger diagnosticLog)
        {

            string outcome;
            var utcTimestamp = DateTime.UtcNow;
            AzureApplication azureApplication = null;
            AzureSecretCheckResult azureSecretCheckResult = null;
            AzureSecretCheckResultKey azureSecretCheckKeyResult = null;
            AzureSecretCheckResultPassword azureSecretCheckPasswordResult = null;
            DateTimeOffset? keyExpiration = null;
            DateTimeOffset? passwordExpiration = null;
            List<AzureSecretCheckResultPassword> azureSecretCheckResultPasswords = null;
            List<AzureSecretCheckResultKey> azureSecretCheckResultKeys = null;
            CertificateInformation certificateInformation = null;
            bool keyIsValid = true;
            bool passwordIsValid = true;
            try
            {
                GraphHelper.InitializeGraphForUserAuth(_settings);
                azureApplication = await GraphHelper.GetAzureApplication(_appObjectId);
                keyExpiration = await azureApplication.GetKeyExpiration();
                passwordExpiration = await azureApplication.GetPasswordExpiration();
                azureSecretCheckResultKeys = await azureApplication.GetKeys();
                azureSecretCheckResultPasswords = await azureApplication.GetPasswords();
                if ((!(keyExpiration.HasValue) && azureApplication.HasValidKey) ||
                   (keyExpiration.HasValue && keyExpiration > DateTime.UtcNow.AddDays(_validityDays) && azureApplication.HasValidKey))
                {
                    keyIsValid = true;
                }
                else
                {
                    keyIsValid = false;
                }
                if ((!(passwordExpiration.HasValue) && azureApplication.HasValidPassword) ||
                   (passwordExpiration.HasValue && passwordExpiration > DateTime.UtcNow.AddDays(_validityDays) && azureApplication.HasValidPassword))
                {
                    keyIsValid = true;
                }
                else
                {
                    keyIsValid = false;
                }
                bool valid = keyIsValid && passwordIsValid;

                outcome = valid ? OutcomeSucceeded : OutcomeFailed;
            }
            catch (Exception exception)
            {
                diagnosticLog.Error(exception, "Something went wrong while checking secrets for {AppObjectId}", _appObjectId);
                outcome = OutcomeFailed;
            }

            var level = outcome == OutcomeFailed ? "Error" : null;

            return new AzureSecretCheckResult(utcTimestamp
                                 , azureApplication.AppId
                                 , azureApplication.AppObjectId
                                 , azureApplication.DisplayName
                                 , azureApplication.Description
                                 , azureApplication.DisabledByMicrosoftStatus
                                 , azureApplication.Tags
                                 , azureApplication.CreatedDateTime
                                 , keyExpiration
                                 , passwordExpiration
                                 , azureSecretCheckResultKeys
                                 , azureSecretCheckResultPasswords
                                 , outcome
                                 , level
                                 );


        }
    }
}