using Firebase.Storage;

namespace Mastery_Quotient.Service
{
    public class FirebaseService
    {

        private readonly FirebaseStorage _storage;

        public FirebaseService(string firebaseApiKey)
        {
            _storage = new FirebaseStorage("", new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(firebaseApiKey)
            });
        }

        //public async Task<byte[]> DownloadFileAsync(string fileUrl)
        //{
        //    using (var stream = await _storage.Child(fileUrl).GetDownloadUrlAsync())
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await stream.CopyToAsync(memoryStream);
        //            return memoryStream.ToArray();
        //        }
        //    }
        //}
    }
}
