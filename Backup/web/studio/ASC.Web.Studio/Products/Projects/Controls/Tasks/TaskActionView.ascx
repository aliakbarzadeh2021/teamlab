<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskActionView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Tasks.TaskActionView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<script type="text/javascript" language="javascript">

function OnAllUploadCompleteCallback_function()
{

   jq("#attachment_list").val(filesUploadedInfo.join(";"));
   
   ASC.Projects.TaskActionPage.saveOrUpdate(jq("#<%= hfTaskId.ClientID %>").val());
   
   jq(".pm-ajax-info-block span").text(ASC.Projects.Resources.SavingMessage);
   
   //__doPostBack("","");
  // jq(theForm).submit();

}

 jq(document).click(function(event)
 {

 ASC.Projects.Common.dropdownRegisterAutoHide(event, "#userSelector_switcher","#userSelector_dropdown");
 ASC.Projects.Common.dropdownRegisterAutoHide(event, "#milestoneSelector_switcher","#milestoneSelector_dropdown");

 });
 
 jq(function()
 {
        jq("[id$=taskDeadline]").mask('<%= System.DateTimeExtension.DateMaskForJQuery %>');
        jq("[id$=taskDeadline]").datepick({ 
            popupContainer: "#addTaskPanel",
            selectDefaultDate: true,
            showAnim: '',
            onSelect: function(dates){  jq("#taskDeadlineContainer").find("a").each(
                                        function(index) { jq(this).css("border-bottom","1px dotted"); });
                                      }
        });
 }); 

function keyPress(event)
{
    var code;
    if (!e) var e = event;
    if (!e) var e = window.event;

    if (e.keyCode)
    { 
        code = e.keyCode;
    }
    else if (e.which)
    {
        code = e.which;
    }
    
    if (code >= 48 && code <= 57)
    {
        jq("#taskDeadlineContainer").find("a").each(
        function(index) { jq(this).css("border-bottom","1px dotted"); });
    }
} 

function OnUploadCompleteCallback_function(serverData)
{
if(serverData)
    if(serverData.Data != null && serverData.Data != "")
        filesUploadedInfo.push(serverData.Data);
  
}

function OnBeginCallback_function(data)
{

   jq(".pm-ajax-info-block span").text(jq.format(ASC.Projects.Resources.UploadingFile, data.CurrentFile));
  
}

var filesUploadedInfo = new Array();

var FileUploader;

var swfu;

function initFileUploaderInPopup(listUploadedFiles)
{
    <% if (ASC.Web.Projects.Classes.Global.ModuleManager.IsVisible(ModuleType.TMDocs)) %>
                <% { %>
            if(typeof FileReader == 'undefined')
                jq("#pm_upload_pnl").show();
            <% } %>            
            var FileUploaderConfig = {                       
                    FileUploadHandler: "ASC.Web.Projects.Classes.FileUploaderHandler, ASC.Web.Projects",
                    AutoSubmit : false,
                    UploadButtonID : (typeof FileReader != 'undefined' ? 'pm_upload_btn_html5': 'pm_upload_btn'),
                    TargetContainerID : 'task_uploadContainer',
                    ProgressTimeSpan: 300,
                    FileSizeLimit : 100000,                   
                    Data : {
                            'ProjectID': '<%= Project.ID %>',
                            'prjID' : '<%= Project.ID %>',
                            'UserID':'<%=ASC.Core.SecurityContext.CurrentAccount.ID%>',
                            "ASPSESSID" : "<%=Session.SessionID %>",                            
                            "AUTHID" : "<% = Request.Cookies["asc_auth_key"]==null ? "" : Request.Cookies["asc_auth_key"].Value %>"
                          },
                    FileSizeLimit : "<%=ASC.Web.Studio.Core.SetupInfo.MaxUploadSizeInKB%>",                        
                    OnAllUploadComplete : OnAllUploadCompleteCallback_function,
                    OnUploadComplete : OnUploadCompleteCallback_function, 
                    OnBegin : OnBeginCallback_function,
                    DeleteLinkCSSClass : 'pm_deleteLinkCSSClass',
                    LoadingImageCSSClass : 'pm_loadingCSSClass',
                    CompleteCSSClass : 'pm_completeCSSClass',
                    DragDropHolder : jq("#pm_DragDropHolder"),
                    OverAllProcessHolder : jq("#pm_overallprocessHolder"),
                    OverAllProcessBarCssClass : 'pm_overAllProcessBarCssClass',                    
                    AddFilesText : "<%=ProjectsFileResource.AttachFiles%>", 
                    SelectFilesText : "<%=ProjectsFileResource.AttachFiles%>",
                    OverallProgressText : "<%=ProjectsFileResource.OverallProgress%>"         
                };

             if (typeof(listUploadedFiles) !== 'undefined')              
                FileUploaderConfig.UploadedFiles  = listUploadedFiles;
             
             filesUploadedInfo = new Array(0);
                        
             FileHtml5Uploader.InitFileUploader(FileUploaderConfig);
}

 jq(
        function()
        {    
        
            ASC.Projects.TaskActionPage.unlockForm(); 
            
        }
 );

