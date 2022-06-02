var OAuthCallback = function(token, source) {
    ASC.Files.Import.getImportData(source, token);
};

var OAuthError = function(error, source) {
    ASC.Files.UI.displayInfoPanel(error, true);
};

var OAuthPopup = function(url, width, height) {
    var newwindow;
    try {
        var params = "height=" + height + ",width=" + width + ",resizable=0,status=0,toolbar=0,menubar=0,location=1";
        newwindow = window.open(url, "Authorization", params);
    }
    catch (err) {
        newwindow = window.open(url, "Authorization");
    }
    return newwindow;
};

window.ASC.Files.Import = (function($) {
    var ImportStatus = false;
    var isImport = false;
    var isInit = false;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(ASC.Files.TemplateManager.events.IsZohoAuthentificated, onIsZohoAuthentificated);
            serviceManager.bind(ASC.Files.TemplateManager.events.GetImportData, onGetImportData);
            serviceManager.bind(ASC.Files.TemplateManager.events.ExecImportData, ASC.Files.EventHandler.onGetTasksStatuses);
        }
    };

    var setFolderImportTo = function(idTo, folderName) {
        jq("#files_importToFolder").text(folderName);
        jq("#files_importToFolderId").val(idTo);

        ASC.Files.Actions.hideAllActionPanels();
    };

    var showSubmitLoginDialog = function() {
        jq("#files_pass").val("");
        jq("#files_login, #files_pass").removeAttr("disabled");
        jq("#LoginDialogTemp .action-block").show();
        jq("#LoginDialogTemp .ajax-info-block").hide();
        jq("#LoginDialogTemp div[id$='InfoPanel']").hide();

        ASC.Files.Common.blockUI(jq("#LoginDialogTemp"), 400, 500);
        PopupKeyUpActionProvider.EnterAction = 'jq("#files_submitLoginDialog").click();';
        PopupKeyUpActionProvider.CloseDialogAction = 'jq("#files_pass").val("");';

        jq("#files_login").focus();
    };

    var submitLoginDialog = function() {
        var login = jq("#files_login").val().trim();
        var pass = jq("#files_pass").val().trim();

        if (login == "" || pass == "") {
            var infoBlock = jq("#LoginDialogTemp div[id$='InfoPanel']");

            if (infoBlock.css("display") == "none")
                infoBlock
						.removeClass("infoPanel")
						.addClass("errorBox")
						.css("margin", "10px 16px 0")
						.show();

            infoBlock.find("div").text(ASC.Files.Resources.ErrorMassage_FiledsIsEmpty);

            jq("#files_pass").val("");
            return;
        }

        jq("#files_login, #files_pass").attr("disabled", "disabled");
        jq("#LoginDialogTemp .action-block").hide();
        jq("#LoginDialogTemp .ajax-info-block").show();

        isZohoAuthentificated({ login: login, pass: pass });
    };

    var execImportData = function(params) {
        var checkedDocuments = jq("#ImportDialogTemp input[name='checked_document']:checked");
        var infoBlock = jq("#ImportDialogTemp div[id$='InfoPanel']");

        if (checkedDocuments.length == 0) {
            if (infoBlock.css("display") == "none")
                infoBlock
						.removeClass("infoPanel")
						.addClass("errorBox")
						.css("margin", "10px 16px 0")
						.show();

            infoBlock.find("div").text(ASC.Files.Resources.EmptyListSelectedForImport);

            return;
        }

        var dataToSend = "<ArrayOfDataToImport>";
        Encoder.EncodeType = "!entity";
        checkedDocuments.each(function(index) {
            var cells = jq(this).parents('tr').find('td')
            dataToSend += "<DataToImport>";
            dataToSend += "<content_link>" + Encoder.htmlEncode(jq("<div/>").text(jq(this).val().trim()).html()) + "</content_link>";
            dataToSend += "<title>" + Encoder.htmlEncode(jq(cells[1]).text().trim()) + "</title>";
            dataToSend += "</DataToImport>";
        });
        Encoder.EncodeType = "entity";

        dataToSend += "</ArrayOfDataToImport>";

        params.tofolder = jq("#files_importToFolderId").val();
        params.ignoreCoincidenceFiles = jq("#ImportDialogTemp input[name='file_conflict']:checked").val();

        var importProgressPanel = jq("#ImportDialogTemp div.import_progress_panel");

        importProgressPanel.find(".studioFileUploaderProgressBar").width(0);
        importProgressPanel.find("span.percent").width(0);
        importProgressPanel.show();
        jq("#ImportDialogTemp input:checkbox").attr("disabled", "disabled");

        jq("#ImportDialogTemp .action-block").hide();
        jq("#ImportDialogTemp .action-block-progress").show();
        infoBlock.hide();
        var importsButton = jq("#files_import_container li.import");

        importsButton.unbind("click");
        importsButton.css("cursor", "pointer");

        requestImportData(params, dataToSend);
    };

    var showImportToSelector = function(obj_id) {
        ASC.Files.Import.isImport = true;

        ASC.Files.Tree.showTreePath();
        ASC.Files.Tree.showSelect("stree_selector_" + ASC.Files.Constants.FOLDER_ID_MY_FILES);

        ASC.Files.Actions.hideAllActionPanels();

        ASC.Files.Common.dropdownToggle(jq("#" + obj_id), 'files_treeViewPanelSelector');

        jq("#files_treeViewPanelSelector").css("z-index", "700");

        jq("body").click(function(event) {
            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf().is("#files_treeViewPanelSelector"))
                ASC.Files.Actions.hideAllActionPanels();
        });
    };

    var minimizeImportStatus = function(source) {
        var item;
        switch (source) {
            case "google":
                item = jq("#files_import_container	li.import_from_google");
                break;
            case "zoho":
                item = jq("#files_import_container	li.import_from_zoho");
                break;
            case "box.net":
            case "boxnet":
                item = jq("#files_import_container	li.import_from_boxnet");
                break;

            default:
                return false;
        }

        item.append(jq("#import_progress_min").clone(true).attr('id', "import_progress_min_clone").show());
    };

    var cancelImportData = function(text, isError) {
        ASC.Files.Import.ImportStatus = false;
        jq("#import_progress_min_clone").remove();
        if (jq("#ImportDialogTemp:visible").length != 0) {
            var idTo = jq("#files_importToFolderId").val();
            PopupKeyUpActionProvider.CloseDialog();
            ASC.Files.UI.navigationSet(idTo);
        }
        ASC.Files.UI.displayInfoPanel(text, isError === true);
    };

    //request

    var isZohoAuthentificated = function(params) {
        serviceManager.request("get",
                                "xml",
                                ASC.Files.TemplateManager.events.IsZohoAuthentificated,
                                params,
                                'import', 'isauth?source=zoho&login=' + params.login + '&pass=' + params.pass);
    };


    var getImportData = function(source) {

        var url = "";

        var params = {};
        params.source = source;
        params.showLoading = (params.source != 'zoho');

        if (source == "google" || source == "box.net")
            params.token = arguments[1];

        if (source == "zoho") {
            params.login = arguments[1];
            params.pass = arguments[2];
        }

        switch (params.source) {
            case 'google':
                url = 'import?source=google&token=' + params.token;
                break;
            case 'box.net':
                url = 'import?source=boxnet&token=' + params.token;
                break;
            case 'zoho':
                url = 'import?source=zoho&login=' + params.login + '&pass=' + params.pass;
                break;
            default:
                return;
        }
        serviceManager.request("get",
                                "xml",
                                ASC.Files.TemplateManager.events.GetImportData,
                                params,
                                url);
    };

    var requestImportData = function(params, data) {
        params.showLoading = true;
        var url = "";

        switch (params.source) {
            case 'google':
                url = 'import/exec?source=google&tofolder=' + params.tofolder + '&ignoreCoincidenceFiles=' + params.ignoreCoincidenceFiles + '&token=' + params.token;
                break;
            case 'zoho':
                url = 'import/exec?source=zoho&tofolder=' + params.tofolder + '&ignoreCoincidenceFiles=' + params.ignoreCoincidenceFiles + '&login=' + params.login + '&pass=' + params.pass;
                break;
            case 'box.net':
                url = 'import/exec?source=boxnet&tofolder=' + params.tofolder + '&ignoreCoincidenceFiles=' + params.ignoreCoincidenceFiles + '&token=' + params.token;
                break;
            default:
                return;
        }
        serviceManager.request("post",
                                "json",
                                ASC.Files.TemplateManager.events.ExecImportData,
                                params,
                                data,
                                url);
    };

    //event handler
    var onIsZohoAuthentificated = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }
        jq("#files_pass").val("");

        var infoBlock = jq("#LoginDialogTemp div[id$='InfoPanel']");
        if (jq(xmlData).text() != 'true') {

            if (infoBlock.css("display") == "none")
                infoBlock
						.removeClass("infoPanel")
						.addClass("errorBox")
						.css("margin", "10px 16px 0")
						.show();

            infoBlock.find("div").text(ASC.Files.Resources.ErrorMassage_AuthentificatedFalse);

            jq("#files_login, #files_pass").removeAttr("disabled");
            jq("#LoginDialogTemp .action-block").show();
            jq("#LoginDialogTemp .ajax-info-block").hide();
            return;
        }

        infoBlock.hide();
        ASC.Files.Import.getImportData('zoho', params.login, params.pass);
    };

    var onGetImportData = function(xmlData, params, errorMessage, commentMessage) {
        PopupKeyUpActionProvider.CloseDialog();

        if (typeof errorMessage != "undefined" || xmlData == null) {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        if (xmlData.getElementsByTagName("DataToImportList").length == 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.EmptyDataToImport, true);
            return;
        }

        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getImportData);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#import_data").html(htmlXML);

        jq("#ImportDialogTemp input[name='all_checked_document']").unbind("click").click(function() {
            var value = jq(this).is(':checked');
            jq("#ImportDialogTemp input[name='checked_document']").each(function() {
                jq(this).attr('checked', value);
            });
        });

        jq("#ImportDialogTemp input[name='checked_document']").unbind("click").click(function() {
            jq("#ImportDialogTemp input[name='all_checked_document']").attr('checked',
						jq("#ImportDialogTemp input[name='checked_document']:checked").length ==
						jq("#ImportDialogTemp input[name='checked_document']").length);
        });

        jq("#files_startImportData").unbind("click").click(function() {
            ASC.Files.Import.execImportData(params);
            return false;
        });

        jq("#files_importToFolderBtn").unbind("click").click(function() {
            ASC.Files.Import.showImportToSelector("files_importToFolderBtn");
            return false;
        });

        jq("#files_import_minimize").unbind("click").click(function() {
            PopupKeyUpActionProvider.CloseDialog();
            return false;
        });

        var importToFolderId = ASC.Files.Folders.currentFolderId;
        var importToFolder = jq("#files_folderName").text();

        if (!
                (ASC.Files.Folders.folderContainer == "my"
                    && ASC.Files.UI.accessibleItem("folder_" + importToFolderId)
                || importToFolderId == ASC.Files.Constants.FOLDER_ID_COMMON_FILES
                    && ASC.Files.Constants.USER_ADMIN)
            ) {
            importToFolderId = ASC.Files.Constants.FOLDER_ID_MY_FILES;
            importToFolder = jq("#stree_selector_" + importToFolderId).text();
        }

        jq("#files_importToFolderId").val(importToFolderId);
        jq("#files_importToFolder").text(importToFolder);

        switch (params.source) {
            case "google":
                jq("#ImportDialogTemp .containerHeaderBlock span.header-content, #import_to_folder span").text(ASC.Files.Resources.ImportFromGoogle);
                break;
            case "zoho":
                jq("#ImportDialogTemp .containerHeaderBlock span.header-content, #import_to_folder span").text(ASC.Files.Resources.ImportFromZoho);
                break;
            case "box.net":
                jq("#ImportDialogTemp .containerHeaderBlock span.header-content, #import_to_folder span").text(ASC.Files.Resources.ImportFromBoxNet);
                break;

            default:
                PopupKeyUpActionProvider.CloseDialog();
        }

        jq("#ImportDialogTemp input:checkbox").removeAttr("disabled");
        jq("#ImportDialogTemp .action-block").show();
        jq("#ImportDialogTemp .action-block-progress").hide();

        jq("#ImportDialogTemp .import_progress_panel").hide();
        jq("#ImportDialogTemp .studioFileUploaderProgressBar").width(0);
        jq("#ImportDialogTemp span.percent").text("0");
        jq("#ImportDialogTemp div[id$='InfoPanel']").hide();

        PopupKeyUpActionProvider.EnterAction = 'jq("#files_startImportData").click();';

        ASC.Files.Common.blockUI(jq("#ImportDialogTemp"), 900, 540);

    };

    var onGetImportStatus = function(data) {
        if (jq("#import_progress_min_clone").length == 0)
            ASC.Files.Import.minimizeImportStatus(data.source);

        ASC.Files.Import.ImportStatus = true;

        try {
            var progress = parseInt(data.progress) || 0;
        }
        catch (e) { progress = prBar.css('width') || 0; }

        if (progress > 0) {
            jq("#ImportDialogTemp .studioFileUploaderProgressBar").css('width', progress + '%');
            jq("#import_progress_min_clone .files_minimize_uploader").css('width', progress + '%');

            jq("#ImportDialogTemp span.percent").text(progress);
        }

        if (progress == 100) {
            ASC.Files.Import.cancelImportData(data.error || ASC.Files.Resources.InfoFinishImprot, data.error != null);
            if (data.error != null) {
                ASC.Files.UI.displayInfoPanel(data.error, true);
            }
            return true;
        }

        return false;
    };

    return {
        init: init,

        submitLoginDialog: submitLoginDialog,
        showSubmitLoginDialog: showSubmitLoginDialog,
        getImportData: getImportData,
        execImportData: execImportData,
        cancelImportData: cancelImportData,
        minimizeImportStatus: minimizeImportStatus,
        showImportToSelector: showImportToSelector,

        setFolderImportTo: setFolderImportTo,
        isImport: isImport,
        ImportStatus: ImportStatus,
        onGetImportStatus: onGetImportStatus

    };
})(jQuery);

(function($) {
    ASC.Files.Import.init();

    $(function() {

        jq("#files_import_container li.import_from_google").click(function() {
            if (jq("#import_progress_min_clone").length != 0 || ASC.Files.Import.ImportStatus) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoImprotNotFinish, true);
                return;
            }
            OAuthPopup(ASC.Files.Constants.URL_OAUTH_GOOGLE, 800, 600);
            return false;
        });

        jq("#files_import_container li.import_from_boxnet").click(function() {
            if (jq("#import_progress_min_clone").length != 0 || ASC.Files.Import.ImportStatus) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoImprotNotFinish, true);
                return;
            }
            OAuthPopup(ASC.Files.Constants.URL_OAUTH_BOXNET, 960, 600);
            return false;
        });

        jq("#files_import_container li.import_from_zoho").click(function() {
            if (jq("#import_progress_min_clone").length != 0 || ASC.Files.Import.ImportStatus) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoImprotNotFinish, true);
                return;
            }
            ASC.Files.Import.showSubmitLoginDialog();
            return false;
        });

        jq("#files_submitLoginDialog").click(function() {
            ASC.Files.Import.submitLoginDialog();
            return false;
        });

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("div.import_data_table").css({ "height": "auto" });
        }
    });
})(jQuery);