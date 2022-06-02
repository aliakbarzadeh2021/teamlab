#if DEBUG
namespace ASC.Core.Common.Tests
{
    using System;
    using System.Threading;
    using ASC.Core.Users;
    using NUnit.Framework;

    [TestFixture]
    public class UserManagerTest
    {
        [Test]
        public void SearchUsers()
        {
            CoreContext.TenantManager.SetCurrentTenant(0);

            var users = CoreContext.UserManager.Search(null, EmployeeStatus.Active);
            CollectionAssert.IsEmpty(users);

            users = CoreContext.UserManager.Search("", EmployeeStatus.Active);
            CollectionAssert.IsEmpty(users);

            users = CoreContext.UserManager.Search("  ", EmployeeStatus.Active);
            CollectionAssert.IsEmpty(users);

            users = CoreContext.UserManager.Search("АбРаМсКй", EmployeeStatus.Active);
            CollectionAssert.IsEmpty(users);

            users = CoreContext.UserManager.Search("АбРаМсКий", EmployeeStatus.Active);
            CollectionAssert.IsEmpty(users);//Абрамский уволился

            users = CoreContext.UserManager.Search("АбРаМсКий", EmployeeStatus.All);
            CollectionAssert.IsNotEmpty(users);

            users = CoreContext.UserManager.Search("иванов николай", EmployeeStatus.Active);
            CollectionAssert.IsNotEmpty(users);

            users = CoreContext.UserManager.Search("ведущий програм", EmployeeStatus.Active);
            CollectionAssert.IsNotEmpty(users);

            users = CoreContext.UserManager.Search("иванов николай", EmployeeStatus.Active, new Guid("cc8eea30-1260-427e-83c4-ff9e9680edba"));
            CollectionAssert.IsNotEmpty(users);

            users = CoreContext.UserManager.Search("иванов николай", EmployeeStatus.Active, new Guid("613fc896-3ddd-4de1-a567-edbbc6cf1fc8"));
            CollectionAssert.IsEmpty(users);
        }

        //[Test]
        public void DepartmentManagers()
        {
            CoreContext.TenantManager.SetCurrentTenant(1024);
            
            var deps = CoreContext.UserManager.GetDepartments();
            var users = CoreContext.UserManager.GetUsers();
            var users2 = CoreContext.UserManager.GetUsers();

            var g1 = deps[0];
            var g2 = deps[1];
            var ceo = users[0];
            var u1 = users[1];
            var u2 = users[2];
            var u3 = users[3];

            //проверка кэша ceo
            var ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            CoreContext.UserManager.SetCompanyCEO(ceo.ID);
            ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            Assert.AreEqual(ceo, ceoTemp);

            Thread.Sleep(TimeSpan.FromSeconds(6));
            ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            Assert.AreEqual(ceo, ceoTemp);

            //установка манагеров
            CoreContext.UserManager.SetDepartmentManager(g1.ID, u1.ID);

            var groupUsers = CoreContext.UserManager.GetUsersByGroup(g1.ID);
            CoreContext.UserManager.SetDepartmentManager(g1.ID, u2.ID);
        }

        //[Test]
        public void UserGroupsTest()
        {
            CoreContext.TenantManager.SetCurrentTenant(0);
            var groups = CoreContext.UserManager.GetUserGroups(new Guid("25685f12-8bdb-4e72-bded-2a4323bde24b"));
            var users = CoreContext.UserManager.GetUsersByGroup(Constants.GroupVisitor.ID);
        }
    }
}
#endif
