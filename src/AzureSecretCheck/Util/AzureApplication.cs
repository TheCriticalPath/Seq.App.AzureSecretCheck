using System;
using System.Text;
using Microsoft.Graph;
using Microsoft.Graph.Models;
namespace Seq.App.AzureSecretCheck
{
    public class AzureApplication
    {
        private Microsoft.Graph.Models.Application _application;
        public string AppId { get { return _application.AppId; } }
        /// <summary>
        /// Corresponds to id from the Application Object.
        /// </summary>
        public string AppObjectId { get { return _application.Id; } }
        public DateTimeOffset? CreatedDateTime { get { return _application.CreatedDateTime; } }
        public string Description { get { return _application.Description; } }
        public string DisabledByMicrosoftStatus { get { return _application.DisabledByMicrosoftStatus; } }
        public string DisplayName { get { return _application.DisplayName; } }
        public List<KeyCredential> KeyCredentials { get { return _application.KeyCredentials; } }
        public List<PasswordCredential> PasswordCredentials { get { return _application.PasswordCredentials; } }
        public List<string> Tags { get { return _application.Tags; } }

        public string MaxPasswordExpirationString
        {
            get
            {
                double? maxDays = double.MinValue;
                string retVal = string.Empty;
                foreach (var cred in this.PasswordCredentials)
                {
                    if (cred.ExpiresIn() > maxDays)
                    {
                        retVal = $" [Hint: {cred.Hint}...] expires in: {Math.Floor((double)cred.ExpiresIn())} days ";
                        maxDays = cred.ExpiresIn();
                    }
                }
                return retVal;
            }

        }
        public string MaxCertificateExpirationString
        {
            get
            {
                double? maxDays = double.MinValue;
                string retVal = string.Empty;
                foreach (var cred in this.KeyCredentials)
                {
                    if (cred.ExpiresIn() > maxDays)
                    {
                        retVal = $" [Hint: {cred.Usage}...] expires in: {Math.Floor((double)cred.ExpiresIn())} days ";
                        maxDays = cred.ExpiresIn();
                    }
                }
                return retVal;
            }

        }

       
        public async Task<List<AzureSecretCheckResultPassword>> GetAllPasswords(){
            List<AzureSecretCheckResultPassword> retVal = new List<AzureSecretCheckResultPassword>();
            foreach(var cred in PasswordCredentials){
                retVal.Add(new AzureSecretCheckResultPassword(cred.DisplayName, cred.StartDateTime, cred.EndDateTime, cred.Hint, cred.KeyId.ToString()));
            }
            return retVal;
        }
        public async Task<List<AzureSecretCheckResultKey>> GetAllKeys(){
            List<AzureSecretCheckResultKey> retVal = new List<AzureSecretCheckResultKey>();
            foreach(var cred in KeyCredentials){
                retVal.Add(new AzureSecretCheckResultKey(cred.DisplayName, cred.StartDateTime, cred.EndDateTime));
            }
            return retVal;
        }
        
        public async Task<DateTimeOffset?> GetKeyExpiration(int index)
        {
            return  KeyCredentials[index].EndDateTime;
        }
        public async Task<DateTimeOffset?> GetPasswordExpiration(int index)
        {
            return PasswordCredentials[index].EndDateTime;
        }

        public bool HasValidPassword
        {
            get {   return this.PasswordCredentials.Count == 0 || this.PasswordCredentials.HasValidPassword(); }
        }
        public bool HasValidKey
        {
            get {   return this.KeyCredentials.Count == 0 || this.KeyCredentials.HasValidKey(); }
        }

        public AzureApplication(Microsoft.Graph.Models.Application application)
        {
            _application = application;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Display Name: {this.DisplayName}");
            stringBuilder.AppendLine($"App Id      : {this.AppId}");
            stringBuilder.AppendLine($"App Obj ID  : {this.AppObjectId}");
            stringBuilder.AppendLine($"Description : {this.Description}");
            stringBuilder.AppendLine($"Created Date: {this.CreatedDateTime}");
            stringBuilder.AppendLine($"Disabled    : {this.DisabledByMicrosoftStatus}");
            stringBuilder.AppendLine($"Tag         :");
            foreach (var tag in Tags)
            {
                stringBuilder.AppendLine($" - {tag}");
            }
            stringBuilder.AppendLine($"Password Cnt : {this.PasswordCredentials.Count}");
            stringBuilder.AppendLine($"Has Valid Pwd: {this.HasValidPassword}.  Expires in {this.MaxPasswordExpirationString}");
            foreach (var cred in this.PasswordCredentials)
            {
                stringBuilder.AppendLine($"   Expires in: {cred.ExpiresIn()} days [Hint: {cred.Hint}...]");
            }
            stringBuilder.AppendLine($"Certif. Cnt  : {this.KeyCredentials.Count}");
            stringBuilder.AppendLine($"Has Valid Crt: {this.HasValidKey}");
            foreach (var cred in this.KeyCredentials)
            {
                stringBuilder.AppendLine($"   Expires in: {cred.ExpiresIn()} days ");
            }
            return stringBuilder.ToString();
        }
    }
}