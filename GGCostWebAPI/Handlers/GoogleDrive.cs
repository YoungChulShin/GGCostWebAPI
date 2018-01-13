using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GGCostWebAPI.Handlers
{
    public class GoogleDrive
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Drive API Quickstart";

        private DriveService mService = null;

        private DriveService GetService()
        {
            if (mService != null)
            {
                return mService;
            }

            UserCredential credential;
            using (var stream = new System.IO.FileStream("Resources\\client_secret.json",
                                                        System.IO.FileMode.Open,
                                                        System.IO.FileAccess.Read))
            {
                var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = System.IO.Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        public void UpdateFile(string aFilePath)
        {
            var service = GetService();
            var fileMetaData = new File()
            {
                Name = "db.sqlite"
            };
            FilesResource.CreateMediaUpload reqeust;
            
            using (var uploadStream = new System.IO.FileStream(aFilePath,
                                                               System.IO.FileMode.Open,
                                                               System.IO.FileAccess.Read))
            {
                reqeust = service.Files.Create(
                    fileMetaData, uploadStream, GetMimeType(aFilePath));
                reqeust.Upload();
                var file = reqeust.ResponseBody;
            }
        }

        public void InsertFiles(string aFilePath)
        {
            var service = GetService();

            File body = new File();
            body.Name = System.IO.Path.GetFileName(aFilePath);
            body.Description = "File uploaded by Diamto Drive Sample";
            body.MimeType = GetMimeType(aFilePath);
            body.Parents = new List<string> { "root" };

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(aFilePath);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, stream, GetMimeType(aFilePath));
                request.Upload();
                var result = request.ResponseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                
            }
        }

        private string GetMimeType(string aFilePath)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(aFilePath).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
        
        public void GetDBDataFromDrive()
        {
            var service = GetService();
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = "name = 'GGCostData'";
            listRequest.PageSize = 1;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Console.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.Read();

        }

        public void SaveDBDataToDrive()
        {

        }
    }
}
