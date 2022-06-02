<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListMilestonesTemplate.ascx.cs"
	Inherits="ASC.Web.Projects.Controls.ProjectTemplates.ListMilestonesTemplate" %>

<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<style>
	.pm-tablebase thead tr td
	{
		padding: 5px 15px;
		border-left: medium none !important;
		border-right: medium none !important;
	}
</style>
<div class="clearFix">
	
	<div class="pm-headerPanel-splitter">
		<table class="pm-tablebase" cellpadding="15" cellspacing="0">
			<thead>
				<tr>
					<td class="borderBase" width="80px" style="padding-left: 0px;">
					</td>
					<td class="borderBase">
						<%=MilestoneResource.MilestoneTitle%>
					</td>
					<td class="borderBase" width="50px" style="text-align: center;">
						<%=TaskResource.Tasks%>
					</td>
				</tr>
			</thead>
			<tbody>
				<asp:Repeater ID="MilestoneTemplatesRepeater" runat="server">
					<ItemTemplate>
						<tr>
							<td class="borderBase" style="text-align: center;">
								<%#GetMilestoneDeadLine(Container.DataItem as TemplateMilestone)%>
							</td>
							<td class="borderBase">
								<div style="padding-bottom: 5px; overflow: hidden; width: 500px;">
									<%# GetCaption(Container.DataItem as TemplateMilestone)%>
								</div>
							</td>
							<td class="borderBase" style="text-align: center;">
								<a href="<%# GenerateMilestoneLink(Container.DataItem as TemplateMilestone) %>" class="linkHeaderLightMedium">
									<%# (Container.DataItem as TemplateMilestone).TasksCount%>
								</a>
							</td>							
						</tr>
					</ItemTemplate>
				</asp:Repeater>
			</tbody>
		</table>
	</div>
</div>
