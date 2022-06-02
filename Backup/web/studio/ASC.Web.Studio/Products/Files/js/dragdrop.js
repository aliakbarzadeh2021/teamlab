window.ASC.Files.DragDrop = (function($) {
    var isInit = false;
    var totalDD = 0;
    var countDD = 0;
    var successDD = 0;

    var check_ready = true;
    var file_stek = new Array();

    var init = function() {
        if (isInit === false) {
            isInit = true;

            if (canDragDrop() && isFileAPI()) {
                jq("div.drag_drop_image, #empty_move_message").show();

                jq("#files_mainContent, #emptyScreen")
                    .bind("dragover", function(event) { return ASC.Files.DragDrop.over(event); })// show area for drop
	                .bind("dragenter", function() { return false; })//empty event
	                .bind("dragleave", function(event) { return ASC.Files.DragDrop.leave(event); })//hide area
	                .bind("drop", function(event) { return ASC.Files.DragDrop.drop(event); })// drop event

                blockDocumentDrop();
            }
            else {
                jq("div.drag_drop_image, #empty_move_message").remove();
            }
        }
    };

    var canDragDrop = function() {
        return 'draggable' in document.createElement('span')
                && typeof (new XMLHttpRequest()).upload != "undefined";
    };

    var isFileAPI = function() {
        return typeof FileReader != "undefined"
                && typeof (new XMLHttpRequest()).upload != "undefined";
    };

    var over = function(e) {
        var dt = e.originalEvent.dataTransfer;
        if (!dt) return;

        if (ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessibleItem()) {
            return;
        }

        //file?
        //FF
        if (dt.types.contains && !dt.types.contains("Files")) return;
        //Chrome
        if (dt.types.indexOf && dt.types.indexOf("Files") == -1) return;

        //bugfix chrome
        if (jq.browser.webkit) dt.dropEffect = 'copy';

        jq("#files_mainContent, #emptyScreen").addClass("selected");

        return false;
    };

    var leave = function(e) {
        return hideHighlight();
    };

    var drop = function(e) {
        var dt = e.originalEvent.dataTransfer;
        if (!dt && !dt.files) return;

        if (ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessibleItem()) {
            return;
        }

        hideHighlight();
        var allFiles = dt.files;
        var filesToUpload = new Array(0);

        for (var i = 0; i < allFiles.length; i++) {
            if (correctFile(allFiles[i])) {
                filesToUpload.push(allFiles[i]);
            }
        }

        ASC.Files.DragDrop.totalDD = ASC.Files.DragDrop.totalDD + filesToUpload.length;

        if (filesToUpload.length != 0) {
            for (var i = 0; i < filesToUpload.length; i++) {
                ASC.Files.DragDrop.uploadFile({ file: filesToUpload[i], folder: ASC.Files.Folders.currentFolderId });
            }
        }
        return false;
    };

    var hideHighlight = function() {
        jq("#files_mainContent, #emptyScreen").removeClass("selected");
        return false;
    };

    var correctFile = function(obj) {
        var posExt = ASC.Files.Utils.getFileExts(obj.name || obj.fileName);

        if ((obj.name || obj.fileName).length - posExt > 20)
            return false;

        if ((obj.size || obj.fileSize) <= 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_EmptyFile, true);
            return false;
        }

        if (!ASC.Files.Utils.FileCanBeUpload(obj.name || obj.fileName)) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_FileTypeText, true);
            return false;
        }

        if (ASC.Files.Constants.MAX_UPLOAD_KB < (obj.size || obj.fileSize) / 1024) {
            ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.ErrorMassage_LimitFileSize, ASC.Files.Constants.MAX_UPLOAD_KB / 1024), true);
            return false;
        }

        return true;
    };

    var blockDocumentDrop = function() {
        jq(document)
            .bind('dragenter', function() { return false; })
            .bind('dragleave', function() { return false; })
            .bind('dragover', function(e) {
                var dt = e.originalEvent.dataTransfer;
                if (!dt) { return; }
                dt.dropEffect = 'none';
                return false;
            } .bind(this));
    };

    //DragDrop
    var uploadFile = function(fileElem) {
        if (file_stek.length == 0 && check_ready == true) {
            check_ready = false;
            startUpload(fileElem);
        }
        else {
            file_stek.push(fileElem);
        }
    };

    var startUploadWithDelay = function() {
        var file = file_stek.shift();
        check_ready = false;
        startUpload(file);
    };

    var startUpload = function(fileElem) {
        var file = fileElem.file;
        var folder = fileElem.folder;

        try {
            var fileName = ASC.Files.Utils.replaceSpecCharacter(file.name || file.fileName);
            if (jq("#drag_drop_process").length == 0) {
                jq("#files_progress_template").clone().attr("id", "drag_drop_process").appendTo("#files_bottom_loader");
            }
            jq("#drag_drop_process .progress").css('width', '0%');
            jq("#drag_drop_process .percent").text('0%').css("color", "#1A6DB3");
            jq("#drag_drop_process .headerBaseMedium").html(ASC.Files.Resources.FileUploading + " " + fileName);
            jq("#drag_drop_process").show();

            var xhr = new XMLHttpRequest();

            xhr.upload.addEventListener("progress", function(e) { ASC.Files.DragDrop.onProgressDD(e); }, false);
            xhr.onprogress = function(e) { ASC.Files.DragDrop.onProgressDD(e); } .bind(this);

            xhr.onload = function(e) { onCompleteUpload(e); } .bind(this);

            var sitePath = ASC.Files.Common.getSitePath();

            sitePath += jq.format(ASC.Files.Constants.URL_HANDLER_UPLOAD, folder, encodeURIComponent(fileName));

            // parameter address is passed to the file name - otherwise nothing
            xhr.open('POST', sitePath, true);
            //xmlhttp.setRequestHeader('Content-Type', 'application/octet-stream');
            xhr.send(file);
        } catch (e) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_DnDError, true);
            onCompleteUpload();
        }
        return false;
    };

    var onCompleteUpload = function(e) {
        check_ready = true;
        ASC.Files.DragDrop.countDD++;

        if (e != undefined) {
            var result = e.target.responseText.split('|');
            var id = result[0];
            if (id != undefined && parseInt(id) > 0) {
                ASC.Files.DragDrop.successDD++;

                //current folder
                var folderDnD = result[1];
                if (folderDnD == ASC.Files.Folders.currentFolderId) {
                    if (jq("#content_file_" + id).html() != null && jq("#content_file_" + id).html() != "")
                        ASC.Files.Folders.replaceVersion(id, false, false);
                    else
                        ASC.Files.Folders.getFile(id, false);
                }
            } else {
                var strTitle = result[0].split("title>")[1].split("</")[0];
                if (strTitle && strTitle.length > 0)
                    ASC.Files.UI.displayInfoPanel(strTitle, true);
            }
        }

        jq("#drag_drop_process .progress").css('width', '100%');
        jq("#drag_drop_process .percent").text('100%').css("color", "white");
        jq("#drag_drop_process .textSmallDescribe").text(jq.format(ASC.Files.Resources.ProcessUploadCountFiles, ASC.Files.DragDrop.countDD, ASC.Files.DragDrop.totalDD));

        if (ASC.Files.DragDrop.successDD > 0)
            ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoUploadedSuccess, ASC.Files.DragDrop.successDD));

        var list = jq("div.newFile");
        if (list.length != 0) {
            ASC.Files.UI.hideEmpty();
            list.each(function() {
                ASC.Files.UI.addRowHandlers(this.id);
            });
        }
        list.removeClass("newFile").show().yellowFade();

        //end DnD
        if (file_stek.length == 0) {

            ASC.Files.DragDrop.totalDD = 0;
            ASC.Files.DragDrop.countDD = 0;
            ASC.Files.DragDrop.successDD = 0;

            setTimeout("jq('#drag_drop_process').hide();", 800);
        }
        else {
            if (check_ready == true) {
                setTimeout("ASC.Files.DragDrop.startUploadWithDelay()", 100);
            }
        }
    };

    var onProgressDD = function(e) {
        var progress = Math.min(Math.round(e.loaded * 100 / e.total), 100);
        jq("#drag_drop_process .progress").css('width', progress + '%');
        jq("#drag_drop_process .percent").text(progress + '%').css("color", progress > 45 ? "white" : "#1A6DB3");
        jq("#drag_drop_process .textSmallDescribe").text(jq.format(ASC.Files.Resources.ProcessUploadCountFiles, ASC.Files.DragDrop.countDD, ASC.Files.DragDrop.totalDD));
    };

    return {
        init: init,

        canDragDrop: canDragDrop,
        isFileAPI: isFileAPI,

        leave: leave,
        over: over,
        drop: drop,

        totalDD: totalDD,
        countDD: countDD,
        successDD: successDD,

        uploadFile: uploadFile,
        startUploadWithDelay: startUploadWithDelay,
        onProgressDD: onProgressDD
    };

})(jQuery);

(function($) {
    $(function() {
        ASC.Files.DragDrop.init();
    });
})(jQuery);