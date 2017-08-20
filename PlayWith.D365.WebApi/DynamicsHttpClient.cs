using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PlayWith.D365.WebApi
{
    public class DynamicsHttpClient : HttpClient
    {
        public string BaseOrganizationApiUrl { get; }
        public string UserName { get; }
        public SecureString Password { get; }
        public string ClientId { get; }
        public AuthenticationContext OAuthContext { get; set; }
        public AuthenticationParameters OAuthParams { get; set; }

        public DynamicsHttpClient(string baseOrganizationApiUrl, string username, SecureString password, string clientId = "2ad88395-b77d-4561-9441-d0e40824f9bc") // This client Id and redirectUri are hardcoded in the Plugin Registration Tool. It seems like it is set for all CRM Online tenants by default.
        {
            this.BaseOrganizationApiUrl = baseOrganizationApiUrl;
            this.UserName = username;
            this.Password = password;
            this.ClientId = clientId;

            // Set request headers
            this.Timeout = TimeSpan.FromMinutes(5);
            this.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            this.DefaultRequestHeaders.Add("OData-Version", "4.0");
            this.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            this.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.BaseAddress = new Uri(BaseOrganizationApiUrl);

            // Get OAuth Context
            OAuthParams = AuthenticationParameters
                .CreateFromResourceUrlAsync(new Uri(BaseOrganizationApiUrl)).Result;

            OAuthContext = new AuthenticationContext(OAuthParams.Authority); //todo: use cache // , new TokenCache());
        }

        public string GetAccessToken()
        {
            var task = OAuthContext.AcquireTokenAsync(OAuthParams.Resource
                , this.ClientId
                , new UserPasswordCredential(this.UserName, this.Password));

            task.Wait();

            if (task.IsCompleted)
            {
                this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", task.Result.AccessToken);
                return task.Result.AccessToken;
            }

            if (task.Exception == null)
            {
                throw new WebException("Getting OAuth access token wasn't succesful.");
            }

            throw task.Exception;
        }
    }
}
