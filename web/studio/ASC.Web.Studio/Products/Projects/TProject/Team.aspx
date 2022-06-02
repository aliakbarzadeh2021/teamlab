<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="Team.aspx.cs" Inherits="ASC.Web.Projects.TProject.Team" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Web.Studio.Controls.Common" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>


<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">
   
    <script type="text/javascript" language="javascript">

jq(function() {
	UserSelector.OnOkButtonClick = function() {
		AjaxPro.onLoading = function(b) {
			if (b)
				jq("#team_container").block();
		}

		var userID = new Array();

		jq("div.rightBox input[type=checkbox]").each(function() {
			userID.push(jq(this).attr("id").substr(51, 36));
		});

		var notifyIsChecked = jq('#notify').attr("checked");

		ProjectTemplateTeam.UserManager(userID.join(","), notifyIsChecked,
                                           function(res) {
                                           		jq("#team_container").css('display', 'block');
                                           		jq("#button").css('display', 'block');
                                           		jq("#empty").css('display', 'none');

                                           		jq("#team_container").html(res.value);
                                           		jq("#team_container").unblock();


                                           		jq("div.NewUserInTeam").each(function() {
                                           			jq(this).animate({ backgroundColor: "#ffffcc" }, 500)
													.animate({ backgroundColor: "#ffffff" }, 500);
                                           		});

                                           		jq("div.OldUserInTeam").each(function() {
                                           			jq(this).animate({ opacity: "hide" }, "slow");
                                           		});

                                           		jq("div").each(function() {
                                           			jq(this).removeClass("OldUserInTeam");
                                           			jq(this).removeClass("NewUserInTeam");
                                           		});

                                           		manageEmptyScreen();
                                           }
                                        );
	}
	manageEmptyScreen();
});

function manageEmptyScreen() {
	if (jq('#team_container').children().length < 1) {
		jq('#MainPageContainer').hide();
		jq('div[id$=_EmptyScreenContainer]').show();
	} else {
		jq('#MainPageContainer').show();
		jq('div[id$=_EmptyScreenContainer]').hide();
	}
}
   
   
   </script>
    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>"></script>
    <link href="<%= PathProvider.GetFileStaticRelativePath("projectTeam.css") %>" rel="stylesheet" type="text/css" />
    <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("projecttemplates.js") %>"></script>

</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">

	<div id="MainPageContainer">    
		<div id="team_container" class="pm-headerPanel-splitter">
			<asp:Literal runat="server" ID="_ltlTeam" />        
		</div>
	     
	     <%if (ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckEditPermission(Template)) { %>
		<div class="pm-projectTeam-container" id="button">
			<asp:PlaceHolder runat="server" ID="_phUserSelector" />			
			<a class="baseLinkButton" onclick="javascript:UserSelector.ShowDialog();"><%=ProjectResource.ManagmentTeam%></a>			
		</div>
		<%} %>
    </div>
    
    <div id="EmptyScreenContainer" runat="server" style="display: none;">
    </div>

</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
    <ascwc:SideActions runat="server" ID="SideActionsPanel">
    </ascwc:SideActions>
</asp:Content>
