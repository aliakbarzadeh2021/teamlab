window.ASC.Files.UI = (function($) {
    var isInit = false;

    var timeIntervalInfo = "";

    var keepLoading = false,
        timerLoading = null;

    //    var studioContentClass = 'studioRightContent';

    var viewFilter = '0';
    var viewSubject = "";
    var viewOrder = '00';
    var viewFolder = 'l';

    var currentPage = 0;
    var amountPage = 0;

    var countEntityInFolder = 0;

    var currentCombo = "";
    var currentRowOver = null;

    var startX = 0;
    var startY = 0;
    var prevX = 0;
    var prevY = 0;

    var mouseBtn = false;

    var init = function() {
        if (isInit === false) {
            isInit = true;
        }

        ASC.Files.UI.mouseBtn = false;
    };

    var madeAnchor = function(folderId, safemode) {
        jq(".mainContentHeader_item_actions").removeClass("display");
        ASC.Files.UI.resetSelectAll(false);

        var str;

        if (folderId == undefined || folderId == null || folderId === "" || folderId == 0)
            folderId = ASC.Files.Folders.currentFolderId;

        str = folderId;

        if (ASC.Files.UI.viewOrder == undefined || ASC.Files.UI.viewOrder == "") {
            ASC.Files.UI.viewOrder = '00';
        }

        if (ASC.Files.UI.viewOrder != '00' || ASC.Files.UI.viewFolder == "g") {
            str += '/' + ASC.Files.UI.viewOrder;
            //str += "/" + ASC.Files.UI.viewFolder;
        }
        if (safemode === true) {
            ASC.Controls.AnchorController.safemove(str);
        } else {
            ASC.Controls.AnchorController.move(str);
        }
    };

    var setOrderValue = function(obj) {
        var name = jq(obj).attr("id").replace('files_sort_', "");
        ASC.Files.UI.viewOrder = (ASC.Files.UI.viewOrder.charAt(0) == 0 ? '1' : '0');
        switch (name) {
            case "AZ":
                ASC.Files.UI.viewOrder += '1';
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoOrderByTitle);
                break;
            case "Size":
                ASC.Files.UI.viewOrder += '2';
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoOrderBySize);
                break;
            case "DateAndTime":
                ASC.Files.UI.viewOrder += '3';
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoOrderByModified);
                break;
            case "Author":
                ASC.Files.UI.viewOrder += '4';
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoOrderByAuthor);
                break;
            default:
                ASC.Files.UI.viewOrder = '00';
        }
        madeAnchor();
    };

    var setFilterType = function(obj) {
        jq("[id^='files_filter_value_'] a.select").removeClass('select');
        jq(obj).addClass('select');
        jq("#files_filter_value span").html(jq(obj).html());

        var value = jq(obj).parent().attr("id").replace('files_filter_value_', "").split('_');
        setFilterValue(value[0], value[1]);
    };

    var setFilterValue = function(newFilter, newSubject) {
        ASC.Files.UI.viewFilter = parseInt(newFilter);
        ASC.Files.UI.viewSubject = newSubject;
        madeAnchor();
    };

    var defaultFolderSet = function() {
        if (ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT == 0) {
            if (ASC.Files.Constants.FOLDER_ID_MY_FILES == 0) {
                ASC.Files.UI.navigationSet(ASC.Files.Constants.FOLDER_ID_COMMON_FILES);
            }
            else {
                if (ASC.Files.Folders.currentFolderId != ASC.Files.Constants.FOLDER_ID_MY_FILES) {
                    ASC.Files.UI.navigationSet(ASC.Files.Constants.FOLDER_ID_MY_FILES);
                }
            }
        }
        else {
            if (ASC.Files.Folders.currentFolderId != ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT) {
                ASC.Files.UI.navigationSet(ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT);
            }
        }
    };

    var navigationSet = function(param) {
        ASC.Files.UI.viewFilter = 0;
        ASC.Files.UI.amountPage = 0;

        ASC.Files.UI.viewSubject = "";
        jq("#mainContentHeader_panel a").removeClass('select');
        jq(".mainContentHeader_item_actions").removeClass("display");
        jq("#files_filter_category span").html(jq("#files_filter_show_0 a").html());
        jq("#files_filter_value").hide();

        ASC.Files.UI.viewFolder = "l";
        ASC.Files.UI.viewOrder = '00';

        if (param == parseInt(param)) {
            ASC.Files.Folders.currentFolderId = parseInt(param);
            ASC.Files.Folders.folderContainer = "";
            madeAnchor();
        } else {
            ASC.Files.Folders.currentFolderId = -1;
            ASC.Files.Folders.folderContainer = param;
            madeAnchor(param);
        }
    };

    var setViewFolder = function(galery) {
        if (galery == undefined || galery != "g") {
            ASC.Files.UI.viewFolder = "l";
            jq("#files_gallery_btn").css("display", "");
            jq("#files_list_btn").hide();
        }
        else {
            ASC.Files.UI.viewFolder = "g";
            jq("#files_list_btn").css("display", "");
            jq("#files_gallery_btn").hide();
        }
    };

    var switchViewFolder = function() {
        if (ASC.Files.UI.viewFolder == 'l') {
            ASC.Files.UI.viewFolder = 'g';
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoViewFolderGallery);
        }
        else {
            ASC.Files.UI.viewFolder = 'l';
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoViewFolderList);
        }
        madeAnchor();
    };

    var updateFolderView = function() {
        if (ASC.Files.Folders.currentFolderId == 0) {
            ASC.Controls.AnchorController.move("");
            return;
        }

        ASC.Files.Folders.getFolderItems(false);

        if (ASC.Files.Folders.eventAfter != null) {
            ASC.Files.Folders.eventAfter();
            ASC.Files.Folders.eventAfter = null;
        }
    };

    var showOverwriteMessage = function(list, folderID, destTitle, isCopyOperation, data) {

        var folder = jq("#content_folder_" + folderID + " div.fileName a").attr("title") || jq("#stree_selector_" + folderID).text();

        var message;
        if (data.length > 1) {
            var files = "";
            for (var i = 0; i < data.length; i++) {
                files += jq.format("<li title='{0}'>{0}</li>", data[i].Value);
            }
            message = "<b>" + jq.format(ASC.Files.Resources.FilesAlreadyExist, data.length, folder) + "</b>";
            jq("#files_overwrite_list").html(files).show();
        }
        else {
            jq("#files_overwrite_list").hide();
            message = jq.format(ASC.Files.Resources.FileAlreadyExist,
                                "<span title='" + data[0].Value + "'>" + data[0].Value + "</span>",
                                "<b>" + folder + "</b>");
        }

        jq("#files_overwrite_msg").html(message);

        jq("#files_overwrite_overwrite").unbind("click").click(function() {
            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
            serviceManager.moveItems(ASC.Files.TemplateManager.events.MoveItems,
                                     {
                                         destID: folderID,
                                         overwrite: true,
                                         isCopyOperation: (isCopyOperation == true)
                                     },
                                    { stringList: list });
        });

        jq("#files_overwrite_skip").unbind("click").click(function() {

            for (var i = 0; i < data.length; i++) {
                ASC.Files.UI.blockItem("content_file_" + data[i].Key);
                var pos = jq.inArray("file_" + data[i].Key, list.entry);
                if (pos != -1)
                    list.entry.splice(pos, 1);
            }

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
            serviceManager.moveItems(ASC.Files.TemplateManager.events.MoveItems,
                                     {
                                         destID: folderID,
                                         overwrite: false,
                                         isCopyOperation: (isCopyOperation == true)
                                     },
                                    { stringList: list });
        });

        jq("#files_overwrite_cancel").unbind("click").click(function() {
            for (var i = 0; i < list.entry.length; i++) {
                ASC.Files.UI.blockItem("content_" + list.entry[i]);
            }

            PopupKeyUpActionProvider.CloseDialogAction = "";
            PopupKeyUpActionProvider.CloseDialog();
        });

        ASC.Files.Common.blockUI(jq("#files_overwriteFiles"), 400, 300);

        PopupKeyUpActionProvider.EnterAction = 'jq("#files_overwrite_overwrite").click();';
        PopupKeyUpActionProvider.CloseDialogAction = 'jq("#files_overwrite_cancel").click();';
    };

    var blockItem = function(id, value, descr) {
        var obj = jq("#" + id);

        if (obj.hasClass("checkloading") && value)
            return;

        ASC.Files.UI.selectRow(obj, false);
        ASC.Files.UI.currentCombo = "";
        ASC.Files.UI.hideLinks(obj);

        if (value === true) {
            if (typeof descr != "undefined") {
                if (descr != "") {
                    jq("div.name a", obj).hide();
                    jq("div.name", obj).append("<a class='name descr_block_name'>" + descr + "</a>");
                }
            }
            obj.addClass('loading checkloading');
            obj.block({ message: "", baseZ: 99 });
        } else {
            jq("div.name a.descr_block_name", obj).remove();
            jq("div.name a", obj).css("display", "");

            obj.removeClass('loading checkloading');
            obj.unblock();
            obj.css('position', 'static');
        }
    };

    var hideLinks = function(obj) {
        jq(obj).removeClass("row-over");

        jq("div.combobox", obj).addClass("no_combobox").removeClass("combobox")

        if (ASC.Files.UI.currentCombo != "")
            jq("#" + ASC.Files.UI.currentCombo).parent().removeClass("no_combobox").addClass("combobox");

        jq("div.fileFavorites a.file_fav_notyet", jq(obj)).removeClass("file_fav_action");

    };

    var showLinks = function(obj) {
        if (jq("#files_selector").css("display") == "block"
            || jq("#" + obj.id + '.loading').length != 0)
            return;

        jq(obj).addClass("row-over");
        jq("div.no_combobox", obj).addClass("combobox").removeClass("no_combobox")

        jq("div.fileFavorites a.file_fav_notyet", jq(obj)).addClass("file_fav_action");
    };

    var editingFile = function(rowID) {
        return jq("div.editing_file_icn:visible", jq("#" + rowID)).length != 0;
    };

    var editableFile = function(rowID, webEditor) {
        var title = jq('div.name a', jq("#" + rowID)).attr("title");
        webEditor = (webEditor == true);
        return ASC.Files.Folders.folderContainer != "trash"
                && jq("#" + rowID).is("div:not(.folderRow)")
                && jq("#" + rowID).is("div:not(.rowRename)")
                && ASC.Files.UI.accessibleItem(rowID.replace("content_", ""))
                && (!webEditor && ASC.Files.Utils.FileCanBeEditOO(title)
                    || webEditor && ASC.Files.Utils.FileCanBeEdit(title))
               ;
    };

    var paintRows = function() {
        if (ASC.Files.UI.viewFolder != "g") {
            jq('div.fileRow:odd').removeClass("even");
            jq('div.fileRow:even').addClass("even");
        }
    };

    var addRowHandlers = function(id) {

        ASC.Files.UI.paintRows();

        if (ASC.Files.Folders.folderContainer == "trash" ||
            ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT != 0 ||
            jq("#favorites_addcurrent").length == 0) {
            jq("div.fileFavorites").remove();
        }

        jq('div.fileRow#' + (id || "")).each(function() {
            var row = jq(this);
            var idObj = this.id.replace("content_", "");
            var idFile = idObj.replace("file_", "");

            row.unbind("mousedown").mousedown(function(event) { ASC.Files.UI.preparingMoveTo(this, event); });

            row.find("div[id*='combo_file_']").unbind("click").click(function(event) { ASC.Files.Actions.showFileActions(this, event); });

            row.find("div[id*='combo_folder_']").unbind("click").click(function(event) { ASC.Files.Actions.showFolderActions(this, event); });

            if (ASC.Files.Folders.folderContainer == "trash") {
                row.find("div.fileInfo span.titleCreated").remove();
                row.find("div.fileInfo span.titleRemoved").css("display", "");
                if (row.is(".folderRow")) {
                    row.find("span.create_date").remove();
                    row.find("span.modified_date").css("display", "");
                }
                else {
                    row.find("div.fileInfo:not(.folderRow) span.create_by").remove();
                    row.find("div.fileInfo:not(.folderRow) span.fortrash_create_by").show().removeClass("fortrash_create_by").addClass("create_by");
                }
            } else {
                row.find("a.file_fav_notyet").attr("alt", ASC.Files.Resources.AddToFavorites).attr("title", ASC.Files.Resources.AddToFavorites);
                if (ASC.Files.Favorites) ASC.Files.Favorites.setIfFavorites(row);

                row.find("div.fileInfo span.titleRemoved").remove();
                row.find("div.fileInfo span.fortrash_create_by").remove();
            }

            if (!row.hasClass('checkloading')) {
                var linkRow = row.find(".fileName a.name");
                var title = linkRow.attr("title");
                var itFolder = row.hasClass("folderRow");
                var isEditable = ASC.Files.UI.editableFile(jq(this).attr("id"), true);

                linkRow.html(ASC.Files.Utils.clippingTitle(title, itFolder, (ASC.Files.UI.viewFolder == "g"), isEditable));

                if (ASC.Files.Folders.folderContainer == "trash")
                    linkRow.attr("href", "#" + ASC.Files.Constants.FOLDER_ID_TRASH);

                if (!itFolder) {
                    if (ASC.Files.Folders.folderContainer != "trash") {
                        var vers = jq("#file_version_" + idFile).val() || "";
                        if (ASC.Files.Utils.FileCanBePreview(title)) {
                            var url = ASC.Files.Utils.getPreviewUrl(title, idFile, vers);
                            if (ASC.Files.Utils.itIsDocument(title)) {
                                linkRow.attr("href", url).attr("target", "_blank");
                            } else {
                                linkRow.attr("href", url);
                            }
                        }
                        else {
                            linkRow.attr("href", ASC.Files.Utils.fileDownloadUrl(idFile, vers));
                        }
                    }

                    if (isEditable) {
                        row.find("div.fileEdit").show();
                        if (ASC.Files.UI.editingFile(jq(this).attr("id"))) {
                            row.find("div.fileEdit").hide();
                            row.find("div.fileEditing").show();
                        } else {
                            row.find("div.fileEdit").show();
                            row.find("div.fileEditing").hide();
                        }
                    } else {
                        jq("#content_file_" + idFile).addClass("cannotEdit");
                        jq("#content_file_" + idFile + " div.fileEdit").remove();
                        jq("#content_file_" + idFile + " div.fileEditing").remove();
                    }
                }

                ASC.Files.UI.markForMe(idObj);
            }
        });
    };

    var displayEmptyScreen = function() {
        var container = ASC.Files.Folders.folderContainer == "trash"
                        || !ASC.Files.UI.accessibleItem();

        jq('#files_mainContent').hide();
        jq("#files_pageNavigatorHolder").hide();
        jq("#files_clearFilter_btn, #files_clearFilterCont_btn").parent().show();

        if (container === true) {
            jq('#emptyContainer').show();
            if (ASC.Files.UI.viewFilter == 0 && ASC.Files.UI.viewSubject == "") {
                jq("#mainContentHeader, #mainContentHeader_panel").hide();
                jq("#files_clearFilterCont_btn").parent().hide();
            }
        } else {
            jq('#emptyScreen').show();
            if (ASC.Files.UI.viewFilter == 0 && ASC.Files.UI.viewSubject == "") {
                jq("#mainContentHeader, #mainContentHeader_panel").hide();
                jq("#files_clearFilter_btn").parent().hide();
            }
        }
    };

    var hideEmpty = function() {
        jq('#emptyScreen').hide();
        jq('#emptyContainer').hide();
        jq('#files_mainContent').css("display", "");
        jq("#mainContentHeader").css("display", "");
    };

    var checkTarget = function(target) {
        if (target.nodeName == "HTML")
            return true;

        if (jq("div.popupModal:visible").length != 0)
            return true;

        if (jq.inArray(jq("div.mainContainerClass")[0], jq(target).parents()) == -1)
            return true;

        if (jq("#imageViewerToolbox:visible, #imageViewerContainer:visible").length != 0)
            return true;

        if (jq(target).is("div.fileRow") || jq(target).is("div.fileRow div") ||
           jq(target).is("div.fileRow input") || jq(target).is("div.fileRow span") ||
           jq(target).is("div.fileRow a") || jq(target).is("div.files_action_panel") ||
           jq(target).is("div.files_action_panel a") || jq(target).is("div.files_action_panel div") ||
           jq(target).is("div.files_treeViewPanel") || jq(target).is("div.files_treeViewPanel a") ||
           jq(target).is("div.files_treeViewPanel div") || jq(target).is("div.files_treeViewPanel div") ||
           jq(target).is("div.files_treeViewPanel ul") || jq(target).is("div.files_treeViewPanel ins") ||
           jq(target).is("div.files_treeViewPanel a") || jq(target).is("div.files_treeViewPanel li") ||
           jq(target).is("ul#mainMenuHolder") || jq(target).is("ul#mainMenuHolder img") || jq(target).is("ul#mainMenuHolder a") ||
           jq(target).is("#files_actions_open") || jq(target).is("#files_pageNavigatorHolder a") ||
           jq(target).is(".files_progress_box"))
            return true;

        return false;
    };

    var beginSelecting = function(e) {

        e = ASC.Files.Common.fixEvent(e);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1)))
            return true;
        if (jq("div.fileRow:not(.checkloading):not(.newFolder):not(.newFile)").length == 0)
            return true;

        var target = e.target || e.srcElement;

        try {
            if (checkTarget(target))
                return true;
        }
        catch (e) {
            return true;
        }

        var posX = e.pageX;
        var posY = e.pageY;

        ASC.Files.UI.startX = posX;
        ASC.Files.UI.startY = posY;

        jq("#studioPageContent").append("<div id='files_selector'></div>");

        jq("#files_selector").css({
            'left': posX + 'px',
            'top': posY + 'px',
            'width': '0px',
            'height': '0px',
            'z-index': '300',
            'background-color': '#6CA3BF',
            'position': 'absolute'
        });
        jq("#files_selector").fadeTo(0, 0.3);

        jq("body").addClass("select_action");

        var windowFix = (jq.browser.msie && jq.browser.version < 9 ? jq("body") : jq(window));
        windowFix.unbind("mouseout").unbind("mousemove").unbind("mouseup")
        .mousemove(function(e) {

            e = ASC.Files.Common.fixEvent(e);
            var target = e.target || e.srcElement;
            if (target == undefined)
                return;

            var posX = e.pageX;
            var posY = e.pageY;

            var width, height, top, left;

            width = Math.abs(posX - ASC.Files.UI.startX);
            height = Math.abs(posY - ASC.Files.UI.startY);

            if (width < 5 && height < 5)
                return;

            left = Math.min(ASC.Files.UI.startX, posX);
            top = Math.min(ASC.Files.UI.startY, posY);

            jq("#files_selector").css({
                'width': width + "px",
                'height': height + 'px',
                'left': left + "px",
                'top': top + 'px',
                'display': 'block',
                'border': '1px solid #034',
                'cursor': 'default'
            });

            ASC.Files.Actions.hideAllActionPanels();

            jq("div.fileRow:not(.checkloading):not(.newFolder):not(.newFile)").each(function() {
                var startX = Math.min(ASC.Files.UI.startX, posX);
                var startY = Math.min(ASC.Files.UI.startY, posY);
                var endX = Math.max(ASC.Files.UI.startX, posX);
                var endY = Math.max(ASC.Files.UI.startY, posY);

                var offset = jq(this).offset();

                var difX = this.offsetWidth;
                var difY = this.offsetHeight;

                if ((offset.top > startY && offset.top < endY && offset.left > startX && offset.left < endX)
                    || (offset.top + difY > startY && offset.top + difY < endY && offset.left > startX && offset.left < endX)
                    || (offset.top + difY > startY && offset.top + difY < endY && offset.left + difX > startX && offset.left + difX < endX)
                    || (offset.top > startY && offset.top < endY && offset.left + difX > startX && offset.left + difX < endX)
                    || (offset.top > startY && offset.top < endY && offset.left < startX && offset.left + difX > endX)
                    || (offset.top + difY > startY && offset.top + difY < endY && offset.left < startX && offset.left + difX > endX)
                    || (offset.top < startY && offset.top + difY > endY && offset.left > startX && offset.left < endX)
                    || (offset.top < startY && offset.top + difY > endY && offset.left + difX > startX && offset.left + difX < endX)
                    ) {
                    ASC.Files.UI.selectRow(this, true);
                }
                else {
                    if (!e.ctrlKey)
                        ASC.Files.UI.selectRow(this, false);
                }
            });

            resetSelectAll(jq("#files_mainContent input[type='checkbox']").length == jq("#files_mainContent input:checked").length);

            return false;
        });

        windowFix.bind("mouseup", function(e) {
            jq("#files_selector").remove();
            ASC.Files.UI.startX = 0;
            ASC.Files.UI.startY = 0;
            ASC.Files.UI.mouseBtn = false;
            windowFix.unbind("mousemove").unbind("mouseup");
            jq("body").removeClass("select_action");
        });

        return false;
    };

    var handleMove = function(e) {

        if (jq("div.files_popup_win:visible,\
                div.blockDialog:visible,\
                #imageViewerToolbox:visible").length != 0)
            return;

        e = ASC.Files.Common.fixEvent(e);
        var posX = e.pageX;
        var posY = e.pageY;
        var target = e.target || e.srcElement;
        var noOver = true;

        var list = jq("#files_mainContent div.fileRow").each(function() {

            var offset = jq(this).offset();

            var difX = this.offsetWidth;
            var difY = this.offsetHeight;

            if ((offset.top < posY && offset.top + difY > posY &&
                offset.left < posX && offset.left + difX > posX)) {
                if (ASC.Files.UI.currentRowOver == this) {
                    noOver = false;
                    return undefined;
                }
                var prev = ASC.Files.UI.currentRowOver;

                ASC.Files.UI.currentRowOver = this;
                ASC.Files.UI.showLinks(this);
                noOver = false;

                if (prev != null)
                    ASC.Files.UI.hideLinks(prev);
            }
        });

        if (noOver) {
            if (ASC.Files.UI.currentRowOver != null)
                ASC.Files.UI.hideLinks(ASC.Files.UI.currentRowOver);
            ASC.Files.UI.currentRowOver = null;
        }

    };

    var preparingMoveTo = function(obj, e) {

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1)))
            return undefined;

        if (ASC.Files.Folders.folderContainer == "trash")
            return false;

        if (jq("#files_treeViewPanelSelector").length == 0)
            return false;

        if (jq("#files_prompt_rename").length != 0)
            return true;

        var list = jq("#files_mainContent div.fileRow:not(.checkloading):not(.newFolder):not(.newFile) input:checked").parent().parent();

        if (list.length === 0)
            return undefined;

        var shouldShow = false;
        list.each(function() {
            if (jq(this).parent().attr("id") == jq(obj).attr("id"))
                shouldShow = true;
        });

        if (shouldShow == false)
            return undefined;

        e = ASC.Files.Common.fixEvent(e);

        ASC.Files.UI.prevX = e.pageX;
        ASC.Files.UI.prevY = e.pageY;

        jq("body").unbind("mouseout").mouseout(function(event) {
            ASC.Files.UI.beginMoveTo(obj, event);
            return true;
        })
       .unbind("mousemove").mousemove(function(event) {
           ASC.Files.UI.beginMoveTo(obj, event);
           return true;
       })

    };

    var beginMoveTo = function(obj, e) {

        e = ASC.Files.Common.fixEvent(e);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))
            || ASC.Files.UI.mouseBtn == false) {
            jq("body").unbind("mouseout").unbind("mousemove");
            return undefined;
        }

        if (Math.abs(e.pageX - ASC.Files.UI.prevX) < 5
            && Math.abs(e.pageY - ASC.Files.UI.prevY) < 5) {
            return false;
        }

        ASC.Files.Actions.hideAllActionPanels();

        var posX = jq("#files_mainContent").offset().left;
        var posY = jq("#files_mainContent").offset().top;

        var list = jq("#files_mainContent input[type='checkbox']").parent().parent();
        var main;

        jq("#files_moving").remove();
        jq("#files_mainContent").parent().append("<div id='files_moving'></div>");

        if (ASC.Files.UI.viewFolder == "g") {
            jq("#files_moving").addClass("thumbnails");
        }

        main = jq("#files_moving").css({
            'display': 'none',
            'left': posX + "px",
            'top': posY + 'px',
            'z-index': '300',
            'width': (ASC.Files.UI.viewFolder == "g" ? 726 : 722) + 'px',
            'height': 'auto',
            'position': 'absolute',
            'cursor': (jq.browser.msie ? 'move' : 'alias')
        });

        list.each(function() {
            if (jq("input:checked", jq(this)).html() != null) {
                jq(this).parent().clone(false).fadeTo(0, 0.4).appendTo(main).css({
                    'display': 'block',
                    'background-color': '#DAE8F3'
                });
            }
            else {
                jq(this).parent().clone(false).fadeTo(0, 0.4).appendTo(main).css({
                    'display': 'block',
                    'visibility': 'hidden'
                });
            }
        });

        jq("*", main).css("cursor", (jq.browser.msie ? 'move' : 'alias'));
        jq("* [title!='']", main).attr("title", "");
        jq("div.checkbox", main).css('visibility', 'hidden');
        jq("body").addClass("moveRow");

        jq("div.folderRow:not(.checkloading):not(.newFolder):not(.row-selected),\
            ul.files_navigation li a,\
            ul.favoritesBody li.favoritesRow,\
            div.levelUp\
        ").each(function() {

            var folderToId = this.id.replace("content_folder_", "").replace("files_navigation_", "").replace("levelUp_", "");
            if (folderToId.search("favoritesRow_") == 0) {
                folderToId = jq(this).children('.fav_list').val();
                if (folderToId.search("file_") == 0)
                    folderToId = "";
                else
                    folderToId = folderToId.replace("folder_", "");
            }

            if (folderToId != ""
            && folderToId != ASC.Files.Folders.currentFolderId
            && ASC.Files.UI.accessibleItem("folder_" + folderToId)
            && (folderToId != ASC.Files.Constants.FOLDER_ID_TRASH
                || ASC.Files.UI.accessibleItem())
            ) {
                jq(this).addClass("may-row-to");
            }
        });

        jq("body").unbind("mouseout").mouseout(function(event) {
            ASC.Files.UI.continueMoveTo(main, event);
            return true;
        })
       .unbind("mousemove").mousemove(function(event) {
           ASC.Files.UI.continueMoveTo(main, event);
           return true;
       })
       .unbind("mouseup").mouseup(function(event) {
           ASC.Files.UI.finishMoveTo(main, event);
       });
    };

    var continueMoveTo = function(obj, e) {
        e = ASC.Files.Common.fixEvent(e);
        ASC.Files.Folders.moveToFolder = "";

        if (ASC.Files.UI.mouseBtn == false || obj == undefined) {
            ASC.Files.UI.finishMoveTo(obj, e);
            return;
        }

        if (Math.abs(e.pageX - ASC.Files.UI.prevX) < 5
            && Math.abs(e.pageY - ASC.Files.UI.prevY) < 5) {
            return false;
        }

        var posX = e.pageX + jq("#files_mainContent").offset().left - ASC.Files.UI.prevX;
        var posY = e.pageY + jq("#files_mainContent").offset().top - ASC.Files.UI.prevY;

        jq(obj).css({
            'left': posX + "px",
            'display': 'block',
            'top': posY + 'px'
        });

        if (jq("#files_moving_tooltip").length == 0) {
            var list = jq("#files_mainContent input:checked").parent().parent();

            var textInfo = "";
            if (list.length == 1)
                textInfo = jq("#" + jq(list[0]).parent().attr("id") + " div.fileName a.name").attr("title");
            else
                textInfo = jq.format(ASC.Files.Resources.InfoSelectCount, list.length);
            textInfo = jq.format(ASC.Files.Resources.InfoSelectingDescribe, "<b>" + textInfo + "</b><br/>");

            jq("#files_mainContent").parent().append("<div id='files_moving_tooltip'></div>");
            jq("#files_moving_tooltip").html(textInfo);
        }
        jq("#files_moving_tooltip").css({ 'left': e.pageX + "px", 'top': e.pageY + 'px' });

        if (!ASC.Files.UI.accessibleItem()
            || e.ctrlKey) {
            if (!jq(obj).hasClass('copy')) {
                jq(obj).css('cursor', (jq.browser.msie ? 'crosshair' : 'copy')).addClass('copy');
                jq("*", obj).css("cursor", (jq.browser.msie ? 'crosshair' : 'copy'));
            }

            jq("#files_navigation_" + ASC.Files.Constants.FOLDER_ID_TRASH).removeClass("may-row-to");
        }
        else {
            if (jq(obj).hasClass('copy')) {
                jq(obj).css('cursor', (jq.browser.msie ? 'move' : 'alias')).removeClass('copy');
                jq("*", obj).css("cursor", (jq.browser.msie ? 'move' : 'alias'));
            }

            jq("#files_navigation_" + ASC.Files.Constants.FOLDER_ID_TRASH).addClass("may-row-to");
        }

        jq(".row-to").removeClass("row-to");

        jq(".may-row-to").each(function() {

            var folderToId = this.id.replace("content_folder_", "").replace("files_navigation_", "").replace("levelUp_", "");
            if (folderToId.search("favoritesRow_") == 0) {
                folderToId = jq(this).children('.fav_list').val();
                if (folderToId.search("file_") == 0)
                    folderToId = "";
                else
                    folderToId = folderToId.replace("folder_", "");
            }

            if (folderToId != "") {
                var difX = this.offsetWidth;
                var difY = this.offsetHeight;

                var pos = jq(this).offset();
                if (pos.top < e.pageY
                    && pos.top + difY > e.pageY
                    && pos.left < e.pageX
                    && pos.left + difX > e.pageX) {

                    ASC.Files.Folders.moveToFolder = folderToId;
                    jq(this).addClass("row-to");
                    return true;
                }
            }
        });

        if (jq.browser.opera) {
            var target = e.target || e.srcElement;
            target = jq(target);
            if (!target.is("div.fileRow"))
                target = jq(target).parents('.fileRow');

            var nameFix = "fix_select_text";
            var el = target.children('#' + nameFix);
            if (el.length == 0) {
                jq('#' + nameFix).remove();
                var el = document.createElement('INPUT');
                el.style.width = 0;
                el.style.height = 0;
                el.style.border = 0;
                el.style.margin = 0;
                el.style.padding = 0;
                el.id = nameFix;
                el.disabled = true;

                target.prepend(el);
                el = jq('#' + nameFix);
            }

            try { el.focus(); }
            catch (e) { el.disabled = false; el.focus(); el.disabled = true }
        }
    };

    var finishMoveTo = function(obj, e) {

        var data = {};
        data.entry = new Array();

        var idTo = ASC.Files.Folders.moveToFolder;
        ASC.Files.Folders.moveToFolder = "";
        jq(".row-to").removeClass("row-to");
        jq(".may-row-to").removeClass("may-row-to");
        jq("body").removeClass("moveRow");

        if (idTo == ASC.Files.Constants.FOLDER_ID_TRASH) {
            ASC.Files.Folders.deleteItem();
        } else {
            if (idTo != "") {
                ASC.Files.Folders.isCopyTo =
                    (!ASC.Files.UI.accessibleItem()
                    || e.ctrlKey === true);

                jq("div.fileRow", obj).each(function() {
                    if ((this.id.search("content_file_") == 0 || this.id.search("content_folder_") == 0)) {
                        if (jq(this).css("visibility") != "hidden") {
                            var objId = this.id.replace("content_", "");

                            if (ASC.Files.Folders.isCopyTo == true
                                || ASC.Files.UI.accessAdmin(objId)
                                ) {
                                if (ASC.Files.Folders.isCopyTo == true
                                    || jq("#file_editing_icn_" + this.id.replace("content_file_", "") + ":visible").length == 0) {
                                    ASC.Files.UI.blockItem(this.id, true,
                                                           (ASC.Files.Folders.isCopyTo == true) ?
                                                            ASC.Files.Resources.DescriptCopy :
                                                            ASC.Files.Resources.DescriptMove);
                                    data.entry.push(objId);
                                }
                            }
                        }
                    }
                });

                var destTitle = jq('#content_folder_' + idTo + ' div.name a').attr("title")
                                || jq("#files_navigation_" + idTo + " a").html()
                                || jq("input.fav_list[value='folder_" + idTo + "']").parent().children(".fav_folder").html();

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
            }
        }

        ASC.Files.Folders.isCopyTo = false;
        jq("body").unbind("mouseout").unbind("mousemove").unbind("mouseup");
        jq(obj).remove();
        jq("#files_moving").remove();
        jq("#files_moving_tooltip").remove();
    };

    var clickRow = function(obj, e) {
        e = ASC.Files.Common.fixEvent(e);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1)))
            return true;

        var target = e.target || e.srcElement;

        try {
            if (jq(target).is("a") || jq(target).is("div.combobtn") ||
                jq(target).is("div.fav") || jq(target).is("a.file_fav_action") || jq(target).is("a.file_fav_already"))
                return true;
        }
        catch (e) {
            return true;
        }

        ASC.Files.UI.selectRow(obj, !jq(obj).hasClass("row-selected"));
    };

    var selectRow = function(row, value) {
        if (jq(row).hasClass("row-selected") == value)
            return;

        if (jq(row).hasClass("checkloading") && value)
            return;

        if (jq(row).hasClass("newFolder") || jq(row).hasClass("rowRename"))
            value = false;

        var input = jq("input[type='checkbox']", jq(row));
        input.attr("checked", value === true);

        if (value) {
            jq(row).addClass("row-selected");
            if (jq.browser.msie) {
                jq(row).find('*').attr('unselectable', "on");
            }
        }
        else {
            jq(row).removeClass("row-selected");
            if (jq.browser.msie) {
                jq(row).find('*').attr('unselectable', "");
            }
        }

        resetSelectAll(jq("#files_mainContent input[type='checkbox']").length == jq("#files_mainContent input:checked").length);

        if (jq("#files_mainContent input:checked").length == 0 && ASC.Files.Folders.folderContainer != "trash")
            jq(".mainContentHeader_item_actions").removeClass("display");
        else
            jq(".mainContentHeader_item_actions").addClass("display");
    }

    var resetSelectAll = function(param) {
        jq("#files_selectAll_check").attr('checked', param === true);
    };

    var checkSelectAll = function(value) {
        jq("div.fileRow:not(.checkloading)").each(function() {
            ASC.Files.UI.selectRow(this, value);
        });
    };

    //    var studioSideHide = function(param) {
    //        var studioContent = jq("div.mainContainerClass").parent();

    //        if (param == true) {
    //            if (studioContent.hasClass('studioRightContent'))
    //                ASC.Files.UI.studioContentClass = 'studioRightContent';
    //            else
    //                ASC.Files.UI.studioContentClass = 'studioLeftContent';

    //            studioContent.css('widht', '100%').removeClass(ASC.Files.UI.studioContentClass);
    //            jq("#studio_sidePanel").hide();
    //        } else {
    //            ASC.Files.UI.studioContentClass = ASC.Files.UI.studioContentClass || 'studioRightContent';
    //            studioContent.css('widht', "").addClass(ASC.Files.UI.studioContentClass);
    //            jq("#studio_sidePanel").show();
    //            jq("ul#mainMenuHolder,\
    //                div.files_action_panel,\
    //                div.folderHeader,\
    //                #mainContentHeader,\
    //                div.mainContent,\
    //                #files_bottom_loader").css("display", "");
    //        }
    //    };

    var showQuota = function(value) {
        value = parseFloat(value);
        if (value > 0) {
            jq("#studio_sidePanel div.quotaValue").text(value + "%")
                .parents("div.studioSideBox").show();
            jq("#studio_sidePanel div.quotaProgressBar").animate({ width: value + "%" }, 1000);
        }
        else {
            jq("#studio_sidePanel div.quotaValue").parents("div.studioSideBox").hide();
        }
    };

    var displayLoading = function(message, key) {
        clearTimeout(timerLoading);

        if (key == true)
            ASC.Files.UI.keepLoading = true;

        if (jq("#loading_container").length != 0)
            return;
        var container = document.createElement('div');

        container.innerHTML = jq.format("{0}<div class='wait_container'>{1}</div>",
                                        ASC.Files.Resources.LoadingProcessing,
                                        (message || ASC.Files.Resources.LoadingWait));

        container.className = 'loading_container';
        container.setAttribute('id', 'loading_container');
        document.body.appendChild(container);

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("#loading_container").css("top", jq(window).scrollTop() + 150 + "px");
        }

        jq("body").addClass('loading');
    };

    var hideLoading = function(key) {
        if (ASC.Files.UI.keepLoading && !key)
            return;

        ASC.Files.UI.keepLoading = false;

        timerLoading =
                setTimeout(function() {
                    jq("#loading_container").remove();
                    jq("body").removeClass('loading');
                }, 10);
    };

    var displayInfoPanel = function(str, warn) {
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
    };

    var hideInfoPanel = function() {
        clearTimeout(timeIntervalInfo);
        jq("#infoPanelContainer").hide().children("div").html("&nbsp;");
    };

    var accessibleItem = function(itemId) {
        itemId = itemId || ("folder_" + ASC.Files.Folders.currentFolderId);
        var access = ASC.Files.Constants.USER_ADMIN
                    || jq("#by_" + itemId).length == 0
                    || jq("#by_" + itemId).val() == ASC.Files.Constants.USER_ID;

        if (ASC.Files.Share != undefined) {
            if (itemId == "folder_" + ASC.Files.Constants.FOLDER_ID_COMMON_FILES
                && !ASC.Files.Constants.USER_ADMIN)
                return false;

            if (itemId == "folder_" + ASC.Files.Constants.FOLDER_ID_SHARE)
                return false;

            var curAccess = jq("input[type='hidden']#access_" + itemId).val();

            switch (curAccess) {
                case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                    return true;
                    break;
                case ASC.Files.Constants.AceStatusEnum.Read:
                case ASC.Files.Constants.AceStatusEnum.Restrict:
                    return false;
                    break;
            }

            access = true;
        }

        return access;
    };

    var accessAdmin = function(itemId) {
        itemId = itemId || ("folder_" + ASC.Files.Folders.currentFolderId);
        var access = (ASC.Files.Constants.USER_ADMIN
                     || jq("#by_" + itemId).length == 0
                     || jq("#by_" + itemId).val() == ASC.Files.Constants.USER_ID);

        if (ASC.Files.Share != undefined) {
            var item = jq("input[type='hidden']#access_" + itemId);

            var curAccess = (item.length == 0 ?
                                ASC.Files.Constants.AceStatusEnum.None :
                                item.val());

            if (curAccess == ASC.Files.Constants.AceStatusEnum.Restrict) {
                jq("#content_" + itemId).remove();
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resource.AceStatusEnum_Restrict, true);
                return false;
            }

            access = (curAccess == ASC.Files.Constants.AceStatusEnum.None
                     || ASC.Files.Folders.folderContainer == "corporate"
                        && ASC.Files.Constants.USER_ADMIN);
        }
        return access;
    };

    var markForMe = function(itemId) {
        if (ASC.Files.Folders.folderContainer == "forme"
            && ASC.Files.Share != undefined) {

            var item = jq("input[type='hidden']#access_" + itemId);

            var curAccess = (item.length == 0 ?
                                ASC.Files.Constants.AceStatusEnum.None :
                                item.val());

            if (curAccess == ASC.Files.Constants.AceStatusEnum.ReadWrite
                || curAccess == ASC.Files.Constants.AceStatusEnum.Read) {
                jq("#share_icn_" + itemId).css("display", "block");
                jq("#content_" + itemId + " div.fileInfo").addClass("markForMe");
                return true;
            }
        }
        return false;
    };

    return {
        init: init,

        viewFilter: viewFilter,
        viewSubject: viewSubject,
        viewOrder: viewOrder,
        viewFolder: viewFolder,

        currentPage: currentPage,
        amountPage: amountPage,

        countEntityInFolder: countEntityInFolder,

        currentRowOver: currentRowOver,
        currentCombo: currentCombo,

        showOverwriteMessage: showOverwriteMessage,

        switchViewFolder: switchViewFolder,
        updateFolderView: updateFolderView,

        setOrderValue: setOrderValue,
        setFilterType: setFilterType,
        setFilterValue: setFilterValue,
        setViewFolder: setViewFolder,

        defaultFolderSet: defaultFolderSet,
        navigationSet: navigationSet,
        madeAnchor: madeAnchor,

        blockItem: blockItem,
        hideLinks: hideLinks,
        showLinks: showLinks,

        preparingMoveTo: preparingMoveTo,
        beginMoveTo: beginMoveTo,
        finishMoveTo: finishMoveTo,
        continueMoveTo: continueMoveTo,

        checkSelectAll: checkSelectAll,
        selectRow: selectRow,
        clickRow: clickRow,

        hideEmpty: hideEmpty,
        displayEmptyScreen: displayEmptyScreen,

        paintRows: paintRows,
        addRowHandlers: addRowHandlers,

        resetSelectAll: resetSelectAll,

        beginSelecting: beginSelecting,
        handleMove: handleMove,

        mouseBtn: mouseBtn,

        startX: startX,
        startY: startY,
        prevX: prevX,
        prevY: prevY,

        keepLoading: keepLoading,
        displayLoading: displayLoading,
        hideLoading: hideLoading,

        displayInfoPanel: displayInfoPanel,
        hideInfoPanel: hideInfoPanel,

        //        studioSideHide: studioSideHide,
        //        studioContentClass: studioContentClass,
        editingFile: editingFile,
        editableFile: editableFile,

        showQuota: showQuota,

        accessibleItem: accessibleItem,
        accessAdmin: accessAdmin,
        markForMe: markForMe
    };

})(jQuery);


