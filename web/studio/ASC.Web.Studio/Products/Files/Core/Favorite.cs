#region Import

using System.Runtime.Serialization;

#endregion

namespace ASC.Files.Core
{
    [DataContract(Name = "favorite", Namespace = "")]
    public class Favorite
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        private string ID
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "folder_path", IsRequired = false)]
        public string FolderPath
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "title", IsRequired = true)]
        public string Title
        {
            get;
            set;
        }


        public static Favorite FromTag(Tag tag)
        {
            return new Favorite
            {
                ID = tag.Id.ToString(),
                Title = string.IsNullOrEmpty(tag.TagName) ? string.Empty : tag.TagName.Split('/')[0],
                FolderPath = (tag.EntryType == FileEntryType.Folder ? "folder_" : "file_") + tag.EntryId.ToString(),
            };
        }
    }
}
