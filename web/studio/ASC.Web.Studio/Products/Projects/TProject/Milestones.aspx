<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="Milestones.aspx.cs" Inherits="ASC.Web.Projects.TProject.Milestones" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>    

<%@ Import Namespace="ASC.Web.Studio.Controls.Common" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">
	<link href="<%= PathProvider.GetFileStaticRelativePath("projecttemplates.css") %>" rel="stylesheet" type="text/css" />
    <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("projecttemplates.js") %>"></script>
    
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("milestones.js") %>"></script>
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>
   
   
    <link href="<%= PathProvider.GetFileStaticRelativePath("milestones.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("common.css") %>" rel="stylesheet" type="text/css" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("tasks.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("timetracking.css") %>" rel="stylesheet" type="text/css" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("dashboard.css")%>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">

	<%--Milestones--%>
	<div id="MilestonesHolder" runat="server" visible="false">
	</div>
	
	<%--Milestone Info--%>
	<div id="MilestoneInfoPanel" runat="server" visible="false">		
	</div>


	<%--Create milestone--%>
	<div id="CreateMilestonePanel" runat="server" visible="false">
		<div class="pm-headerPanel-splitter">
            <div class="headerPanel"><%=MilestoneResource.MilestoneTitle %></div>            
            <input class="textEdit" ID="MilestoneTitleInput" style="width: 100%;" maxlength="100" onkeydown="return ProjectTemplatesController.InputOnKeyDown(event, 'CreateMilestoneButton');"/>
            <div class="textBigDescribe"><%=MilestoneResource.Example %></div>
            <asp:HiddenField ID="hfMilestoneTitle" runat="server" />
        </div>
        
        <div class="pm-headerPanel-splitter">
			<div class="headerPanel"><%=MilestoneResource.MilestoneDate %></div>
			<select id="MilestoneWeekCombobox" style="margin-left: 0px;">
				<%for(int i=1; i<=52; i++){ %>
					<option value="<%=i %>"><%=ReportResource.Week %> <%=i %></option>
				<%} %>
			</select>
			
			<select id="MilestoneDayCombobox" style="margin-left: 15px;">
				<%for (int i = FirstDay; i <= 6; i++) { %>
					<option value="<%=i %>"><%=DayName(i)%></option>
				<%} %>
				<%if(FirstDay != 0){ %>
					<option value="0"><%=DayName(0)%></option>
				<%} %>
			</select>
        </div>
		
		
		<div class="clearFix projectTemplatesCheckboxContainer" style="margin-top: 20px;">
			<input id="notify_manager" checked='checked' type="checkbox" style="margin-left: 0px;"/>
			<label for="notify_manager">
				<%= MilestoneResource.RemindMe %></label>
		</div>
		<div class="clearFix projectTemplatesCheckboxContainer" style="margin-top: 10px;">
			<input id="is_key" type="checkbox" style="margin-left: 0px;"/>
			<label for="is_key">
				<%= MilestoneResource.RootMilestone%></label>
		</div>
		
		<div class="pm-h-line"><!-- --></div>
        
        <div class="clearFix">
						
			<ascwc:ActionButton ButtonID="CreateMilestoneButton" ID="CreateMilestoneButton"  EnableRedirectAfterAjax="true"
					OnClickJavascript="ProjectTemplatesController.CreateMilestone(); return false;" DisableInputs="true"
					ButtonCssClass="baseLinkButton" runat="server"></ascwc:ActionButton>
			
			<a href="<%=MilestonesPath %>" id="CreateMilestoneCancelButton" class="grayLinkButton" style="margin-left: 10px;">
				<%= ProjectsCommonResource.Cancel %>
			</a>
		</div>		
    </div>
    
    <input type="hidden" id="ProjectIdHiddenInput" value="<%=Template.Id%>" />
    
    <%if(Milestone !=null){ %>
			<input type="hidden" id="MilestoneIdHiddenInput" value="<%=Milestone.Id %>" />
		<%} %>
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >
    <ascwc:SideActions runat="server" ID="SideActionsPanel"> 
    </ascwc:SideActions>
</asp:Content>
