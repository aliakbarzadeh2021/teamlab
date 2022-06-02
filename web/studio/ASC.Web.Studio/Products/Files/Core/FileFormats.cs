using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace ASC.Files.Core
{
    public static class FileFormats
    {
        private static readonly IEqualityComparer<string> comparer;

        public static readonly List<String> SupportedFormats = new List<string>();

        public static readonly List<String> PresentationFormats = new List<string> 
        { 
            ".ppt",
            ".pptx",
            ".odp"
        };

        public static readonly List<String> DocumentsFormats = new List<String>
        {
          ".docx",
          ".doc",
          ".odt",
          ".rtf",
          ".txt",
          ".html",
          ".htm",
          ".mht",

          ".pdf",
          ".djvu",
          ".fb2",
          ".epub"
        };

        public static readonly List<String> SpreadsheetFormats = new List<String>
        {
            ".ods",
            ".xls",
            ".xlsx",
            ".csv"
        };

        public static readonly List<String> PictureFormats = new List<String>
        { 
            ".bmp", ".cod", ".gif",".ief", ".jpe",".jpeg",
            ".jpg",".jfif",".tiff",".tif",".cmx",".ico",".pnm",
            ".pbm",".ppm",".rgb",".xbm",".xpm",".xwd", ".png"
        };

        public static readonly List<String> EditableFormats = new List<String>();

        public static readonly List<String> PreviewedImageFormats = new List<String>();

        public static readonly List<String> PreviewedDocFormats = new List<String>();

        public static readonly List<String> EditedDocFormats = new List<String>();


        static FileFormats()
        {
            comparer = StringComparer.CurrentCultureIgnoreCase;

            if (!bool.TrueString.Equals(WebConfigurationManager.AppSettings["SupportAllFormats"] ?? "true", StringComparison.InvariantCultureIgnoreCase))
            {
                SupportedFormats.AddRange(PresentationFormats);
                SupportedFormats.AddRange(DocumentsFormats);
                SupportedFormats.AddRange(SpreadsheetFormats);
                SupportedFormats.AddRange(PictureFormats);
            }

            if (bool.TrueString.Equals(WebConfigurationManager.AppSettings["EnablePlugin"] ?? "true", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["EditableExtensions"]))
                {
                    EditableFormats = WebConfigurationManager.AppSettings["EditableExtensions"].Split('|').ToList();
                }
            }

            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["PreviewedDocExtensions"]) && !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["UrlDocumentService"]))
            {
                EditedDocFormats = WebConfigurationManager.AppSettings["EditedDocExtensions"].Split('|').ToList();
            }

            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["PreviewedDocExtensions"]) && !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["UrlDocumentService"]))
            {
                PreviewedDocFormats = WebConfigurationManager.AppSettings["PreviewedDocExtensions"].Split('|').ToList();
            }

            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["PreviewedImageExtensions"]))
            {
                PreviewedImageFormats = WebConfigurationManager.AppSettings["PreviewedImageExtensions"].Split('|').ToList();
            }

        }

        public static bool IsSupported(string title)
        {
            return SupportedFormats.Count == 0 ? true : SupportedFormats.Contains(GetExtension(title), comparer);
        }

        public static bool IsPreviewed(string title)
        {
            return PreviewedImageFormats.Contains(GetExtension(title), comparer);
        }

        public static bool IsEdited(string title)
        {
            return EditableFormats.Contains(GetExtension(title), comparer);
        }

        public static FilterType GetFileType(string title)
        {
            var ext = GetExtension(title);
            if (DocumentsFormats.Contains(ext, comparer)) return FilterType.DocumentsOnly;
            if (PictureFormats.Contains(ext, comparer)) return FilterType.PicturesOnly;
            if (PresentationFormats.Contains(ext, comparer)) return FilterType.PresentationsOnly;
            if (SpreadsheetFormats.Contains(ext, comparer)) return FilterType.SpreadsheetsOnly;
            return FilterType.None;
        }

        public static string GetExtension(string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;
            var pos = title.LastIndexOf('.');
            return 0 <= pos ? title.Substring(pos).ToLower() : string.Empty;
        }
    }
}