using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FirstConverse.Shared.Models;
using System.Collections.Generic;
using FirstConverse.Shared.BindingModel;

namespace FirstConverse.Shared.Services
{
    
    public static class RestClient
    {
        static string FCAPIURL = "http://fc-api.azurewebsites.net/";
        //static string FCAPIURL ="http://localhost:49824/";   
        //public RestClient()
        //{
        //    FCAPIURL = "http://fc-api.azurewebsites.net/";
        //}

        public static async Task<AuthResponse> Authenticate(string username, string password)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(FCAPIURL);
            StringContent content = new StringContent("grant_type=password&username=" + username + "&password=" + password, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.PostAsync(new Uri(FCAPIURL + "token"), content);
                if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new AuthResponse { ErrorResponse = new ResponseStatus { ErrorCode = response.StatusCode, ErrorMessage = "Unauthorized Access" } };
                }
                else if(response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new AuthResponse { ErrorResponse = new ResponseStatus { ErrorCode = response.StatusCode, ErrorMessage = response.ReasonPhrase } };
                }
                var jsonData = response.Content.ReadAsStringAsync().Result;
                User userdata = JsonConvert.DeserializeObject<User>(jsonData);
                return new AuthResponse { UserInfo = userdata, ErrorResponse = null };
            }
            catch (HttpRequestException ex) {
                //log ex here.
                return new AuthResponse { ErrorResponse = new ResponseStatus { ErrorCode = System.Net.HttpStatusCode.NoContent, ErrorMessage = "No Internet" } };
            }
            catch(Exception ex)
            {
                return new AuthResponse { ErrorResponse = new ResponseStatus { ErrorCode = System.Net.HttpStatusCode.NoContent, ErrorMessage = "General Exception" } };
            }
        }
        public static async Task<string> Logout(string token)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(FCAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", token);
            var response = await client.PostAsync(new Uri(FCAPIURL + "api/account/logout"), null);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return "ERROR";
            else
                return "DONE";
        }

        

        public static async Task<bool> RegisterForPush(string token, string registrationId)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(FCAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", token);
            //StringContent content = new StringContent("registrationId=" + registrationId, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.PostAsync(new Uri(FCAPIURL + "api/Account/RegisterDevice?registrationId=" + registrationId), null);
                return response.StatusCode == System.Net.HttpStatusCode.OK;
                //return true;
            }
            catch 
            {
                //log ex here.
                return false;
            }
        }
        public static ResponseHeadersViewModel GetConversations(string token)
        {
            try
            {
                ResponseHeadersViewModel conversations = new ResponseHeadersViewModel();
                var client = new HttpClient();
                client.BaseAddress = new Uri(FCAPIURL);
                //StringContent content = new StringContent("{Authorization:" + token+"}", System.Text.Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", token);
               // client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                var response = client.GetAsync(new Uri(FCAPIURL + "api/messages"));
                if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    var jsonData = response.Result.Content.ReadAsStringAsync().Result;
                    if (jsonData.StartsWith("["))
                    {
                        jsonData = jsonData.Substring(1).Substring(0, jsonData.Length - 2);
                    }
                    // store json data in shared preferences
                    conversations = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                    conversations.Forms.Items.Sort(delegate (MessageHeadersViewModel x, MessageHeadersViewModel y)
                    {
                    //if (x.PartName == null && y.PartName == null) return 0;
                    //else if (x.PartName == null) return -1;
                    //else if (y.PartName == null) return 1;
                    //else
                    return y.Id.CompareTo(x.Id);
                    });
                }
                return conversations;
            }
            catch (Exception ex)
            {
                //log ex here.
                return null;
            }
        }

        public static async Task<ResponseHeadersViewModel> RefreshConversationsAsync(string token)
        {
            try
            {
                ResponseHeadersViewModel conversations = new ResponseHeadersViewModel();
                var client = new HttpClient();
                client.BaseAddress = new Uri(FCAPIURL);
                //StringContent content = new StringContent("{Authorization:" + token+"}", System.Text.Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", token);
                // client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                var response = await client.GetAsync(new Uri(FCAPIURL + "api/messages"));
                if (response.StatusCode== System.Net.HttpStatusCode.OK)
                {
                    var jsonData = response.Content.ReadAsStringAsync().Result;
                    if (jsonData.StartsWith("["))
                    {
                        jsonData = jsonData.Substring(1).Substring(0, jsonData.Length - 2);
                    }
                    // TODO:   sometimes the error is thrown here.
                    // store json data in shared preferences
                    conversations = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                    conversations.Forms.Items.Sort(delegate (MessageHeadersViewModel x, MessageHeadersViewModel y)
                    {
                        //if (x.PartName == null && y.PartName == null) return 0;
                        //else if (x.PartName == null) return -1;
                        //else if (y.PartName == null) return 1;
                        //else
                        return y.Id.CompareTo(x.Id);
                    });
                }
                return conversations;
            }
            catch (Exception ex)
            {
                //log ex here.
                throw (new Exception("ERROR from API"));
            }
        }

        public static async Task<MessageDetailsResponse> GetMessageDetails(string token, int formId)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(FCAPIURL);
                //StringContent content = new StringContent("{Authorization:" + token+"}", System.Text.Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", token);
                // client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                var response = await client.GetAsync(new Uri(FCAPIURL + "api/messages/" + formId));
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                }
                var jsonData = response.Content.ReadAsStringAsync().Result;
                MessageDetailsResponse conversation = JsonConvert.DeserializeObject<MessageDetailsResponse>(jsonData);
                
                return conversation;
            }
            catch (Exception ex)
            {
                //log ex here.
                return null;
            }
            
        }

        public async static Task<string> SubmitFormResponse(string token, int messageID, FormResponseBindingModel response)
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("Authorization", token);
            response.Id = messageID;
            //string str = JsonConvert.SerializeObject(response);
            //str = str.Replace("\"DateTime\"", "\"DateTimeBox\"");
            StringContent content = new StringContent(JsonConvert.SerializeObject(response), System.Text.Encoding.UTF8, "application/json");
            client.BaseAddress = new Uri(FCAPIURL);
            HttpResponseMessage result = await client.PutAsync(new Uri(FCAPIURL + "api/messages/" + messageID + "/response"), content);
            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                return "SUCCESS";
            else
                return result.ReasonPhrase;
        }

        public async static Task<string> SubmitSurveyResponse(string token, int messageId, SurveyResponseBindingModel response)
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("Authorization", token);
            response.Id = messageId;
            StringContent content = new StringContent(JsonConvert.SerializeObject(response), System.Text.Encoding.UTF8, "application/json");
            client.BaseAddress = new Uri(FCAPIURL);
            HttpResponseMessage result = await client.PutAsync(new Uri(FCAPIURL + "api/surveys/" + messageId + "/response"), content);
            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                return "SUCCESS";
            else
                return result.ReasonPhrase;
        }
    }

}
