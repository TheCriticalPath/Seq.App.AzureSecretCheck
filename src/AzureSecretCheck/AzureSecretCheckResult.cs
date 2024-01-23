using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Seq.App.AzureSecretCheck
{
    public class AzureSecretCheckResultKey
    {
        public string DisplayName { get; }
        public DateTimeOffset? StartDateTime { get; }
        public DateTimeOffset? EndDateTime { get; }
        public int ExpirationDays { get; }
        public AzureSecretCheckResultKey(string displayName, DateTimeOffset? startDateTime, DateTimeOffset? endDateTime)
        {
            DisplayName = displayName;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            ExpirationDays = EndDateTime.HasValue ? Convert.ToInt32(Math.Floor((EndDateTime.Value - DateTime.UtcNow).TotalDays)) : 9999;
        }
    }
    public class AzureSecretCheckResultPassword
    {
        public string DisplayName { get; }
        public DateTimeOffset? StartDateTime { get; }
        public DateTimeOffset? EndDateTime { get; }
        public string Hint { get; }
        public string KeyId { get; }
        public int ExpirationDays { get; }
        public AzureSecretCheckResultPassword(string displayName, DateTimeOffset? startDateTime, DateTimeOffset? endDateTime, string hint, string keyId)
        {
            DisplayName = displayName;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Hint = hint;
            KeyId = keyId;
            ExpirationDays = EndDateTime.HasValue ? Convert.ToInt32(Math.Floor((EndDateTime.Value - DateTime.UtcNow).TotalDays)) : 9999;

        }
    }
    public class AzureSecretCheckResult
    {
        #region SEQ Properties
        [JsonProperty("@t")]
        public DateTime UtcTimestamp { get; }

        [JsonProperty("@mt")]
        public string MessageTemplate { get; } = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}, Certificate expires in {KeyExpirationDays} days, Secret Expires in {PasswordExpirationDays}";
        [JsonProperty("@l", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Level { get; }
        #endregion
        #region Certificate Properties

        //public int ExpirationDays => Expiration.HasValue ? Convert.ToInt32(Math.Floor((Expiration.Value - DateTime.UtcNow).TotalDays)) : -1;
        /*         
                public string Issuer { get; }
                public string Subject { get; }
                public string Thumbprint { get; }
                public string SerialNumber { get; set; }
                public IEnumerable<string> SubjectAlternativeNames { get; set; }
                public string CertificateCheckTitle { get; }
                public string TargetUrl { get; } 
        */

        #endregion
        #region Azure Application Properties
        public string ApplicationAppId { get; }
        public string ApplicationAppObjectId { get; }
        public string ApplicationDisplayName { get; }
        public string ApplicationDescription { get; }
        public string ApplicationDisabledByMicrosoftStatus { get; }
        public List<string> ApplicationTags { get; }
        public DateTimeOffset? CreatedDateTime { get; }
        public DateTimeOffset? KeyExpiration { get; }
        public DateTimeOffset? PasswordExpiration { get; }
        public int? KeyExpirationDays => KeyExpiration.HasValue ? Convert.ToInt32(Math.Floor((KeyExpiration.Value - DateTime.UtcNow).TotalDays)) : null;
        public int? PasswordExpirationDays => PasswordExpiration.HasValue ? Convert.ToInt32(Math.Floor((PasswordExpiration.Value - DateTime.UtcNow).TotalDays)) : null;
        public List<AzureSecretCheckResultKey> AzureSecretCheckResultKeys { get; }
        public List<AzureSecretCheckResultPassword> AzureSecretCheckResultPasswords { get; }
        public bool PasswordIsValid { get; }
        public bool KeyIsValid { get; }
        #endregion

        public string Outcome { get; }

        public AzureSecretCheckResult(DateTime utcTimestamp
                                    , string applicationAppId
                                    , string applicationAppObjectId
                                    , string applicationDisplayName
                                    , string applicationDescription
                                    , string applicationDisabledByMicrosoftStatus
                                    , List<string> applicationTags
                                    , DateTimeOffset? createdDateTime
                                    , DateTimeOffset? keyExpiration
                                    , DateTimeOffset? passwordExpiration
                                    , List<AzureSecretCheckResultKey> azureSecretCheckResultKeys
                                    , List<AzureSecretCheckResultPassword> azureSecretCheckResultPassword
                                    , string outcome
                                    , string level
                                    , bool keyIsValid
                                    , bool passwordIsValid)
        {
            if (utcTimestamp.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("The timestamp must be UTC.", nameof(utcTimestamp));
            }
            UtcTimestamp = utcTimestamp;
            ApplicationAppId = applicationAppId ?? throw new ArgumentNullException(nameof(applicationAppId));
            ApplicationAppObjectId = applicationAppObjectId ?? throw new ArgumentNullException(nameof(applicationAppObjectId));
            ApplicationDisplayName = applicationDisplayName ?? throw new ArgumentNullException(nameof(applicationDisplayName));
            Outcome = outcome ?? throw new ArgumentNullException(nameof(outcome));
            Level = level;
            ApplicationDescription = applicationDescription;
            ApplicationDisabledByMicrosoftStatus = applicationDisabledByMicrosoftStatus;
            ApplicationTags = applicationTags;
            CreatedDateTime = createdDateTime;
            KeyExpiration = keyExpiration;
            PasswordExpiration = passwordExpiration;
            PasswordIsValid = passwordIsValid;
            KeyIsValid = keyIsValid;
            AzureSecretCheckResultKeys = azureSecretCheckResultKeys;
            AzureSecretCheckResultPasswords = azureSecretCheckResultPassword;
            if (keyIsValid && passwordIsValid && azureSecretCheckResultKeys.Count > 0 && azureSecretCheckResultPassword.Count > 0)
            {
                MessageTemplate = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}: Certificate expires in {KeyExpirationDays} days, Secret Expires in {PasswordExpirationDays}";
            }
            else if (keyIsValid && !passwordIsValid && azureSecretCheckResultPassword.Count > 0)
            {
                MessageTemplate = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}: Secret Expires in {PasswordExpirationDays}";
            }
            else if (!keyIsValid && passwordIsValid && azureSecretCheckResultKeys.Count > 0)
            {
                MessageTemplate = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}: Certificate expires in {KeyExpirationDays} days";
            }
            else
            {
                MessageTemplate = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}: Expiration is not in current threshold";
            }
        }
    }
}