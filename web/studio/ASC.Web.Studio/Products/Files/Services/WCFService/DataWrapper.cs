using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ASC.Files.Core;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "composite_data")]
    public class DataWrapper
    {
        [DataMember(IsRequired = false, Name = "files", EmitDefaultValue = false)]
        public List<File> Files { get; set; }

        [DataMember(IsRequired = false, Name = "folders", EmitDefaultValue = false)]
        public List<Folder> Folders { get; set; }

        [DataMember(IsRequired = false, Name = "total", EmitDefaultValue = true)]
        public int Total { get; set; }

        [DataMember(IsRequired = false, Name = "path_parts", EmitDefaultValue = true)]
        public ItemDictionary<String, String> FolderPathParts { get; set; }

        [DataMember(IsRequired = false, Name = "folder_info", EmitDefaultValue = true)]
        public Folder FolderInfo { get; set; }

        [DataMember(IsRequired = false, Name = "tip", EmitDefaultValue = true)]
        public String Tip { get; set; }

        [DataMember(IsRequired = false, Name = "quota", EmitDefaultValue = true)]
        public long Quota { get; set; }

        [DataMember(IsRequired = false, Name = "count_new", EmitDefaultValue = false)]
        public ItemDictionary<int, int> CountNewInShare { get; set; }
    }
}