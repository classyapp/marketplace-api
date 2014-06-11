using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Repository;

namespace classy.Manager
{
    public class DefaultTempMediaFileManager : ITempMediaFileManager
    {
        ITempMediaFileRepository _tempMediaFileRepository;
        IStorageRepository _storageRepository;

        public DefaultTempMediaFileManager(ITempMediaFileRepository tempMediaFileRepo, IStorageRepository storageRepo)
        {
            _tempMediaFileRepository = tempMediaFileRepo;
            _storageRepository = storageRepo;
        }

        public string StoreTempFile(string appId, ServiceStack.ServiceHost.IFile file)
        {
            Classy.Models.TempMediaFile mediafile = new Classy.Models.TempMediaFile 
            { 
                AppId = appId, 
                ContentType = file.ContentType, 
                Key = Guid.NewGuid().ToString(),
                Type = (file.ContentType.ToLowerInvariant().StartsWith("image") ? Classy.Models.MediaFileType.Image : Classy.Models.MediaFileType.File)
            };
            string fileId = _tempMediaFileRepository.Save(mediafile);
            
            byte[] content = new byte[file.InputStream.Length];
            file.InputStream.Read(content, 0, content.Length);

            _storageRepository.SaveFile(mediafile.Key, content, file.ContentType, false, null);

            return mediafile.Key;
        }

        public void DeleteTempFile(string appId, string fileIdOrKey)
        {
            Classy.Models.TempMediaFile mediafile = _tempMediaFileRepository.Get(appId, fileIdOrKey);
            if (mediafile != null)
            {
                _storageRepository.DeleteFile(mediafile.Key);
                _tempMediaFileRepository.Delete(appId, mediafile.Id);
            }
        }
    }
}