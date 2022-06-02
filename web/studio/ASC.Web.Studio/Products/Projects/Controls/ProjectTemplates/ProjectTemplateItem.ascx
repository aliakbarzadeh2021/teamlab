<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectTemplateItem.ascx.cs" Inherits="ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplateItem" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<div class="clearFix projectTemplateContainer" id="ProjectTemplateHolder_<%=Template.Id %>" onmouseover="ProjectTemplatesController.ShowPopupItem('ProjectTemplateHolderPopup_<%=Template.Id %>', true, 'ProjectTemplateHolder_<%=Template.Id %>', 'projectTemplatesTableItemActive');" onmouseout="ProjectTemplatesController.ShowPopupItem('ProjectTemplateHolderPopup_<%=Template.Id %>', false, 'ProjectTemplateHolder_<%=Template.Id %>', 'projectTemplatesTableItemActive');">
		<%--<a href="<%=TemplateDefaultUrl() %>" class="linkHeaderLightBig"><%=Template.Title %></a>--%>
		<div style="float: left; margin-left: 15px;overflow:hidden;<%=GetContentWidth%>">
			<a href="<%=TemplateMilestoneUrl() %>" class="linkHeaderLightSmall"><%=Template.Title.HtmlEncode() %></a>
			<span style="margin-left: 13px;white-space:nowrap;" class="projectTemplatesTextDescribe">(<%=MilestonesCount%>, <%=MessagesCount%>, <%=TasksCount %>, <%=EmployeesCount %>)</span>
		</div>
		<div style="float: right; display: none;" id="ProjectTemplateHolderPopup_<%=Template.Id %>">
			<%if(ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckEditPermission(Template)) { %>
			<div class="clearFix">
				<span class="pm_deleteLinkCSSClass" style="float: right; margin-right: 16px; margin-bottom: -2px;" onclick="ProjectTemplatesController.DeleteProjectTemplate(<%=Template.Id %>, true);" title="<%=ProjectResource.DeleteProjectTemplate %>">&nbsp;</span>
				<a href="<%=string.Format(ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesConst.CreateProjectFromTemplatePath, Template.Id) %>" class="linkDescribe" style="float: right; margin-right: 17px;"><%=ProjectResource.CreateProjectFromTemplate%></a>
			</div>
			<%} %>
		</div>
</div>


