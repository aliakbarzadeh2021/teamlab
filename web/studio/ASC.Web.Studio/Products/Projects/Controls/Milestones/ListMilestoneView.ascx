<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListMilestoneView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Milestones.ListMilestoneView" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<div id="activeMilestones">
   <table width="100%" cellspacing="0" cellpadding="0" border="0" style="padding-top: 18px;" class="tableContainer">
   <asp:Repeater ID="MilestonesRepeater" runat="server">
   <ItemTemplate> 
   <tr>
       <td align="left" style="padding: 15px 0 15px 0;">
           <div align="left">
               <span style="color:#82878D; font-size:16px;"><%# DataBinder.Eval(Container.DataItem, "DeadLine")%></span>
               <a class="headerPanel" style='<%# DataBinder.Eval(Container.DataItem, "TitleColor")%>' href="Milestones.aspx?ID=<%# DataBinder.Eval(Container.DataItem, "MilestoneID")%>"><%# DataBinder.Eval(Container.DataItem, "MilestoneTitle")%></a>
           </div>
           <div style="float:left">
               <span class="headerColumn" style="margin-left:45px;"><%=MilestoneResource.Status %>:</span>
               <span class="headerBaseSmall"><%# DataBinder.Eval(Container.DataItem, "MilestoneStatus")%></span>
           </div>
           <div align="right">
               <span class="headerColumn"><%=MilestoneResource.Total %>:</span>
               <a style="margin-right:20px;" href="Milestones.aspx?ID=<%# DataBinder.Eval(Container.DataItem, "MilestoneID")%>"><%# DataBinder.Eval(Container.DataItem, "TaskCount")%> <%=ProjectResource.Tasks %></a>
               <span class="headerColumn" style=""><%=MilestoneResource.Progress %>:</span>
               <a href="Milestones.aspx?ID=<%# DataBinder.Eval(Container.DataItem, "MilestoneID")%>"><%# DataBinder.Eval(Container.DataItem, "ActiveTasksCount")%> <%=MilestoneResource.TasksNotDone %></a>
           </div>
        </td>
   </tr>
   </ItemTemplate>
   </asp:Repeater>
    
   <tr><td></td></tr>
   </table>
</div>