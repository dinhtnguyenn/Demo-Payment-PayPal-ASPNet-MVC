namespace WebApplication2.helper
{
    using PayPal.Api;
    using System.Collections.Generic;


    public static class Configuration
    {

        public readonly static string ClientId;

        public readonly static string ClientSecret;

        static Configuration()
        {
            var config = GetConfig();
            ClientId = config["clientId"];
            ClientSecret = config["clientSecret"];
        }

        public static Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }

        private static string GetAccessToken()
        {
            string accessToken = new OAuthTokenCredential(ClientId, ClientSecret,
           GetConfig()).GetAccessToken();
            return accessToken;
        }

     
        public static APIContext GetAPIContext()
        {
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}

