using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Configuration;
using ASC.Web.Studio.Utility;

namespace ASC.Files.Core
{
    [Flags]
    [DataContract]
    public enum FileStatus
    {
        [EnumMember]
        None = 0x0,

        [EnumMember]
        IsEditing = 0x1,

        [EnumMember]
        IsNew = 0x2
    }

    [DataContract(Name = "file", Namespace = "")]
    [DebuggerDisplay("{Title} v{Version}")]
    public class File : FileEntry
    {
        private static readonly TimeSpan editTimeout;

        private FileStatus status;

        public static readonly Dictionary<int, DateTime> NowEditing = new Dictionary<int, DateTime>();


        static File()
        {
            editTimeout = TimeSpan.FromMilliseconds(Convert.ToInt32(WebConfigurationManager.AppSettings["EditingTimeout"] ?? "6000"));
        }

        public File()
        {
            Version = 1;
        }

        [DataMember(EmitDefaultValue = false, Name = "folder_id", IsRequired = false)]
        public int FolderID { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "version", IsRequired = false)]
        public int Version { get; set; }

        public long ContentLength { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "content_length", IsRequired = true)]
        private String ContentLengthString
        {
            get { return FileUtility.ContentLengthToString(ContentLength); }
            set { }
        }

        public String ContentType { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "file_status", IsRequired = false)]
        public FileStatus FileStatus
        {
            get
            {
                lock (NowEditing)
                {
                    if (!NowEditing.ContainsKey(ID))
                    {
                        status &= (~FileStatus.IsEditing);
                        return status;
                    }
                    if ((DateTime.UtcNow - NowEditing[ID]).Duration() >= editTimeout)
                    {
                        NowEditing.Remove(ID);
                        status &= (~FileStatus.IsEditing);
                    }
                    else
                    {
                        status |= FileStatus.IsEditing;
                    }
                    return status;
                }
            }
            set
            {
                status = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "thumbnail_url", IsRequired = false)]
        public String ThumbnailURL { get; set; }

        public String FileUri
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(FileConst.UrlFileHandler) + string.Format(FileConst.ParamsDownload, ID, Version);
            }
            set { }
        }

        public String ViewUrl
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(FileConst.UrlFileHandler) + string.Format(FileConst.ParamsView, ID, Version);
            }
            set { }
        }

        public string ConvertedType
        {
            get;
            set;
        }
    }
}