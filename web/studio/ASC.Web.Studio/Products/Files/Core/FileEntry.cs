using System;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core.Security;
using ASC.Web.Files.Resources;

namespace ASC.Files.Core
{
    [DataContract(Namespace = "")]
    public abstract class FileEntry
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }

        [DataMember(Name = "title", IsRequired = true)]
        public string Title { get; set; }

        [DataMember(Name = "create_by_id")]
        public Guid CreateBy { get; set; }

        [DataMember(Name = "create_by")]
        public string CreateByString
        {
            get { return GetUserName(CreateBy); }
            set { }
        }

        [DataMember(Name = "create_on")]
        private string CreateOnString
        {
            get { return CreateOn.ToString("g"); }
            set { }
        }

        [DataMember(Name = "modified_on")]
        private string ModifiedOnString
        {
            get { return ModifiedOn.ToString("g"); }
            set { }
        }


        [DataMember(Name = "access")]
        public FileShare Access { get; set; }

        [DataMember(Name = "shared")]
        public bool SharedByMe { get; set; }

        public DateTime CreateOn { get; set; }

        [DataMember(Name = "modified_by_id")]
        public Guid ModifiedBy { get; set; }

        [DataMember(Name = "modified_by")]
        public string ModifiedByString
        {
            get { return GetUserName(ModifiedBy); }
            set { }
        }

        public DateTime ModifiedOn { get; set; }

        public FolderType RootFolderType { get; set; }

        public Guid RootFolderCreator { get; set; }

        public int RootFolderId { get; set; }

        public String UniqID
        {
            get { return String.Format("{0}_{1}", GetType().Name.ToLower(), ID); }
        }


        public override bool Equals(object obj)
        {
            var f = obj as FileEntry;
            return f != null && f.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Title;
        }


        public static string GetUserName(Guid userId)
        {
            return userId == SecurityContext.CurrentAccount.ID
                       ? FilesCommonResource.Author_Me
                       : CoreContext.UserManager.GetUsers(userId).DisplayUserName(true);
        }
    }
}