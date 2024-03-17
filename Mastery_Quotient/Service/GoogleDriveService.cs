using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mastery_Quotient.Class
{
    public class GoogleDriveService
    {

        //private DriveService _driveService;

        //public async Task InitializeDriveServiceAsync(string email, string password)
        //{
        //    var accessToken = await GetGoogleDriveAccessToken(email, password);

        //    var credential = GoogleCredential.FromAccessToken(accessToken);

        //    _driveService = new DriveService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = "MasteryQuotient"
        //    });
        //}

        //private async Task<string> GetGoogleDriveAccessToken(string email, string password)
        //{
        //    // Запрос на получение токена доступа к Google Drive
        //    var tokenEndpoint = "https://oauth2.googleapis.com/token";

        //    var clientId = "376246642795-vhl3460vbdu9crnar0rk92oe4e3ebtkd.apps.googleusercontent.com";
        //    var clientSecret = "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";
        //    var scope = "https://www.googleapis.com/auth/drive";

        //    using (var httpClient = new HttpClient())
        //    {
        //        var parameters = new Dictionary<string, string>
        //        {
        //            { "client_id", clientId },
        //            { "client_secret", clientSecret },
        //            { "grant_type", "password" },
        //            { "username", email },
        //            { "password", password },
        //            { "scope", scope }
        //        };

        //        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var responseData = JsonConvert.DeserializeObject<JObject>(responseContent);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            return responseData.Value<string>("access_token");
        //        }
        //        else
        //        {
        //            throw new Exception($"Failed to get access token: {responseData.Value<string>("error_description")}");
        //        }
        //    }
        //}

        

        //public GoogleDriveService(string clientId, string clientSecret)
        //{
        //    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //        new ClientSecrets
        //        {
        //            ClientId = clientId,
        //            ClientSecret = clientSecret
        //        },
        //        new[]
        //        {
        //            DriveService.Scope.Drive
        //        },
        //        "user",
        //        CancellationToken.None).Result;

        //    _driveService = new DriveService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = "MasteryQuotient"
        //    });
        //}

        //public string GetFileUrl(string nameFile)
        //{
        //    var request = _driveService.Files.Get(nameFile);
        //    var file = request.Execute();
        //    return file.WebViewLink;

        //}

    }
}
