using System.Collections.Generic;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Projects.Core.Domain;
using ASC.Projects.Data;
using ASC.Projects.Engine;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Classes
{
    class FileEngine2 : IFileSecurityProvider
    {
        private static FileEngine Engine
        {
            get { return Global.EngineFactory.GetFileEngine(); }
        }

        public static int GetRoot(int projectId)
        {
            return Engine.GetRoot(projectId);
        }

        public static File GetFile(int id, int version)
        {
            return Engine.GetFile(id, version);
        }

        public static File SaveFile(File file, System.IO.Stream stream)
        {
            return Engine.SaveFile(file, stream);
        }

        public static void RemoveFile(int id)
        {
            Engine.RemoveFile(id);
        }

        public static Folder SaveFolder(Folder folder)
        {
            return Engine.SaveFolder(folder);
        }

        public static void AttachFileToMessage(int messageId, int fileId)
        {
            Engine.AttachFileToMessage(messageId, fileId);
        }

        public static void AttachFileToTask(int taskId, int fileId)
        {
            Engine.AttachFileToTask(taskId, fileId);
        }

        public static List<File> GetTaskFiles(Task task)
        {
            return Engine.GetTaskFiles(task);
        }

        public static List<File> GetMessageFiles(Message message)
        {
            return Engine.GetMessageFiles(message);
        }


        public static bool CanCreate(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanCreate(file, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanDelete(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanDelete(file, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanEdit(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanEdit(file, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanRead(FileEntry file, int projectId)
        {
            return GetFileSecurity(projectId).CanRead(file, SecurityContext.CurrentAccount.ID);
        }


        private static IFileSecurity GetFileSecurity(int projectId)
        {
            return new SecurityAdapter(new DaoFactory(Global.DB_ID, TenantProvider.CurrentTenantID), projectId);
        }


        public IFileSecurity GetFileSecurity(string data)
        {
            int id;
            return int.TryParse(data, out id) ? GetFileSecurity(id) : null;
        }
    }
}
