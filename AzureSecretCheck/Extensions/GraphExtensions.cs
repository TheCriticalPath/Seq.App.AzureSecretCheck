using Microsoft.Graph;
using Microsoft.Graph.Models;
namespace SEQ.App.AzureSecretCheck
{
    public static class GraphExtensions
    {
        public static bool HasValidPassword(this List<PasswordCredential> credentials)
        {
            bool retVal = false;
            var now = DateTime.UtcNow;

            foreach (var cred in credentials)
            {
                if (cred.EndDateTime > now)
                {
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }
    
            public static bool HasValidKey(this List<KeyCredential> credentials)
        {
            bool retVal = false;
            var now = DateTime.UtcNow;
            
            foreach (var cred in credentials)
            {
                if (cred.EndDateTime > now)
                {
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }
        public static double? ExpiresIn(this KeyCredential credential){
             var now = DateTime.UtcNow;
            return (credential.EndDateTime - now)?.TotalDays;
        }
        public static double? ExpiresIn(this PasswordCredential credential){
             var now = DateTime.UtcNow;
            return (credential.EndDateTime - now)?.TotalDays;
        }
    }
}