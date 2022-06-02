<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskDetailsView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Tasks.TaskDetailsView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Core" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>
<%@ Register Src="~/Products/Projects/Controls/TimeSpends/TimeSpendActionView.ascx" TagPrefix="ctrl" TagName="TimeSpendActionView" %>

<div id="mainpanel">

        <dl class="pm-flexible">
            <dt class="textBigDescribe">
                <%= TaskResource.CreatingDate %>: 
            </dt>
            <dd>
                <span><%= Task.CreateOn.ToString(System.DateTimeExtension.DateFormatPattern)%></span>
                
                <% if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking)) %>
                <% { %>
                <span class="splitter"></span>
                <span class="textBigDescribe"><%= ProjectsCommonResource.SpentTotally%>:</span>
                <span style="padding-left:5px;">
                    <a href="javascript:void(0)">
                        <img id="timeLogImg" align="absmiddle" style="border:0px; margin-top:-3px" onclick='<%= GetTimeTrackingAction()%>'
                          src="<%= GetTimeTrackingImagePath()  %>" title="<%= ProjectsCommonResource.TimeTracking %>" alt="<%= ProjectsCommonResource.TimeTracking %>" />
                    </a>
                    <% if(Task.Responsible == SecurityContext.CurrentAccount.ID) %>
                    <% { %>
                    <a href="javascript:void(0)">
                        <img align="absmiddle" style="border:0px; margin-top:-3px" onclick="<%=GetTimeTrackingTimerAction()%>"
                          src="<%=GetTimeTrackingTimerImagePath()%>" title="<%=ProjectsCommonResource.AutoTimer%>" alt="<%=ProjectsCommonResource.AutoTimer%>" />
                    </a>  
                    <% } %>                  
                    <a class="linkAction" href="timetracking.aspx?prjID=<%=Task.Project.ID%>&ID=<%=Task.ID%>">
                        <span id="timeLogHours">
                            <%=TaskHoursCount().ToString("N2")%>
                        </span>
                        <%=ProjectsCommonResource.Hours%>
                    </a>
                </span>
                <% } %>
                <% if (Task.Deadline != null && Task.Deadline != DateTime.MinValue) %>
                <% { %>
                <span class="splitter"></span>
                <span class="textBigDescribe"><%=TaskResource.DeadLine%>:</span>
                <span style="padding-left:5px;">
                    <%=GetTaskDeadline()%>
                </span>
                <% } %>
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
            
            <% if (Task.Milestone != 0) %>
            <% { %>  
            <dt class="textBigDescribe" style="clear:left">
                <%= MilestoneResource.Milestone %>:
            </dt>
            <dd>
                <%=ASC.Web.Projects.Classes.Global.EngineFactory.GetMilestoneEngine().GetByID(Task.Milestone).Title.HtmlEncode()  %>
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
            
            <%=GetAttachedImages()%>
        </dl>

    <%= RenderActionBlock(Task) %>   
    
</div>
<ascw:CommentsList ID="commentList" runat="server" BehaviorID="commentsList">
</ascw:CommentsList>

<asp:PlaceHolder runat="server" ID="phAddTaskPanel" />

<% if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking)) %>
<% { %>
<ctrl:TimeSpendActionView runat="server" ID="_timeSpendActionView" />
<% } %>