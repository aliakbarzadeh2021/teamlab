<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskActionTemplateView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Tasks.TaskActionTemplateView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<style type="text/css">
    .headerPanelSmall
    {
        margin-bottom: 5px;
    }
    .pm-headerPanelSmall-splitter
    {
        margin-bottom: 10px;
    }
</style>

<script type="text/javascript" language="javascript">

 jq(document).click(function(event)
 {
 	ASC.Projects.Common.dropdownRegisterAutoHide(event, "#userSelector_switcherContainer", "#userSelector_dropdown");
 	ASC.Projects.Common.dropdownRegisterAutoHide(event, "#milestoneSelector_switcherContainer", "#milestoneSelector_dropdown");
 });

 jq(function() { jq("#addTaskPanel  div[id$=taskContainer_InfoPanel]").remove(); });

</script>

<div id="addTaskPanel" style="display: none;">
    <ascwc:container id="_taskContainer" runat="server">
        <header>    
        <%= TaskResource.AddNewTask%>
   </header>
        <body>
            <div class="pm-headerPanelSmall-splitter">
                <div class="headerPanelSmall">
                    <b><%= TaskResource.TaskTitle%>:</b>
                </div>
                <input class="textEdit" id="tbxTitle" style="width:100%" type="text" maxlength="100" onkeydown="return ProjectTemplatesController.InputOnKeyDown(event, 'AddTaskButton');"/>
            </div>
            <div class="pm-headerPanelSmall-splitter">
                <div class="headerPanelSmall">
                    <b><%= TaskResource.TaskDescription%>:</b>
                </div>
                <textarea id="tbxDescribe" style="width:100%" rows="3" cols="70" onkeydown="return ProjectTemplatesController.TextAreaOnKeyDown(event, 'AddTaskButton');"></textarea>
            </div>
            
            <div class="pm-headerPanelSmall-splitter" style="position:relative;">
                <span id="taskResponsibleTitle" style="display:none;">
                    <%= TaskResource.TaskResponsible%>:
                    <span class="splitter"></span>
                    <img src="<%= WebImageSupplier.GetAbsoluteWebPath("profile.gif")%>" />
                </span>
                <%if (isMobileVersion)
                  {%>
                <select id="userSelector_switcher_mobile" class="comboBox" onchange="ASC.Projects.TaskActionPage.changeResponsible(jq('#userSelector_switcher_mobile').val());">
                    <asp:Repeater runat="server" ID="rptUserSelectorMobile">
                        <ItemTemplate>
                            <option value="<%# Eval("ID") %>">
                                <%# GetUserName((Container.DataItem as UserInfo))%>
                            </option>
                        </ItemTemplate>
                    </asp:Repeater>
                </select>
                <%}
                  else
                  { %>
                <span id="userSelector_switcherContainer" style="cursor:pointer;border-bottom: 1px dotted;">
                    <a id="userSelector_switcher" style="text-decoration:none;" onclick="javascript:jq('#userSelector_dropdown').show();">
                        <%= TaskResource.TaskResponsible%>&nbsp;<small style="color:#111;">▼</small>
                    </a>
                </span>
                <div id="userSelector_dropdown" class="pm-dropdown" style="position:absolute;max-height: 200px;width:50%;overflow-y: auto;">
                    <asp:Repeater runat="server" ID="rptUserSelector">
                        <ItemTemplate>
                            <a onclick="ASC.Projects.TaskActionPage.changeResponsible('<%# Eval("ID") %>')" style="min-height: 45px;" class="pm-dropdown-item">
                                <img src="<%# GetUserAvatarUrl((Container.DataItem as UserInfo)) %>" alt="" style="float: left; padding: 4px;" />
                                <div>
                                    <div style="padding: 4px 0pt 0pt 40px;" id="userName_<%# Eval("ID") %>">
                                        <%# GetUserName((Container.DataItem as UserInfo))%>
                                    </div>
                                    <div style="padding: 4px 0pt 0pt 40px;" class="pm-grayText">
                                        <%# GetUserTitle((Container.DataItem as UserInfo))%>
                                    </div>
                                </div>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <%} %>
            </div>
            
            <div class="pm-headerPanelSmall-splitter" style="position:relative;">
                <span id="taskMilestoneTitle" style="display:none;">
                    <%= TaskResource.AssociateToMilestone%>:
                    <span class="splitter"></span>
                </span>
                <span id="milestoneSelector_selectedMilestone"></span>
                <%if (isMobileVersion)
                  {%>
                <select id="milestoneSelector_switcher_mobile" onchange="ASC.Projects.TaskActionPage.changeMilestone(jq('#milestoneSelector_switcher_mobile').val())">
                    <option value="0">
                        <%= TaskResource.None%>
                    </option>
                    <asp:Repeater runat="server" ID="rptMilestoneSelectorMobile">
                        <ItemTemplate>
                            <option value="<%# Eval("ID")%>">
                                <%# GetMilestoneTitle((Container.DataItem as TemplateMilestone))%>
                            </option>
                        </ItemTemplate>
                    </asp:Repeater>
                </select>
                <%}
                  else
                  { %>
                  
                <span id="milestoneSelector_switcherContainer" style="cursor:pointer;border-bottom: 1px dotted;">
                    <a id="milestoneSelector_switcher" style="text-decoration:none;" onclick="javascript:jq('#milestoneSelector_dropdown').show();">
                        <%= TaskResource.AssociateToMilestone%>&nbsp;<small style="color:#111;">▼</small>
                    </a>
                </span>
                <div id="milestoneSelector_dropdown" class="pm-dropdown" style="position:absolute;max-height: 200px;width:50%;overflow-y: auto;">
                    <a onclick="ASC.Projects.TaskActionPage.changeMilestone(0)" class="pm-dropdown-item" id='milestoneTitle_0'>
                        <%= TaskResource.None%>
                    </a>
                    <asp:Repeater runat="server" ID="rptMilestoneSelector">
                        <ItemTemplate>           
                            <a onclick="ASC.Projects.TaskActionPage.changeMilestone('<%# Eval("Id")%>')" class="pm-dropdown-item" id='milestoneTitle_<%# Eval("Id")%>'>
                                <%# GetMilestoneTitle((Container.DataItem as TemplateMilestone))%>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <%} %>
                
            </div>
            
            
            
            <div class="pm-h-line">
                <!– –></div>
            
            <input type="hidden" name='attachment_list' id='attachment_list' />
            <input type="hidden" name="filesUploadedInfo" id="filesUploadedInfo" />            
            <asp:HiddenField ID="hfTaskId" runat="server" />
            <input type="hidden" value="<%= Guid.Empty.ToString()%>" id="taskResponsible" />
            <input type="hidden" value="0" id="taskMilestone" />
            
            <div class="pm-action-block">
                <a href="javascript:void(0)" class="baseLinkButton" id="AddTaskButton">
                    <%= TaskResource.AddThisTask%>
                </a><span class="button-splitter"></span><a class="grayLinkButton" href="javascript:void(0)"
                    onclick="javascript: PopupKeyUpActionProvider.EnableEsc = true;  jq.unblockUI();">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
            </div>
            <div class='pm-ajax-info-block' style="display: none;">
                <span class="textMediumDescribe">
                    <%= TaskResource.SavingTask%></span><br />
                <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
            </div>
        </body>
    </ascwc:container>
</div>
