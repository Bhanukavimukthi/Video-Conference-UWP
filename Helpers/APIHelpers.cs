using Eminutes.UWP.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Eminutes.UWP.Helpers
{
    public class APIHelper : IAPIHelper
    {
        private HttpClient apiClient;
        private string jwtToken;

        public APIHelper()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            apiClient = new HttpClient();
            apiClient.BaseAddress = new Uri("http://localhost:5000/");
            apiClient.DefaultRequestHeaders.Accept.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<AuthenticatedUser> Authenticate(string username, string password)
        {
            var myObject = new
            {
                userName = username,
                password = password,
            };

            JsonContent content = JsonContent.Create(myObject);

            using (HttpResponseMessage response = await apiClient.PostAsync("api/UserAuth", content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<AuthenticatedUser>();
                    jwtToken = result.jwtToken;
                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<List<Rootobject>> GetMeetings()
        {
            apiClient.DefaultRequestHeaders.Authorization =
  new AuthenticationHeaderValue("Bearer", jwtToken);
            using (HttpResponseMessage response = await apiClient.GetAsync("api/Temp_Conference"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var asd = JsonConvert.DeserializeObject<List<Rootobject>>(result);
                    return asd;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
