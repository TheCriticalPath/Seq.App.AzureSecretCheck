using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SEQ.App.AzureSecretCheck
{
    public class AzureSecretCheckResultKey
    {
        public string DisplayName { get; }
        public DateTimeOffset? StartDateTime { get; }
        public DateTimeOffset? EndDateTime { get; }
        public AzureSecretCheckResultKey(string displayName, DateTimeOffset? startDateTime, DateTimeOffset? endDateTime)
        {
            DisplayName = displayName;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }
    }
    public class AzureSecretCheckResultPassword
    {
        public string DisplayName { get; }
        public DateTimeOffset? StartDateTime { get; }
        public DateTimeOffset? EndDateTime { get; }
        public string Hint { get; }
        public string KeyId { get; }
        public AzureSecretCheckResultPassword(string displayName, DateTimeOffset? startDateTime, DateTimeOffset? endDateTime, string hint, string keyId)
        {
            DisplayName = displayName;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Hint = hint;
            KeyId = keyId;
        }
    }
    public class AzureSecretCheckResult
    {
        #region SEQ Properties
        [JsonProperty("@t")]
        public DateTime UtcTimestamp { get; }

        [JsonProperty("@mt")]
        public string MessageTemplate { get; } =
            "App {ApplicationAppDisplayName} ({ApApplicationAppId}) {Outcome}, expires in {ExpirationDays} days";
        [JsonProperty("@l", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Level { get; }
        #endregion
        #region Certificate Properties
        public DateTime? Expiration { get; set; }
        //public int ExpirationDays => Expiration.HasValue ? Convert.ToInt32(Math.Floor((Expiration.Value - DateTime.UtcNow).TotalDays)) : -1;
        public string Issuer { get; }
        public string Subject { get; }
        public string Thumbprint { get; }
        public string SerialNumber { get; set; }
        public IEnumerable<string> SubjectAlternativeNames { get; set; }
        public string CertificateCheckTitle { get; }
        public string TargetUrl { get; }

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
        public int KeyExpirationDays => KeyExpiration.HasValue ? Convert.ToInt32(Math.Floor((KeyExpiration.Value - DateTime.UtcNow).TotalDays)) : -9999;
        public int PasswordExpirationDays => PasswordExpiration.HasValue ? Convert.ToInt32(Math.Floor((PasswordExpiration.Value - DateTime.UtcNow).TotalDays)) : -9999;
        public int ExpirationDays => Math.Abs(KeyExpirationDays) > Math.Abs(PasswordExpirationDays) ? PasswordExpirationDays : KeyExpirationDays;
        public List<AzureSecretCheckResultKey> AzureSecretCheckResultKeys { get; }
        public List<AzureSecretCheckResultPassword> AzureSecretCheckResultPasswords { get; }

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
                                    , string level)
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
            AzureSecretCheckResultKeys = azureSecretCheckResultKeys;
            AzureSecretCheckResultPasswords = azureSecretCheckResultPassword;
        }
    }
}