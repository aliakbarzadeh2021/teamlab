window.ASC.Files.EventHandler = (function($) {
    var isInit = false;
    var timoutTasksStatuses = null;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            ASC.Controls.AnchorController.bind(ASC.Files.Constants.anchorRegExp.anyanchor, ASC.Files.EventHandler.onValidationAnchor);
            ASC.Controls.AnchorController.bind(ASC.Files.Constants.anchorRegExp.error, ASC.Files.EventHandler.onError);
            ASC.Controls.AnchorController.bind(ASC.Files.Constants.anchorRegExp.folder, ASC.Files.EventHandler.onSelectAnchor);
            ASC.Controls.AnchorController.bind(ASC.Files.Constants.anchorRegExp.preview, ASC.Files.EventHandler.onPreviewAnchor);

            serviceManager.bind(ASC.Files.TemplateManager.events.GetFolderItems, ASC.Files.EventHandler.onGetFolderItems);

            serviceManager.bind(ASC.Files.TemplateManager.events.CheckEditFile, ASC.Files.EventHandler.onCheckEditFile);

            serviceManager.bind(ASC.Files.TemplateManager.events.GetFolderInfo, ASC.Files.EventHandler.onGetFolderInfo);
            serviceManager.bind(ASC.Files.TemplateManager.events.CreateFolder, ASC.Files.EventHandler.onCreateFolder);
            serviceManager.bind(ASC.Files.TemplateManager.events.CreateNewFile, ASC.Files.EventHandler.onCreateNewFile);

            serviceManager.bind(ASC.Files.TemplateManager.events.GetFile, ASC.Files.EventHandler.onGetFile);

            serviceManager.bind(ASC.Files.TemplateManager.events.FolderRename, ASC.Files.EventHandler.onRenameFolder);
            serviceManager.bind(ASC.Files.TemplateManager.events.FileRename, ASC.Files.EventHandler.onRenameFile);

            serviceManager.bind(ASC.Files.TemplateManager.events.GetFileHistory, ASC.Files.EventHandler.onGetFileHistory);
            serviceManager.bind(ASC.Files.TemplateManager.events.SetCurrentVersion, ASC.Files.EventHandler.onSetCurrentVersion);
            serviceManager.bind(ASC.Files.TemplateManager.events.ReplaceVersion, ASC.Files.EventHandler.onReplaceVersion);

            serviceManager.bind(ASC.Files.TemplateManager.events.MoveFilesCheck, ASC.Files.EventHandler.onMoveFilesCheck);

            serviceManager.bind(ASC.Files.TemplateManager.events.MoveItems, ASC.Files.EventHandler.onGetTasksStatuses);
            serviceManager.bind(ASC.Files.TemplateManager.events.DeleteItem, ASC.Files.EventHandler.onGetTasksStatuses);
            serviceManager.bind(ASC.Files.TemplateManager.events.EmptyTrash, ASC.Files.EventHandler.onGetTasksStatuses);
            serviceManager.bind(ASC.Files.TemplateManager.events.Download, ASC.Files.EventHandler.onGetTasksStatuses);
            serviceManager.bind(ASC.Files.TemplateManager.events.TerminateTasks, ASC.Files.EventHandler.onGetTasksStatuses);
            serviceManager.bind(ASC.Files.TemplateManager.events.GetTasksStatuses, ASC.Files.EventHandler.onGetTasksStatuses);
        }
    };

    /* Events */

    var onValidationAnchor = function(anchor) {
        if (ASC.Files.Constants.anchorRegExp.error.test(anchor)
            || ASC.Files.Constants.anchorRegExp.folder.test(anchor)
            || ASC.Files.Constants.anchorRegExp.preview.test(anchor)
            )
            return;

        ASC.Files.UI.defaultFolderSet();
    };

    var onError = function(errorString) {
        ASC.Files.UI.displayInfoPanel(decodeURIComponent(errorString).replace(/\+/g, " "), true);
        ASC.Files.UI.defaultFolderSet();
    };

    var onSelectAnchor = function(itemid, param1, order, galery) {
        jq("#files_trewViewContainer li.jstree-open, #files_trewViewSelector li.jstree-open")
            .addClass("jstree-closed").removeClass("jstree-open");

        if (order != undefined && order != "")
            ASC.Files.UI.viewOrder = order;
        else
            ASC.Files.UI.viewOrder = '00';

        ASC.Files.UI.setViewFolder(galery);
        if (itemid == parseInt(itemid))
            ASC.Files.Folders.currentFolderId = itemid;
        else
            ASC.Files.Folders.currentFolderId = 0;

        ASC.Files.Actions.hideAllActionPanels();

        ASC.Files.UI.updateFolderView();
    };

    var onPreviewAnchor = function(fileId) {
        if (ASC.Files.ImageViewer != undefined)
            ASC.Files.ImageViewer.init(fileId);
        else
            ASC.Files.UI.defaultFolderSet();
    };

    var onGetFolderItems = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            ASC.Files.UI.defaultFolderSet();
            return undefined;
        }

        var xslData;
        if (ASC.Files.UI.viewFolder == "g") {
            xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFolderItems_thumb);
            jq("#files_mainContent").addClass("thumbnails")
        }
        else {
            xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFolderItems);
            jq("#files_mainContent").removeClass("thumbnails")
        }

        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        ASC.Files.UI.viewFilter = parseInt(ASC.Files.UI.viewFilter) || 0;
        ASC.Files.UI.resetSelectAll(false);

        ASC.Files.UI.hideEmpty();

        if (params.append == true)
            jq("#files_mainContent").append(htmlXML);
        else
            jq("#files_mainContent").html(htmlXML);

        if (!ASC.Files.Constants.ENABLE_VERSIONS)
            jq("div.file-info div.version").remove();

        jq("div[name='addRow']").each(function() {
            if (jq("[id='" + this.id + "'][name!='addRow']").length != 0)
                jq(this).remove();
        });

        var countTotal = 0;
        if (htmlXML != "") {
            countTotal = xmlData.getElementsByTagName('total')[0];
            countTotal = parseInt(countTotal.text || countTotal.textContent) || 0;
        }

        ASC.Files.UI.countEntityInFolder = countTotal;
        var countShowOnPage = parseInt(ASC.Files.Constants.COUNT_ON_PAGE) || 0;
        ASC.Files.UI.amountPage = parseInt((countTotal / countShowOnPage).toFixed(0));

        if (ASC.Files.UI.amountPage - (countTotal / countShowOnPage) < 0)
            ASC.Files.UI.amountPage++;

        ASC.Files.UI.currentPage = parseInt((jq("div.fileRow[name!='addRow']").length - 1) / countShowOnPage) + 1;
        var countLeft = countTotal - jq("div.fileRow").length;
        if (ASC.Files.UI.currentPage < ASC.Files.UI.amountPage && countLeft > 0) {
            jq("#files_pageNavigatorHolder")
											.show()
											.children('a')
											.unbind("click")
											.click(function() { ASC.Files.Folders.showMore(); return false; })
                                            .text(countShowOnPage < countLeft ?
                                                    jq.format(ASC.Files.Resources.ButtonShowMoreOf, countShowOnPage, countLeft) :
                                                    jq.format(ASC.Files.Resources.ButtonShowMore, countLeft));
        } else {
            jq("#files_pageNavigatorHolder").hide();
        }

        if (ASC.Files.Folders.currentFolderId === 0) {
            ASC.Files.UI.defaultFolderSet();
            return;
        }

        if (onGetFolderInfo(xmlData.getElementsByTagName("folder_info")[0], { id: ASC.Files.Folders.currentFolderId }))
            return;

        if (onGetPathParts(xmlData.getElementsByTagName("path_parts")[0], { id: ASC.Files.Folders.currentFolderId }))
            return;

        //        if (ASC.Files.Share != undefined && xmlData.getElementsByTagName("count_new").length != 0)
        //            ASC.Files.Share.displayCountNew(xmlData.getElementsByTagName("count_new")[0]);

        if (htmlXML == "")
            ASC.Files.UI.displayEmptyScreen();
        else
            ASC.Files.UI.addRowHandlers();

        var tipText = xmlData.getElementsByTagName("tip")[0];
        if (tipText != undefined) {
            tipText = tipText.text || tipText.textContent;
            jq("#tips_side_container").text(tipText);
        }

        var quota = xmlData.getElementsByTagName("quota")[0];
        ASC.Files.UI.showQuota(quota.text || quota.textContent);
    };

    var onGetFolderInfo = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return false;
        }

        if (xmlData == null) {
            ASC.Files.UI.defaultFolderSet();
            return true;
        }

        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFolderInfo);
        if (typeof xslData === "undefined") { return false; }

        var htmlData = ASC.Controls.XSLTManager.translate(xmlData, xslData);
        jq("#headerContainer").html(htmlData);

        var folderBigIcon = jq("#headerContainer div.folderBigIcon");

        if (ASC.Files.Share != undefined) {
            var curAccess = jq("input[type='hidden']#access_folder_" + ASC.Files.Folders.currentFolderId).val();
            if (curAccess == ASC.Files.Constants.AceStatusEnum.Restrict) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resource.AceStatusEnum_Restrict, true);
                ASC.Files.UI.defaultFolderSet();
                return true;
            }
        }

        switch (ASC.Files.Folders.currentFolderId) {
            case ASC.Files.Constants.FOLDER_ID_TRASH:
                folderBigIcon.addClass("Trash");
                break;
            case ASC.Files.Constants.FOLDER_ID_MY_FILES:
                folderBigIcon.addClass("myFiles");
                break;
            case ASC.Files.Constants.FOLDER_ID_COMMON_FILES:
                folderBigIcon.addClass("corporateFiles");
                break;
            case ASC.Files.Constants.FOLDER_ID_SHARE:
                folderBigIcon.addClass("sharedformeFiles");
                break;
            case ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT:
                folderBigIcon.addClass("projectFiles");
                jq(".levelUp").parent().remove();
                jq("#files_folderName").html(ASC.Files.Resources.ProjectFiles);
                break;
            case 0:
                ASC.Files.UI.defaultFolderSet();
                return true;
                break;
        }

        document.title = jq("#files_folderName").html() + ' - ' + ASC.Files.Constants.TITLE_PAGE;
        return false;
    };

    var onGetPathParts = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return false;
        }
        if (xmlData == undefined || xmlData == null)
            return false;

        if (typeof params !== "object") {
            params = {};
        }

        var data = new Array();
        xmlData = xmlData.childNodes;
        for (var i = 0; i < xmlData.length; i++) {
            data.push(
					{
					    Key: xmlData[i].childNodes[0].textContent || xmlData[i].childNodes[0].text,
					    Value: xmlData[i].childNodes[1].textContent || xmlData[i].childNodes[1].text
					});
        }

        var visibleItems = [];

        var maxLength = 100;
        var maxLengthItem;

        var html = "";
        var i;

        if (ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT != 0) {
            var outside = -1;
            jq(data).each(function(i) {
                if (this.Key == ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT)
                    outside = i;
            })

            if (outside == -1) {
                ASC.Files.UI.navigationSet(ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT);
                return true;
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

            if (i != visibleItems.length - 1)
                html += jq.format("<a href='#{0}' onclick='ASC.Files.UI.madeAnchor({0});return false;' title='{1}' >{2}</a><span>{3}</span>",
                                  id, title, content, ASC.Files.Resources.BreadCrumbsSeparator);
            else
                html += jq.format("<span class='breadCrumbsContainer_currentFolder' title='{0}'>{1}</span>",
                                    visibleItems[visibleItems.length - 1].title, visibleItems[visibleItems.length - 1].content);
        }

        jq("#files_breadCrumbsContainer").html(html);

        jq("ul.files_navigation li a.current").removeClass("current");

        if (data.length == 0)
            return false;

        var container = data[0].Key
        jq("#files_navigation_" + container).addClass("current");
        jq("#files_filter_value_1_, #files_filter_value_2_").show();

        switch (container) {
            case ASC.Files.Constants.FOLDER_ID_MY_FILES:
                ASC.Files.Folders.folderContainer = "my";
                break;
            case ASC.Files.Constants.FOLDER_ID_COMMON_FILES:
                ASC.Files.Folders.folderContainer = "corporate";
                break;
            case ASC.Files.Constants.FOLDER_ID_SHARE:
                ASC.Files.Folders.folderContainer = "forme";
                break;
            case ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT:
                ASC.Files.Folders.folderContainer = "project";
                break;
            case ASC.Files.Constants.FOLDER_ID_TRASH:
                ASC.Files.Folders.folderContainer = "trash";
                jq(".mainContentHeader_item_actions").addClass("display");
                break;
            default:
                ASC.Files.UI.defaultFolderSet();
                return true;
        }

        if (ASC.Files.Share != undefined
            && (container != ASC.Files.Constants.FOLDER_ID_COMMON_FILES
                || ASC.Files.Constants.USER_ADMIN)
            && container != ASC.Files.Constants.FOLDER_ID_TRASH
            && container != ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT
            && ASC.Files.UI.accessAdmin("folder_" + id)
            && jq("#folder_shareable").val() != "false"
            ) {
            jq("#mainMenuHolder").addClass("withShare");
        } else {
            jq("#mainMenuHolder").removeClass("withShare");
        }

        ASC.Files.UI.markForMe("folder_" + ASC.Files.Folders.currentFolderId);

        return false;
    };

    var onCreateNewFile = function(xmlData, params, errorMessage, commentMessage) {
        ASC.Files.UI.blockItem("content_file_NEW_FILE");
        if (typeof errorMessage != "undefined") {
            jq("#content_file_NEW_FILE").remove();
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);

            return undefined;
        }

        var curTemplate = (ASC.Files.UI.viewFolder == "g" ? ASC.Files.TemplateManager.templates.getFolderItem_thumb : ASC.Files.TemplateManager.templates.getFolderItem);
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }

        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#content_file_NEW_FILE").replaceWith(htmlXML);
        ASC.Files.UI.resetSelectAll(false);

        var title = jq("div.newFile div.name a").attr("title");
        var id = jq("div.newFile").attr("id").replace("content_file_", "");

        if (!ASC.Files.Constants.ENABLE_VERSIONS)
            jq("div.file-info div.version").remove();

        var fileId = jq("div.newFile").show().yellowFade().removeClass("newFile").attr("id");

        ASC.Files.UI.addRowHandlers(fileId);
        jq("#content_info_count_files").html(1 + (parseInt(jq("#content_info_count_files").html()) || 0));

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoCrateFile, title));

        var webEditor = ASC.Files.Utils.FileCanBeEdit(title);

        if (jq.browser.mozilla && webEditor) {
            jq("#" + fileId).addClass("isNewForWebEditor");
        } else {
            ASC.Files.Actions.checkEditFile(id, title, webEditor, true);
        }
    };

    var onCreateFolder = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            jq("#content_folder_NEW_FOLDER").remove();
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var curTemplate = (ASC.Files.UI.viewFolder == "g" ? ASC.Files.TemplateManager.templates.getFolderItem_thumb : ASC.Files.TemplateManager.templates.getFolderItem);
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }

        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#content_folder_NEW_FOLDER").replaceWith(htmlXML);
        ASC.Files.UI.resetSelectAll(false);

        var title = jq("div.newFolder div.name a").attr("title");

        var folderId = jq("div.newFolder").yellowFade().removeClass("newFolder").attr("id");

        ASC.Files.UI.addRowHandlers(folderId);

        jq("#content_info_count_folders").html(1 + (parseInt(jq("#content_info_count_folders").html()) || 0));

        jq("#tree_node_" + ASC.Files.Folders.currentFolderId).addClass('jstree-closed').removeClass('jstree-open')
															.children('ul').remove();
        jq("#stree_node_" + ASC.Files.Folders.currentFolderId).addClass('jstree-closed').removeClass('jstree-open')
															.children('ul').remove();

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoCrateFolder, title));
    };

    var onRenameFolder = function(xmlData, params, errorMessage, commentMessage) {
        ASC.Files.UI.blockItem("content_folder_" + params.id);
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var newName = xmlData.childNodes[0].text || xmlData.childNodes[0].textContent;

        jq("#content_folder_" + params.id + " div.sub-folder").attr('title', newName);
        jq("#content_folder_" + params.id + " div.folder_thumbnail").attr('title', newName);
        jq("#content_folder_" + params.id + " div.fileName a.name").attr('title', newName);

        var clippingTitle = ASC.Files.Utils.clippingTitle(newName, true, (ASC.Files.UI.viewFolder == "g"))
        jq("#content_folder_" + params.id + " div.fileName a.name").text(clippingTitle);

        var treeHtml = jq("#tree_selector_" + params.id).html()
        if (treeHtml != null) jq("#tree_selector_" + params.id).html(treeHtml.replace(params.name, newName))
        treeHtml = jq("#stree_selector_" + params.id).html()
        if (treeHtml != null) jq("#stree_selector_" + params.id).html(treeHtml.replace(params.name, newName))

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoRenameFolder, params.name, newName));
    };

    var onRenameFile = function(xmlData, params, errorMessage, commentMessage) {
        ASC.Files.UI.blockItem("content_file_" + params.id);
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var newName = xmlData.childNodes[0].text || xmlData.childNodes[0].textContent;

        jq("#content_file_" + params.id + " div.folder-file").attr('title', newName);
        jq("#content_file_" + params.id + " div.folder_thumbnail").attr('title', newName);
        jq("#content_file_" + params.id + " div.fileName a.name").attr('title', newName);

        var isEditing = ASC.Files.UI.editableFile("content_file_" + params.id, true) && !ASC.Files.UI.editingFile("content_file_" + params.id);
        var clippingTitle = ASC.Files.Utils.clippingTitle(newName, false, (ASC.Files.UI.viewFolder == "g"), isEditing);

        jq("#content_file_" + params.id + " div.fileName a.name").text(clippingTitle);

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoRenameFile, params.name, newName));
    };

    var onSetCurrentVersion = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.blockItem("content_file_" + params.id);
            jq("div.version_restore,div.versions_btn").css('visibility', "");
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var version = (parseInt(jq(".versionRow .version_num:first").html()) || 0) + 1;
        var modified_on = jq("#content_file_" + params.id + " span.modified_date").html();
        var href = ASC.Files.Utils.fileDownloadUrl(params.id, version);
        var content_length = jq("#content_file_" + params.id + " span.content_length").html();
        var modifiedById = jq("#modified_by_file_" + params.id).val();
        var modifiedByName = jq("#content_file_" + params.id + " span.create_by").html().trim();

        var xmlRowString =