</script>

<div id="addTaskPanel" style="display: none;overflow-x: hidden;">
    <ascwc:container id="_taskContainer" runat="server">
        <header>    
        <%= TaskResource.AddNewTask %>
   </header>
        <body>
            <div class="pm-headerPanelSmall-splitter">
                <div class="headerPanelSmall">
                    <b><%= TaskResource.TaskTitle %>:</b>
                </div>
                <input class="textEdit" id="tbxTitle" style="width:100%" type="text" maxlength="100"/>
            </div>
            <div class="pm-headerPanelSmall-splitter">
                <div class="headerPanelSmall">
                    <b><%= TaskResource.TaskDescription%>:</b>
                </div>
                <textarea id="tbxDescribe" style="width:100%;resize:none;" rows="3" cols="70"></textarea>
            </div>
            
            <div class="pm-headerPanelSmall-splitter" style="position:relative;">
                <div style="float:right;">
                    <input type="checkbox" id="notifyResponsible" checked="checked" />
                    <label for="notifyResponsible"><%= ProjectsCommonResource.NotifyPeopleViaEmail %></label>
                </div>
                <div>
                    <span id="taskResponsibleTitle" style='<%=isMobileVersion? "" : "display: none;"%>'>
                        <%= TaskResource.TaskResponsible%>:
                        <span class="splitter"></span>
                        <img src="<%= WebImageSupplier.GetAbsoluteWebPath("profile.gif")%>" />
                    </span>
                    
                    <%if (isMobileVersion)
                      {%>
                    <select id="userSelector_switcher_mobile" class="comboBox" onchange="ASC.Projects.TaskActionPage.changeResponsible(jq('#userSelector_switcher_mobile').val());">
                        <asp:Repeater runat="server" ID="rptUserSelectorMobile">
                            <ItemTemplate>
                                <option value="<%# Eval("ID") %>" >
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
            </div>
            
            <div class="pm-headerPanelSmall-splitter" style="position:relative;">
                <span id="taskMilestoneTitle" style='<%=isMobileVersion? "": "display: none;"%>'>
                    <%= TaskResource.AssociateToMilestone%>:
                    <span class="splitter"></span>
                </span>
                <span id="milestoneSelector_selectedMilestone" style="white-space: nowrap;"></span>
                
                <%if (isMobileVersion)
                  {%>
                <select id="milestoneSelector_switcher_mobile" onchange="ASC.Projects.TaskActionPage.changeMilestone(jq('#milestoneSelector_switcher_mobile').val())">
                    <option value="0">
                        <%= TaskResource.None%>
                    </option>
                    <asp:Repeater runat="server" ID="rptMilestoneSelectorMobile">
                        <ItemTemplate>
                            <option value="<%# Eval("ID")%>">
                                <%# GetMilestoneTitle((Container.DataItem as Milestone))%>
                            </option>
                        </ItemTemplate>
                    </asp:Repeater>
                </select>                   
                <%}
                  else
                  { %>
                <span id="milestoneSelector_switcherContainer" style="cursor:pointer;border-bottom: 1px dotted;white-space: nowrap;">
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
                            <a onclick="ASC.Projects.TaskActionPage.changeMilestone('<%# Eval("ID")%>')" class="pm-dropdown-item" id='milestoneTitle_<%# Eval("ID")%>'>
                                <%# GetMilestoneTitle((Container.DataItem as Milestone))%>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <%} %>
                
            </div>
            
            <div class="pm-headerPanelSmall-splitter" id="taskDeadlineContainer">
                <div class="headerPanelSmall">
                    <b><%= TaskResource.DeadLine %>:</b>
                </div>
                <a style="text-decoration:none;border-bottom: 1px dotted;border-color:#111;cursor:pointer;" onclick="ASC.Projects.TaskActionPage.changeDeadline(this);" id="deadline_0">
                    <%= ProjectsCommonResource.Today %>
                </a>
                <span class="splitter"></span>
                <a style="text-decoration:none;border-bottom: 1px dotted;border-color:#111;cursor:pointer;" onclick="ASC.Projects.TaskActionPage.changeDeadline(this);" id="deadline_3">
                    3 <%= GrammaticalResource.DayGenitiveSingular%>
                </a>
                <span class="splitter"></span>
                <a style="text-decoration:none;border-bottom: 1px dotted;border-color:#111;cursor:pointer;" onclick="ASC.Projects.TaskActionPage.changeDeadline(this);" id="deadline_7">
                    <%= ReportResource.Week %>
                </a>
                <span class="splitter"></span>
                <asp:TextBox runat="server" ID="taskDeadline" onkeypress="javascript:keyPress(event);" CssClass="pm-ntextbox textEditCalendar" Width="75px" />
            </div>
            
            <div class="pm-headerPanelSmall-splitter">
                <% if (ASC.Web.Projects.Classes.Global.ModuleManager.IsVisible(ModuleType.TMDocs) && ASC.Projects.Engine.ProjectSecurity.CanReadFiles(Project) && !ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context)) %>
                <% { %>
                <div class="headerPanelSmall">
                    <b><%= TaskResource.FilesToTheTask%>:</b>
                </div>
                <div id='pm_DragDropHolder' style="border:1px solid #d1d1d1; padding:15px">
                <%--<div class="header" id="pm_uploadHeader"></div>--%>
                <table cellpadding="0" cellspacing="0" border="0">
                    <tr valign="middle">
                        <td style="width:50px; padding:5px 0 0 10px;">
                            <div class="pm_uploadIcon"></div>
                        </td>
                        <td height="20"> 
                            <div class="describeUpload">
                                <%=String.Format(ProjectsFileResource.FileSizeNote, ASC.Web.Studio.Core.SetupInfo.MaxUploadSizeToMBFormat, "<strong>", "</strong>")%></div>
                        </td>
                    </tr>
                </table>
                
                <div id="pm_overallprocessHolder">
                </div>
                <div id="task_uploadContainer" class="task_uploadContainer">
                </div>
                <div id="pm_upload_pnl" style="display: none; padding:15px 0 10px;">
                    <div class="clearFix" id="pm_swf_button_container">
                        <a class="grayLinkButton pm_upload_btn" id="pm_upload_btn" style="width:100px; z-index:100"><%=ProjectsFileResource.AttachFiles%></a>
                        <div id="ProgressFileUploader">
                            <ascwc:ProgressFileUploader ID="_fileUploader" EnableHtml5="true"  runat="server" />
                        </div>
                        <div style="float:right;">
                            <asp:PlaceHolder ID="_uploadSwitchHolder" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>
                <div id="pm_upload_btn_html5">
                </div>
            </div><% } %>
            </div>
            <div class="pm-h-line">
                <!– –></div>
            
            <input type="hidden" name='attachment_list' id='attachment_list' />
            <input type="hidden" name="filesUploadedInfo" id="filesUploadedInfo" />            
            <asp:HiddenField ID="hfTaskId" runat="server" />
            <input type="hidden" value="<%= Guid.Empty.ToString()%>" id="taskResponsible" />
            <input type="hidden" value="0" id="taskMilestone" />
            
            <div class="pm-action-block">
                <a href="javascript:void(0)" class="baseLinkButton">
                    <%= TaskResource.AddThisTask%>
                </a><span class="button-splitter"></span><a class="grayLinkButton" href="javascript:void(0)"
                    onclick="javascript: PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
            </div>
            <div class='pm-ajax-info-block' style="display: none;">
                <span class="textMediumDescribe">
                    <%= TaskResource.SavingTask %></span><br />
                <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
            </div>
        </body>
    </ascwc:container>
</div>
