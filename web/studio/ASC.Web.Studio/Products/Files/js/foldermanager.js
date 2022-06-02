window.ASC.Files.Folders = (function($) {
    var tasksTimeout;
    var bulkStatuses = false;
    var currentFolderId = 0;
    var filesListReceived = false;
    var folderListReceived = false;

    var folderContainer = "";

    var eventAfter = null;
    var typeNewDoc = "";

    var moveToFolder = "";
    var curItemMoveTo = "";
    var multiMoveTo = false;
    var isCopyTo = false;

    var pathParts;

    var clip = null;

    /* Methods*/

    var getFolderItems = function(isAppend, countAppend) {
        serviceManager.getFolderItems(ASC.Files.TemplateManager.events.GetFolderItems,
            {
                id: ASC.Files.Folders.currentFolderId,
                from: (isAppend ? jq("div.fileRow[name!='addRow']").length : 0),
                count: countAppend || ASC.Files.Constants.COUNT_ON_PAGE,
                filter: parseInt(ASC.Files.UI.viewFilter),
                subject: ASC.Files.UI.viewSubject,
                append: isAppend === true,
                setThumbnailURLBool: (ASC.Files.UI.viewFolder == "g")
            }, { orderBy: getOrderByAnchor() });
    };

    var getOrderByDateAndTime = function(asc) {
        return getOrderBy("DateAndTime", asc);
    };

    var getOrderByAnchor = function() {
        var name = "DateAndTime";
        var asc = false;

        if (ASC.Files.UI.viewOrder != '00' && ASC.Files.UI.viewOrder.length == 2) {
            asc = (ASC.Files.UI.viewOrder.charAt(0) != '0');
            switch (ASC.Files.UI.viewOrder.charAt(1)) {
                case '1':
                    name = 'AZ';
                    break;
                case '2':
                    name = 'Size';
                    break;
                case '3':
                    name = 'DateAndTime';
                    break;
                case '4':
                    name = 'Author';
                    break;
                default:
                    name = 'DateAndTime';
            }
        }
        jq(".files_sort_asc, .files_sort_desc").removeClass("files_sort_asc files_sort_desc");

        jq("#files_sort_value").html(jq("#files_sort_" + name + " a").html())
                                .removeClass("files_sort_asc files_sort_desc")
                                .addClass("files_sort_" + (asc ? "asc" : "desc"));

        jq("#files_sort_" + name + " a").addClass("files_sort_" + (asc ? "asc" : "desc"));

        return getOrderBy(name, asc);
    };

    var getOrderBy = function(name, asc) {
        var data = {};
        data.is_asc = (asc == true);
        data.property = name;

        return data;
    };

    var clickOnFolder = function(id, isFavorite) {
        if (ASC.Files.Folders.folderContainer == "trash" && !isFavorite) {
            ASC.Files.Actions.onContextMenu("folder_" + id);
            return;
        }
        ASC.Files.UI.madeAnchor(id);
    };

    var clickOnFile = function(id, version, title, isFavorite) {
        if (ASC.Files.Folders.folderContainer == "trash" && !isFavorite) {
            ASC.Files.Actions.onContextMenu("file_" + id);
            return;
        }
        //jq("#content_file_" + id + " div.is_new").remove();

        if (title == undefined || title == null)
            title = jq("#content_file_" + id + " .fileName a.name").attr("title");

        if (version == undefined || version == null) {
            version = jq("#file_version_" + id).val() || "";
        }

        if (ASC.Files.Utils.FileCanBePreview(title)) {
            var url = ASC.Files.Utils.getPreviewUrl(title, id, version);
            if (ASC.Files.Utils.itIsDocument(title)) {
                window.open(url, "_blank");
            } else {
                ASC.Controls.AnchorController.move(url);
            }
            return;
        }

        window.open(ASC.Files.Utils.fileViewUrl(id, version), "_blank");
    };

    var download = function(id, version) {

        if (id != undefined) {
            id = id.replace("combo_", "");
        }
        else {
            var list = jq("#files_mainContent input:checked");
            if (list.length == 0) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.EmptyListSelectedForDownload, true);
                return;
            }
            if (list.length == 1) {
                id = list[0].id.replace("check_", "");
            }
        }

        if (id != undefined && id.indexOf("file") != -1) {
            id = id.replace("file_", "");
            if (version != undefined)
                var fileVersion = version
            else
                fileVersion = jq("#file_version_" + id).val();

            location.href = ASC.Files.Utils.fileDownloadUrl(id, fileVersion);
        }
        else {
            if (ASC.Files.Folders.bulkStatuses == true) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_SecondDownload, true);
                return;
            }

            var data = {};
            data.entry = new Array();

            if (id != undefined) {
                data.entry.push(id);
            }
            else {
                list.each(function() {
                    data.entry.push(this.id.replace("check_", ""));
                });
            }
            ASC.Files.Folders.bulkStatuses = true;
            serviceManager.download(ASC.Files.TemplateManager.events.Download, {}, { stringList: data });
        }
    };

    var createFolder = function() {
        if (ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessibleItem()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_SecurityException, true);
            ASC.Files.Folders.eventAfter = createFolder;
            ASC.Files.UI.defaultFolderSet();
            return;
        }

        jq(document).scrollTo(0);
        if (ASC.Controls.Constants.isMobileAgent) {
            ASC.Files.MainMenu.toggleMainMenu();
        }

        if (jq("#files_prompt_create_folder").length != 0) {
            return;
        }

        var emptyFolder = {
            folder:
            {
                id: 'NEW_FOLDER',
                title: ASC.Files.Resources.TitleNewFolder
            }
        };
        var stringData = serviceManager.jsonToXml(emptyFolder);

        var curTemplate = (ASC.Files.UI.viewFolder == "g" ? ASC.Files.TemplateManager.templates.getFolderItem_thumb : ASC.Files.TemplateManager.templates.getFolderItem);
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }

        try {
            var htmlXML = ASC.Controls.XSLTManager.translateFromString(stringData, xslData);
        } catch (err) { return undefined; };

        ASC.Files.UI.hideEmpty();

        var mainContent = jq("#files_mainContent");

        if (mainContent.children().length == 0)
            mainContent.append(htmlXML);
        else
            jq(jq(mainContent).children()[0]).before(htmlXML);

        ASC.Files.UI.paintRows();

        jq("div.newFolder").yellowFade();

        var obj = jq("#content_folder_NEW_FOLDER div.fileName a.name");
        var newContainer = document.createElement('input');
        newContainer.id = 'files_prompt_create_folder';
        newContainer.type = 'text';
        newContainer.style.display = 'none';
        document.body.appendChild(newContainer);

        newContainer = jq("#files_prompt_create_folder");
        newContainer.attr('maxlength', ASC.Files.Constants.MAX_NAME_LENGTH);
        newContainer.addClass("textEdit renameInput");
        newContainer.val(ASC.Files.Resources.TitleNewFolder);
        newContainer.insertAfter(obj);
        newContainer.show();
        obj.hide();
        newContainer.focus().select();

        var saveFolder = function() {
            var newName = ASC.Files.Utils.replaceSpecCharacter(jq("#files_prompt_create_folder").val().trim());

            jq("#content_folder_NEW_FOLDER div.fileName a.name").show();
            jq("#files_prompt_create_folder").remove();
            jq("#content_folder_NEW_FOLDER div.renameAction").remove();

            if (newName == "" || newName == null)
                newName = ASC.Files.Resources.TitleNewFolder;

            ASC.Files.UI.blockItem("content_folder_NEW_FOLDER", true, ASC.Files.Resources.DescriptCreate);
            serviceManager.createFolder(ASC.Files.TemplateManager.events.CreateFolder, { parentFolderID: ASC.Files.Folders.currentFolderId, title: newName });
        };

        jq("#content_folder_NEW_FOLDER").append("<div class='renameAction'><div class='apllyName' title=" + ASC.Files.Resources.TitleCreate +
                                                "></div><div class='cancelName' title=" + ASC.Files.Resources.TitleCancel + "></div></div>");
        jq("#content_folder_NEW_FOLDER div.apllyName").click(saveFolder);
        jq("#content_folder_NEW_FOLDER div.cancelName").click(function() {
            jq("#content_folder_NEW_FOLDER").remove();
            if (jq("#files_mainContent input[type='checkbox']").length == 0)
                ASC.Files.UI.displayEmptyScreen();
        });

        jq("#files_prompt_create_folder").keyup(function(event) {

            if (jq("#files_prompt_create_folder").length == 0)
                return;

            var code;
            if (!e) var e = event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;

            if (code == 27) {
                jq("#content_folder_NEW_FOLDER").remove();
                if (jq("#files_mainContent input[type='checkbox']").length == 0)
                    ASC.Files.UI.displayEmptyScreen();
            }
            else if (code == 13) {
                saveFolder();
            }
        });
    };

    var createNewDoc = function() {
        jq(document).scrollTo(0);

        if (ASC.Files.Folders.folderContainer == "trash"
            || !ASC.Files.UI.accessibleItem()) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_SecurityException, true);
            ASC.Files.Folders.eventAfter = createNewDoc;
            ASC.Files.UI.defaultFolderSet();
            return;
        }

        if (jq("#files_prompt_create_file").length != 0) {
            return;
        }

        var titleNewDoc;
        switch (ASC.Files.Folders.typeNewDoc) {
            case "text":
                titleNewDoc = ASC.Files.Resources.TitleNewFileText + ASC.Files.Utils.typeNewDoc.text;
                break;
            case "spreadsheet":
                titleNewDoc = ASC.Files.Resources.TitleNewFileSpreadsheet + ASC.Files.Utils.typeNewDoc.spreadsheet;
                break;
            case "presentation":
                titleNewDoc = ASC.Files.Resources.TitleNewFilePresentation + ASC.Files.Utils.typeNewDoc.presentation;
                break;
            default:
                return;
        }

        if (!ASC.Files.Utils.FileCanBeEdit(titleNewDoc)) {
            switch (ASC.Files.Plugin.GetOpenOfficeState()) {
                case ASC.Files.Plugin.OPEN_OFFICE_STATE_NOT_INSTALLED:
                case ASC.Files.Plugin.OPEN_OFFICE_STATE_INSTALLING:
                    jq("#plugin_informer").slideDown();
                    return;
            }
        }

        var emptyFile = {
            file:
            {
                id: "NEW_FILE",
                title: titleNewDoc
            }
        };

        var stringData = serviceManager.jsonToXml(emptyFile);

        var curTemplate = (ASC.Files.UI.viewFolder == "g" ? ASC.Files.TemplateManager.templates.getFolderItem_thumb : ASC.Files.TemplateManager.templates.getFolderItem);
        var xslData = ASC.Files.TemplateManager.getTemplate(curTemplate);
        if (typeof xslData === "undefined") { return undefined; }

        try {
            var htmlXML = ASC.Controls.XSLTManager.translateFromString(stringData, xslData);
        } catch (err) { return undefined; };

        ASC.Files.UI.hideEmpty();

        var mainContent = jq("#files_mainContent");

        if (mainContent.children().length == 0)
            mainContent.append(htmlXML);
        else
            jq(jq(mainContent).children()[0]).before(htmlXML);

        jq("div.newFile").show().yellowFade();

        ASC.Files.UI.paintRows();

        var obj = jq("#content_file_NEW_FILE div.fileName a.name");
        jq("#content_file_NEW_FILE").addClass('rowRename');

        var lenExt = ASC.Files.Utils.getFileExts(titleNewDoc).length;
        titleNewDoc = titleNewDoc.substring(0, titleNewDoc.length - lenExt)

        var newContainer = document.createElement('input');
        newContainer.id = 'files_prompt_create_file';
        newContainer.type = 'text';
        newContainer.style.display = 'none';
        document.body.appendChild(newContainer);
        newContainer = jq("#files_prompt_create_file");
        newContainer.attr('maxlength', ASC.Files.Constants.MAX_NAME_LENGTH - lenExt);
        newContainer.addClass("textEdit renameInput");
        newContainer.val(titleNewDoc);
        newContainer.insertAfter(obj);
        newContainer.show().focus().select();

        var saveFile = function() {
            if (jq("#files_prompt_create_file").length === 0)
                return;
            var newName = ASC.Files.Utils.replaceSpecCharacter(jq("#files_prompt_create_file").val().trim());
            var oldName = jq("#content_file_NEW_FILE div.fileName a.name").attr("title");

            if (newName == "" || newName == null)
                newName = oldName;
            else {
                var lenExt = ASC.Files.Utils.getFileExts(oldName).length;
                newName += oldName.substring(oldName.length - lenExt);
            }

            jq("#content_file_NEW_FILE").removeClass('rowRename');
            jq("#files_prompt_create_file").remove();
            jq("#content_file_NEW_FILE div.renameAction").remove();

            var params = {};
            params.folderID = ASC.Files.Folders.currentFolderId;
            params.fileTitle = newName;

            ASC.Files.UI.blockItem("content_file_NEW_FILE", true, ASC.Files.Resources.DescriptCreate);
            serviceManager.createNewFile(ASC.Files.TemplateManager.events.CreateNewFile, params);
        };

        jq("#content_file_NEW_FILE").append("<div class='renameAction'><div class='apllyName' title=" + ASC.Files.Resources.TitleCreate +
                                            "></div><div class='cancelName' title=" + ASC.Files.Resources.TitleCancel + "></div></div>");
        jq("#content_file_NEW_FILE div.apllyName").click(saveFile);
        jq("#content_file_NEW_FILE div.cancelName").click(function() {
            jq("#content_file_NEW_FILE").remove();
            if (jq("#files_mainContent input[type='checkbox']").length == 0)
                ASC.Files.UI.displayEmptyScreen();
        });

        jq("#files_prompt_create_file").keyup(function(event) {
            if (jq("#files_prompt_create_file").length == 0)
                return;

            var code;
            if (!e) var e = event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;

            if (code == 27) {
                jq("#content_file_NEW_FILE").remove();
                if (jq("#files_mainContent input[type='checkbox']").length == 0)
                    ASC.Files.UI.displayEmptyScreen();
            }
            else if (code == 13) {
                saveFile();
            }
        });
    };

    var showVersions = function(obj_id) {
        if (ASC.Files.Folders.folderContainer == "trash") {
            return;
        }
        var id = obj_id.replace("combo_file_", "");

        var empty = jq("#content_file_" + id + " #content_versions");
        if (empty.length != 0) {
            closeVersions();
            return;
        }

        ASC.Files.UI.blockItem("content_file_" + id, true, ASC.Files.Resources.DescriptLoadVersion);
        serviceManager.getFileHistory(ASC.Files.TemplateManager.events.GetFileHistory, { id: id }, {});
    };

    var makeCurrentVersion = function(id, version) {
        jq("div.version_restore,div.versions_btn").css('visibility', 'hidden');
        ASC.Files.UI.blockItem("content_file_" + id, true, ASC.Files.Resources.DescriptSetVersion);
        serviceManager.setCurrentVersion(ASC.Files.TemplateManager.events.SetCurrentVersion, { id: id, version: version })
    };

    var closeVersions = function() {
        jq("#content_versions").remove();
        jq(".row-over").removeClass("row-over");
    };

    var replaceVersion = function(id, showVer, show) {
        serviceManager.getFile(ASC.Files.TemplateManager.events.ReplaceVersion,
            {
                id: id,
                showVer: showVer,
                show: show,
                setThumbnailURLBool: (ASC.Files.UI.viewFolder == "g")
            });
    };

    var getFolderInfo = function() {
        serviceManager.getFolderInfo(ASC.Files.TemplateManager.events.GetFolderInfo, { id: ASC.Files.Folders.currentFolderId });
    };

    var curItemFolderMoveTo = function(idTo, destTitle) {
        var pathDest = new Array();
        pathDest.push((ASC.Files.Folders.multiMoveTo ? 'check_folder_' : 'combo_folder_') + idTo);
        jq("#stree_node_" + idTo).parents('li').each(function() {
            pathDest.push(this.id.replace('stree_node_', ASC.Files.Folders.multiMoveTo ? 'check_folder_' : 'combo_folder_'));
        });

        var data = {};
        data.entry = new Array();

        if (ASC.Files.Folders.multiMoveTo == true) {
            jq("#files_mainContent div.fileRow:not(.checkloading):not(.newFolder):not(.newFile) input:checked").each(function() {

                var objId = this.id.replace("content_", "");

                if (jq("#file_editing_icn_" + this.id.replace("check_file_", "") + ":visible").length == 0
                    && (ASC.Files.Folders.isCopyTo == true
                        || ASC.Files.UI.accessAdmin(objId))
                     ) {
                    if (jq.inArray(this.id, pathDest) != -1) {
                        ASC.Files.UI.displayInfoPanel(((ASC.Files.Folders.isCopyTo == true) ? ASC.Files.Resources.InfoFolderCopyError : ASC.Files.Resources.InfoFolderMoveError), true);
                    }
                    else {
                        ASC.Files.UI.blockItem(this.id.replace("check", "content"),
                                               true,
                                               (ASC.Files.Folders.isCopyTo == true) ? ASC.Files.Resources.DescriptCopy : ASC.Files.Resources.DescriptMove);
                        data.entry.push(this.id.replace("check_", ""));
                    }
                }
            });
        }
        else {
            if (ASC.Files.Folders.curItemMoveTo != null && ASC.Files.Folders.curItemMoveTo != "") {
                if (jq.inArray(ASC.Files.Folders.curItemMoveTo, pathDest) != -1) {
                    ASC.Files.UI.displayInfoPanel(((ASC.Files.Folders.isCopyTo == true) ? ASC.Files.Resources.InfoFolderCopyError : ASC.Files.Resources.InfoFolderMoveError), true);
                }
                else {
                    ASC.Files.UI.blockItem(ASC.Files.Folders.curItemMoveTo.replace("combo", "content"),
                                           true,
                                           (ASC.Files.Folders.isCopyTo == true) ? ASC.Files.Resources.DescriptCopy : ASC.Files.Resources.DescriptMove);
                    data.entry.push(ASC.Files.Folders.curItemMoveTo.replace("combo_", ""));
                }
            }
        }

        ASC.Files.Actions.hideAllActionPanels();

        if (data.entry && data.entry.length != 0) {
            serviceManager.moveFilesCheck(ASC.Files.TemplateManager.events.MoveFilesCheck,
                                        {
                                            destTitle: destTitle,
                                            destID: idTo,
                                            list: data,
                                            isCopyOperation: (ASC.Files.Folders.isCopyTo == true)
                                        },
                                        { stringList: data });
        }
        ASC.Files.Folders.isCopyTo = false;

    };

    var showMore = function() {
        jq("#files_pageNavigatorHolder a").unbind("click")
										.text(ASC.Files.Resources.ButtonShowMoreLoad);

        ASC.Files.Folders.getFolderItems(true);
    };

    var emptyTrash = function() {
        if (ASC.Files.Folders.folderContainer != "trash") {
            return;
        }

        ASC.Files.Actions.hideAllActionPanels();

        ASC.Files.UI.checkSelectAll(true);

        jq("#confirmRemoveText").html("<b>" + ASC.Files.Resources.ConfirmEmptyTrash + "</b>");
        jq("#confirmRemoveList").hide();
        jq("#confirmRemoveTextDescription").show();

        jq("#removeConfirmBtn").unbind("click").click(function() {
            PopupKeyUpActionProvider.CloseDialog();

            serviceManager.emptyTrash(ASC.Files.TemplateManager.events.EmptyTrash, {});
        });

        ASC.Files.Common.blockUI(jq("#files_confirm_remove"), 420, 0, -150);
        PopupKeyUpActionProvider.EnterAction = 'jq("#removeConfirmBtn").click();';
    };

    var deleteItem = function(id) {
        if (!ASC.Files.UI.accessibleItem())
            return;

        ASC.Files.Actions.hideAllActionPanels();

        var caption = ""
        var list = new Array();

        if (id == undefined) {
            jq("#files_mainContent div.fileRow:not(.checkloading):not(.newFolder):not(.newFile) input:checked").each(function() {
                var idEl = jq(this).attr("id").replace("check_", "");

                if (jq("#file_editing_icn_" + idEl.replace("file_", "") + ":visible").length == 0
                    && ASC.Files.UI.accessAdmin(idEl)) {
                    list.push(idEl);
                }
            });

            caption = ASC.Files.Resources.ConfirmRemoveList;
        }
        else {
            if (ASC.Files.UI.accessAdmin(id)) {
                list.push(id);
            }
            else {
                return;
            }

            if (id.search("folder_") == 0) {
                caption = ASC.Files.Resources.ConfirmRemoveFolder;
            }
            else {
                if (jq("#file_editing_icn_" + id + ":visible").length != 0)
                    return;

                caption = ASC.Files.Resources.ConfirmRemoveFile;
            }
        }

        if (list.length == 0)
            return;

        var textFolder = "";
        var textFile = "";
        for (var i = 0; i < list.length; i++) {
            var name = jq("#content_" + list[i] + " div.fileName a.name").attr("title");
            if (list[i].search("folder_") == 0)
                textFolder += jq.format("<div title='{0}'>{0}</div>", name);
            else
                textFile += jq.format("<div title='{0}'>{0}</div>", name);
        }
        jq("#confirmRemoveText").html("<b>" + caption + "</b>");
        jq("#confirmRemoveList dd.confirmRemoveFiles").html(textFile);
        jq("#confirmRemoveList dd.confirmRemoveFolders").html(textFolder);

        jq("#confirmRemoveList, .confirmRemoveFolders,.confirmRemoveFiles").show();
        if (textFolder == "")
            jq(".confirmRemoveFolders").hide();
        if (textFile == "")
            jq(".confirmRemoveFiles").hide();


        if (ASC.Files.Folders.folderContainer == "trash" || ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT != 0)
            jq("#confirmRemoveTextDescription").show();
        else
            jq("#confirmRemoveTextDescription").hide();

        jq("#removeConfirmBtn").unbind("click").click(function() {
            PopupKeyUpActionProvider.CloseDialog();

            var data = {};
            data.entry = new Array();
            for (var i = 0; i < list.length; i++) {
                ASC.Files.UI.blockItem('content_' + list[i], true, ASC.Files.Resources.DescriptRemove);
                data.entry.push(list[i]);
            }
            serviceManager.deleteItem(ASC.Files.TemplateManager.events.DeleteItem, { list: list }, { stringList: data });
        });

        ASC.Files.Common.blockUI(jq("#files_confirm_remove"), 420, 0, -150);

        PopupKeyUpActionProvider.EnterAction = 'jq("#removeConfirmBtn").click();';
    };

    var rename = function(fullID) {
        var id;
        var name;
        var ext;
        var obj;
        var lenExt = 0;

        if (jq("#files_prompt_rename").length != 0)
            jq("div.fileRow.rowRename div.cancelName").click();

        if (fullID.indexOf('combo_file_') != -1) {
            id = fullID.replace("combo_file_", "");
            obj = jq("#content_file_" + id + " div.fileName a.name");
            jq("#content_file_" + id).addClass('rowRename');
            ASC.Files.UI.selectRow(jq("#content_file_" + id), false);
            name = obj.attr("title");
            lenExt = ASC.Files.Utils.getFileExts(name).length;
            name = name.substring(0, name.length - lenExt);
        } else {
            id = fullID.replace("combo_folder_", "");
            obj = jq("#content_folder_" + id + " div.fileName a.name");
            jq("#content_folder_" + id).addClass('rowRename');
            ASC.Files.UI.selectRow(jq("#content_folder_" + id), false);
            name = obj.attr("title");
        }

        var newContainer = document.createElement('input');
        newContainer.id = 'files_prompt_rename';
        newContainer.type = 'text';
        newContainer.style.display = 'none';
        document.body.appendChild(newContainer);

        newContainer = jq("#files_prompt_rename");
        newContainer.attr('maxlength', ASC.Files.Constants.MAX_NAME_LENGTH - lenExt);
        newContainer.addClass("textEdit renameInput");
        newContainer.val(name);
        newContainer.insertAfter(obj);
        newContainer.show().focus().select();

        var saveRename = function() {
            var newName = ASC.Files.Utils.replaceSpecCharacter(jq("#files_prompt_rename").val().trim());
            if (newName == "" || newName == null)
                return;

            var curId = jq("#files_prompt_rename").parents('div.fileRow').attr("id");
            var oldName = jq("#" + curId + " div.fileName a.name").attr("title");

            if (curId.indexOf("content_file_") != -1) {
                var lenExt = ASC.Files.Utils.getFileExts(oldName).length;
                newName += oldName.substring(oldName.length - lenExt);
            }

            jq("#" + curId).removeClass('rowRename');
            jq("#files_prompt_rename").remove();
            jq("#" + curId + " .renameAction").remove();

            if (newName != oldName) {
                ASC.Files.UI.blockItem(curId, true, ASC.Files.Resources.DescriptRename);

                if (curId.indexOf("content_file_") != -1) {
                    serviceManager.renameFile(ASC.Files.TemplateManager.events.FileRename, { id: curId.replace("content_file_", ""), name: oldName, newname: newName });
                }
                else {
                    serviceManager.renameFolder(ASC.Files.TemplateManager.events.FolderRename, { id: curId.replace('content_folder_', ""), name: oldName, newname: newName });
                }
            }
        };

        var rowId = fullID.replace('combo_', 'content_');
        jq("#" + rowId).append("<div class='renameAction'><div class='apllyName' title=" + ASC.Files.Resources.TitleRename +
                                                "></div><div class='cancelName' title=" + ASC.Files.Resources.TitleCancel + "></div></div>");
        jq("#" + rowId + " div.apllyName").click(saveRename);
        jq("#" + rowId + " div.cancelName").click(function() {
            var curId = jq("#files_prompt_rename").parents('div.fileRow').attr("id");
            jq("#files_prompt_rename").remove();
            jq("#" + curId).removeClass('rowRename');
            jq("#" + curId + " .renameAction").remove();
        });

        jq("#" + rowId).removeClass('row-selected');

        jq("#files_prompt_rename").keyup(function(event) {
            if (jq("#files_prompt_rename").length === 0)
                return;

            var code;
            if (!e) var e = event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;

            if ((code == 27 || code == 13) && jq("#files_prompt_rename").length != 0) {
                if (code == 27) {
                    var curId = jq("#files_prompt_rename").parents('div.fileRow').attr("id");
                    jq("#files_prompt_rename").remove();
                    jq("#" + curId).removeClass('rowRename');
                    jq("#" + curId + " .renameAction").remove();
                }
                else if (code == 13) {
                    saveRename();
                }
            }
        });
    };

    var getFile = function(id, show) {
        serviceManager.getFile(ASC.Files.TemplateManager.events.GetFile,
            {
                id: id,
                showVer: false,
                show: show,
                setThumbnailURLBool: (ASC.Files.UI.viewFolder == "g")
            });
    };

    var cancelTasksStatuses = function() {
        ASC.Files.Folders.bulkStatuses = false;

        jq("#files_tasks .progress").css('width', '0%');
        jq("#files_tasks .percent").text('0%').css("color", "#1A6DB3");
        jq("#files_tasks .headerBaseMedium").html("");
        jq("#files_tasks").hide();
    };

    var terminateTasks = function() {
        clearTimeout(ASC.Files.Folders.tasksTimeout);

        serviceManager.terminateTasks(ASC.Files.TemplateManager.events.TerminateTasks, {});
    };

    var getTasksStatuses = function() {
        clearTimeout(ASC.Files.Folders.tasksTimeout);

        ASC.Files.Folders.tasksTimeout = setTimeout(
            function() {
                serviceManager.getTasksStatuses(ASC.Files.TemplateManager.events.GetTasksStatuses, {});
            }, 2000);
    };

    var getLink = function(title, fileId) {
        var vers = jq("#file_version_" + fileId).val() || "";
        var url = ASC.Files.Utils.fileViewUrl(fileId, vers);
        if (ASC.Files.Utils.itIsDocument(title))
            url = ASC.Files.Utils.getPreviewUrl(title, fileId);
        url = ASC.Files.Common.getSitePath() + url;

        ASC.Files.Common.blockUI(jq("#files_get_link"), 0, 200);
        jq("#files_getlink_link").val(encodeURI(url)).select();

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("#files_getlink_actions").remove();
        } else {
            ASC.Files.Folders.clip = new ZeroClipboard.Client();
            ASC.Files.Folders.clip.setText(url);
            ASC.Files.Folders.clip.glue('files_getlink_actions', 'files_getlink_actions',
                {
                    zIndex: 670,
                    left: jq("#files_getlink_actions").offset().left - jq("#files_get_link").offset().left + "px",
                    top: jq("#files_getlink_actions").offset().top - jq("#files_get_link").offset().top + "px"
                });
            ASC.Files.Folders.clip.addEventListener('onComplete', function(client, text) { jq("#files_getlink_link").yellowFade(); });

            PopupKeyUpActionProvider.CloseDialogAction = "ASC.Files.Folders.clip.destroy();";
        }
    };

    return {

        eventAfter: eventAfter,

        currentFolderId: currentFolderId,

        moveToFolder: moveToFolder,
        curItemMoveTo: curItemMoveTo,
        multiMoveTo: multiMoveTo,
        isCopyTo: isCopyTo,

        createFolder: createFolder,
        getFolderInfo: getFolderInfo,
        getFile: getFile,
        replaceVersion: replaceVersion,

        showVersions: showVersions,
        makeCurrentVersion: makeCurrentVersion,
        closeVersions: closeVersions,

        getOrderByDateAndTime: getOrderByDateAndTime,
        getOrderByAnchor: getOrderByAnchor,

        curItemFolderMoveTo: curItemFolderMoveTo,

        rename: rename,
        deleteItem: deleteItem,
        emptyTrash: emptyTrash,

        getFolderItems: getFolderItems,

        folderContainer: folderContainer,

        clip: clip,
        getLink: getLink,

        clickOnFolder: clickOnFolder,
        clickOnFile: clickOnFile,

        showMore: showMore,

        pathParts: pathParts,

        createNewDoc: createNewDoc,
        typeNewDoc: typeNewDoc,

        download: download,
        getTasksStatuses: getTasksStatuses,
        cancelTasksStatuses: cancelTasksStatuses,
        terminateTasks: terminateTasks,
        tasksTimeout: tasksTimeout,
        bulkStatuses: bulkStatuses
    };
})(jQuery);

