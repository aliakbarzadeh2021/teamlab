window.ASC.Files.PreviewFile = (function($) {
    var isInit = false,
        previewTimeout = null;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(ASC.Files.TemplateManager.events.GetPreviewData, onGetPreviewData);
            serviceManager.bind(ASC.Files.TemplateManager.events.GetSiblingsFile, onGetSiblingsFile);

            on_download_document = ASC.Files.PreviewFile.downloadFile;
            on_edit_document = ASC.Files.PreviewFile.editFile;
            on_next_document = ASC.Files.PreviewFile.nextDoc;
            on_previos_document = ASC.Files.PreviewFile.prevDoc;
        }
    };

    var initFlash = function(uri) {

        var idDoc = jq("#hdnFileId").val();
        var curElem = jq("#hdn_" + idDoc);
        if (curElem.length == 0) {
            ASC.Files.PreviewFile.downloadFile();
            ASC.Files.UI.madeAnchor(ASC.Files.Folders.currentFolderId);
            return;
        }
        var title = jq("#PreviewFileTitle").html();
        var waitMsg, errorMsg;
        var canEdit = ASC.Utils.FileCanBeEdit(title) &&
                      jq("#file_IsEditing_" + idDoc).val() != 'true' &&
                      (ASC.Files.Constants.USER_ADMIN === true ||
                       jq("#by_previewfile_" + idDoc).val() === ASC.Files.Constants.USER_ID);

        clearTimeout(ASC.Files.PreviewFile.previewTimeout);
        switch (jq("#file_preview_status").val()) {
            case "error":
                errorMsg = ASC.Files.Common.getSitePath() + jq.format(ASC.Files.Constants.URL_HANDLER_PREVIEW, idDoc, true);
                waitMsg = "";
                uri = "";
                previewTimeout = setTimeout("ASC.Files.PreviewFile.loadDataInfo(" + idDoc + ");", 30000);
                break;
            case "wait":
                errorMsg = "";
                waitMsg = ASC.Files.Common.getSitePath() + jq.format(ASC.Files.Constants.URL_HANDLER_PREVIEW, idDoc, false);
                uri = "";
                previewTimeout = setTimeout("ASC.Files.PreviewFile.loadDataInfo(" + idDoc + ");", 30000);
                break;
            default:
                errorMsg = "";
                waitMsg = "";
                uri = Encoder.htmlDecode(uri);
                break;
        }

        var movie = jq("#DocumentPlayer");
        if (movie.length != 0) {
            movie[0].SetURL(uri,
                            (curElem.prev().length == 0 ? '0' : '1'),
                            (curElem.next().length == 0 ? '0' : '1'),
                            canEdit ? "1" : "0");

            movie[0].ShowMessage(waitMsg);
            if (errorMsg != "")
                jq("#DocumentPlayer")[0].ShowError(errorMsg);
        }
        else {
            jq('#forFlashContent').empty();
            jq("#flashContentTemplate").clone(true).appendTo("#forFlashContent");
            jq("#flashContentTemplate").attr('id', 'flashContent').show();

            var swfVersionStr = "9.0.124";
            var xiSwfUrlStr = "${expressInstallSwf}";
            var flashvars = {
                SwfFile: encodeURIComponent(uri),
                prevdoc: (curElem.prev().length == 0 ? '0' : '1'),
                nextdoc: (curElem.next().length == 0 ? '0' : '1'),
                editbtn: canEdit ? "1" : "0",
                Scale: 0.6,
                ZoomTransition: "easeOut",
                ZoomTime: 0.5,
                ZoomInterval: 0.1,
                FitPageOnLoad: false,
                FitWidthOnLoad: true,
                PrintEnabled: true,
                FullScreenAsMaxWindow: true,
                localeChain: ASC.Files.Constants.CURRENT_CULTURE || "en_US"
            };

            if (waitMsg != "")
                flashvars.ShowMessage = encodeURIComponent(waitMsg);
            if (errorMsg != "")
                flashvars.ShowError = encodeURIComponent(errorMsg);

            var params = {};
            params.quality = "high";
            params.bgcolor = "#ffffff";
            params.allowscriptaccess = "always";
            params.allowfullscreen = "true";
            params.wmode = "transparent";
            var attributes = {};
            attributes.id = "DocumentPlayer";
            attributes.name = "DocumentPlayer";

            var nSwfHeight = 800;

            swfobject.embedSWF(
                ASC.Files.Constants.URL_PREVIEW_SWF, "flashContent",
                "100%", nSwfHeight,
                swfVersionStr, xiSwfUrlStr,
                flashvars, params, attributes);
        }
    };

    var downloadFile = function() {
        var id = jq("#hdnFileId").val();
        if (id == undefined || id == '')
            return;

        ASC.Files.Folders.downloadFile(id);
    };

    var editFile = function() {
        var id = jq("#hdnFileId").val();
        if (id == undefined || id == '') return;

        var title = jq("#PreviewFileTitle").html();

        if (!ASC.Utils.FileCanBeEdit(title) ||
            jq("#file_IsEditing_" + id).val() === 'true' ||
            (ASC.Files.Constants.USER_ADMIN != true &&
             jq("#by_previewfile_" + id).val() != ASC.Files.Constants.USER_ID))
            return;

        ASC.Files.Actions.checkEditFile(id, title);
    };

    var getSiblingsFile = function(fileId) {
        serviceManager.getSiblingsFile(ASC.Files.TemplateManager.events.GetSiblingsFile, { fileID: fileId });
    };

    var prevDoc = function() {
        var item = jq("#hdn_" + jq("#hdnFileId").val()).prev();
        if (item.length == 0)
            return;

        var link = item.attr('id').replace('hdn_', '');
        selectDoc(link);
    };

    var nextDoc = function() {
        var item = jq("#hdn_" + jq("#hdnFileId").val()).next();
        if (item.length == 0)
            return;

        var link = item.attr('id').replace('hdn_', '');
        selectDoc(link);
    };

    var selectDoc = function(id) {
        ASC.Controls.AnchorController.move(ASC.Files.PreviewFile.GetPreviewUrl(id));
    };

    var GetPreviewUrl = function(FileId) {
        var url = "#preview/" + FileId;
        return url;
    };

    var loadDataInfo = function(id) {
        if (id != jq("#hdnFileId").val() || jq("#PreviewFileAscx:visible").length == 0) {
            clearTimeout(ASC.Files.PreviewFile.previewTimeout);
            return;
        }

        serviceManager.getPreviewData(ASC.Files.TemplateManager.events.GetPreviewData, { fileID: id });
    };

    var onGetPreviewData = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != 'undefined') {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            ASC.Files.Folders.currentFolderId = 0;
            ASC.Files.UI.defaultFolderSet();
            return;
        }

        if (xmlData == null) {
            ASC.Files.Folders.currentFolderId = 0;
            ASC.Files.UI.defaultFolderSet();
            return;
        }

        if (jq("#hdnFileId").val() != params.fileID)
            return;

        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFilePreview);
        if (typeof xslData === 'undefined') { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq('#preview_file_info').html(htmlXML);

        ASC.Files.Folders.currentFolderId = jq("#file_preview_folder_id").val();

        jq('#preview_container').find('div.container:first').removeClass('loading');

        document.title = jq("#PreviewFileTitle").html() + ' - ' + ASC.Files.Constants.TITLE_PAGE;

        if (jq("#hiddenPreviewContainer").html().trim() == "") {
            ASC.Files.PreviewFile.getSiblingsFile(jq("#hdnFileId").val());
            return;
        }

        initFlash(jq("#file_preview_physical_path").val());
    };

    var onGetSiblingsFile = function(jsonData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != 'undefined') {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            ASC.Files.PreviewFile.downloadFile();
            ASC.Files.UI.madeAnchor(ASC.Files.Folders.currentFolderId);
            return;
        }

        if (jsonData == null) {
            ASC.Files.PreviewFile.downloadFile();
            ASC.Files.UI.madeAnchor(ASC.Files.Folders.currentFolderId);
            return;
        }

        var data = eval(jsonData.path_parts);

        var visibleItems = [];

        var maxLength = 100;
        var maxLengthItem;

        var html = '';
        var i;

        if (ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT != 0) {
            var outside = -1;
            jq(data).each(function(i) {
                if (this.Key == ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT)
                    outside = i;
            })

            if (outside == -1) {
                ASC.Files.UI.navigationSet(ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT);
                return;
            }

            data.splice(0, outside);
            data[0].Value = ASC.Files.Resources.ProjectFiles;
        }

        if (data.length > 4) {
            maxLengthItem = maxLength / 3;

            visibleItems[0] = { id: data[0].Key,
                title: data[0].Value,
                content: data[0].Value.substring(0, maxLengthItem)
            };

            visibleItems[1] = { id: data[data.length - 3].Key,
                content: "...",
                title: ""
            };

            jq(data.slice(1, data.length - 2)).each(
                function(index, obj) {
                    if (index == data.length - 4) {
                        visibleItems[1].title += obj.Value;
                        visibleItems[1].id = obj.Key;
                    }
                    else
                        visibleItems[1].title += obj.Value + " > ";
                }
            );

            visibleItems[2] = { id: data[data.length - 2].Key,
                title: data[data.length - 2].Value,
                content: data[data.length - 2].Value.substring(0, maxLengthItem)
            };

            visibleItems[3] = { id: data[data.length - 1].Key,
                title: data[data.length - 1].Value,
                content: data[data.length - 1].Value.substring(0, maxLengthItem)
            };
        }
        else {
            maxLengthItem = maxLength / data.length;

            for (i = 0; i < data.length; i++) {

                var id = data[i].Key;
                var content = data[i].Value;
                var title = content;

                content = content.substring(0, maxLengthItem) + (content.length > maxLengthItem ? "..." : "");

                visibleItems[i] = { id: id, content: content, title: title };
            }
        }

        ASC.Files.Folders.pathParts = data;

        for (i = 0; i < visibleItems.length; i++) {

            var id = visibleItems[i].id;
            var content = visibleItems[i].content;
            var title = visibleItems[i].title;

            html += jq.format("<a href='#{0}' onclick='ASC.Files.UI.madeAnchor({0});return false;' title='{1}' >{2}</a>",
                              id, title, content, ASC.Files.Resources.BreadCrumbsSeparator);

            if (i < visibleItems.length - 1)
                html += jq.format("<span>{0}</span>", ASC.Files.Resources.BreadCrumbsSeparator);
        }

        jq("#previewBreadCrumbs").html(html);



        var str = "";
        for (var i = 0; i < jsonData.files.length; i++)
            str += jq.format("<input type='hidden' id='hdn_{0}' value='{0}'/>", jsonData.files[i]);

        jq("#hiddenPreviewContainer").html(str);

        if (str == "") {
            ASC.Files.PreviewFile.downloadFile();
            ASC.Files.UI.madeAnchor(ASC.Files.Folders.currentFolderId);
            return;
        }

        jq(document).scrollTo("#PreviewFileAscx");
        initFlash(jq("#file_preview_physical_path").val());
    };

    var onUpdatePreviewAnchor = function(docId) {
        jq("#hdnFileId").val(docId)
        ASC.Files.PreviewFile.loadDataInfo(docId);
    };

    return {
        init: init,
        previewTimeout: previewTimeout,

        prevDoc: prevDoc,
        nextDoc: nextDoc,
        selectDoc: selectDoc,

        loadDataInfo: loadDataInfo,

        downloadFile: downloadFile,
        editFile: editFile,

        getSiblingsFile: getSiblingsFile,

        onUpdatePreviewAnchor: onUpdatePreviewAnchor,
        GetPreviewUrl: GetPreviewUrl
    };
})(jQuery);

(function($) {
    ASC.Files.PreviewFile.init();

    $(function() {

    });
})(jQuery);