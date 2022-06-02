using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Controls;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Reports;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using System.Globalization;
using System.Threading;

namespace ASC.Web.Projects
{
    public partial class Templates : BasePage
    {
        protected override void PageLoad()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(Reports));
            ((ASC.Web.Projects.Masters.BasicTemplate)(base.Master)).BreadCrumbs.Add(new BreadCrumb { Caption = ReportResource.Reports, NavigationUrl = "reports.aspx" });

            ((ASC.Web.Projects.Masters.BasicTemplate)(base.Master)).BreadCrumbs.Add(new BreadCrumb { Caption = ReportResource.MyTemplates });

            Title = HeaderStringHelper.GetPageTitle(ReportResource.Reports, ((ASC.Web.Projects.Masters.BasicTemplate)(base.Master)).BreadCrumbs);

            InitActionPanel();
        }

        public void InitActionPanel()
        {
            SideNavigatorPanel.Controls.Add(new NavigationItem
            {
                Name = ReportResource.CreateNewReport,
                URL = "reports.aspx"
            });/*
            SideNavigatorPanel.Controls.Add(new NavigationItem
            {
                Name = ReportResource.ReportHistory,
                URL = "reports.aspx"
            });*/
            SideNavigatorPanel.Controls.Add(new NavigationItem
            {
                Name = ReportResource.MyTemplates,
                URL = "templates.aspx"
            });
        }

    }
}