(function($) {
    $(function() {

        //for skin
        //jq("body").addClass("TMDocuments");

        jq(".files_navigation li:last a").addClass("files_navigation_last");

        ASC.Files.Folders.getTasksStatuses();

        jq("[id^='files_navigation_']").click(function() {
            var container = this.id.replace('files_navigation_', "");
            ASC.Files.UI.navigationSet(container);
            return false;
        });

        jq("#tree_selector_" + ASC.Files.Constants.FOLDER_ID_MY_FILES
        + ",#tree_selector_" + ASC.Files.Constants.FOLDER_ID_COMMON_FILES
        + ",#tree_selector_" + ASC.Files.Constants.FOLDER_ID_SHARE
        + ",#tree_selector_" + ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT).click(function() {
            var container = this.id.replace('tree_selector_', "");
            ASC.Files.UI.navigationSet(container);
            return false;
        });

        jq("#files_createfolder_btn").click(function() {
            ASC.Files.Folders.createFolder();
        });

        jq("#files_cancelUpload").click(function() {
            PopupKeyUpActionProvider.CloseDialog();
        });

        jq("#files_deleteButton").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.deleteItem();
        });

        jq("#files_emptyTrashButton").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.emptyTrash();
        });

        jq("#files_download_btn, #files_downloadButton").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.download();
        });

        jq("#files_gallery_btn").click(function() {
            ASC.Files.UI.switchViewFolder();
        });

        jq("#files_list_btn").click(function() {
            ASC.Files.UI.switchViewFolder();
        });

        jq("#files_sortPanel [id^='files_sort_']").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.UI.setOrderValue(this);
        });

        jq("#[id^='files_filter_value_'] a").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.UI.setFilterType(this);
            return false;
        });

        jq("#files_clearFilter_btn, #files_clearFilterCont_btn").unbind("click").click(function() {
            jq("#files_filter_show_0 a").click();
            return false;
        });

        jq("#files_selectAll_check").click(function() {
            ASC.Files.UI.checkSelectAll(jq("#files_selectAll_check").attr('checked'));
        });

        jq("#files_create_text, #files_create_spreadsheet, #files_create_presentation").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.typeNewDoc = this.id.replace("files_create_", "");
            ASC.Files.Folders.createNewDoc();
        });

        jq("#files_getlink_link").bind("mousedown", function() {
            jq(this).select();
            return false;
        });

        if (!ASC.Files.Constants.ENABLE_VERSIONS) {
            jq("#files_versions_files, #files_uploads_files").remove();
        }

        if (ASC.Files.Share == undefined) {
            jq("#files_share_btn,\
                #files_shareaccess_folders, #files_shareaccess_files,\
                #files_unsubscribe_files, #files_unsubscribe_folders").remove();
        }

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("#confirmRemoveList,\
                #files_overwrite_list,\
                #mainContentHeader_panel,\
                #files_filter_usersPanel,\
                #files_filter_departmensPanel").css({ "max-height": "none" });
        }

    });
})(jQuery);