(function($) {
    ASC.Files.UI.init();
    $(function() {

        jq("#infoPanelContainer").click(function() {
            ASC.Files.UI.hideInfoPanel();
        });

        jq(document).keydown(function(event) {
            if (jq('div.blockDialog:visible').length != 0 ||
                jq('div.files_popup_win:visible').length != 0 ||
                jq('#files_prompt_rename').length != 0 ||
                jq('#files_prompt_create_folder').length != 0 ||
                jq('#files_prompt_create_file').length != 0)
                return;

            var code;
            if (!e) var e = event;

            e = ASC.Files.Common.fixEvent(e);

            var target = e.target || e.srcElement;
            try {
                if (jq(target).is('input'))
                    return true;
            }
            catch (e) {
                return true;
            }

            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;

            if (code == 65 && e.ctrlKey) {
                if (jq.browser.opera)
                    setTimeout('jq("#files_selectAll_check").focus()', 1);
                ASC.Files.UI.checkSelectAll(true);
                return false;
            }
        });

        jq(document).keyup(function(event) {
            if (jq('div.blockDialog:visible').length != 0 ||
                jq('div.files_popup_win:visible').length != 0 ||
                jq('#files_prompt_rename').length != 0 ||
                jq('#files_prompt_create_folder').length != 0 ||
                jq('#files_prompt_create_file').length != 0)
                return;

            var code;
            if (!e) var e = event;

            e = ASC.Files.Common.fixEvent(e);

            var target = e.target || e.srcElement;
            try {
                if (jq(target).is('input'))
                    return true;
            }
            catch (e) {
                return true;
            }

            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;

            if (code == 46) {
                ASC.Files.Folders.deleteItem();
                return false;
            }

            if (code == 70 && e.shiftKey) {
                ASC.Files.Folders.createFolder();
                return false;
            }

            if (code == 78 && e.shiftKey) {
                ASC.Files.Folders.typeNewDoc = 'text';
                ASC.Files.Folders.createNewDoc();
                return false;
            }
        });

    });
})(jQuery);