using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FirstConverse.Shared.Models;
using System.Collections.Generic;

namespace FirstConverse.Shared
{
    public class Register
    {
        string FCAPIURL = "http://fc-api.azurewebsites.net/";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }


        public async Task<string> Submit()
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            StringContent content = new StringContent(JsonConvert.SerializeObject(this), System.Text.Encoding.UTF8, "application/json");
            client.BaseAddress = new Uri(FCAPIURL);
            HttpResponseMessage response = await client.PostAsync(new Uri(FCAPIURL + "api/account/register"), content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return "Registered Successfully!!";
            else
                return response.ReasonPhrase;
        }
    }
     
    
}
