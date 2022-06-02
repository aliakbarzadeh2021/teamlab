<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="true" CodeBehind="MessageViewTemplate.ascx.cs" Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Messages.MessageViewTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<script>

function GetPreview(id)
{
    var prjID = jq.getURLParam("prjID");
    var ID = jq.getURLParam("ID");
    var title = jq("[id$=tbxTitle]").val();
    var content = FCKeditorAPI.GetInstance(id).GetHTML(true);
    
    
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

</script>

<div id="page_content">
    <div class="pm-headerPanel-splitter">
        <div class="headerPanel">
            <%= MessageResource.MessageTitle %>
        </div>
        <asp:TextBox ID="tbxTitle" Width="99%" runat="server" CssClass="textEdit" MaxLength="100" onkeydown="return ProjectTemplatesController.InputOnKeyDown(event, 'CreateMessageButton');"/>
    </div>
    <div>
        <div class="headerPanel">
            <%=MessageResource.MessageContent %>
        </div>
        <FCKeditorV2:FCKeditor ID="_fcKeditor" Height="400px" runat="server">
        </FCKeditorV2:FCKeditor>        
        </div>
    
    <div class="pm-h-line">
        <!– –></div>

    <div class="pm-action-block pm-headerPanel-splitter">
        <a href="javascript:void(0)" onclick="javascript:ASC.Projects.Messages.savingMessage('<%= Permission %>');" class="baseLinkButton" id="CreateMessageButton">
            <% if (Message != null) { %>
				<%= ProjectsCommonResource.SaveChanges%>
            <%} else {%>
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
