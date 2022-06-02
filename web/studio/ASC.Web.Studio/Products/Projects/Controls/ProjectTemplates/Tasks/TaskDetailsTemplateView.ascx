<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskDetailsTemplateView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Tasks.TaskDetailsTemplateView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Core.Users" %>


<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>

<div id="mainpanel">



        <dl class="pm-flexible">
            
            <dt class="textBigDescribe">
                <%= TaskResource.CreatingDate %>: 
            </dt>
            <dd>
                <%= Task.CreateOn.ToString(System.DateTimeExtension.DateFormatPattern)%>
            </dd>
            
            <dt class="textBigDescribe">
                <%= TaskResource.TaskProducer %>: 
            </dt>
            <dd>
                <%= StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(Task.CreateBy), ProductEntryPoint.ID)%>
                
                <span class="splitter"></span>
                <span class="textBigDescribe"><%= TaskResource.TaskResponsible%>:</span>
                <span style="padding-left:5px;"><%= GetTaskResponsible()%></span>
                
            </dd>
            
            <% if (Task.MilestoneId != 0) %>
            <% { %>  
            <dt class="textBigDescribe" style="clear:left">
                <%= MilestoneResource.Milestone %>:
            </dt>
            <dd>
                <%=MilestoneTitle  %>
            </dd>
            <% } %>
            
            <% if (!String.IsNullOrEmpty(Task.Description.Trim())) %>
            <% { %>
            <dt class="textBigDescribe">
                <%=TaskResource.TaskDescription%>: 
            </dt>
            <dd>
				<%=Task.Description.Trim().HtmlEncode().Replace("\n","<br/>") %>
            </dd>
            <% } %>
        </dl>
    

    


    
    <%= RenderActionBlock(Task) %>   
    
</div>

<asp:PlaceHolder runat="server" ID="phAddTaskPanel" />