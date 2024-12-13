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
        public string Description {get;}
        public AzureSecretCheckResultKey(string displayName, string description, DateTimeOffset? startDateTime, DateTimeOffset? endDateTime)
        {
            this.Description = description;
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
        public string Description {get;}
        public AzureSecretCheckResultPassword(string description, string displayName, DateTimeOffset? startDateTime, DateTimeOffset? endDateTime, string hint, string keyId)
        {
            this.Description = description;
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
        public string MessageTemplate { get; } = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}: {ObjectType} named {Description} expires in {ExpirationDays} days";
        
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
        public DateTimeOffset? ExpirationDate { get; }
        public int? ExpirationDays => ExpirationDate.HasValue ? Convert.ToInt32(Math.Floor((ExpirationDate.Value - DateTime.UtcNow).TotalDays)) : null;
        public string Description { get; }
        public string ObjectType { get; }
        public bool ObjectIsValid { get; }
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
                                    , DateTimeOffset? expirationDate
                                    , string description
                                    , string outcome
                                    , string level
                                    , string objectType
                                    , bool objectIsValid
                                    )
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
            this.ExpirationDate = expirationDate;
            this.ObjectIsValid = objectIsValid;
            this.ObjectType = objectType;
            this.Description = description;
            //MessageTemplate = "App {ApplicationDisplayName} ({ApplicationAppId}) {Outcome}: {ObjectType} named {Description} expires in {ExpirationDays}";
            //MessageTemplate = "App {"
        }
    }
}