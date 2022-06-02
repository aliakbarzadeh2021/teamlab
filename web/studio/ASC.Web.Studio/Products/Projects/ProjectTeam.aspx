<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    CodeBehind="projectTeam.aspx.cs" Inherits="ASC.Web.Projects.ProjectTeam" %>
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
   
    jq(function()
      {                                 
          UserSelector.OnOkButtonClick = function() 
           {
              var userID = new Array();                                                                                                   
              
                jq("div.rightBox input[type=checkbox]").each(function(){
                    userID.push(jq(this).attr("id").substr(51,36));
                    });
              
              var notifyIsChecked = jq('#notify').attr("checked");  
                                                               
               AjaxPro.Project.UserManager(userID.join(","), notifyIsChecked,
               function(res)
               {
                
                 jq("#team_container").css('display','block');
                 jq("#button").css('display','block');
                 jq("#empty").css('display','none');   
                
                 jq("#team_container").html(res.value);
                 jq("#team_container").unblock();
                 
                 
                 jq("div.NewUserInTeam").each(function(){
                    jq(this).css({ "background-color": "#ffffcc" });
                    jq(this).animate({ backgroundColor: "#ffffff" }, 1000);
                    });
                     
                 jq("div.OldUserInTeam").each(function(){
                    jq(this).animate({ opacity: "hide" }, "slow");
                    });
                 
                 jq("div").each(function(){
                    jq(this).removeClass("OldUserInTeam");
                    jq(this).removeClass("NewUserInTeam");
                    });       

               });
           }       
      });
   
    function changePermission(obj, id, module)
    {
        var prjID = jq.getURLParam("prjID");
        var visible = jq(obj).hasClass("pm-projectTeam-modulePermissionOn");

        AjaxPro.Project.SetTeamSecurity(prjID, id, module, !visible, function(res)
        {
            if (res.error != null) {alert(res.error.Message);return;}
            if(visible)
                jq(obj).removeClass("pm-projectTeam-modulePermissionOn").addClass("pm-projectTeam-modulePermissionOff");
            else
                jq(obj).removeClass("pm-projectTeam-modulePermissionOff").addClass("pm-projectTeam-modulePermissionOn");
        });                
    }

    jq(document).click(function(event) 
    {
        var elt = (event.target) ? event.target : event.srcElement;
        var isHide = true;
        if(jq(elt).hasClass('pm-dropdown-item'))
        {
            jq("#actionPanel").hide();
        }
        if (jq(elt).is('[id="actionPanel"]') || jq(elt).is('[id^="actionPanelSwitcher_"]'))
            isHide = false;
        if (isHide)
            jq(elt).parents().each(function() {
                if (jq(this).is('[id="actionPanel"]') || jq(this).is('[id^="actionPanelSwitcher_"]')) {
                    isHide = false;
                }
            });    

        if (isHide)
            jq("#actionPanel").hide();
    });
    
    function showActions(obj, id)
    {
        var prjID = jq.getURLParam("prjID");
        AjaxPro.Project.GetActionContent(prjID, id, function(res){
            if (res.error != null) {alert(res.error.Message);return;}
            jq('#actionPanelContent').html(res.value);
            ASC.Projects.Common.dropdownToggle(jq(obj).children()[1],'actionPanel',5,-20);
            jq("#actionPanel").show();
        });
    }

   </script>
    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>"></script>
    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>
    <link href="<%= PathProvider.GetFileStaticRelativePath("projectTeam.css") %>" rel="stylesheet" type="text/css" />

</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
       
    <div class="headerBase pm-headerPanel-splitter">
        <%= ProjectResource.ProjectLeader%>
    </div>
    
    <div class="pm-headerPanel-splitter clearFix">
        <div class="pm-projectTeam-projectLeaderCard" style="float:left;">
            <asp:PlaceHolder runat="server" ID="_phProjectLeaderCard" />
        </div>
        <div style="margin-left: 380px;padding: 0 20px;">
            <div><%=ProjectResource.ClosedProjectTeamManagerPermission%>:</div>
            <div class="pm-projectTeam-infoText"><%=ProjectResource.ClosedProjectTeamManagerPermissionArticleA%></div>
            <div class="pm-projectTeam-infoText"><%=ProjectResource.ClosedProjectTeamManagerPermissionArticleB%></div>
            <div class="pm-projectTeam-infoText"><%=ProjectResource.ClosedProjectTeamManagerPermissionArticleC%></div>
        </div>
    </div>
    
    <div class="pm-headerPanel-splitter">
        <div class="headerBase pm-headerPanelSmall-splitter">
            <% if (ASC.Projects.Engine.ProjectSecurity.CanEditTeam(Project)) %>
            <% { %>
            <div class="pm-projectTeam-container" id="button" style="float:right;">
                <a class="baseLinkButton" onclick="javascript:UserSelector.ShowDialog();"><%=ProjectResource.ManagmentTeam%></a>    
            </div>
            <% } %>
 <%= ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ProjectResource>("EmployeesAndPermissions")%>
        </div>
        <% if (Project.Private) %>
        <% { %>
        <div style="background-color:#F2F2F2;padding: 10px 20px;">
            <table width="100%" cellpadding="3" cellspacing="0">
                <tbody>
                    <tr>
                        <td><b><%=MessageResource.Messages%></b></td>
                        <td><b><%=ProjectsFileResource.Documents%></b></td>
                        <td><b><%=TaskResource.AllTasks%></b></td>
                        <td><b><%=MilestoneResource.Milestones%></b></td>
                    </tr>
                    <tr>
                        <td width="25%" valign="top"><%=ProjectResource.ClosedProjectTeamDiscussionsInfoPanel%></td>
                        <td width="25%" valign="top"><%=ProjectResource.ClosedProjectTeamDocumentsInfoPanel%></td>
                        <td width="25%" valign="top"><%=ProjectResource.ClosedProjectTeamAllTasksInfoPanel%></td>
                        <td width="25%" valign="top"><%=ProjectResource.ClosedProjectTeamMilestonesInfoPanel%></td>
                    </tr>
                </tbody>
            </table>
        </div>
        <% } %>
    </div>
   
    <div id="team_container" class="pm-headerPanel-splitter">
        <asp:Literal runat="server" ID="_ltlTeam" />        
    </div>
     
    <% if (ASC.Projects.Engine.ProjectSecurity.CanEditTeam(Project)) %>
    <% { %>
    <div class="pm-projectTeam-container" id="button">
        <asp:PlaceHolder runat="server" ID="_phUserSelector" />
        <a class="baseLinkButton" onclick="javascript:UserSelector.ShowDialog();"><%=ProjectResource.ManagmentTeam%></a>    
    </div>
    <% } %>
    
    <asp:PlaceHolder runat="server" ID="phAddTaskPanel" />

</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
</asp:Content>
