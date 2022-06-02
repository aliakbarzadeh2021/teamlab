<%@ Assembly Name="ASC.Web.Files" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/BaseTemplate.master"
    CodeBehind="DocViewer.aspx.cs" Inherits="ASC.Web.Files.DocViewer" %>

<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Files.Core" %>
<%@ Import Namespace="ASC.Web.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeaderContent" runat="server">
    <% #if (DEBUG) %>
    <link href="<%=PathProvider.GetFileStaticRelativePath("common.css")%>" type="text/css" rel="stylesheet" />
    <link href="<%=String.Concat(PathProvider.BaseAbsolutePath, "Controls/AccessRights/AccessRights.css")%>" type="text/css" rel="stylesheet" />
    <% #else %>
    <link href="<%=PathProvider.GetFileStaticRelativePath("files-min.css")%>" type="text/css" rel="stylesheet" />
    <% #endif %>
    

    <script type="text/javascript" language="javascript" src="<%=PathProvider.GetFileStaticRelativePath("common.js")%>"></script>
    <script type="text/javascript" language="javascript" src="<%=PathProvider.GetFileStaticRelativePath("templatemanager.js")%>"></script>
    <script type="text/javascript" language="javascript" src="<%=PathProvider.GetFileStaticRelativePath("servicemanager.js")%>"></script>
    <script type="text/javascript" language="javascript" src="<%=PathProvider.GetResourcesPath()%>"></script>

    <script type="text/javascript">
        jq(function() {
            if (window.addEventListener) {
                window.addEventListener("message", listener, false);
            } else {
                window.attachEvent("onmessage", listener);
            }

            jq("#iframeDocViewer").parents().css("height", "100%").removeClass("clearFix");
            jq(".mainPageLayout").removeClass("mainPageLayout");
            jq("#studioPageContent").attr("id", "");
            jq("#studioFooter").remove();
            jq("body").css("overflow", "hidden");
        });

        function listener(event) {
            if (ASC.Files.Actions == undefined) ASC.Files.Actions = (function($) { return { hideAllActionPanels: function() { } }; })(jQuery);
            if (ASC.Files.UI == undefined) ASC.Files.UI = (function($) { return { displayInfoPanel: function() { } }; })(jQuery);
        
            var fileId = "<%=FileId%>";
            var fileVersion = "<%=FileVersion%>";
            var folderUrl = "<%=FolderUrl%>";
        
            var input = jq.parseJSON(event.data);
            switch (input.type) {
                case "download":
                    location.href = ASC.Files.Utils.fileDownloadUrl(fileId, fileVersion);
                    break;
                case "share":
                    if (ASC.Files.Share) 
                        ASC.Files.Share.clickShareButton("file_" + fileId);
                    break;
                case "edit":
                    location.href = jq.format(ASC.Files.Constants.URL_DOCUMENT_EDIT, fileId);
                    break;
                case "tofiles":
                    location.href = folderUrl;
                    break;
            }
        };
    </script>
</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="PageContent" runat="server">
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
    <iframe id="iframeDocViewer" width="100%" height="100%" frameborder="0" src="<%=SrcIframe%>"
        style="background-color: Transparent; min-width: 1000px;"></iframe>
</asp:Content>