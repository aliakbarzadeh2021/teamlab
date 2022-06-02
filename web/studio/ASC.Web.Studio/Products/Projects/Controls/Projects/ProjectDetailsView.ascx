<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectDetailsView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Projects.ProjectDetailsView" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Import Namespace="ASC.Web.Projects.Configuration"%>
<%@ Import Namespace="ASC.Projects.Core.Domain"%>

<%=GetProjectDescription()%>

<% if (GetActiveMilestonesCount() > 0) %>
<% {%>
<div class="pm-headerPanel-splitter">
<div class="headerBase pm-headerPanel-splitter"><%=MilestoneResource.Milestones%></div>
<asp:PlaceHolder runat="server" ID="content"></asp:PlaceHolder>
<a href="milestones.aspx?prjID=<%=ProjectFat.Project.ID %>"><%=ProjectResource.SeeAllProjectsMilestone%></a>
</div>
<% }%>

<div class="pm-headerPanel-splitter">
    <div class="headerBase pm-headerPanel-splitter"><%=ProjectResource.ProjectTeam%></div>
    <div class="textBigDescribe pm-projectLabelBox"><%=ProjectResource.ProjectLeader%> </div>
    <div class="pm-projectTeam-projectLeaderCard">
        <asp:PlaceHolder runat="server" ID="user_content">
        </asp:PlaceHolder>
    </div>
    <div class="textBigDescribe" style="padding-top:10px;">
        <%=ProjectResource.ProjectTeam %>:
        <a href="projectteam.aspx?prjID=<%=ProjectFat.Project.ID %>" ><%=GetGrammaticalHelperParticipantCount()%></a>
    </div>
</div>

<% if (LastActivity.Count > 0) %>
<% {%>
<div>
<div class="headerBase pm-headerPanel-splitter"><%=ProjectResource.LastActivity%></div>
<asp:PlaceHolder runat="server" ID="phTimeLine" />
<a href="history.aspx?prjID=<%=ProjectFat.Project.ID %>"><%=ProjectResource.SeeHistory%></a>
</div>
<% }%>

<asp:PlaceHolder runat="server" ID="phAddTaskPanel" />

<script type="text/javascript">
jq("#ActiveMilestonesHeader").remove();
</script>