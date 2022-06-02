﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Collections;
using ASC.Common.Threading.Workers;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core.Users
{
    internal class ResizeWorkerItem
    {
        private readonly Guid _moduleId;
        private readonly Guid _userId;
        private readonly byte[] _data;
        private readonly long _maxFileSize;
        private readonly Size _size;
        private readonly IDataStore _dataStore;

        public ResizeWorkerItem(Guid moduleId, Guid userId, byte[] data, long maxFileSize, Size size, IDataStore dataStore)
        {
            _moduleId = moduleId;
            _userId = userId;
            _data = data;
            _maxFileSize = maxFileSize;
            _size = size;
            _dataStore = dataStore;
        }

        public Size Size
        {
            get { return _size; }
        }

        public IDataStore DataStore
        {
            get { return _dataStore; }
        }

        public long MaxFileSize
        {
            get { return _maxFileSize; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public Guid UserId
        {
            get { return _userId; }
        }

        public Guid ModuleId
        {
            get { return _moduleId; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ResizeWorkerItem)) return false;
            return Equals((ResizeWorkerItem) obj);
        }

        public bool Equals(ResizeWorkerItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.ModuleId.Equals(ModuleId) && other.UserId.Equals(UserId) && other.MaxFileSize == MaxFileSize && other.Size.Equals(Size);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = ModuleId.GetHashCode();
                result = (result*397) ^ UserId.GetHashCode();
                result = (result*397) ^ MaxFileSize.GetHashCode();
                result = (result*397) ^ Size.GetHashCode();
                return result;
            }
        }
    }

    public class UserPhotoManager
    {
        private static readonly IDictionary<Guid, IDictionary<Size, string>> Photofiles = new SynchronizedDictionary<Guid, IDictionary<Size, string>>();


        public static string GetDefaultPhotoAbsoluteWebPath()
        {
            return WebImageSupplier.GetAbsoluteWebPath(_defaultAvatar);
        }

        public static string GetPhotoAbsoluteWebPath(Guid userID)
        {
            return GetPhotoAbsoluteWebPath(Guid.Empty, userID);
        }

        public static string GetBigPhotoURL(Guid userID)
        {
            return GetSizedPhotoAbsoluteWebPath(Guid.Empty, userID, BigFotoSize);
        }

        public static string GetMediumPhotoURL(Guid userID)
        {
            return GetSizedPhotoAbsoluteWebPath(Guid.Empty, userID, MediumFotoSize);
        }

        public static string GetSmallPhotoURL(Guid userID)
        {
            return GetSizedPhotoAbsoluteWebPath(Guid.Empty, userID, SmallFotoSize);
        }

        public static string GetSizedPhotoUrl(Guid userId, int width, int height)
        {
            return GetSizedPhotoAbsoluteWebPath(Guid.Empty, userId, new Size(width, height));
        }


        public static Size MaxFotoSize
        {
            get { return new Size(200, 300); }
        }

        public static Size BigFotoSize
        {
            get { return new Size(82, 82); }
        }

        public static Size MediumFotoSize
        {
            get { return new Size(48, 48); }
        }

        public static Size SmallFotoSize
        {
            get { return new Size(32, 32); }
        }


        private static string _defaultAvatar = "default_user_photo_size_100-100.gif";
        private static string _defaultSmallAvatar = "default_user_photo_size_32-32.gif";
        private static string _defaultMediumAvatar = "default_user_photo_size_48-48.gif";
        private static string _defaultBigAvatar = "default_user_photo_size_82-82.gif";
        private static string _tempDomainName = "temp";


        internal static string GetPhotoAbsoluteWebPath(Guid moduleID, Guid userID)
        {

            var path = SearchInCache(moduleID, userID, Size.Empty);
            if (!string.IsNullOrEmpty(path)) return path;

            try
            {
                var data = CoreContext.UserManager.GetUserPhoto(userID, moduleID);
                var photoUrl = data == null || data.Length == 0 ?
                    GetDefaultPhotoAbsoluteWebPath() :
                    SaveOrUpdatePhoto(moduleID, userID, data, -1, new Size(-1, -1), false);

                var fileName = Path.GetFileName(photoUrl);

                AddToCache(userID,Size.Empty, fileName);

                return photoUrl;
            }
            catch { }
            return GetDefaultPhotoAbsoluteWebPath();
        }

        private static void AddToCache(Guid userId, Size size, string fileName)
        {
            AddToCache(userId, size, fileName, true);
        }

        private static void AddToCache(Guid userId, Size size, string fileName, bool replace)
        {
            if (!Photofiles.ContainsKey(userId)) Photofiles[userId] = new SynchronizedDictionary<Size, string>();
            if (replace)
            {
                Photofiles[userId][size]= fileName;
            }
            else
            {
                if (!Photofiles[userId].ContainsKey(size))
                    Photofiles[userId].Add(size, fileName);
            }
        }

        internal static Size GetPhotoSize(Guid moduleID, Guid userID)
        {
            var virtualPath = GetPhotoAbsoluteWebPath(moduleID, userID);
            if (virtualPath == null) return Size.Empty;

            try
            {
                var sizePart = virtualPath.Substring(virtualPath.LastIndexOf('_'));
                sizePart = sizePart.Trim('_');
                sizePart = sizePart.Remove(sizePart.LastIndexOf('.'));
                return new Size(Int32.Parse(sizePart.Split('-')[0]), Int32.Parse(sizePart.Split('-')[1]));
            }
            catch
            {
                return Size.Empty;
            }
        }

        private static string GetSizedPhotoAbsoluteWebPath(Guid moduleID, Guid userID, Size size)
        {

            var res = SearchInCache(moduleID, userID, size);
            if (!string.IsNullOrEmpty(res)) return res;

            try
            {
                var data = CoreContext.UserManager.GetUserPhoto(userID, moduleID);

                if (data == null || data.Length == 0)
                {
                    //empty photo. cache default
                    string photoUrl = GetDefaultPhotoAbsoluteWebPath(size);
                    var fileName = Path.GetFileName(photoUrl);
                    AddToCache(userID,size,fileName);
                    return photoUrl;
                }

                //Enqueue for sizing
                SizePhoto(moduleID, userID, data, -1, size);
            }
            catch { }

            return GetDefaultPhotoAbsoluteWebPath(size);
        }

        private static string GetDefaultPhotoAbsoluteWebPath(Size size)
        {
            if (size == BigFotoSize) return WebImageSupplier.GetAbsoluteWebPath(_defaultBigAvatar);
            if (size == SmallFotoSize) return WebImageSupplier.GetAbsoluteWebPath(_defaultSmallAvatar);
            if (size == MediumFotoSize) return WebImageSupplier.GetAbsoluteWebPath(_defaultMediumAvatar);
            return GetDefaultPhotoAbsoluteWebPath();
        }

        //Regex for parsing filenames into groups with id's
        private static readonly Regex ParseFile =
                new Regex(@"^(?'module'\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1}){0,1}" +
                    @"(?'user'\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1}){1}" +
                    @"_(?'kind'orig|size){1}_(?'size'(?'width'[0-9]{1,5})-{1}(?'height'[0-9]{1,5})){0,1}\..*", RegexOptions.Compiled);

        private static readonly HashSet<int> TenantDiskCache = new HashSet<int>();
        private static readonly object DiskCacheLoaderLock = new object();

        private static bool IsCacheLoadedForTennant()
        {
            return TenantDiskCache.Contains(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        private static bool SetCacheLoadedForTennant(bool isLoaded)
        {
            return isLoaded ? TenantDiskCache.Add(CoreContext.TenantManager.GetCurrentTenant().TenantId) : TenantDiskCache.Remove(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }


        private static string SearchInCache(Guid moduleID, Guid userID, Size size)
        {
            if (!IsCacheLoadedForTennant())
                LoadDiskCache();

            if (!Photofiles.ContainsKey(userID))
            {
                return null;
            }

            var findResult = (from profile in Photofiles[userID]
                             where profile.Value!=null && profile.Key==size && (moduleID==Guid.Empty || CheckMatch(moduleID,size,ParseFile.Match(profile.Value)))
                             select profile.Value).FirstOrDefault();
                                 
                                 
            //if (findResult == null)
            //{
            //    var resul
            //    var pattern = size == Size.Empty ?
            //        string.Format(@"{0}_orig_.*$", (moduleID == Guid.Empty ? "" : moduleID.ToString())) :
            //        string.Format(@"{0}_size_{1}-{2}.*$", (moduleID == Guid.Empty ? "" : moduleID.ToString()), size.Width, size.Height);
            //    var regexp = new Regex(pattern);
            //    findResult = photofiles[userID].Find(item => !string.IsNullOrEmpty(item) && regexp.IsMatch(item));
            //}

            if (findResult != null && findResult.StartsWith("default")) return WebImageSupplier.GetAbsoluteWebPath(findResult);

            var store = GetDataStore();
            if (!string.IsNullOrEmpty(findResult)) return store.GetUri(findResult).ToString();

            return null;
        }

        private static bool CheckMatch(Guid moduleID, Size size, Match match)
        {
            var result = true;
            //if (size != Size.Empty)
            //{
            //    var sizeMatched = match.Groups["size"];
            //    if (sizeMatched.Success)
            //    {
            //        result = sizeMatched.Value.Equals(string.Format("{0}-{1}", size.Width, size.Height),
            //                                          StringComparison.OrdinalIgnoreCase);
            //    }
            //}
            if (moduleID!=Guid.Empty)
            {
                var moduleMatched = match.Groups["module"];
                if (moduleMatched.Success)
                {
                    try
                    {
                        result = new Guid(moduleMatched.Value).Equals(moduleMatched);
                    }
                    catch
                    {
                        
                    }
                }
            }
            return result;
        }


        private static void LoadDiskCache()
        {
            lock (DiskCacheLoaderLock)
            {
                if (!IsCacheLoadedForTennant())
                {
                    try
                    {
                        var list = GetDataStore().ListFiles("", "*.*", false);
                        foreach (var uri in list)
                        {
                            var fileName = Path.GetFileName(uri.ToString());
                            //Try parse uri
                            if (fileName != null)
                            {
                                var match = ParseFile.Match(fileName);
                                if (match.Success && match.Groups["user"].Success)
                                {
                                    var parsedUserId = new Guid(match.Groups["user"].Value);
                                    Size size = Size.Empty;
                                    if (match.Groups["width"].Success && match.Groups["height"].Success)
                                    {
                                        //Parse size
                                        size = new Size(int.Parse(match.Groups["width"].Value), int.Parse(match.Groups["height"].Value));
                                    }
                                    AddToCache(parsedUserId, size, fileName, true);
                                }
                            }
                        }
                        SetCacheLoadedForTennant(true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static void ClearCache(Guid moduleID, Guid userID)
        {
            try
            {
                Photofiles.Remove(userID);
                var storage = GetDataStore();
                storage.DeleteFiles("", (moduleID == Guid.Empty ? "" : moduleID.ToString()) + userID.ToString() + "*.*", false);
                SetCacheLoadedForTennant(false);
            }
            catch { };
        }


        public static string SaveOrUpdatePhoto(Guid userID, byte[] data)
        {
            return SaveOrUpdatePhoto(Guid.Empty, userID, data, -1, MaxFotoSize, true);
        }

        public static void RemovePhoto(Guid idModule, Guid idUser)
        {
            CoreContext.UserManager.SaveUserPhoto(idUser, idModule, null);
            ClearCache(idModule, idUser);
        }


        private static string SaveOrUpdatePhoto(Guid moduleID, Guid userID, byte[] data, long maxFileSize, Size size, bool saveInCoreContext)
        {
            ImageFormat imgFormat;
            int width;
            int height;
            data = TryParseImage(data, maxFileSize, size, out imgFormat, out width, out height);

            var widening = GetImgFormatName(imgFormat);
            var trueFileName = string.Format("{0}{1}_orig_{2}-{3}.{4}", (moduleID == Guid.Empty ? "" : moduleID.ToString()), userID, width, height, widening);

            if (saveInCoreContext)
            {
                CoreContext.UserManager.SaveUserPhoto(userID, moduleID, data);
                ClearCache(moduleID, userID);
            }

            var store = GetDataStore();

            var photoUrl = GetDefaultPhotoAbsoluteWebPath();
            if (data != null && data.Length > 0)
            {
                using (var stream = new MemoryStream(data))
                {
                    photoUrl =  store.Save(trueFileName, stream).ToString();
                }
                //Queue resizing
                SizePhoto(moduleID, userID, data, -1, SmallFotoSize, true);
                SizePhoto(moduleID, userID, data, -1, MediumFotoSize, true);
                SizePhoto(moduleID, userID, data, -1, BigFotoSize, true);
            }
            return photoUrl;
        }


        private static byte[] TryParseImage(byte[] data, long maxFileSize, Size maxsize, out ImageFormat imgFormat, out int width, out int height)
        {
            if (data == null || data.Length <= 0) throw new UnknownImageFormatException();
            if (maxFileSize != -1 && data.Length > maxFileSize) throw new ImageWeightLimitException();

            try
            {
                using (var stream = new MemoryStream(data))
                using (var img = new Bitmap(stream))
                {
                    imgFormat = img.RawFormat;
                    width = img.Width;
                    height = img.Height;
                    var maxWidth = maxsize.Width;
                    var maxHeight = maxsize.Height;

                    if ((maxHeight != -1 && img.Height > maxHeight) || (maxWidth != -1 && img.Width > maxWidth))
                    {
                        #region calulate height and width

                        if (width > maxWidth && height > maxHeight)
                        {

                            if (width > height)
                            {
                                height = (int)((double)height * (double)maxWidth / (double)width + 0.5);
                                width = maxWidth;
                            }
                            else
                            {
                                width = (int)((double)width * (double)maxHeight / (double)height + 0.5);
                                height = maxHeight;
                            }
                        }

                        if (width > maxWidth && height <= maxHeight)
                        {
                            height = (int)((double)height * (double)maxWidth / (double)width + 0.5);
                            width = maxWidth;
                        }

                        if (width <= maxWidth && height > maxHeight)
                        {
                            width = (int)((double)width * (double)maxHeight / (double)height + 0.5);
                            height = maxHeight;
                        }

                        #endregion

                        using (var b = new Bitmap(width, height))
                        using (var gTemp = Graphics.FromImage(b))
                        {
                            gTemp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gTemp.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gTemp.DrawImage(img, 0, 0, width, height);

                            data = SaveToBytes(b);
                        }
                    }
                    return data;
                }
            }
            catch (OutOfMemoryException)
            {
                throw new ImageWeightLimitException();
            }
            catch (ArgumentException error)
            {
                throw new UnknownImageFormatException(error);
            }
        }

        //note: using auto stop queue
        private static readonly WorkerQueue<ResizeWorkerItem> ResizeQueue = new WorkerQueue<ResizeWorkerItem>(2,TimeSpan.FromSeconds(30),1,true);//TODO: configure

        private static string SizePhoto(Guid moduleID, Guid userID, byte[] data, long maxFileSize, Size size)
        {
            return SizePhoto(moduleID, userID, data, maxFileSize, size, false);
        }

        private static string SizePhoto(Guid moduleID, Guid userID, byte[] data, long maxFileSize, Size size, bool now)
        {
            if (data == null || data.Length <= 0) throw new UnknownImageFormatException();
            if (maxFileSize != -1 && data.Length > maxFileSize) throw new ImageWeightLimitException();

            var resizeTask = new ResizeWorkerItem(moduleID, userID, data, maxFileSize, size, GetDataStore());
            if (now)
            {
                //Resize synchronously
                ResizeImage(resizeTask);
                return GetSizedPhotoAbsoluteWebPath(moduleID, userID,size);
            }
            else
            {
                if (!ResizeQueue.GetItems().Contains(resizeTask))
                {
                    //Add
                    ResizeQueue.Add(resizeTask);
                    if (!ResizeQueue.IsStarted)
                    {
                        ResizeQueue.Start(ResizeImage);
                    }
                }
                return GetDefaultPhotoAbsoluteWebPath(size);
                    //NOTE: return default photo here. Since task will update cache
            }
        }

        private static void ResizeImage(ResizeWorkerItem item)
        {
            try
            {
                var data = item.Data;
                using (var stream = new MemoryStream(data))
                using (var img = new Bitmap(stream))
                {
                    var imgFormat = img.RawFormat;
                    if (item.Size != img.Size)
                    {
                        using (var img2 = DoThumbnail(img, item.Size, true))
                        {
                            data = SaveToBytes(img2);
                        }
                    }
                    else
                    {
                        data = SaveToBytes(img);
                    }

                    var widening = GetImgFormatName(imgFormat);
                    var trueFileName = string.Format("{0}{1}_size_{2}-{3}.{4}", (item.ModuleId == Guid.Empty ? "" : item.ModuleId.ToString()), item.UserId, item.Size.Width, item.Size.Height, widening);

                    using (var stream2 = new MemoryStream(data))
                    {
                        var photoUrl = item.DataStore.Save(trueFileName, stream2).ToString();
                        //NOTE: Update cache here
                        var fileName = Path.GetFileName(photoUrl);

                        AddToCache(item.UserId, item.Size, fileName, true);
                    }
                }
            }
            catch (ArgumentException error)
            {
                throw new UnknownImageFormatException(error);
            }
        }

        public static string GetTempPhotoAbsoluteWebPath(string fileName)
        {
            return GetDataStore().GetUri(_tempDomainName, fileName).ToString();
        }

        public static string SaveTempPhoto(byte[] data, long maxFileSize, int maxWidth, int maxHeight)
        {
            ImageFormat imgFormat;
            int width;
            int height;
            data = TryParseImage(data, maxFileSize, new Size(maxWidth, maxHeight), out imgFormat, out width, out height);
            string fileName = Guid.NewGuid().ToString() + "." + GetImgFormatName(imgFormat);

            var store = GetDataStore();
            using (var stream = new MemoryStream(data))
            {
                return store.Save(_tempDomainName, fileName, stream).ToString();
            }
        }

        public static byte[] GetTempPhotoData(string fileName)
        {
            using (var s = GetDataStore().GetReadStream(_tempDomainName, fileName))
            {
                var data = new MemoryStream();
                var buffer = new Byte[1024 * 10];
                while (true)
                {
                    var count = s.Read(buffer, 0, buffer.Length);
                    if (count == 0) break;
                    data.Write(buffer, 0, count);
                }
                return data.ToArray();
            }
        }

        public static string GetSizedTempPhotoAbsoluteWebPath(string fileName, int newWidth, int newHeight)
        {
            var store = GetDataStore();
            if (store.IsFile(_tempDomainName, fileName))
            {
                using (var s = store.GetReadStream(_tempDomainName, fileName))
                using (var img = Image.FromStream(s))
                {
                    var imgFormat = img.RawFormat;
                    byte[] data;

                    if (img.Width != newWidth || img.Height != newHeight)
                    {
                        using (var img2 = DoThumbnail(img, new Size(newWidth, newHeight), true))
                        {
                            data = SaveToBytes(img2);
                        }
                    }
                    else
                    {
                        data = SaveToBytes(img);
                    }
                    var widening = GetImgFormatName(imgFormat);
                    var index = fileName.LastIndexOf('.');
                    var fileNameWithoutExt = (index != -1) ? fileName.Substring(0, index) : fileName;

                    var trueFileName = fileNameWithoutExt + "_size_" + newWidth.ToString() + "-" + newHeight.ToString() + "." + widening;
                    using (var stream = new MemoryStream(data))
                    {
                        return store.Save(_tempDomainName, trueFileName, stream).ToString();
                    }
                }
            }
            return GetDefaultPhotoAbsoluteWebPath(new Size(newWidth, newHeight));
        }

        public static void RemoveTempPhoto(string fileName)
        {
            var index = fileName.LastIndexOf('.');
            var fileNameWithoutExt = (index != -1) ? fileName.Substring(0, index) : fileName;
            try
            {
                var store = GetDataStore();
                store.DeleteFiles(_tempDomainName, "", fileNameWithoutExt + "*.*", false);
            }
            catch { };
        }

        private static Image DoThumbnail(Image image, Size size, bool crop)
        {
            var width = size.Width;
            var height = size.Height;
            var realWidth = image.Width;
            var realHeight = image.Height;

            var thumbnail = new Bitmap(width, height);

            var maxSide = realWidth > realHeight ? realWidth : realHeight;
            var minSide = realWidth < realHeight ? realWidth : realHeight;

            var alignWidth = true;
            if (crop) alignWidth = (minSide == realWidth);
            else alignWidth = (maxSide == realWidth);

            double scaleFactor = (alignWidth) ? (realWidth / (1.0 * width)) : (realHeight / (1.0 * height));

            if (scaleFactor < 1) scaleFactor = 1;

            int locationX, locationY;
            int finalWidth, finalHeigth;

            finalWidth = (int)(realWidth / scaleFactor);
            finalHeigth = (int)(realHeight / scaleFactor);

            locationY = (int)((height / 2.0) - (finalHeigth / 2.0));
            locationX = (int)((width / 2.0) - (finalWidth / 2.0));

            var rect = new Rectangle(locationX, locationY, finalWidth, finalHeigth);

            using (var graphic = Graphics.FromImage(thumbnail))
            {
                graphic.FillRectangle(new SolidBrush(Color.Black), 0, 0, width, height);
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.DrawImage(image, rect);
            }

            return thumbnail;
        }

        private static byte[] SaveToBytes(Image img)
        {
            byte[] data = new byte[0];
            using (var memoryStream = new MemoryStream())
            {
                var encParams = new EncoderParameters(1);
                encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                img.Save(memoryStream, GetCodecInfo(img.RawFormat), encParams);
                data = memoryStream.ToArray();
            }
            return data;
        }


        public static Bitmap GetPhotoBitmap(Guid userID)
        {
            try
            {
                var data = CoreContext.UserManager.GetUserPhoto(userID, Guid.Empty);
                if (data != null)
                {
                    using (var s = new MemoryStream(data))
                    {
                        return new Bitmap(s);
                    }
                }
            }
            catch { }
            return null;
        }

        public static string SaveThumbnail(Guid userID, Image img, ImageFormat format)
        {
            var moduleID = Guid.Empty;
            var widening = GetImgFormatName(format);
            Size size = img.Size;
            var trueFileName = string.Format("{0}{1}_size_{2}-{3}.{4}", (moduleID == Guid.Empty ? "" : moduleID.ToString()), userID, img.Width, img.Height, widening);

            var store = GetDataStore();
            var photoUrl = string.Empty;
            using (var s = new MemoryStream(SaveToBytes(img)))
            {
                img.Dispose();
                photoUrl = store.Save(trueFileName, s).ToString();
            }
            var fileName = Path.GetFileName(photoUrl);
            AddToCache(userID, size, fileName);
            return photoUrl;
        }


        private static IDataStore GetDataStore()
        {
            return StorageFactory.GetStorage(CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(), "userPhotos");
        }

        private static string GetImgFormatName(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Bmp)) return "bmp";
            if (format.Equals(ImageFormat.Emf)) return "emf";
            if (format.Equals(ImageFormat.Exif)) return "exif";
            if (format.Equals(ImageFormat.Gif)) return "gif";
            if (format.Equals(ImageFormat.Icon)) return "icon";
            if (format.Equals(ImageFormat.Jpeg)) return "jpeg";
            if (format.Equals(ImageFormat.Png)) return "png";
            if (format.Equals(ImageFormat.Tiff)) return "tiff";
            if (format.Equals(ImageFormat.Wmf)) return "wmf";
            return "jpg";
        }

        private static ImageCodecInfo GetCodecInfo(ImageFormat format)
        {
            var mimeType = string.Format("image/{0}", GetImgFormatName(format));
            if (mimeType == "image/jpg") mimeType = "image/jpeg";
            var encoders = ImageCodecInfo.GetImageEncoders();
            foreach (var e in
                encoders.Where(e => e.MimeType.Equals(mimeType, StringComparison.InvariantCultureIgnoreCase)))
            {
                return e;
            }
            return 0 < encoders.Length ? encoders[0] : null;
        }
    }


    public class UnknownImageFormatException : Exception
    {
        public UnknownImageFormatException() : base("unknown image file type") { }

        public UnknownImageFormatException(Exception inner) : base("unknown image file type", inner) { }
    }

    public class ImageWeightLimitException : Exception
    {
        public ImageWeightLimitException() : base("image with is too large") { }
    }

    public class ImageSizeLimitException : Exception
    {
        public ImageSizeLimitException() : base("image size is too large") { }
    }
}