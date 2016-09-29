using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstConverse.Shared.Models
{
    public class ResponseStatus
    {
        public string ErrorMessage { get; set; }
        public System.Net.HttpStatusCode ErrorCode { get; set; }
    }
    public class AuthResponse
    {
        public User UserInfo { get; set; }
        public ResponseStatus ErrorResponse { get; set; }
    }
    public class User
    {
        string _accessToken;
        [JsonProperty("access_token")]
        public string AccessToken { get { return this.TokenType + " " + _accessToken; } set { _accessToken = value; } }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("expires_in")]
        public long ValidityInSeconds { get; set; }
    }
}
