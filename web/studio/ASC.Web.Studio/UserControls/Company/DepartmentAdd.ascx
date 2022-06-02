<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DepartmentAdd.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Company.DepartmentAdd" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<div id="studio_departmentAddDialog" style="display: none;">
    <ascwc:Container runat="server" ID="_departmentAddContainer">
        <Header>
            <%= ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("AddDepartmentDlgTitle")%>
        </Header>
        <Body>
            <asp:HiddenField runat="server" ID="_depProductID" />
            <input type="hidden" id="addDepartment_infoID" value="<%=_departmentAddContainer.ClientID%>_InfoPanel" />
            <div class="clearFix">
                <div>
                    <%=Resources.Resource.Title%>:
                </div>
                <div style="margin: 3px 0px;">
                    <input type="text" id="studio_newDepName" class="textEdit" style="width: 99%;" maxlength="100" />
                    </div>
            </div>
            <div class="clearFix" style="margin-top: 10px;">
                <div class="clearFix">
                    <%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("DepartmentMaster")%>:
                </div>
                <div style="margin: 3px 0px;">
                    <select  class="comboBox"  id="studio_addep_user_selector" style="width:99%;">
                        <option value="<%=Guid.Empty%>"></option>
                        <%foreach (ASC.Core.Users.UserInfo user in GetSortedUsers())
                          {%>
                        <option value="<%=user.ID%>" >
                            <%=DisplayUserSettings.GetFullUserName(user).HtmlEncode()%></option>
                        <%}%>
                    </select>
                    </div>
            </div>
            <div id="dep_panel_buttons" class="clearFix" style="margin-top: 16px;">
                <a class="baseLinkButton" style="float: left;" href="#" onclick="StudioManagement.AddDepartmentCallback('<%=ProductID %>');">
                    <%=Resources.Resource.AddButton%></a> <a class="grayLinkButton" style="float: left;
                        margin-left: 8px;" href="#" onclick="StudioManagement.CloseAddDepartmentDialog();">
                        <%=Resources.Resource.CancelButton%></a>
            </div>
            <div style="padding-top: 16px; display: none;" id="dep_action_loader" class="clearFix">                                    
                <div class="textMediumDescribe">
                    <%=Resources.Resource.PleaseWaitMessage%>
                </div>
                <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>">
            </div>
        </Body>
    </ascwc:Container>
</div>
