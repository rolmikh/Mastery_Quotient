using Firebase.Storage;
using System.Net.Sockets;

namespace Mastery_Quotient.Service
{
    public class FirebaseService
    {
        private static string Bucket = "mastquo.appspot.com";


        public async Task<string> Upload(Stream stream, string fileName)
        {
            var cancellation = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(Bucket);
            string path = "photoProfile / " + fileName;
            var uploadTask = firebaseStorage.Child(path).PutAsync(stream, cancellation.Token);
            var fileUrl = await uploadTask;

            return fileUrl;
        }

        /// <summary>
        /// Загрузка фотографии профиля в Firebase Storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> UploadPhoto(Stream stream, string fileName)
        {
            var cancellation = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(Bucket);
            string path = "photoProfile / " + fileName;
            var uploadTask = firebaseStorage.Child(path).PutAsync(stream, cancellation.Token);
            var fileUrl = await uploadTask;

            return fileUrl;
        }

        /// <summary>
        /// Загрузка файла материала в Firebase Storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> UploadMaterial(Stream stream, string fileName)
        {
            var cancellation = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(Bucket);
            string path = "material/" + fileName;
            var uploadTask = firebaseStorage.Child(path).PutAsync(stream, cancellation.Token);
            var fileUrl = await uploadTask;

            return fileUrl;
        }

        /// <summary>
        /// Загрузка фотографии материала в Firebase Storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> UploadPhotoMaterial(Stream stream, string fileName)
        {
            var cancellation = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(Bucket);
            string path = "photoMaterial/" + fileName;
            var uploadTask = firebaseStorage.Child(path).PutAsync(stream, cancellation.Token);
            var fileUrl = await uploadTask;

            return fileUrl;
        }
    }
}
