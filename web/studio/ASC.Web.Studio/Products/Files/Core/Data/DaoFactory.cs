using System;
using ASC.Files.Core.Security;

namespace ASC.Files.Core.Data
{
    public class DaoFactory : IDaoFactory
    {
        private readonly int tenantID;
        private readonly String storageKey;


        public DaoFactory(int tenantID, String storageKey)
        {
            this.tenantID = tenantID;
            this.storageKey = storageKey;
        }


        public IFileDao GetFileDao()
        {
            return new FileDao(tenantID, storageKey);
        }

        public IFolderDao GetFolderDao()
        {
            return new FolderDao(tenantID, storageKey);
        }

        public IPreviewDao GetPreviewDao()
        {
            return new PreviewDao(tenantID, storageKey);
        }

        public ITagDao GetTagDao()
        {
            return new TagDao(tenantID, storageKey);
        }

        public ISecurityDao GetSecurityDao()
        {
            return new SecurityDao(tenantID, storageKey);
        }
    }
}
