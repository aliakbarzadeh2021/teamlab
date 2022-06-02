using System;

namespace ASC.Files.Core
{
    [Serializable]
    public class Tag
    {
        public string TagName
        {
            get;
            set;
        }

        public TagType TagType
        {
            get;
            set;
        }

        public Guid Owner
        {
            get;
            set;
        }

        public int EntryId
        {
            get;
            set;
        }

        public FileEntryType EntryType
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }


        public Tag()
        {

        }

        public Tag(string name, TagType type, Guid owner)
            : this(name, type, owner, null)
        {
        }

        public Tag(string name, TagType type, Guid owner, FileEntry entry)
        {
            TagName = name;
            TagType = type;
            Owner = owner;
            if (entry != null)
            {
                EntryId = entry.ID;
                EntryType = entry is File ? FileEntryType.File : FileEntryType.Folder;
            }
        }


        public static Tag New(Guid owner, File file)
        {
            return new Tag("new", TagType.New, owner, file);
        }

        public static Tag New(Guid owner, int fileId)
        {
            return new Tag("new", TagType.New, owner) { EntryId = fileId, EntryType = FileEntryType.File };
        }

        public static Tag Favorite(string title, Guid owner, string path)
        {
            var t = new Tag(string.Format("{0}/{1}", title, path), TagType.Favorite, owner);
            t.EntryType = path.StartsWith("folder_") ? FileEntryType.Folder : FileEntryType.File;
            t.EntryId = Convert.ToInt32(path.Substring(path.IndexOf('_') + 1));
            return t;
        }

        public static Tag System(string name, File file)
        {
            return new Tag(name, TagType.System, Guid.Empty, file);
        }
    }

    [Flags]
    public enum TagType
    {
        New = 1,
        Favorite = 2,
        System = 4
    }
}
