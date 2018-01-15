using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GGCostWebAPI.Handlers
{
    public class GoogleDrive
    {
        private string mFileName = "GGCostData.db";
        private DriveService mService = null;

        public void SaveFile(byte[] aFileData)
        {
            var dbDataFile = GetDBDataFromDrive(mFileName);
            if (dbDataFile == null)
            {
                var folder = GetDBDataFromDrive("공감가계부");
                var folderID = (folder == null) ? CreateFolder() : folder.Id;
                CreateFile(aFileData, folderID);
            }
            else
            {
                UpdateFile(aFileData, dbDataFile.Id);
            }
        }
        public byte[] LoadFile()
        {
            var dbDataFile = GetDBDataFromDrive(mFileName);
            if (dbDataFile == null)
            {
                throw new Exception("Error : Saved file is not exists");
            }

            return DownloadFile(dbDataFile.Id);
        }

        private string CreateFolder()
        {
            var service = GetService();

            var fileMetadata = new File()
            {
                Name = "공감가계부",
                MimeType = "application/vnd.google-apps.folder"
            };
            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();

            return file.Id;
        }
        private void CreateFile(byte[] aFileData, string aFolderID)
        {
            var service = GetService();

            File body = new File();
            body.Name = mFileName;
            body.Description = "공감 가계부 데이터 파일";
            body.MimeType = GetMimeType();
            body.Parents = new List<string> { aFolderID };

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(aFileData))
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, stream, GetMimeType());
                request.Upload();
                var result = request.ResponseBody;
            }
        }
        private void UpdateFile(byte[] aFileData, string aFileID)
        {
            var service = GetService();

            File body = new File();
            body.Name = mFileName;
            body.Description = "공감 가계부 데이터 파일";
            body.MimeType = GetMimeType();

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(aFileData))
            {
                FilesResource.UpdateMediaUpload request = service.Files.Update(body, aFileID, stream, GetMimeType());
                request.Upload();
                var result = request.ResponseBody;
            }
        }
        private byte[] DownloadFile(string aFileID)
        {
            var service = GetService();
            var reqeust = service.Files.Get(aFileID);
            byte[] fileData;
            using (var stream = new System.IO.MemoryStream())
            {
                reqeust.Download(stream);
                fileData = stream.ToArray();
            }
            
            return fileData;
        }

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
                    new string[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GGCost",
            });

            return service;
        }
        private File GetDBDataFromDrive(string aName)
        {
            var service = GetService();
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"name = '{aName}'";
            listRequest.PageSize = 1;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                return files[0];
            }
            else
            {
                return null;
            }
        }
        private string GetMimeType()
        {
            string mimeType = "application/unknown";
            string ext = "sqlite";
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }
}
