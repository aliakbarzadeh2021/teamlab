<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    CodeBehind="ProjectTemplates.aspx.cs" Inherits="ASC.Web.Projects.ProjectTemplates" %>

<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">
    <link href="<%= PathProvider.GetFileStaticRelativePath("projecttemplates.css") %>"
        rel="stylesheet" type="text/css" />

    <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("projecttemplates.js") %>"></script>

</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <div id="ProjectTemplatesMainContainer">
        <%--Create project template section--%>
        <div class="clearFix" id="CreateProjectTemplateHolder" runat="server" visible="false">
            <div class="pm-headerPanel-splitter">
                <div class="headerPanel">
                    <%=ProjectsCommonResource.Title%>
                </div>
                <input type="text" class="projectTemplatesInput" id="CreateProjectTemplateInput"
                    onkeydown="return ProjectTemplatesController.InputOnKeyDown(event, 'CreateProjectTemplateButton');"
                    autocomplete="off" maxlength="128" />
                <div class="textBigDescribe">
                    <%= ProjectResource.ProjectTemplateTitleExample%></div>
            </div>
            <div class="pm-h-line">
                <!-- -->
            </div>
            <div class="clearFix">
                <% if (ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckCreatePermission())
                   { %>
                <ascwc:ActionButton ButtonID="CreateProjectTemplateButton" ID="CreateProjectTemplateButton"
                    OnClickJavascript="ProjectTemplatesController.CreateProjectTemplate(this.id); return false;"
                    DisableInputs="true" EnableRedirectAfterAjax="true" ButtonCssClass="baseLinkButton"
                    runat="server">
                </ascwc:ActionButton>
                <a href="<%=ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesConst.ProjectTemplatesPageTitle %>"
                    class="grayLinkButton" style="margin-left: 10px;">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
                <% } %>
            </div>
            <input type="hidden" value="<%=ProjectResource.InvalidProjectTemplateTitle %>" id="InvalidProjectTemplateTitleInputHidden" />
        </div>
        <%--Project Templates collection Section--%>
        <div class="clearFix" id="ProjectTemplatesContainer" runat="server" visible="false">
            <div class="clearFix" id="ProjectTemplatesListContainer" runat="server">
            </div>
            <% if (ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckCreatePermission())
               { %>
            <a class="grayLinkButton" style="margin-top: 15px;" href="<%=ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesConst.CreateProjectTemplateUrl %>">
                <%=ProjectResource.CreateProjectTemplate%></a>
            <% } %>
        </div>
        <div runat="server" id="EmpryScreenContainer" style="display: none;">
        </div>
    </div>
</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
    <ascwc:SideActions runat="server" ID="SideActionsPanel">
    </ascwc:SideActions>
    <%--    <ascwc:SideNavigator runat="server" ID="SideNavigatorPanel">
    </ascwc:SideNavigator> --%>
</asp:Content>
