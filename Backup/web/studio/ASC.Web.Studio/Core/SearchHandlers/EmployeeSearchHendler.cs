using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Studio.Core.SearchHandlers
{
    public class EmployeeSearchHendler : BaseSearchHandlerEx
    {

        public override string AbsoluteSearchURL
        {
            get
            {
                return VirtualPathUtility.ToAbsolute("~/employee.aspx")+"?empid=&depid=";
            }
        }

        public override ImageOptions Logo
        {
            get
            {
                return new ImageOptions() { ImageFileName = "people_search_icon.png", PartID = Guid.Empty };
            }
        }

        public override string PlaceVirtualPath
        {
            get
            {
                return "~/employee.aspx";
            }
        }

        public override string SearchName
        {
            get
            {
                return Resources.Resource.EmployeesSearch;
            }
        }

        public override SearchResultItem[] Search(string text)
        {
            List<UserInfo> users = new List<UserInfo>();
            List<SearchResultItem> result = new List<SearchResultItem>();

            users.AddRange(CoreContext.UserManager.Search(text, EmployeeStatus.Active));

            foreach (UserInfo user in users)
            {
                SearchResultItem sri = new SearchResultItem();
                sri.Name = user.DisplayUserName();
                sri.Description = string.Format("{0}: {1}, {2}: {3}", CustomNamingPeople.Substitute<Resources.Resource>("Department"), user.Department, CustomNamingPeople.Substitute<Resources.Resource>("UserPost"), user.Title);
                sri.URL = GetEmployeeUrl(user);
                result.Add(sri);
            }

            return result.ToArray();
        }

        protected string GetEmployeeUrl(UserInfo ui)
        {
            var currentProduct = UserOnlineManager.Instance.GetCurrentProduct();
            return CommonLinkUtility.GetUserProfile(ui.ID, currentProduct!=null? currentProduct.ProductID : Guid.Empty);

        }
    }
}
