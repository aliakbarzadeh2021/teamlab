#if DEBUG
namespace ASC.Web.Files.Tests
{
    using System;
    using ASC.Common.Security.Authorizing;
    using ASC.Files.Core;
    using ASC.Files.Core.Security;
    using NUnit.Framework;

    [TestFixture]
    public class SecurityTest : BaseTest
    {
        //private ISubject user;
        //private Guid userRole;
        //private FileSecurity fileSecurity;

        public SecurityTest()
        {
            //SecurityContext.AuthenticateMe("nikolay.ivanov@avsmedia.net", "1");
            //fileSecurity = new FilesSecurity(_daoFactory);
            //user = CoreContext.Authentication.GetAccountByID(new Guid("0cc94ac4-721b-4890-af66-2b3d664766a5"));//Николай Погорский
            //userRole = new Guid("1d42a4fb-755e-44ab-bcf5-38482c9b2415");//Отдел программирования форматов
            //creator = SecurityContext.CurrentAccount;
        }


        [Test]
        public void ShareTest()
        {
            var r = SecurityDAO.GetShares(new Folder() { ID = 557, ParentFolderID = 478 });
            //Folder folder1 = null;
            //Folder folder2 = null;
            //File file1 = null;

            try
            {
                /*folder1 = new Folder() { Title = "folder1" };
                var secId = folder1.SecurityId;
                folder1 = FolderDAO.GetFolder(FolderDAO.SaveFolder(folder1));

                folder2 = new Folder() { Title = "folder2", ParentFolderID = folder1.ID };
                folder2 = FolderDAO.GetFolder(FolderDAO.SaveFolder(folder2));

                file1 = new File() { Title = "file1", FolderID = folder2.ID };
                file1 = FileDAO.GetFile(FileDAO.SaveFile(file1).ID, 0);

                //простому пользователю ничего нельзя
                AssertPermission(user, folder1, false);
                AssertPermission(user, folder2, false);
                AssertPermission(user, file1, false);

                //наследуемость по пользовательской группе
                fileSecurity.Share(file1, userRole, FilesSecurityActions.Read);
                AssertPermission(user, file1, true);

                //наследуемость по иерархии объектов
                fileSecurity.Share(folder1, user.ID, FilesSecurityActions.Read);
                AssertPermission(user, folder1, true);
                AssertPermission(user, folder2, true);
                AssertPermission(user, file1, true);

                var shares1 = fileSecurity.GetSharesByMe();
                var shares2 = fileSecurity.GetSharesForMe();
                foreach (var share in fileSecurity.GetShares(folder1))
                {
                    fileSecurity.UnShare(folder1, share.Key, share.Value);
                }
                foreach (var share in fileSecurity.GetShares(folder2))
                {
                    fileSecurity.UnShare(folder2, share.Key, share.Value);
                }
                foreach (var share in fileSecurity.GetShares(file1))
                {
                    fileSecurity.UnShare(file1, share.Key, share.Value);
                }*/
            }
            finally
            {
                // if (folder1 != null) FolderDAO.DeleteFolder(folder1.ID);
            }
        }
        /*
        [Test]
        public void CustomFolder()
        {
            var cat = Array.Find(CoreContext.GroupManager.GetGroupCategories(), c => c.Name == "Custom Category");
            if (cat == null)
            {
                cat = CoreContext.GroupManager.SaveGroupCategory(new GroupCategory(FileModuleId.ModuleId) { Name = "Custom Category" });
            }

            var groups = CoreContext.GroupManager.GetGroups(cat.ID);
            var group = 0 < groups.Length ? groups[0] : null;
            if (group == null)
            {
                group = CoreContext.GroupManager.SaveGroupInfo(new GroupInfo(cat.ID) { Name = "Custom Group" });
            }
            CoreContext.UserManager.AddUserIntoGroup(user.ID, group.ID);
            CoreContext.UserManager.AddUserIntoGroup(SecurityContext.CurrentAccount.ID, group.ID);

            Folder folder1 = null;
            try
            {
                folder1 = new Folder() { Title = "folder1" };
                folder1 = FolderDAO.GetFolder(FolderDAO.SaveFolder(folder1));

                AssertPermission(user, folder1, false);

                fileSecurity.Share(folder1, group.ID, FilesSecurityActions.Read);
                AssertPermission(user, folder1, true);

                CollectionAssert.Contains(fileSecurity.GetSharesForMe(), folder1);
            }
            finally
            {
                if (folder1 != null)
                {
                  //  FolderDAO.DeleteFolder(folder1.ID);
                    foreach (var share in fileSecurity.GetShares(folder1))
                    {
                        fileSecurity.UnShare(folder1, share.Key, share.Value);
                    }
                }
            }
        }
        */

        private void AssertPermission(ISubject subject, FileEntry file, bool canRead)
        {
            //Assert.AreEqual(canRead, fileSecurity.IsShare(file, subject.ID, FilesSecurityActions.Read));
        }
    }
}
#endif