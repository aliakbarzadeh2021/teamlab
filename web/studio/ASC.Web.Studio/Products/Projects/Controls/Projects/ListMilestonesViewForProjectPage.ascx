<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListMilestonesViewForProjectPage.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Projects.ListMilestonesViewForProjectPage" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Import Namespace="ASC.Web.Projects.Configuration"%>
<%@ Import Namespace="ASC.Projects.Core.Domain"%>

<div style="padding-bottom:10px;">
<div class="headerBase"><%=MilestoneResource.Milestones %></div>
<br/>
<table class="pm-tablebase" cellpadding="5" cellspacing="0">
   
   <asp:Repeater ID="MilestonesRepeater" runat="server">
   <ItemTemplate> 
   <tr>
       <td class="borderBase">
           <div align="left">
                <%# GetCaption(Container.DataItem as Milestone)%>
           </div>
           <table width=100% cellspacing="0" cellpadding="6">
            <tr>
                <td class="column" width="43%">
                    <%# GetStatus(Container.DataItem as Milestone)%>
                </td>
                <td class="column" width="25%">
                    <%# GetTotalTasksCount(Container.DataItem as Milestone)%>
                </td>
                <td class="column" width="32%">
                    <%# GetProgress(Container.DataItem as Milestone)%>
                </td>
            </tr>
           </table>
        </td>
   </tr>
   </ItemTemplate>
   </asp:Repeater>
   </table>
</div>


<a href="milestones.aspx?prjID=<%=ProjectID %>"><%=ProjectResource.SeeAllProjectsMilestone %></a>
<br/>