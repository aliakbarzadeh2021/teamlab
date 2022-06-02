using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Controls.BBCodeParser;
using ASC.Web.Controls.CommentInfoHelper;
using System.Linq;
using ASC.Web.Controls.CalendarInfoHelper;
using System.Globalization;
using System.Threading;
using AjaxPro;
using ASC.Core;
using ASC.Data.Storage;
using System.IO;
using System.Web.UI.HtmlControls;
using ASC.Core.Users;
using ASC.Web.Core.Users;
using ASC.Web.Controls.Resources;

namespace ASC.Web.Controls
{
    [ToolboxData("<{0}:UserSelector runat=server></{0}:UserSelector>")]
    [AjaxNamespace("AjaxPro.UserSelector")]
    public class UserSelector : Control
    {
        #region Properties

        public string DepartmentsOnChangeFunction { get; set; }
        public string UsersOnChangeFunction { get; set; }
        public string ElementCssClass { get; set; } 

        #endregion

        #region Methods

        public string HtmlUserSelector()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<script>");
            sb.Append("function changeDepartment(){");
            sb.Append("AjaxPro.UserSelector.InitUsersDdlByDepartment(jq('#Departments option:selected').val(),");
            sb.Append("function(res){jq('#Users').html(res.value);jq('#ddlUser-1').attr('selected','selected');});}");
            sb.Append("</script>");

            sb.Append("<table cellpadding='5px' cellspacing='0px'><tr><td>");
            sb.AppendFormat("<select id='Departments' class='comboBox {2}'  onchange='javascript:changeDepartment();{0}'>{1}</select>",
                DepartmentsOnChangeFunction, InitDepartmentsDdl(), ElementCssClass);
            sb.Append("</td><td>");
            sb.AppendFormat("<select id='Users' class='comboBox {2}' onchange='{0}'>{1}</select>",
                UsersOnChangeFunction, InitUsersDdl(), ElementCssClass);
            sb.Append("</td></tr></table>");

            return sb.ToString();
        }

        public string InitUsersDdl()
        {
            var sb = new StringBuilder().AppendFormat("<option value='-1' id='ddlUser-1'>{0}</option>", UserSelectorResource.AllUsers);

            CoreContext.UserManager.GetUsers()
                .Where(u => string.IsNullOrEmpty(u.Department))
                .OrderBy(u => u, UserInfoComparer.Default)
                .ToList()
                .ForEach(u => sb.AppendFormat("<option value='{0}' id='ddlUser{0}'>{1}</option>", u.ID, u.DisplayUserName(true)));

            foreach (var g in CoreContext.GroupManager.GetGroups().OrderBy(g => g.Name))
            {
                sb.AppendFormat("<optgroup label='{0}'>", g.Name);
                foreach (var u in CoreContext.UserManager.GetUsersByGroup(g.ID).OrderBy(u => u, UserInfoComparer.Default))
                {
                    sb.AppendFormat("<option value='{0}' id='ddlUser{0}'>{1}</option>", u.ID, u.DisplayUserName(true));
                }
            }

            return sb.ToString();
        }

        public string InitDepartmentsDdl()
        {
            var sb = new StringBuilder()
                .AppendFormat("<option value='-1' id='ddlDepartment-1'>{0}</option>", UserSelectorResource.AllDepartments);
            CoreContext.UserManager
                .GetDepartments()
                .OrderBy(g => g.Name)
                .ToList()
                .ForEach(g => sb.AppendFormat("<option value='{0}' id='ddlDepartment{0}'>{1}</option>", g.ID, g.Name.HtmlEncode()));
            return sb.ToString();
        }

        [AjaxMethod]
        public string InitUsersDdlByDepartment(string department)
        {
            var departmentID = !string.IsNullOrEmpty(department) && department != "-1" ? new Guid(department) : Guid.Empty;

            if (departmentID == Guid.Empty)
            {
                return InitUsersDdl();
            }
            else
            {
                var sb = new StringBuilder().AppendFormat("<option value='-1' id='ddlUser-1'>{0}</option>", UserSelectorResource.AllUsers);
                CoreContext.UserManager
                    .GetUsersByGroup(departmentID)
                    .OrderBy(u => u, UserInfoComparer.Default)
                    .ToList()
                    .ForEach(u => sb.AppendFormat("<option value='{0}' id='ddlUser{0}'>{1}</option>", u.ID, u.DisplayUserName(true)));
                return sb.ToString();
            }
        }

        #endregion

        #region Events

        protected override void OnInit(EventArgs e)
        {
            Utility.RegisterTypeForAjax(this.GetType());
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            writer.Write(HtmlUserSelector());
        }

        #endregion

    }
}
