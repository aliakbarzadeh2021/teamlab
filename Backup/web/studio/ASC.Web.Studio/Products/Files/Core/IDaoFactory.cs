#region Import

using System;
using ASC.Files.Core.Security;

#endregion

namespace ASC.Files.Core
{
    public interface IDaoFactory
    {
        IFolderDao GetFolderDao();

        IFileDao GetFileDao();

        IPreviewDao GetPreviewDao();

        ITagDao GetTagDao();

        ISecurityDao GetSecurityDao();
    }
}
