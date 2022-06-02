<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainMenu.ascx.cs"
    Inherits="ASC.Web.Files.Controls.MainMenu" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>

<% #if (DEBUG) %>
    <link href="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/MainMenu/MainMenu.css" )%>" type="text/css" rel="stylesheet" />
<% #endif %>

<asp:PlaceHolder ID="_premiumStubHolder" runat="server"></asp:PlaceHolder>
<ul id="mainMenuHolder" class='<%=EnableShare ? "withShare" : string.Empty %>'>
    <%if (SecurityContext.IsAuthenticated)%>
    <%{ %>
    <li>
        <a id="files_newdoc_btn"
           class="topNewFile"
           title="<%=FilesUCResource.NewFile%>"
           style="margin-left: -30px;" >
            <span><%=FilesUCResource.NewFile%></span>
            <img alt="" src="<%=PathProvider.GetImagePath("black_combobox.png")%>" />
        </a>
    </li>
    <li>
        <a id="files_uploaddialog_btn"
           class="topUpload" 
           title="<%=FilesUCResource.ButtonUpload%>">
            <%=FilesUCResource.ButtonUpload%>
        </a>
    </li>
    <li>
        <a id="files_createfolder_btn"
           class="topNewFolder" 
           title="<%=FilesUCResource.ButtonCreateFolder%>">
            <%=FilesUCResource.ButtonCreateFolder%>
        </a>
    </li>
    <li>
        <a id="files_share_btn"
            class="topShare"
            title="<%=FilesUCResource.ButtonShare+" ("+Resources.Resource.RightsRestrictionNote+")"%>">
            <%=FilesUCResource.ButtonShare%>
        </a>
    </li>
    <%} %>
    <li>
        <a id="files_download_btn"
           class="topDownload"
           title="<%=FilesUCResource.ButtonDownload%>"
           style="margin-right: -30px;" >
            <%=FilesUCResource.ButtonDownload%>
        </a>
    </li>
<%--    <li>
        <a id="files_gallery_btn"
           class="topGallery"
           title="<%=FilesUCResource.ButtonGallery%>"
           style="margin-right: -30px;" >
            <%=FilesUCResource.ButtonGallery%>
        </a>
    </li>
    <li>
        <a id="files_list_btn"
           class="topList"
           title="<%=FilesUCResource.ButtonList%>"
           style="margin-right: -30px;" >
            <%=FilesUCResource.ButtonList%>
        </a>
    </li>
--%>
</ul>
<div id="mainMenuHolderSpacer">&nbsp;</div>

<%-- popup window --%>
<div id="files_newDocumentPanel" class="files_action_panel files_popup_win" style="margin:5px 0 0 -30px; z-index:260;">
    <div class="popup-corner" style="left: 30px;"></div>
    <ul>
        <li id="files_create_text" class="files_create_text">
            <a><%=FilesUCResource.ButtonCreateText%></a>
        </li>
        <li id="files_create_spreadsheet" class="files_create_spreadsheet">
            <a><%=FilesUCResource.ButtonCreateSpreadsheet%></a>
        </li>
        <li id="files_create_presentation" class="files_create_presentation">
            <a><%=FilesUCResource.ButtonCreatePresentation%></a>
        </li>
    </ul>
</div>

<%--dialog window--%>
<div id="files_uploadDialogContainer" class="popupModal" style="display:none;">
    <ascw:container id="uploadDialog" runat="server">
        <header>
            <div id="uploadDialogContainerHeader" style="overflow: hidden; width: 530px;"></div>
        </header>
        <body>
            <div class="panelContent">
                <div id="files_uploadHeader" class="header"></div>
                <table id="files_upload_select" cellpadding="0" cellspacing="0" border="0">
                    <tr valign="top">
                        <td style="width: 50px; padding: 5px 0 0 10px;">
                            <div class="files_uploadIcon"></div>
                        </td>
                        <td height="20">
                            <div class="describeUpload">
                                <%=string.Format(FilesUCResource.NoteFileUpload, ASC.Web.Studio.Core.SetupInfo.MaxUploadSizeInKB / 1024,"<b>","</b>")%>
                            </div>
                        </td>
                    </tr>
                </table>
                <div id="files_overallprocessHolder"></div>
                <div id="files_upload_fileList" class="files_upload_fileList"></div>
                <div id="files_upload_pnl" style="display: none; padding: 15px 0 10px;">
                    <div id="files_swf_button_container" class="clearFix">
                        <a id="files_upload_btn" class="grayLinkButton files_upload_btn">
                            <%=FilesUCResource.ButtonSelectFiles%>
                        </a>
                        <div id="ProgressFileUploader">
                            <ascw:ProgressFileUploader ID="FileUploader" EnableHtml5="true" runat="server" />
                        </div>
                        <div style="float: right">
                            <asp:PlaceHolder ID="_uploadSwitchHolder" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>
                <div id="files_upload_btn_html5" class="files_upload_btn_html5" style="display: none;">
                </div>
            </div>
            <div id="files_uploadDlgPanelButtons" class="panelButtons">
                <div id="panelButtons">
                    <a id="files_uploadSubmit" class="baseLinkButton">
                        <%=FilesUCResource.ButtonUpload%>
                    </a>
                    <a id="files_cancelUpload" class="grayLinkButton" style="margin-left: 8px;">
                        <%=FilesUCResource.ButtonClose%>
                    </a>
                </div>
                <div id="upload_finish" style="display: none;">
                    <a id="files_okUpload" class="baseLinkButton" style="width: 60px;">
                        <%=FilesUCResource.ButtonOk%>
                    </a>
                </div>
            </div>
        </body>
    </ascw:container>
</div>