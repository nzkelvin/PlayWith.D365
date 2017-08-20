using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PlayWith.D365.WebApi
{
    public class Program
    {
        //todo: value from configure 
        //todo: impersonation
        static void Main(string[] args)
        {
            string xrmWebApiUrl = "https://fusion5d365demo1.api.crm6.dynamics.com/api/data/v8.2/";
            string clientId = "4bcd5a01-f1f9-4e06-a52a-f218f730403d";
            string username = "admin@fusion5066.onmicrosoft.com";
            SecureString pwd = new SecureString();

            // Query
            try
            {
                // Get config
                FileConfiguration config = new FileConfiguration("Xrm");
                xrmWebApiUrl = config.ServiceUrl;
                clientId = config.ClientId;
                username = config.Username;
                pwd = config.Password;

                // Query
                var dynClient = new DynamicsHttpClient(xrmWebApiUrl, username, pwd, clientId);
                dynClient.GetAccessToken();
                var resp =
                    dynClient.GetAsync("organizations?$select=_basecurrencyid_value").Result.Content
                        .ReadAsStringAsync().Result; // todo: compare FetchXML with oData
                var jResp = JObject.Parse(resp);
                
                GetBaseCurrency(dynClient);
            }
            catch (Exception e)
            {
                throw e;
            }

            Console.Read();
        }

        private static void GetBaseCurrency(HttpClient client)
        {
            var resp =
                client.GetAsync("organizations?$select=_basecurrencyid_value").Result.Content
                .ReadAsStringAsync().Result; // todo: compare FetchXML with oData
            var jResp = JObject.Parse(resp);
            var userId = (Guid)jResp.Last.First[0].Last.Values().First();

            dynamic jObj = JsonConvert.DeserializeObject(resp); // todo: compare JObject.Parse with JsonCovert.Deserialize
            Guid baseCurrencyId = (Guid)jObj.value[0]._basecurrencyid_value;
            Console.WriteLine($"Base Currency ID: {baseCurrencyId}");
        }

        private static void GetMyId(HttpClient client)
        {
            var resp = client.GetAsync("WhoAmI").Result.Content.ReadAsStringAsync().Result; // todo: this is case sensitive
            JObject jResp = JObject.Parse(resp); // todo: Compare this with Json.Convert
            Guid userId = (Guid)jResp["UserId"];

            Console.WriteLine($"User Id: {userId}");
        }

        private static void GetMyIdFromScratch()
        {
            string xrmWebApiUrl = "https://fusion5d365demo1.api.crm6.dynamics.com/api/data/v8.2/";
            string clientId = "4bcd5a01-f1f9-4e06-a52a-f218f730403d";
            string username = "admin@fusion5066.onmicrosoft.com";
            string pwd = "Mob0278013024";

            AuthenticationParameters authParams = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(xrmWebApiUrl)).Result;
            string authority = authParams.Authority; // Can also manually get from Azure Portal > App registration > EndPoints
            string resource = authParams.Resource;

            var authContext = new AuthenticationContext(authority, false);
            var userCredential = new UserPasswordCredential(username, pwd);

            var accessToken =
                (Task.Run(async () => await authContext.AcquireTokenAsync(resource, clientId, userCredential))).Result.AccessToken;
            //.GetAwaiter()
            //.GetResult();

            // Query
            try
            {
                HttpClient client = new HttpClient()
                {
                    BaseAddress = new Uri(xrmWebApiUrl),
                    Timeout = new TimeSpan(0, 2, 0)
                };
                client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Who am I
                var resp = client.GetAsync("WhoAmI").Result.Content.ReadAsStringAsync().Result; // todo: is this case sensitive?
                JObject jResp = JObject.Parse(resp); // todo: Compare this with Json.Convert
                Guid userId = (Guid)jResp["UserId"];

                Console.WriteLine($"User Id: {userId}");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
