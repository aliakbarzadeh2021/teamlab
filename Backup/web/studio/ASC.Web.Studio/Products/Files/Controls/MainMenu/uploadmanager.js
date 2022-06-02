window.ASC.Files.Uploads = (function($) {
    var isInit = false;
    var currentUploadFolder = "";
    var fileUploader = null;
    var totalCount = 0;


    var init = function() {
        if (isInit === false) {
            isInit = true;
        }
    };

    var onUploadComplete = function(file, response) {
        if (file)
            var id = file.Message;
        if (id != undefined && parseInt(id) > 0) {
            if (currentUploadFolder == ASC.Files.Folders.currentFolderId) {
                if (jq("#content_file_" + id).html() != null && jq("#content_file_" + id).html() != "")
                    ASC.Files.Folders.replaceVersion(id, false);
                else
                    ASC.Files.Folders.getFile(id);
            }
            totalCount++;
        }
        else {
            if (id && id.length > 0)
                ASC.Files.UI.displayInfoPanel(id, true);
        }
    };

    var onSimpleUploadComplete = function(file, response) {
        if (file)
            var id = file.Message;
        if (id != undefined && parseInt(id) > 0) {
            ASC.Files.Folders.replaceVersion(id, true, true);
        }
        else {
            if (id && id.length > 0)
                ASC.Files.UI.displayInfoPanel(id, true);
        }
    };

    var OnProgress = function(data) {
        jq("#uploads_message").text(ASC.Files.Resources.Uploading + " " + data.CurrentFile);
    };

    var OnAllUploadComplete = function() {
        jq("#files_uploadDialogContainer .cancelButton").show();
        jq("#files_swf_button_container").hide();

        OnAllSimpleUploadComplete();
    };

    var OnAllSimpleUploadComplete = function() {
        jq("#files_uploadDialogContainer .cancelButton").show();
        jq("#files_swf_button_container").hide();
        jq("#upload_finish").show();
        PopupKeyUpActionProvider.CloseDialogAction = "";
        PopupKeyUpActionProvider.EnterAction = 'jq("#files_okUpload").click();';
        ASC.Files.UI.addRowHandlers();

        if (jq("div.fileRow").length > 0)
            ASC.Files.UI.hideEmpty();
    };

    var showUploadedFiles = function() {
        var list = jq("div.newFile");
        if (list.length > 0)
            list.removeClass("newFile").show().yellowFade();

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoUploadedSuccess, totalCount));
    };

    var showUploadPopup = function() {
        if (ASC.Controls.Constants.isMobileAgent
            || jq("#files_uploadDialogContainer:visible").length != 0)
            return;

        if (ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessibleItem()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_SecurityException, true);
            ASC.Files.Folders.eventAfter = showUploadPopup;
            ASC.Files.UI.defaultFolderSet();
            return;
        }

        jq("#files_upload_select").css("display", "");
        jq("#files_swf_button_container").show();
        jq("#panelButtons").show();
        jq("#files_upload_btn_html5").show();
        jq("#files_uploadDlgPanelButtons").hide();

        jq("#upload_finish").hide();
        jq("#files_uploadDialogContainer .cancelButton").show();

        jq("#uploadDialogContainerHeader").html(
                                                jq.format('{0} "<span title="{1}">{1}</span>"',
                                                            ASC.Files.Resources.UploadFileTo,
                                                            jq("#files_folderName").text()));

        ASC.Files.Common.blockUI(jq("#files_uploadDialogContainer"), 600, 600);

        ASC.Files.Uploads.documentTitleFix();
        PopupKeyUpActionProvider.EnableEsc = false;
        PopupKeyUpActionProvider.CloseDialogAction = 'ASC.Files.Uploads.documentTitleFix();';
        PopupKeyUpActionProvider.EnterAction = jq.browser.mozilla ? "" : 'jq("#files_uploadSubmit").click();';

        jq("#files_upload_fileList").html("");

        activateUploader('files_upload_fileList', ASC.Files.Folders.currentFolderId);

        jq("#asc_fileuploaderSWFContainer").attr("style", "");

        jq("#files_okUpload").unbind("click").click(function() {
            ASC.Files.Uploads.showUploadedFiles();
            PopupKeyUpActionProvider.CloseDialog();
        });
    };

    var showUploadNewVersion = function(fileid) {
        if (ASC.Controls.Constants.isMobileAgent)
            return;

        if (fileid == undefined
            || ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessibleItem("file_" + fileid)
            || jq("#files_uploadDialogContainer:visible").length != 0) {
            return;
        }

        jq("#files_upload_select").css("display", "");
        jq("#files_swf_button_container").show();
        jq("#panelButtons").show();
        jq("#files_upload_btn_html5").show();
        jq("#files_uploadDlgPanelButtons").hide();
        jq("#upload_finish").hide();
        jq("#files_uploadDialogContainer .cancelButton").show();

        jq("#uploadDialogContainerHeader").html(ASC.Files.Resources.UpdateFileVersionFor + ' "' + jq("#content_file_" + fileid + " div.fileName a.name").attr("title") + '"');

        jq("#files_upload_fileList").html("");

        ASC.Files.Common.blockUI(jq("#files_uploadDialogContainer"), 600, 400);

        PopupKeyUpActionProvider.CloseDialogAction = 'ASC.Files.Uploads.documentTitleFix();';
        PopupKeyUpActionProvider.EnterAction = jq.browser.mozilla ? "" : 'jq("#files_uploadSubmit").click();';
        PopupKeyUpActionProvider.EnableEsc = false;

        ASC.Files.Uploads.documentTitleFix();
        activateSimpleUploader('files_upload_fileList', fileid);

        jq("#asc_fileuploaderSWFContainer").attr("style", "");

        jq("#files_okUpload").unbind("click").click(function() {
            PopupKeyUpActionProvider.CloseDialog();
        });
    };

    var activateUploader = function(pnlId, folderId) {

        jq("object[id*='SWFUpload']").before('<span id="asc_fileuploaderSWFObj"></span>');

        totalCount = 0;
        try { jq("object[id*='SWFUpload']").remove(); } catch (e) { }

        currentUploadFolder = folderId;

        var btnId = jq("#files_upload_btn").attr("id") ||
                    jq("#files_swf_button_container a.files_upload_btn").attr("id"); //fix basic uploader

        if (ASC.Files.DragDrop.isFileAPI()) {
            jq("#files_swf_button_container").hide();
            jq("#files_upload_pnl").hide();
            btnId = jq(".files_upload_btn_html5").attr("id"); //fix basic uploader, when html5 on and flash off
        }
        else {
            jq("#files_swf_button_container").show();
            jq("#files_upload_pnl").show();
        }

        if (typeof ASC !== "undefined" && typeof ASC.Controls !== "undefined" && typeof ASC.Controls.FileUploader !== "undefined") {

            fileUploader = FileHtml5Uploader.InitFileUploader({
                FileUploadHandler: 'ASC.Web.Files.Classes.FilesUploader,ASC.Web.Files',
                FileSizeLimit: ASC.Files.Constants.MAX_UPLOAD_KB,

                AutoSubmit: false,
                ProgressTimeSpan: 1000,
                UploadButtonID: btnId,
                TargetContainerID: pnlId,
                Data: { folderId: folderId },
                OnUploadComplete: onUploadComplete,
                OnAllUploadComplete: OnAllUploadComplete,
                OnProgress: OnProgress,
                file_types: ASC.Files.Utils.SupportFileTypes,

                AddFilesText: ASC.Files.Resources.ButtonAddFiles,
                SelectFilesText: ASC.Files.Resources.ButtonSelectFiles,
                ErrorFileTypeText: ASC.Files.Resources.ErrorMassage_FileTypeText,

                DeleteLinkCSSClass: 'files_deleteLinkCSSClass',
                LoadingImageCSSClass: 'files_loadingCSSClass',
                CompleteCSSClass: 'files_completeCSSClass',
                OverAllProcessBarCssClass: 'files_overAllProcessBarCssClass',

                DragDropHolder: jq("#files_uploadDialogContainer div.panelContent"),
                FilesHeaderCountHolder: jq("#files_uploadHeader"),
                OverAllProcessHolder: jq("#files_overallprocessHolder"),
                SubmitPanelAfterSelectHolder: jq("#files_uploadDlgPanelButtons"),

                OverallProgressText: ASC.Files.Resources.OverallProgress,
                FilesHeaderText: ASC.Files.Resources.UploadedOf,
                SelectFileText: ASC.Files.Resources.SelectFilesToUpload,
                DragDropText: ASC.Files.Resources.OrDragDrop,
                SelectedFilesText: ASC.Files.Resources.SelectedFiles
            });
        }
        jq("#" + btnId).unbind("mouseover");
    };

    var activateSimpleUploader = function(pnlId, fileid) {
        jq("object[id*='SWFUpload']").before('<span id="asc_fileuploaderSWFObj"></span>');

        totalCount = 0;
        try { jq("object[id*='SWFUpload']").remove(); } catch (e) { }

        var btnId = jq("#files_upload_btn").attr("id") ||
                    jq("#files_swf_button_container a.files_upload_btn").attr("id"); //fix basic uploader

        currentUploadFolder = ASC.Files.Folders.currentFolderId;

        if (ASC.Files.DragDrop.isFileAPI()) {
            jq("#files_swf_button_container").hide();
            jq("#files_upload_pnl").hide();
            btnId = jq(".files_upload_btn_html5").attr("id"); //fix basic uploader, when html5 on and flash off
        }
        else {
            jq("#files_swf_button_container").show();
            jq("#files_upload_pnl").show();
        }

        if (typeof ASC !== "undefined" && typeof ASC.Controls !== "undefined" && typeof ASC.Controls.FileUploader !== "undefined") {

            fileUploader = FileHtml5Uploader.InitFileUploader({
                FileUploadHandler: 'ASC.Web.Files.Classes.FilesUploader,ASC.Web.Files',
                FileSizeLimit: ASC.Files.Constants.MAX_UPLOAD_KB,

                AutoSubmit: false,
                ProgressTimeSpan: 1000,
                UploadButtonID: btnId,
                AddFilesText: ASC.Files.Resources.ButtonAddFiles,
                SelectFilesText: ASC.Files.Resources.ButtonSelectFiles,
                TargetContainerID: pnlId,
                Data: { fileid: fileid },
                SingleUploader: true,
                OnUploadComplete: onSimpleUploadComplete,
                OnAllUploadComplete: OnAllSimpleUploadComplete,
                OnProgress: OnProgress,
                file_types: ASC.Files.Utils.SupportFileTypes,
                ErrorFileTypeText: ASC.Files.Resources.ErrorMassage_FileTypeText,

                DeleteLinkCSSClass: 'files_deleteLinkCSSClass',
                LoadingImageCSSClass: 'files_loadingCSSClass',
                CompleteCSSClass: 'files_completeCSSClass',
                DragDropHolder: jq("#files_uploadDialogContainer div.panelContent"),
                FilesHeaderCountHolder: jq("#files_uploadHeader"),
                OverAllProcessHolder: jq("#files_overallprocessHolder"),
                OverAllProcessBarCssClass: 'files_overAllProcessBarCssClass',
                OverallProgressText: ASC.Files.Resources.OverallProgress,
                FilesHeaderText: ASC.Files.Resources.UploadedOf,
                SelectFileText: ASC.Files.Resources.SelectFilesToUpload,

                DragDropText: ASC.Files.Resources.OrDragDrop,
                SelectedFilesText: ASC.Files.Resources.SelectedFiles,
                SubmitPanelAfterSelectHolder: jq("#files_uploadDlgPanelButtons"),
                FileNameFilter: jq("#content_file_" + fileid + " div.fileName a.name").attr("title"),
                FileNameExeptionText: ASC.Files.Resources.FileNameIsNotEqual
            });
        }
        jq("#" + btnId).unbind("mouseover");
    };

    var fileUploaderSubmit = function() {
        PopupKeyUpActionProvider.EnterAction = "";
        totalCount = 0;
        if (fileUploader != null) {
            if (jq("#files_upload_fileList").html() == "") {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.EmptyListUpload, true);
                PopupKeyUpActionProvider.EnterAction = jq.browser.mozilla ? "" : 'jq("#files_uploadSubmit").click();';
                return;
            }
            jq("#uploads_message").text(ASC.Files.Resources.UploadProcess);
            jq("#panelButtons").hide();
            jq("#files_upload_btn_html5").hide();
            jq("#files_uploadDialogContainer .cancelButton").hide();

            fileUploader.Submit();
        }
    };

    var uploadFromHistory = function(obj) {
        var fileId = jq(obj).parents('.fileRow').attr("id").replace("content_file_", "");
        ASC.Files.Uploads.showUploadNewVersion(fileId);
    };

    var documentTitleFix = function() {
        if (jq.browser.msie)
            setInterval("document.title = jq('#files_folderName').html() + ' - ' + ASC.Files.Constants.TITLE_PAGE;", 200);
    };

    return {
        init: init,

        showUploadPopup: showUploadPopup,
        onUploadComplete: onUploadComplete,

        fileUploaderSubmit: fileUploaderSubmit,
        showUploadNewVersion: showUploadNewVersion,
        showUploadedFiles: showUploadedFiles,

        uploadFromHistory: uploadFromHistory,
        documentTitleFix: documentTitleFix
    };
})(jQuery);

(function($) {
    ASC.Files.Uploads.init();

    $(function() {

        jq("#files_uploadSubmit").click(function() {
            ASC.Files.Uploads.fileUploaderSubmit();
        });

        jq("#files_uploaddialog_btn").click(function() {
            ASC.Files.Uploads.showUploadPopup();
        });

        jq("#files_emptyFolder_btn").click(function() {
            ASC.Files.Uploads.showUploadPopup();
            return false;
        });

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("#files_uploaddialog_btn, #files_emptyFolder_btn, #files_uploads_files").remove();
        }
        
    });
})(jQuery);