<%@ Assembly Name="ASC.Web.Files" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/BaseTemplate.master"
    CodeBehind="DocEditor.aspx.cs" Inherits="ASC.Web.Files.DocEditor" %>

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
    <style type="text/css">
    div.files_infoPanel
    {
        display:none;
        left: 50%;
        margin: 0;
        max-width: 722px;
        position: fixed;
        top: 0;
        z-index: 260;
    }
    div.files_infoPanel div
    {
        padding-right: 10px;
        overflow: hidden;
    }
    </style>

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
            if (jq.browser.msie)
                jq("body").css("overflow", "hidden");

            setInterval(trackEdit, 7000);

            window.onbeforeunload = finishEdit;

            if (ASC.Files.Actions == undefined) ASC.Files.Actions = (function() { return { hideAllActionPanels: function() { } }; })();
            if (ASC.Files.UI == undefined) ASC.Files.UI = (function() {
                var timeIntervalInfo;
                return {
                    displayInfoPanel: function(str, warn) {
                        if (str === "" || typeof str === "undefined")
                            return;

                        clearTimeout(timeIntervalInfo);
                        jq("#infoPanelContainer").removeClass("warn").children("div").html(str);
                        jq("#infoPanelContainer").css("margin-left", -jq("#infoPanelContainer").width() / 2);
                        if (ASC.Controls.Constants.isMobileAgent) {
                            jq("#infoPanelContainer").css("top", jq(window).scrollTop() + "px");
                        }

                        if (warn === true)
                            jq("#infoPanelContainer").addClass('warn');
                        jq("#infoPanelContainer").show();

                        timeIntervalInfo = setTimeout("ASC.Files.UI.hideInfoPanel();", 3000);
                    },

                    hideInfoPanel: function() {
                        clearTimeout(timeIntervalInfo);
                        jq("#infoPanelContainer").hide().children("div").html("&nbsp;");
                    }
                };
            })();
        });

        function listener(event) {
            var fileId = "<%=FileId%>";
            var fileVersion = "<%=FileVersion%>";
            var folderUrl = "<%=FolderUrl%>";
        
            var input = jq.parseJSON(event.data);
            switch (input.type) {
                case "share":
                    if (ASC.Files.Share)
                        ASC.Files.Share.clickShareButton("file_" + fileId);
                    break;
                case "save":
                    jq.ajax({
                        type: "get",
                        url: jq.format(ASC.Files.Constants.URL_HANDLER_SAVE, fileId, fileVersion, encodeURIComponent(input.data)) + '<%=string.IsNullOrEmpty(FileNew) ? "": "&" + UrlConstant.New + "=true" %>',
                        complete: completeSave
                    });
                    break;
                case "tofiles":
                    location.href = folderUrl;
                    break;
            }
        };
        
        function completeSave() {
            if (arguments[1] == "error") {
                var win = jq("#iframeDocViewer")[0].contentWindow;
                win.postMessage("SavingError", "*");

                var errorMessage = arguments[0].responseText.split("title>")[1].split("</")[0];
                ASC.Files.UI.displayInfoPanel(errorMessage, true);
            }
        };

        serviceManager.bind("TrackEditFile", completeReq);
        function trackEdit() {
            serviceManager.trackEditFile("TrackEditFile", { fileID: "<%=FileId%>", docKey: "<%=DocKey%>" });
        };
        function finishEdit() {
            serviceManager.trackEditFile("FinishTrackEditFile", { fileID: "<%=FileId%>", docKey: "<%=DocKey%>", finish: true, ajaxsync: true });
        };
        function completeReq(xmlData, params, errorMessage,commentMessage) {
            if (typeof errorMessage != "undefined") {
                errorMessage = commentMessage || errorMessage;
                ASC.Files.UI.displayInfoPanel(errorMessage, true);
            }
        };
    </script>
</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="PageContent" runat="server">
    <div class="mainContainerClass"><div id="infoPanelContainer" class="infoPanel files_infoPanel"><div>&nbsp;</div></div></div>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
    <iframe id="iframeDocViewer" width="100%" height="100%" frameborder="0" src="<%=SrcIframe%>"
        style="background-color: Transparent; min-width: 1000px;"></iframe>
</asp:Content>