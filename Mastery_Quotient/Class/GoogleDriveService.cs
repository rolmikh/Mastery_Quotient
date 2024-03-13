using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Mastery_Quotient.Class
{
    public class GoogleDriveService
    {

        private DriveService _driveService;


        public GoogleDriveService(string credentialsFilePath)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.Scope.Drive);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MasteryQuotient"
            });
        }

        public string GetFileUrl(string nameFile)
        {
            var request = _driveService.Files.Get(nameFile);
            var file = request.Execute();
            return file.WebViewLink;

        }
    }
}
