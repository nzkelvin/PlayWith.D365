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
using System.Web;

namespace PlayWith.D365.WebApi
{
    public class Program
    {
        //todo: value from configure 
        //todo: impersonation
        //todo: call WF and actions
        //todo: send business requests, like create sales order from opprotunity.
        //todo: upload attachment
        static void Main(string[] args)
        {
            // Query
            try
            {
                // Get config values
                FileConfiguration config = new FileConfiguration("Xrm");
                string xrmWebApiUrl = config.ServiceUrl;
                string clientId = config.ClientId;
                string username = config.Username;
                SecureString pwd = config.Password;

                // Prep http client
                var dynClient = new DynamicsHttpClient(xrmWebApiUrl, username, pwd, clientId);
                dynClient.GetAccessToken();

                // Do somethine here
                //GetMyIdFromScratch(xrmWebApiUrl, clientId, username, pwd);
                //GetBaseCurrency(dynClient);
                //CreatContact(dynClient);
            }
            catch (Exception e)
            {
                throw e;
            }

            Console.Read();
        }

        public static void CreatContact(DynamicsHttpClient dynClient)
        {
            JObject contact = new JObject();
            contact.Add("firstname", "Sean");
            contact.Add("lastname", "Pean");

            HttpRequestMessage createMsg =
                new HttpRequestMessage(HttpMethod.Post, dynClient.BaseOrganizationApiUrl + "contacts")
                {
                    Content = new StringContent(contact.ToString(), Encoding.UTF8, "application/json")
                };

            Task<HttpResponseMessage> resp = dynClient.SendAsync(createMsg);
            resp.Wait();

            if (resp.IsCompleted && resp.Result.IsSuccessStatusCode)
            {
                string entityUri = resp.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
            }

            Task<string> message = resp.Result.Content.ReadAsStringAsync();
            message.Wait();
            throw new HttpException($"Operation failed: {resp.Result.ReasonPhrase}, HTTP Response: {message.Result}");
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

        private static void GetMyIdFromScratch(string xrmWebApiUrl, string clientId, string username, SecureString pwd)
        {
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
