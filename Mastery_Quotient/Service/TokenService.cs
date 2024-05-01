using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace Mastery_Quotient.Service
{
    public class TokenService
    {
        public static string token {  get; set; }

        public static string role { get; set; }


        public bool IsTokenExpired(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }

        public async Task<string> RefreshToken(string access_token, string role)
        {
            var client = new HttpClient();
            var apiUrl = "https://localhost:7284/api/";
            var request = new HttpRequestMessage();
            if (role == "Студент")
            {
                request = new HttpRequestMessage(HttpMethod.Post, apiUrl + "Students/refresh_token?access_token=" + access_token);
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Post, apiUrl + "Employees/refresh_token?access_token=" + access_token);

            }
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(responseContent);
                return (string)responseJson["access_token"];
            }
            return null;


        }
    }
}
