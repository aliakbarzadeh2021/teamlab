<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListTaskView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Tasks.ListTaskView" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Projects.Core" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Src="TaskBlockView.ascx" TagPrefix="ctrl" TagName="TaskBlockView" %>
<%@ Register Src="TaskActionView.ascx" TagPrefix="ctrl" TagName="TaskActionView" %>
<%@ Register Src="MoveTaskView.ascx" TagPrefix="ctrl" TagName="MoveTaskView" %>

<script type="text/javascript" language="javascript">

jq(document).ready(function(){

ASC.Projects.Common.tooltip(".pm-task-title a","tooltip");

});
 <% if (!IsAllMyTasks)  %>
 <% { %>
 jq(document).click(function(event)
 {

 ASC.Projects.Common.dropdownRegisterAutoHide(event, "#interval_switcher","#interval_dropdown");

 });
 ASC.Projects.TaskActionPage.isAllMyTasks = false;
 ASC.Projects.TaskActionPage.isOneList = false; 
 <% } %>
 <% else %>
 <% { %>
 ASC.Projects.TaskActionPage.isAllMyTasks = true;
 <% if (OneList)  %>
 <% { %>
  ASC.Projects.TaskActionPage.isOneList = true;
 <% } %>
 <% } %>


</script>

<style  type="text/css">

.pm_projectName
{
	padding: 18px 0px;
	overflow-x: hidden;
	
}
.pm_projectName a
{
	font-size:18px;	
    color:#323232;
    text-decoration:none;
}
.pm_projectName a:hover
{
	font-size:18px;	
    color:#323232;
    text-decoration:underline;
}
#filterBlock
{

    text-align:right;
    border-left: medium none; 
    border-right: medium none; 
    border-top: medium none;
    padding: 10px 0px;
    margin-bottom: 30px;
    
}
<% if (IsAllMyTasks)  %>
<% { %> 
.headerBase
{
font-size:14px !important;
font-weight:bolder;
}
.linkHeader
{
font-size:14px !important;
font-weight:bolder;
	}
.pm_projectName
{
padding-bottom:0px;
padding-top:10px;
font-size:18px !important;
<% if (OneList)  %>
<% { %> 
display:none !important;
<% } %> 

}
.pm-milestoneWithTasksBlock
{
	<% if (OneList)  %>
    <% { %> 
    padding:0 !important;
    <% } %> 
    <% else if (IsAllMyTasks) %>
    <% { %> 
    padding-top:5px !important;
    padding-bottom:15px !important;
    
    <% } %> 
    
}

<% if (OneList)  %>
<% { %> 
.pm-milestoneWithTasksBlock div.header
{
display:none !important;
}
<% } %> 

a.linkHeaderLightMedium
{
	font-size:12px !important;
	font-weight:normal !important;
}
<% } %> 

</style>

<div>
<% if (!IsAllMyTasks)  %>
            <% { %> 
    <div class='clearFix borderBase' id='filterBlock'>
      <div align="left">         
            <asp:PlaceHolder ID="_tabsContainer" runat="server"></asp:PlaceHolder> 
      </div>        
    </div> 
    <% } %> 
   
    <% if (IsAllMyTasks)  %>
    <% { %>  <div class="pm_projectName">
    <div class="pm_projectIcon"></div><a href="projects.aspx?prjID=<%=ProjectId %>"><%= ProjectName%></a>
    </div>
    <% } %> 
    
    <asp:Repeater runat="server" ID="_rptContent"  OnItemDataBound="rptContent_OnItemDataBound">
        <ItemTemplate>           
            <% if (!IsAllMyTasks)  %>
            <% { %>
            <div class="pm-milestoneWithTasksBlock" "<%# (Eval("Milestone") is Milestone) ? "" : "style='padding-bottom:0px;'" %>"  id="milestoneWithTasksBlock_<%# Eval("Milestone") != null ? ((Milestone)Eval("Milestone")).ID  : 0%>">
            <% } %>
            <% else %>
            <% { %>
            <div class="pm-milestoneWithTasksBlock" "<%# (Eval("Milestone") is Milestone) && !IsAllMyTasks ? "" : "style='padding-bottom:0px;padding-left:30px;'" %>"  id="milestoneWithTasksBlock_<%# Eval("Milestone") != null ? ((Milestone)Eval("Milestone")).ID  : 0%>">
            <% } %>
                
                <div class='clearFix header' style="padding-bottom: <%= (IsAllMyTasks ? "3px" : "10px")  %>; ">                                     
                    <div style="float: right;margin-left: 5px;" class="clearFix">
                        <%#  RenderAddTaskButton(Eval("Milestone") as Milestone)%>
                    </div>
                    <div  class='clearFix'>
                        <%#  RenderInfoBlock(Eval("Milestone") as Milestone)%>
                    </div>
                </div>
                <% if (IsAllMyTasks && !OneList)  %>
                <% { %>
                <div style="padding-left:30px;">
                <% } %>                 
                <ctrl:TaskBlockView runat="server" ID="_taskBlockView" />
                <% if (IsAllMyTasks && !OneList)  %>
                <% { %>
                </div>
                <% } %>
                
            </div>
        </ItemTemplate>      
    </asp:Repeater>  
    
    <% if (_rptContent.Items.Count == 0) %>    
    <% { %>
        <asp:PlaceHolder runat="server" ID="_phEmptyScreen" ></asp:PlaceHolder>
    <% } %>  
        

        <ctrl:TaskActionView runat="server"  ID="_taskActionView"/>
        <ctrl:MoveTaskView runat="server"  ID="_moveTaskView"/>

     
</div>
