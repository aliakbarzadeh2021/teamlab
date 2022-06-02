using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ASC.Files.Core
{
    [DataContract]
    public enum FolderType
    {
        [EnumMember]
        DEFAULT = 0,

        [EnumMember]
        COMMON = 1,

        [EnumMember]
        BUNCH = 2,

        [EnumMember]
        TRASH = 3,

        [EnumMember]
        USER = 5,

        [EnumMember]
        SHARE = 6,
    }

    [DataContract(Name = "folder", Namespace = "")]
    [DebuggerDisplay("{Title} ({ID})")]
    public class Folder : FileEntry
    {
        public FolderType FolderType { get; set; }

        [DataMember(IsRequired = true, Name = "parent_folder_id", EmitDefaultValue = true)]
        public Int32 ParentFolderID { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "total_files", IsRequired = false)]
        public int TotalFiles { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "total_sub_folder", IsRequired = false)]
        public int TotalSubFolders { get; set; }

        [DataMember(Name = "shareable")]
        public bool Shareable { get; set; }


        public Folder()
        {
            Title = String.Empty;
        }
    }
}