"<fileList withoutheader='true'>\
    <entry>\
        <id>" + params.id + "</id>\
        <modified_by_id>" + modifiedById + "</modified_by_id>\
        <modified_by>" + modifiedByName + "</modified_by>\
        <modified_on>" + modified_on + "</modified_on>\
        <content_length>" + content_length + "</content_length>\
        <version>" + version + "</version>\
    </entry>\
</fileList>";

        var xslDataRow = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFileHistory);
        if (typeof xslDataRow === "undefined") { return undefined; }
        var lastVersion = ASC.Controls.XSLTManager.translateFromString(xmlRowString, xslDataRow);

        jq("#content_file_" + params.id + " div.headerTableVersions").after(lastVersion);

        var tmpTable = jq("#content_file_" + params.id + " div.tableVersionRow");
        jq("#content_file_" + params.id + " div.headerTableVersions").after(tmpTable.html());
        tmpTable.remove();

        jq("div.version_num a:first").attr("href", ASC.Files.Utils.fileViewUrl(params.id, version));

        jq("div.versionRow.even").removeClass("even");
        jq("div.versionRow:even").addClass("even");

        try {
            var content_versions = jq("#content_file_" + params.id + " #content_versions").clone(true);
        } catch (ex) {
            ASC.Files.Folders.showVersions('combo_file_' + params.id);
            return false;
        }

        var curTemplate = ASC.Files.TemplateManager.templates.getFolderItem;
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#content_file_" + params.id).replaceWith(htmlXML);

        var urlIcn = ASC.Files.Utils.getFileTypeIcon(jq("#content_file_" + params.id + " .fileName a.name").attr("title"));
        jq("#file_icn_" + params.id).css('background', "url('" + urlIcn + "') no-repeat");

        jq("div.newFile").removeClass("newFile").show();

        jq("#content_file_" + params.id).append(content_versions);

        ASC.Files.UI.addRowHandlers("content_file_" + params.id);

        ASC.Files.UI.blockItem("content_file_" + params.id);
        jq("div.version_restore, div.versions_btn").css('visibility', "");

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.FileVersionRecovery, jq("#content_file_" + params.id + " div.fileName a.name").attr("title")), false);
    };

    var onGetFileHistory = function(xmlData, params, errorMessage, commentMessage) {
        ASC.Files.UI.blockItem("content_file_" + params.id);
        ASC.Files.UI.selectRow(jq("#content_file_" + params.id), true);
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }
        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFileHistory);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#content_versions").remove();
        jq("#content_file_" + params.id).append(htmlXML);

        var tmpTable = jq("#content_file_" + params.id + " div.tableVersionRow");
        jq("#content_file_" + params.id + " div.headerTableVersions").after(tmpTable.html());
        tmpTable.remove();

        jq("#content_versions div.versionRow").each(function() {
            var version = this.id.replace('content_fileversion_' + params.id + '_', "");
            jq("div.version_num a", this).attr("href", ASC.Files.Utils.fileViewUrl(params.id, version));
        });

        if (!ASC.Files.UI.accessibleItem("file_" + params.id)) {
            jq("div.version_restore, #content_versions_btn").remove();
        }

        var titleFile = jq("#content_file_" + params.id + " div.fileName a.name").attr("title");
        if (ASC.Files.Utils.FileCanBePreview(titleFile)
            && ASC.Files.Utils.itIsDocument(titleFile)) {
            jq(".versionsRow.notPreview").removeClass("notPreview");
        }

        if (ASC.Controls.Constants.isMobileAgent)
            jq("#content_versions_btn").remove();
    };

    var onReplaceVersion = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var curTemplate = (ASC.Files.UI.viewFolder == "g" ? ASC.Files.TemplateManager.templates.getFolderItem_thumb : ASC.Files.TemplateManager.templates.getFolderItem)
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#content_file_" + params.id).replaceWith(htmlXML);

        var urlIcn = ASC.Files.Utils.getFileTypeIcon(jq("#content_file_" + params.id + " .fileName a.name").attr("title"));
        jq("#file_icn_" + params.id).css("background", "url('" + urlIcn + "') no-repeat");

        if (!ASC.Files.Constants.ENABLE_VERSIONS)
            jq("div.file-info div.version").remove();

        if (params.show) {
            jq("div.newFile").removeClass("newFile").show();
        }

        ASC.Files.UI.addRowHandlers("content_file_" + params.id);

        if (params.showVer == true)
            ASC.Files.Folders.showVersions("combo_file_" + params.id)
    };

    var onGetFile = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var curTemplate = (ASC.Files.UI.viewFolder == "g" ? ASC.Files.TemplateManager.templates.getFolderItem_thumb : ASC.Files.TemplateManager.templates.getFolderItem);
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        if (jq("#files_mainContent div.fileRow").length == 0)
            jq("#files_mainContent").append(htmlXML);
        else
            jq(jq("#files_mainContent div.fileRow")[0]).before(htmlXML);

        jq("#content_info_count_files").html(1 + (parseInt(jq("#content_info_count_files").html()) || 0));


        if (!ASC.Files.Constants.ENABLE_VERSIONS)
            jq("div.file-info div.version").remove();

        if (params.show) {
            jq("div.newFile").removeClass("newFile").show();
        }

        ASC.Files.UI.resetSelectAll();
    };

    var onMoveFilesCheck = function(data, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            jq(params.list.entry).each(function() {
                ASC.Files.UI.blockItem("content_" + this);
            });
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }
        data = eval(data);

        if (data != null && data.length > 0) {
            ASC.Files.UI.showOverwriteMessage(params.list, params.destID, params.destTitle, params.isCopyOperation, data);
        }
        else {
            serviceManager.moveItems(ASC.Files.TemplateManager.events.MoveItems,
                                        {
                                            destID: params.destID,
                                            overwrite: false,
                                            isCopyOperation: params.isCopyOperation
                                        },
                                     { stringList: params.list });
            ASC.Files.Folders.isCopyTo = false;
        }
    };

    var onMoveItemsFinish = function(list, isMoveOperation, countProcessed) {
        var destID = list[0].replace("folder_", "");
        list = list.slice(1);
        for (var i = 0; i < list.length; i++) {
            ASC.Files.UI.blockItem('content_' + list[i]);
        }

        jq("#content_folder_" + destID).removeClass("row-to");

        var foldersCount = 0, filesCount = 0;

        if (list.length == 1)
            var title = jq("#content_" + list[0] + " div.name a").attr("title");

        for (var i = 0; i < list.length; i++) {
            if (list[i].search("folder_") == 0) {
                foldersCount += 1 + (parseInt(jq("#content_folder_countFolders_" + list[i].replace("folder_", "")).html()) || 0);
                filesCount += parseInt(jq("#content_folder_countFiles_" + list[i].replace("folder_", "")).html()) || 0;
            }
            else {
                if (list[i].search("file_") == 0)
                    filesCount++;
            }
            if (isMoveOperation && ASC.Files.Folders.currentFolderId != destID)
                jq("#content_" + list[i]).remove();
        }

        if (foldersCount > 0) {
            jq("#content_folder_countFolders_" + destID).html((parseInt(jq("#content_folder_countFolders_" + destID).html()) || 0) + foldersCount);

            jq("#tree_node_" + destID).addClass('jstree-closed').removeClass('jstree-open')
            												.children('ul').remove();
            jq("#stree_node_" + destID).addClass('jstree-closed').removeClass('jstree-open')
            												.children('ul').remove();
            if (isMoveOperation && destID != ASC.Files.Folders.currentFolderId) {
                jq("#tree_node_" + ASC.Files.Folders.currentFolderId).addClass('jstree-closed').removeClass('jstree-open')
															.children('ul').remove();
                jq("#stree_node_" + ASC.Files.Folders.currentFolderId).addClass('jstree-closed').removeClass('jstree-open')
															.children('ul').remove();
            }
        }

        if (filesCount > 0) {
            jq("#content_folder_countFiles_" + destID).html((parseInt(jq("#content_folder_countFiles_" + destID).html()) || 0) + filesCount);
        }

        if (list.length > 0) {
            ASC.Files.Folders.getFolderInfo();
        }

        if (list.length > 0 && ASC.Files.Folders.currentFolderId != destID) {
            ASC.Files.UI.paintRows();

            var countAppend = ASC.Files.Constants.COUNT_ON_PAGE - jq("#files_mainContent input[type='checkbox']").length;
            if (countAppend > 0) {
                if (ASC.Files.UI.currentPage < ASC.Files.UI.amountPage)
                    ASC.Files.Folders.getFolderItems(true, countAppend);
                else {
                    if (countAppend >= ASC.Files.Constants.COUNT_ON_PAGE) {
                        ASC.Files.UI.displayEmptyScreen();
                    }
                }
            }
        }

        if (isMoveOperation) {
            if (list.length == 1 && title != undefined) {
                ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoMoveItem, title));
            } else {
                ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoMoveGroup, countProcessed));
            }

            if (ASC.Files.Favorites)
                ASC.Files.Favorites.showFavorite();
        }
        else {
            if (list.length == 1 && title != undefined) {
                ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoCopyItem, title));
            } else {
                ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoCopyGroup, countProcessed));
            }
        }
    };

    var onDeleteItemFinish = function(list, totalCount) {
        var fromRootID = list[0].replace("folder_", "");
        list = list.slice(1);
        for (var i = 0; i < list.length; i++)
            ASC.Files.UI.blockItem("content_" + list[i]);

        var foldersCount = 0;
        var countSubFolder = 0, countSubFile = 0;

        if (list.length == 1)
            var title = jq("#content_" + list[0] + " div.name a").attr("title");

        for (var i = 0; i < list.length; i++) {
            if (list[i].search("folder_") == 0) {
                foldersCount++;
                countSubFolder += 1 + (parseInt(jq("#content_folder_countFolders_" + list[i].replace("folder_", "")).html()) || 0);
                countSubFile += parseInt(jq("#content_folder_countFiles_" + list[i].replace("folder_", "")).html()) || 0;
            }
            else
                countSubFile++;

            if (ASC.Files.Folders.pathParts[0].Key === fromRootID)
                jq("#content_" + list[i]).remove();
        }

        if (jq("#files_favoritesBody").html() != null && jq("#files_favoritesBody").html().length == 0) {
            jq("#files_favoritesBody").hide();
            jq("#files_favoritesEmpty").show();
        }

        if (foldersCount > 0) {
            jq("#tree_node_" + ASC.Files.Folders.currentFolderId).addClass('jstree-closed').removeClass('jstree-open')
															.children('ul').remove();
            jq("#stree_node_" + ASC.Files.Folders.currentFolderId).addClass('jstree-closed').removeClass('jstree-open')
															.children('ul').remove();
        }

        if (list.length > 0 && ASC.Files.Folders.pathParts[0].Key === fromRootID) {
            jq("#content_info_count_folders").html((parseInt(jq("#content_info_count_folders").html()) || 0) - countSubFolder);
            jq("#content_info_count_files").html((parseInt(jq("#content_info_count_files").html()) || 0) - countSubFile);

            ASC.Files.UI.paintRows();

            var countAppend = ASC.Files.Constants.COUNT_ON_PAGE - jq("#files_mainContent input[type='checkbox']").length;
            if (countAppend > 0) {
                if (ASC.Files.UI.currentPage < ASC.Files.UI.amountPage)
                    ASC.Files.Folders.getFolderItems(true, countAppend);
                else {
                    if (countAppend >= ASC.Files.Constants.COUNT_ON_PAGE) {
                        ASC.Files.UI.displayEmptyScreen();
                    }
                }
            }
        }

        if (list.length == 1 && title != undefined) {
            if (foldersCount > 0)
                ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoRemoveFolder, title));
            else
                ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoRemoveFile, title));
        } else {
            ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoRemoveGroup, list.length, totalCount));
        }

        if (ASC.Files.Favorites)
            ASC.Files.Favorites.showFavorite();
    };

    var onCheckEditFile = function(jsonData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        jsonData = jsonData.trim();
        if (jsonData == "") {
            return;
        }
        jq("#file_editing_icn_" + params.fileID).show();
        jq("#file_IsEditing_" + params.fileID).val('true');

        serviceManager.openFile(params.fileID, params.title, jsonData);
    };

    var onGetTasksStatuses = function(data, params, errorMessage, commentMessage) {
        if (typeof data !== "object" && typeof errorMessage != "undefined" || data == null) {
            ASC.Files.Folders.cancelTasksStatuses();
            if (ASC.Files.Import) ASC.Files.Import.cancelImportData("");
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return;
        }

        if (data.length == 0) {
            ASC.Files.Folders.cancelTasksStatuses();
            if (ASC.Files.Import) ASC.Files.Import.cancelImportData("");
            return;
        }

        var finishImport = true;
        var progress = 0;
        var operationType = "";
        var operationTypes = [ASC.Files.Resources.TasksOperationMove,
                            ASC.Files.Resources.TasksOperationCopy,
                            ASC.Files.Resources.TasksOperationDelete,
                            ASC.Files.Resources.TasksOperationBulkdownload];
        var blockTypes = [ASC.Files.Resources.DescriptMove,
                            ASC.Files.Resources.DescriptCopy,
                            ASC.Files.Resources.DescriptRemove,
                            ASC.Files.Resources.DescriptBulkdownload];

        //import
        for (var i = 0; i < data.length && ASC.Files.Import; i++) {
            if (data[i].operation == 4) {
                finishImport = ASC.Files.Import.onGetImportStatus(data.splice(i, 1)[0]);
                break;
            }
        }

        if (data.length != 0) {
            //show
            if (jq("#files_tasks:visible").length == 0) {
                clearTimeout(timoutTasksStatuses);

                if (jq("#files_tasks").length == 0) {
                    jq("#files_progress_template").clone().attr("id", "files_tasks").appendTo("#files_bottom_loader");

                    jq("#files_tasks").prepend(jq.format('<div onclick="ASC.Files.Folders.terminateTasks();return false;" title="{0}" \
                                                            class="terminateTasks"></div>', ASC.Files.Resources.TitleCancel));
                }
                jq("#files_tasks .progress").css('width', '0%');
                jq("#files_tasks .percent").text('0%').css("color", "#1A6DB3");
                jq("#files_tasks .headerBaseMedium").html("");
                jq("#files_tasks").show();

                if (ASC.Controls.Constants.isMobileAgent) {
                    jq("#files_bottom_loader").css("bottom", "auto");
                    jq("#files_bottom_loader").css("top", jq(window).scrollTop() + jq(window).height() - jq("#files_bottom_loader").height() + "px");
                }
            }

            //type operation in progress
            if (data.length > 1) {
                operationType = jq.format(ASC.Files.Resources.TasksOperationMixed, data.length);
            }
            else {
                operationType = operationTypes[data[0].operation];
            }
            jq("#files_tasks .headerBaseMedium").html(operationType);
        }

        //in each process
        for (var i = 0; i < data.length; i++) {

            //block descript on each elemets
            var listSource = data[i].source.trim().split(' ');
            jq(listSource).each(function() {
                ASC.Files.UI.blockItem("content_" + this, true, blockTypes[data[i].operation]);
            });

            var curProgress = data[i].progress;
            progress += curProgress;

            //finish
            if (curProgress == 100) {
                if (data[i].result != null) {
                    var listResult = new Array();
                    listResult = data[i].result.trim().split(' ');

                    switch (data[i].operation) {
                        case 0:
                            //move
                            onMoveItemsFinish(listResult, true, data[i].processed);
                            break;

                        case 1:
                            //copy
                            onMoveItemsFinish(listResult, false, data[i].processed);
                            break;

                        case 2:
                            //delete
                            onDeleteItemFinish(listResult, listSource.length);
                            break;

                        case 3:
                            //download
                            if (listResult[0])
                                location.href = listResult[0];
                            ASC.Files.Folders.bulkStatuses = false;
                            break;
                    }
                }
                //unblock
                jq(listSource).each(function() {
                    ASC.Files.UI.blockItem("content_" + this);
                });

                //on error
                if (data[i].error != null) {
                    ASC.Files.UI.displayInfoPanel(data[i].error, true);
                }
            }
        }

        //progress %
        progress = (data.length == 0 ? 100 : progress / data.length);

        jq("#files_tasks .progress").css('width', progress + '%');
        jq("#files_tasks .percent").text(progress + '%').css("color", progress > 45 ? "white" : "#1A6DB3");

        //complate
        if (progress == 100) {
            clearTimeout(timoutTasksStatuses);
            timoutTasksStatuses = setTimeout("ASC.Files.Folders.cancelTasksStatuses()", 500);

            if (finishImport)
                return;
        }

        //next iteration
        ASC.Files.Folders.getTasksStatuses();
    };

    return {
        init: init,
        onError: onError,
        onValidationAnchor: onValidationAnchor,
        onSelectAnchor: onSelectAnchor,
        onPreviewAnchor: onPreviewAnchor,

        onGetFolderItems: onGetFolderItems,
        onGetFolderInfo: onGetFolderInfo,
        onGetPathParts: onGetPathParts,
        onGetFile: onGetFile,
        onCreateNewFile: onCreateNewFile,
        onCreateFolder: onCreateFolder,
        onRenameFolder: onRenameFolder,
        onRenameFile: onRenameFile,
        onSetCurrentVersion: onSetCurrentVersion,
        onGetFileHistory: onGetFileHistory,
        onReplaceVersion: onReplaceVersion,
        onCheckEditFile: onCheckEditFile,

        onMoveFilesCheck: onMoveFilesCheck,
        onGetTasksStatuses: onGetTasksStatuses
    };

})(jQuery);

(function($) {
    ASC.Files.EventHandler.init();

    $(function() {

    });
})(jQuery);