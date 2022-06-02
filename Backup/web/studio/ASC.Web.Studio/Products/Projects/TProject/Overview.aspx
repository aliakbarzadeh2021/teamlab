<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
	CodeBehind="Overview.aspx.cs" Inherits="ASC.Web.Projects.TProject.Overview" %>

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
	<div class="clearFix" style="padding: 52px 82px 53px 61px; background-color: White;">
		<img src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("pic_template.png", ASC.Web.Projects.Configuration.ProductEntryPoint.ID) %>"
			style="float: left;" alt="Project Template Pic" />
		<div style="margin-left: 34px; margin-top: 12px; width: 638px; float: left;">
			<div class="headerBase">
				<%=ProjectResource.ProjectTemplateSetUpLabel%></div>
			<div style="margin-top: 17px;">
				<%=String.Format(ProjectResource.ProjectTemplateSetUpDescription,"<br/>")%></div>
			<ul type="square" style="margin-top: 14px;">
				<li style="margin-bottom: 5px"><a class="linkHeaderLightSmall" href="<%=GetProjectTemplateUrl("milestones")  + "&action=add"%>">
					<%=ProjectResource.ProjectTemplateSetupMilestone%></a></li>				
				<li style="margin-bottom: 5px"><a class="linkHeaderLightSmall" href="<%=GetProjectTemplateUrl("tasks") + "&addTask=true" %>">
					<%=ProjectResource.ProjectTemplateSetupTask%></a></li>
					<li style="margin-bottom: 5px"><a class="linkHeaderLightSmall" href="<%=GetProjectTemplateUrl("messages") + "&action=add" %>">
					<%=ProjectResource.ProjectTemplateSetupDiscussion%></a></li>
				<li style="margin-bottom: 5px"><a class="linkHeaderLightSmall" href="<%=GetProjectTemplateUrl("team") + "&initTeam=true"  %>">
					<%=ProjectResource.ProjectTemplateSetupTeam%></a></li>
			</ul>
		</div>
	</div>
</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
	<%--    <ascwc:SideActions runat="server" ID="SideActionsPanel">
    </ascwc:SideActions>
    <ascwc:SideNavigator runat="server" ID="SideNavigatorPanel">
    </ascwc:SideNavigator> --%>
</asp:Content>
