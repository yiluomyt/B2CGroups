using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PFStudio.B2C
{
    public class B2CGraphClient
    {
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string Tenant { get; set; }

        private AuthenticationContext authContext;
        private ClientCredential credential;

        public B2CGraphClient(string clientId, string clientSecret, string tenant)
        {
            this.ClientId = clientId;
            this.ClientSecret = ClientSecret;
            this.Tenant = tenant;

            // 指定所使用的目录
            this.authContext = new AuthenticationContext(Globals.aadInstance + tenant);

            // 创建客户端认证
            this.credential = new ClientCredential(clientId, clientSecret);
        }

        public async Task<IEnumerable<string>> GetMemberOf(string objectId)
        {
            string groupsAsJson = await SendGraphGetRequest($"/users/{objectId}/memberOf", string.Empty);
            var groups = JObject.Parse(groupsAsJson);

            return groups.SelectToken("value").Select(x => (string)x["displayName"]).ToList();
        }

        public async Task<string> SendGraphGetRequest(string api, string query)
        {
            // First, use ADAL to acquire a token using the app's identity (the credential)
            // The first parameter is the resource we want an access_token for; in this case, the Graph API.
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);

            // For B2C user managment, be sure to use the 1.6 Graph API version.
            HttpClient http = new HttpClient();
            string url = "https://graph.windows.net/" + Tenant + api + "?" + Globals.aadGraphVersion;
            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            // Append the access token for the Graph API to the Authorization header of the request, using the Bearer scheme.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
