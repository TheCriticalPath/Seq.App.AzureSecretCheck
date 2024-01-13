namespace SEQ.App.AzureSecretCheck
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string ClientSecret { get; set; }
        public string[] GraphScopes { get; set; }

    }
}