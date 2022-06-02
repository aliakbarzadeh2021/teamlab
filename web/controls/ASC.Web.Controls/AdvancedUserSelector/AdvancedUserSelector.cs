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
    [ToolboxData("<{0}:AdvancedUserSelector runat=server></{0}:AdvancedUserSelector>")]
    [AjaxNamespace("AjaxPro.AdvancedUserSelector")]
    public class AdvancedUserSelector : Control
    {
        #region Fields

        private string _selectorID = Guid.NewGuid().ToString().Replace('-', '_');
        private string _jsObjName;

        private bool isMobileVersion { get { return ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context); } }

        #endregion

        #region Properties

        public int InputWidth { get; set; }
        public Guid SelectedUserId { get; set; }

        #endregion

        #region Methods

        public AdvancedUserSelector()
        {
            InputWidth = 230;
        }

        #endregion

        #region Events

        protected override void OnLoad(EventArgs e)
        {
            base.OnInit(e);
            Utility.RegisterTypeForAjax(this.GetType());
            _jsObjName = String.IsNullOrEmpty(this.ID) ? "advancedUserSelector" + this.UniqueID.Replace('$', '_') : this.ID;

            if (!Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "ASC_Controls_AdvUserSelector_Script"))
                Page.ClientScript.RegisterClientScriptInclude("ASC_Controls_AdvUserSelector_Script",
                    Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.AdvancedUserSelector.js.AdvUserSelectorScript.js"));

            if (!Page.ClientScript.IsClientScriptBlockRegistered("ASC_Controls_AdvUserSelector_Style"))
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ASC_Controls_AdvUserSelector_Style",
                    "<link href=\"" + Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.AdvancedUserSelector.css.default.css") + "\" type=\"text/css\" rel=\"stylesheet\"/>", false);

            StringBuilder script = new StringBuilder();

            script.AppendFormat("var {0} = new ASC.Controls.AdvancedUserSelector.UserSelectorPrototype('{1}', '{0}', '&lt;{2}&gt;', '{3}', {4});\n",
                                            _jsObjName,
                                            _selectorID,
                                            Resources.AdvancedUserSelectorResource.EmptyList,
                                            Resources.AdvancedUserSelectorResource.ClearFilter,
                                            isMobileVersion.ToString().ToLower());

            var GroupsList = CoreContext.UserManager.GetDepartments().OrderBy(g => g.Name).ToList();

            var userGroupCache = new Dictionary<Guid, List<UserInfo>>();
            userGroupCache[Guid.Empty] = CoreContext.UserManager.GetUsers().ToList();
            foreach (var g in GroupsList)
            {
                userGroupCache[g.ID] = CoreContext.UserManager.GetUsersByGroup(g.ID).ToList();
                foreach (var u in userGroupCache[g.ID])
                {
                    userGroupCache[Guid.Empty].Remove(u);
                }
            }

            GroupInfo allDepGroup = new GroupInfo() { Name = Resources.AdvancedUserSelectorResource.AllDepartments };
            GroupsList.Insert(0, allDepGroup);
            var AllUsers = new List<UserInfo>();
            foreach (var ug in GroupsList)
            {
                string groupVarName = _jsObjName + "_ug_" + ug.ID.ToString().Replace('-', '_');
                script.AppendFormat("var {0} = new ASC.Controls.AdvancedUserSelector.Group('{1}', '{2}');\n", groupVarName, ug.ID, ug.Name.HtmlEncode().ReplaceSingleQuote());
                foreach (var u in userGroupCache[ug.ID].SortByUserName())
                {
                    script.AppendFormat("{2}.Users.push(new ASC.Controls.AdvancedUserSelector.User('{0}', '{1}', {2}, '{3}', '{4}'));\n",
                                        u.ID,
                                        u.DisplayUserName(true).ReplaceSingleQuote().Replace(@"\", @"\\"),
                                        groupVarName,
                                        u.Title.HtmlEncode().ReplaceSingleQuote(),
                                        u.GetSmallPhotoURL());
                    AllUsers.Add(u);
                }
                script.AppendFormat("{0}.Groups.push({1});\n", _jsObjName, groupVarName);
            }
            if (!Guid.Equals(SelectedUserId, Guid.Empty))
                script.AppendFormat("{0}.SelectedUserId = '{1}';\n", _jsObjName, SelectedUserId);
            else
                if (isMobileVersion)
                    script.AppendFormat("{0}.SelectedUserId = {0}.Me().find('option:first').attr('selected', 'selected').val();", _jsObjName);

            script.Append("jq(function(){jq(document).click(function(event){\n");
            script.Append(_jsObjName + ".dropdownRegAutoHide(event);\n");
            script.Append("}); });\n");

            Page.ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(), script.ToString(), true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div id='{0}'>", _jsObjName);

            if (isMobileVersion)
            {

                sb.AppendFormat("<select class='comboBox' style='width:{0}px;' onchange='javascript:{1}.SelectUser(this);' >", InputWidth, _jsObjName);

                var accounts = CoreContext.Authentication.GetUserAccounts().OrderBy(a => a.Department).ThenBy(a => a.Name);

                string department = "";
                foreach (var account in accounts)
                {
                    if (!String.Equals(account.Department, department, StringComparison.InvariantCultureIgnoreCase)
                        && !String.IsNullOrEmpty(account.Department.HtmlEncode()))
                    {
                        if (department != "")
                            sb.Append("</optgroup>");

                        department = account.Department;
                        sb.AppendFormat("<optgroup class='tintLight' label='{0}' style='max-width:300px;'>", department.HtmlEncode());
                    }
                    sb.AppendFormat("<option class='tintMedium' style='cursor:pointer; max-width:300px;' value='{0}' {2}>{1}</option>",
                                        account.ID,
                                        account.Name.HtmlEncode(),
                                        Guid.Equals(account.ID, SelectedUserId) ? "selected = 'selected'" : string.Empty);
                }

                sb.AppendFormat("</select>");

            }
            else
            {

                sb.AppendFormat("<div class='adv-userselector-selector-container clearFix' style='width:{0}px;'>", InputWidth);

                var valueForInput = Guid.Equals(SelectedUserId, Guid.Empty) ?
                                        string.Empty :
                                        CoreContext.UserManager.GetUsers(SelectedUserId).DisplayUserName(true).ReplaceSingleQuote();

                sb.AppendFormat(@"
<input type='text' id='inputUserName'
    oninput='javascript:{0}.SuggestUser(event);' onpaste='javascript:{0}.SuggestUser(event);' onkeyup='javascript:{0}.SuggestUser(event);'
	onclick='{0}.OnInputClick({0}, event);' onkeydown='{0}.ChangeSelection(event);'
    class='textEdit' style='width:{1}px; height:16px; border:none; padding:2px; float: left;' value='{2}'/>",
                                  _jsObjName,
                                  InputWidth - 25,
                                  valueForInput);

                sb.AppendFormat("<input id='login' name='login' value='{0}' type='hidden'/>", SelectedUserId);
                sb.AppendFormat("<div id='uSelector' class='adv-userselector-selector' onclick='javascript:{1}.RenderItems()'><img style='height:14px; width:14px; margin-left:2px; margin-top:2px;' alt='' src='{0}'></img></div>",
                                   Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.AdvancedUserSelector.images.collapse_down_dark.png"),
                                   _jsObjName);

                sb.Append("</div>");
                sb.Append("<div id='DepsAndUsersContainer' class='adv-userselector-DepsAndUsersContainer'>");
                sb.Append("  <div id='divUsers' class='adv-userselector-users'></div>");
                sb.Append("  <div id='divDepartments' class='adv-userselector-deps'></div>");
                sb.Append("</div>");

            }
            sb.Append("</div>");

            writer.Write(sb.ToString());
        }

        #endregion

    }
}