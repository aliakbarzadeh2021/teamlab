using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Utility.Skins;
using ASC.Data.Storage;

namespace ASC.Web.Studio.Utility
{
    public enum FileType
    {
        Unknown = 0,
        Archive = 1,
        Excel = 2,
        Image = 3,
        Pdf = 4,
        Txt = 5,
        WordDoc = 6
    }

    public class FileUtility
    {

        public static readonly List<string> VideoExts = new List<string>
                                                            {
                                                                ".avi", ".mpg", ".vob",
                                                                ".mp4", ".m2ts", ".mov",
                                                                ".3gp", ".mkv"
                                                            };

        public static readonly List<String> ImgExts = new List<string>
                                                            {
                                                               ".bmp", ".cod", ".gif", ".ief", ".jpe", ".jpeg",
                                                               ".jpg", ".jfif", ".tiff", ".cmx", ".ico", ".pnm",
                                                               ".pbm", ".ppm", ".rgb", ".xbm", ".xpm", ".xwd", ".png"
                                                            };

        public static readonly List<String> ArchiveExts = new List<string>
                                                            {
                                                               ".zip", ".rar", ".ace", ".arc", ".arj",
                                                               ".bh", ".cab", ".enc", ".gz", ".ha",
                                                               ".jar", ".lha", ".lzh", ".pak", ".pk3",
                                                               ".tar", ".tgz", ".uu", ".uue", ".xxe",
                                                               ".z", ".zoo"
                                                            };

        public static readonly List<string> AudioExts = new List<string> { ".wav", ".mp3", ".ogg" };

        public static readonly List<String> TextExts = new List<string> { ".txt" };

        public static readonly List<String> PdfExts = new List<string> { ".pdf" };

        public static readonly List<String> ExcelExts = new List<string> { ".xlsx", ".xls" };

        public static readonly List<String> WordExts = new List<string> { ".doc", ".docx", ".odt" };


        public static string GetFileExtension(string fileName)
        {
            string ext = string.Empty;
            Boolean hita = false;
            int i = fileName.Length - 1;
            char[] arr = fileName.ToCharArray();
            while (i > 0 & !hita)
            {
                if (arr[i] == '.') hita = true;
                else ext = arr[i] + ext;
                i = i - 1;
            }

            return ext;
        }

        public static String ContentLengthToString(long contentLength)
        {

            if (contentLength / Math.Pow(10, 6) > 1) return String.Format("{0} {1}", Math.Round((contentLength / Math.Pow(10, 6)), 2), "MB");

            if (contentLength / Math.Pow(10, 3) > 1) return String.Format("{0} {1}", Math.Round((contentLength / Math.Pow(10, 3)), 2), "kb");

            return String.Format("{0} {1}", contentLength, "B");
        }

        public static FileType GetFileTypeByFileName(string name)
        {
            return GetFileTypeByExtention(GetFileExtension(name));
        }

        public static FileType GetFileTypeByExtention(string ext)
        {
            ext = ext.Insert(0, ".");
            FileType result = FileType.Unknown;

            if (ArchiveExts.Contains(ext))
                result = FileType.Archive;
            if (ExcelExts.Contains(ext))
                result = FileType.Excel;
            if (ImgExts.Contains(ext))
                result = FileType.Image;
            if (PdfExts.Contains(ext))
                result = FileType.Pdf;
            if (TextExts.Contains(ext))
                result = FileType.Txt;
            if (WordExts.Contains(ext))
                result = FileType.WordDoc;

            return result;
        }
    }
}