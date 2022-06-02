using System;
using System.Drawing;
using System.IO;
using System.Web;
using ASC.Data.Storage;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Resources;
using ASC.Web.Controls;
using ASC.Web.Controls.FileUploader.HttpModule;
using ASC.Web.Core;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility;
using ASC.Web.Studio;
using ASC.Web.Studio.Helpers;
using ASC.Web.Studio.Utility;
using StorageFactory = ASC.PhotoManager.Data.StorageFactory;

namespace ASC.Web.Community.PhotoManager.Common
{
    public class FilesUploader : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            ASC.Core.SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey));

            var result = "";

            try
            {
                if (ProgressFileUploader.HasFilesToUpload(context))
                {
                    var postedFile = new ProgressFileUploader.FileToUpload(context);
                    var contentLength = postedFile.ContentLength;
                    var fileName = postedFile.FileName;
                    var inputStream = postedFile.InputStream;
                    var contentType = postedFile.FileContentType;


                    IDataStore store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
                    var storage = StorageFactory.GetStorage();

                    string uid = context.Request["uid"];
                    string eventID = context.Request["eventID"];

                    bool clearSession = false;

                    Album currentAlbum = null;

                    var albums = storage.GetAlbums(Convert.ToInt64(eventID), uid);
                    clearSession = true;

                    currentAlbum = 0 < albums.Count ? albums[0] : null;

                    if (currentAlbum == null)
                    {
                        Event Event = storage.GetEvent(Convert.ToInt64(eventID));

                        currentAlbum = new Album();
                        currentAlbum.Event = Event;
                        currentAlbum.UserID = uid;

                        storage.SaveAlbum(currentAlbum);
                    }

                    if (context.Session["photo_albumid"] != null)
                    {
                        if (currentAlbum.Id != (long)context.Session["photo_albumid"])
                            clearSession = true;
                        context.Session["photo_albumid"] = currentAlbum.Id;
                    }
                    else
                        clearSession = true;

                    string fileNamePath = PhotoConst.ImagesPath + uid + "/" + currentAlbum.Id + "/";

                    ImageInfo currentImageInfo = new ImageInfo();
                    string[] listFiles;

                    if (context.Session["photo_listFiles"] != null && !clearSession)
                        listFiles = (string[])context.Session["photo_listFiles"];
                    else
                    {
                        listFiles = store.ListFilesRelative("", fileNamePath, "*.*", false);
                        context.Session["photo_listFiles"] = listFiles;
                    }

                    string fileExtension = GetFileExtension(fileName);
                    string fileNameWithOutExtension = GetFileName(fileName);
                    string addSuffix = string.Empty;


                    //if file already exists
                    int i = 1;

                    while (CheckFile(listFiles, fileNameWithOutExtension + addSuffix + ASC.PhotoManager.PhotoConst.THUMB_SUFFIX + fileExtension))
                    {
                        addSuffix = "(" + i.ToString() + ")";
                        i++;
                    }

                    string fileNameThumb = fileNamePath + fileNameWithOutExtension + addSuffix + ASC.PhotoManager.PhotoConst.THUMB_SUFFIX + "." + ASC.PhotoManager.PhotoConst.jpeg_extension;
                    string fileNamePreview = fileNamePath + fileNameWithOutExtension + addSuffix + ASC.PhotoManager.PhotoConst.PREVIEW_SUFFIX + "." + ASC.PhotoManager.PhotoConst.jpeg_extension;


                    currentImageInfo.Name = fileNameWithOutExtension;
                    currentImageInfo.PreviewPath = fileNamePreview;
                    currentImageInfo.ThumbnailPath = fileNameThumb;


                    Stream fs = inputStream;

                    try
                    {
                        EXIFReader reader = new EXIFReader(fs);
                        currentImageInfo.ActionDate = (string)reader[PropertyTagId.DateTime];
                    }
                    catch { }

                    ImageHelper.GenerateThumbnail(fs, fileNameThumb, ref currentImageInfo, store);
                    ImageHelper.GeneratePreview(fs, fileNamePreview, ref currentImageInfo, store);

                    fs.Dispose();

                    AlbumItem image = new AlbumItem(currentAlbum);
                    image.Name = currentImageInfo.Name;
                    image.Timestamp = ASC.Core.Tenants.TenantUtil.DateTimeNow();
                    image.UserID = uid;

                    image.Location = currentImageInfo.Name;


                    image.PreviewSize = new Size(currentImageInfo.PreviewWidth, currentImageInfo.PreviewHeight);
                    image.ThumbnailSize = new Size(currentImageInfo.ThumbnailWidth, currentImageInfo.ThumbnailHeight);

                    storage.SaveAlbumItem(image);

                    currentAlbum.FaceItem = image;
                    storage.SaveAlbum(currentAlbum);

                    string response = image.Id.ToString();

                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(response);
                    result = Convert.ToBase64String(byteArray);
                }

            }
            catch (Exception ex)
            {
                return new FileUploadResult()
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

            return new FileUploadResult()
            {
                Success = true,
                Data = "",
                Message = result
            };
        }

        #region Private methods

        public override string GetFileName(string fileName)
        {
            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }

        private bool CheckFile(string[] listFiles, string fileName)
        {
            foreach (var file in listFiles)
            {
                if (fileName == file.ToString())
                    return true;
            }
            return false;
        }

        private string GetFileExtension(string fileName)
        {
            if (fileName == null)
            {
                return fileName;
            }

            return fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.'));

        }

        private string GetFileNameThumbnail(string fileName)
        {
            return fileName.Insert(fileName.LastIndexOf('.'), ASC.PhotoManager.PhotoConst.THUMB_SUFFIX);
        }

        private string GetNextFileName(string fileName, int nextValue)
        {
            int position = fileName.LastIndexOf('(');
            if (position < 0)
            {
                position = fileName.Length;
            }
            return fileName.Substring(0, position) + "(" + nextValue + ")";
        }

        #endregion
    }
}
