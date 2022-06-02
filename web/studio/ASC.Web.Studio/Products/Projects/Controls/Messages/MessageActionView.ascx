<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="true" CodeBehind="MessageActionView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Messages.MessageActionView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<ascwc:JsHtmlDecoder id="JsHtmlDecoder" runat="Server"></ascwc:JsHtmlDecoder>

<script type="text/javascript" language="javascript">

var IsMobile = <%=_mobileVer?"true":"false" %>;

function PreSave(){
    if(IsMobile){
        var text = jq('#mobiletextEdit').val();
        jq('#mobiletext').val(ASC.Controls.HtmlHelper.Text2EncodedHtml(text));
    }    
}

function OnAllUploadCompleteCallback_function()
{

   jq("#attachment_list").val(filesUploadedInfo.join(";"));

   jq(".pm-ajax-info-block span").text(ASC.Projects.Resources.SavingMessage);
   
   
   __doPostBack("","");
  // jq(theForm).submit();

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

 jq(
        function()
        {    
        
            ASC.Projects.Messages.unlockForm();
          
            var countNotifyCheckbox = jq("#notify_participant tbody input[type=checkbox]").length;
  
            jq("#notify_participant tbody input[type=checkbox]").bind("click", 
            function()
            {
                 
                jq("#notify_all").attr('checked', countNotifyCheckbox == jq("#notify_participant tbody input[type=checkbox]:checked").length); 
                        
            });
            <% if (ASC.Web.Projects.Classes.Global.ModuleManager.IsVisible(ModuleType.TMDocs)) %>
                <% { %>
            if(typeof FileReader == 'undefined')
                jq("#pm_upload_pnl").show();
            <% } %>
            var FileUploaderConfig = {                   
                    FileUploadHandler: "ASC.Web.Projects.Classes.FileUploaderHandler, ASC.Web.Projects",               
                    AutoSubmit : false,
                    UploadButtonID : (typeof FileReader != 'undefined' ? 'pm_upload_btn_html5': 'pm_upload_btn'),
                    TargetContainerID : 'message_uploadContainer',
                    ProgressTimeSpan: 300,
                    FileSizeLimit : 100000,                   
                    Data : {
                            'ProjectID': '<%= ProjectFat.Project.ID %>',
                            'prjID' : '<%= ProjectFat.Project.ID %>',
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
//                    FilesHeaderCountHolder : jq("#pm_uploadHeader"),
                    OverAllProcessHolder : jq("#pm_overallprocessHolder"),
                    OverAllProcessBarCssClass : 'pm_overAllProcessBarCssClass',
                    
                    AddFilesText : "<%=ProjectsFileResource.AttachFiles%>", //ASC.Files.Resources.ButtonAddFiles,
                    SelectFilesText : "<%=ProjectsFileResource.AttachFiles%>",// ASC.Files.Resources.ButtonSelectFiles,
                    OverallProgressText : "<%=ProjectsFileResource.OverallProgress%>"// ASC.Files.Resources.OverallProgress,
//                    FilesHeaderText : "{0} / {1} files uploaded", // ASC.Files.Resources.UploadedOf,
//                    SelectFileText : "Select Files to upload",// ASC.Files.Resources.SelectFilesToUpload,
//                    DragDropText : "or drag'n'drop to this window", //ASC.Files.Resources.OrDragDrop,
//                    SelectedFilesText : "{0} selected files" //ASC.Files.Resources.SelectedFiles//,                
                };   
                        
             if (typeof(uploadedFiles) !== 'undefined')              
                FileUploaderConfig.UploadedFiles  = uploadedFiles;
              

                        
             FileHtml5Uploader.InitFileUploader(FileUploaderConfig); 
            
                   
                                              
        }
 );

function GetPreview(id)
{
    var prjID = jq.getURLParam("prjID");
    var ID = jq.getURLParam("ID");
    var title = jq("[id$=tbxTitle]").val();
    var content = '';
    
    if(IsMobile)
        content = ASC.Controls.HtmlHelper.Text2EncodedHtml(jq('#mobiletextEdit').val());
    else
        content = FCKeditorAPI.GetInstance(id).GetHTML(true);
    
    
    AjaxPro.MessageActionView.PreviewMessage(prjID,ID,title,content,
    function(res)
     {
      
       if (res.error!=null)
       {
       
         alert(res.error.Message);
         
         return;
       
       }
       
       jq("#previewBoxBody").html(res.value);
       jq('#previewBox').show();
       var scroll_to = jq('#previewBox').position(); 
       jq.scrollTo(scroll_to.top, {speed:500});
     }
    );
}

function HidePreview()
{
    jq('#previewBox').hide();
    var scroll_to = jq('#page_content').position(); 
    jq.scrollTo(scroll_to.top, {speed:500});
}

function FCKConfig_OnLoad(config)
{   
    config.RedirectUrlToUpload("<%=RenderRedirectUpload()%>"); 
    config.MaxImageWidth = 570; 
}

</script>

<script type="text/javascript" language="javascript">

	jq(function() {
		UserSelector.OnOkButtonClick = function() {
			AjaxPro.onLoading = function(b) {
				if (b)
					jq("#selectedUsers_container").block();
			}
			var userID = new Array();

			jq("div.rightBox input[type=checkbox]").each(function() {
				userID.push(jq(this).attr("id").substr(51, 36));
			});

			AjaxPro.MessageActionView.RenderNotifyBlock(userID,
                                                   function(res) {
                                                   		if (res.error != null) { alert(res.error.Message); return; }
                                                   		jq("#selectedUsers_container").html(res.value);
                                                   		showInformOtherPeople();
                                                   });
		}

		showInformOtherPeople();


	});


      function showInformOtherPeople() {
      	if (jq('#selectedUsers_container').children().length < 1) {
      		jq('#InformOtherPeopleContainer').hide();
      	} else {
      		jq('#InformOtherPeopleContainer').show();
      	}
      }
   
   
   </script>

<div id="page_content">
    <div class="pm-headerPanel-splitter">
        <div class="headerPanel">
            <%= MessageResource.MessageTitle %>
        </div>
        <asp:TextBox ID="tbxTitle" Width="99%" runat="server" CssClass="textEdit" MaxLength="100"/>
    </div>
    <div>
        <div class="headerPanel">
            <%=MessageResource.MessageContent %>
        </div>
        
        <% if (_mobileVer){%>    
            <textarea ID="mobiletextEdit" style="width:100%; height:200px;"></textarea>
            <textarea id="mobiletext" name="mobiletext" style="display:none;"><%=_text%></textarea>
                <script type="text/javascript">
                    jq(function() {

                        var node = jq('<div>' + jq('#mobiletext').val() + '</div>').get(0);
                        jq('#mobiletextEdit').val(ASC.Controls.HtmlHelper.HtmlNode2FormattedText(node));
                    })
                    
                </script>
               
            <%}else{ %>
                <FCKeditorV2:FCKeditor ID="_fcKeditor" Height="400px" runat="server">
                </FCKeditorV2:FCKeditor>
        <%} %>
        
        
        <% if (Global.ModuleManager.IsVisible(ModuleType.TMDocs) && ASC.Projects.Engine.ProjectSecurity.CanReadFiles(ProjectFat.Project) && !ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context))%>
        <% { %>
            <div id='pm_DragDropHolder' style="border:1px solid #d1d1d1; padding:15px"> 
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
                <div id="message_uploadContainer" class="message_uploadContainer">
                </div>
                <div id="pm_upload_pnl" style="display: none; padding:15px 0 10px;">
                    <div class="clearFix" id="pm_swf_button_container">
                        <a class="grayLinkButton pm_upload_btn" id="pm_upload_btn" style="width:100px;"><%=ProjectsFileResource.AttachFiles%></a>
                        <div id="ProgressFileUploader">
                            <ascwc:ProgressFileUploader ID="_fileUploader" EnableHtml5="true"  runat="server" />
                        </div>
                        <div style="float:right;margin-top:-10px">
                            <asp:PlaceHolder ID="_uploadSwitchHolder" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>
                <div id="pm_upload_btn_html5">
                </div>
          </div>
        <% } %>
    </div>
    
    <div style="margin-top: 26px; margin-bottom: 32px;">
        <div class="headerBaseMedium">
            <%=MessageResource.SubscribePeopleInfo %>
        </div>
        <div class="textBigDescribe" style="margin-bottom: 16px;">
            <%=MessageResource.SubscribePeopleInfoComment%></div>
    
		<%= RenderNotifyBlock() %>
    </div>
    
    <div style="display: none;" id="InformOtherPeopleContainer">
		<div>
			<div class="headerBaseMedium">
				<%=MessageResource.SubscribeAnotherPeopleInfo%>
			</div>        
		</div>    
	    
		<div id="selectedUsers_container" class="pm-headerPanel-splitter">
			<asp:Literal runat="server" ID="ltlSelectedUsers" />        
		</div>
       
     
		<div class="pm-projectTeam-container" id="button" style="display: none;">
			<asp:PlaceHolder runat="server" ID="_phUserSelector" />
			<a class="baseLinkButton" onclick="javascript:UserSelector.ShowDialog();">
				<%=MessageResource.SelectUsers%>
			</a>
		</div>
    
    </div>
    
    <div class="clearFix">
		<div class="dottedDiv" style="float: left;" onclick="javascript:UserSelector.ShowDialog();"><%=MessageResource.NotifyMorePeopleOutOfTheProject%></div>
    </div>
    
    <% if (Message != null) %>
    <%{%>
    <div class="pm-h-line">
        <!– –></div>    
        
    <asp:Panel runat="server" ID="_panelEditNotify" Visible="false">
        <div>
            <div class="headerBaseMedium">
                <%=MessageResource.SubscribePeopleInfoEdit%>
            </div>
        </div>
        <br />
        <div>
            <input type="checkbox" name="notify_edit" id="notify_edit" />
            <label for="notify_edit">
                <%= ProjectsCommonResource.ConfirmEmailNotify %></label>
        </div>
    </asp:Panel>
    <%}%>
    <div class="pm-h-line">
        <!– –></div>
    <div class="pm-action-block pm-headerPanel-splitter">
        <a href="javascript:void(0)" onclick="javascript:PreSave(); ASC.Projects.Messages.savingMessage('True');"
            class="baseLinkButton">
            <% if (Message != null) %>
            <% { %>
            <%= ProjectsCommonResource.SaveChanges%>
            <% } %>
            <% else %>
            <% { %>
            <%= MessageResource.PostThisMessage %>
            <% } %>
        </a>
        
        <span class="button-splitter"></span>
        
        <a class="baseLinkButton" onclick="javascript:GetPreview('<%= _fcKeditor.ClientID %>')"><%= ProjectsCommonResource.Preview %></a>
        
        <span class="button-splitter"></span>
        
        <asp:LinkButton CssClass="grayLinkButton cancelFckEditorChangesButtonMarker" OnClick="lbCancel_Click" runat="server" ID="_cancelButton">
			<%=ProjectsCommonResource.Cancel %>
		</asp:LinkButton>
        
    </div>
    <div class='pm-ajax-info-block pm-headerPanel-splitter' style="display: none;">
        <span class="textMediumDescribe">
            <%= MessageResource.SavingMessage %></span><br />
        <img alt="" title="" src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
    </div>
    <input type="hidden" name="filesUploadedInfo" id="filesUploadedInfo" />
    <input type="hidden" name="notify_participant_checked" id="notify_participant_checked" />
    <input type="hidden" name="another_notify_participant_checked" id="another_notify_participant_checked" />
    <input type="hidden" name="notify_edit_checked" id="notify_edit_checked" />
    <input type="hidden" name='attachment_list' id='attachment_list' />
    
    
    <div id="previewBox" class="pm-hidden">
        <div id="previewBoxBody">    </div>
        <a class="baseLinkButton" onclick="javascript:HidePreview();"> <%=ProjectsCommonResource.Collapse %> </a>
    </div>
    
    
</div>
