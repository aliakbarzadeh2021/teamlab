window.ASC.Files.Actions = (function($) {
    var isInit = false;
    var filterCategory = 0;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            jq(document).click(function(event) {
                ASC.Files.Common.dropdownRegisterAutoHide(event, "#files_showTreeView", "#files_treeViewPanel");
                ASC.Files.Common.dropdownRegisterAutoHide(event, "#files_actions_open", "#files_actionsPanel");
            });
        }
    };

    /* Methods*/

    var showSortViewPanel = function(obj, event) {
        if (ASC.Files.Folders.folderContainer === "my") {
            jq("#files_sort_Author").hide();
        }
        else {
            jq("#files_sort_Author").show();
        }
        jq("#files_sortPanel").show();

        showMainContentHeader(obj, event, "files_sort_open");
    };

    var showFilterTypePanel = function(obj, event) {
        if (ASC.Files.Folders.folderContainer === "my") {
            jq("#files_filter_show_1").hide();
            jq("#files_filter_show_2").hide();
        }
        else {
            jq("#files_filter_show_1").show();
            jq("#files_filter_show_2").show();
        }
        jq("#files_filterPanel").show();

        showMainContentHeader(obj, event, "files_filter_category");
    };

    var showFilterValuePanel = function(obj, event) {
        showMainContentHeader(obj, event, "files_filter_value");

        switch (filterCategory) {
            case 1:
                jq("#files_filter_usersPanel").show();
                if (!ASC.Controls.Constants.isMobileAgent) {
                    if (jq("#files_filter_usersPanel").data('jsp')) {
                        jq("#files_filter_usersPanel").data('jsp').reinitialise();
                    } else {
                        jq("#files_filter_usersPanel").jScrollPane();
                    }
                }
                break;
            case 2:
                jq("#files_filter_departmensPanel").show();
                if (!ASC.Controls.Constants.isMobileAgent) {
                    if (jq("#files_filter_departmensPanel").data('jsp')) {
                        jq("#files_filter_departmensPanel").data('jsp').reinitialise();
                    } else {
                        jq("#files_filter_departmensPanel").jScrollPane();
                    }
                }
                break;
            case 3:
                jq("#files_filter_typesPanel").show();
                break;
            default:
                jq("#files_filter_value").hide();
                ASC.Files.UI.setFilterValue(0, "");
                return;
        }
    };

    var setFilterCategory = function(obj, event) {
        jq("#mainContentHeader_panel a.select").removeClass('select');
        jq(obj).addClass('select')
        jq("#files_filter_category span").html(jq(obj).html());
        jq("#files_filter_value span").html(ASC.Files.Resources.SelectFilterValue);

        filterCategory = parseInt(jq(obj).parent().attr("id").replace('files_filter_show_', "")) || 0;

        jq("#files_filter_value").show();
        jq("#files_filter_value").click();
    };

    var showMainContentHeader = function(obj, event, button) {
        ASC.Files.Common.dropdownToggle(jq("#mainContentHeader"), "mainContentHeader_panel");
        jq("#" + button).addClass("select");

        jq("body").click(function(event) {
            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf().is("#mainContentHeader_panel, #" + obj.id))
                ASC.Files.Actions.hideAllActionPanels();
        });
    };

    var showActionsViewPanel = function(obj, event) {
        jq("#files_deleteButton, #files_movetoButton, #files_copytoButton").hide();
        var count = jq("#files_mainContent div.fileRow:not(.checkloading):not(.newFolder):not(.newFile) input:checked").length;
        var countWithRights = count;

        jq("#files_mainContent div.fileRow:not(.checkloading):not(.newFolder):not(.newFile) input:checked").each(function() {
            if (jq("#file_editing_icn_" + this.id.replace('check_file_', "") + ":visible").length != 0
                || !ASC.Files.UI.accessAdmin(this.id.replace("check_", ""))) {
                countWithRights--;
            }
        });

        if (count > 0) {
            jq("#files_downloadButton, #files_restoreButton, #files_copytoButton").show().find('span').html(count);
        }

        if (countWithRights > 0) {
            jq("#files_deleteButton, #files_movetoButton").show().find('span').html(countWithRights);
        }

        if (ASC.Files.Folders.folderContainer == "trash") {
            jq("#files_deleteButton, #files_movetoButton, #files_copytoButton").hide();
        }

        if (!ASC.Files.UI.accessAdmin()) {
            jq("#files_deleteButton, #files_movetoButton").hide();
        }

        jq("#files_restoreButton, #files_emptyTrashButton").hide();
        if (ASC.Files.Folders.folderContainer == "trash") {
            if (count > 0) {
                jq("#files_downloadButton, #files_deleteButton, #files_restoreButton").show();
            }
            else {
                jq("#files_downloadButton").hide();
            }
            jq("#files_emptyTrashButton").show();
        }

        ASC.Files.Common.dropdownToggle(jq("#files_actions_open").parent().parent(), "files_actionsPanel");
    };

    var showActionsPanel = function(type, obj, event) {
        if (jq("#content_" + obj.id.replace('combo_', "") + '.loading').length != 0)
            return;

        ASC.Files.UI.currentCombo = obj.id;
        jq("#files_mainContent div.combobox").removeClass("combobox").addClass("no_combobox");
        jq("#" + ASC.Files.UI.currentCombo).parent().removeClass("no_combobox").addClass("combobox");

        ASC.Files.UI.checkSelectAll(false);
        ASC.Files.UI.selectRow(jq("#content_" + obj.id.replace('combo_', "")), true);

        jq("#files_actionPanel_folders, #files_actionPanel_files").hide();
        if (type == 'folder') {
            var objId = obj.id.replace("combo_folder_", "");

            jq("#files_open_folders,\
				#files_download_folders,\
				#files_shareAccess_folders,\
				#files_unsubscribe_folders,\
				#files_moveto_folders,\
				#files_copyto_folders,\
				#files_rename_folders,\
				#files_restore_folders,\
                #files_remove_folders").css("display", "");

            jq("#files_open_folders").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.UI.madeAnchor(objId);
            });

            jq("#files_download_folders").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.download(obj.id);
            });

            jq("#files_moveto_folders, #files_restore_folders, #files_copyto_folders").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Tree.showMoveToSelector(obj.id, this.id == "files_copyto_folders");
                jq("#" + obj.id).parent().removeClass("no_combobox").addClass("combobox");
            });

            jq("#files_rename_folders").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.rename(obj.id);
            });

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#files_open_folders,\
                    #files_shareAccess_folders,\
                    #files_moveto_folders,\
                    #files_copyto_folders,\
                    #files_rename_folders").hide();
            }
            else {
                jq("#files_restore_folders").hide();
            }

            if (ASC.Files.Folders.folderContainer == "project") {
                jq("#files_shareAccess_folders").hide();
            }

            if (!ASC.Files.UI.accessibleItem("folder_" + objId)) {
                jq("#files_rename_folders").hide();
            }

            if (ASC.Files.UI.accessAdmin("folder_" + objId)) {
                if (jq("#files_share_btn:visible").length != 0) {
                    jq("#files_shareAccess_folders").unbind("click").click(function() {
                        ASC.Files.Actions.hideAllActionPanels();
                        ASC.Files.Share.clickShareButton(obj.id);
                    });
                } else {
                    jq("#files_shareAccess_folders").hide();
                }

                jq("#files_remove_folders").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Folders.deleteItem("folder_" + objId);
                });
            } else {
                jq("#files_moveto_folders,\
                    #files_shareAccess_folders,\
                    #files_remove_folders").hide();
            }

            if (jq("#content_folder_" + objId + " div.fileInfo").hasClass("markForMe")) {
                jq("#files_unsubscribe_folders").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Share.unSubscribeMe("folder_" + objId);
                });
            } else {
                jq("#files_unsubscribe_folders").hide();
            }

            jq("#files_actionPanel_folders").show();
        }
        else {
            var objId = obj.id.replace("combo_file_", "");
            var title = jq("#content_file_" + objId + " .fileName a.name").attr("title");

            jq("#files_open_files,\
                #files_edit_files,\
                #files_editOO_files,\
                #files_download_files,\
                #files_shareaccess_files,\
                #files_unsubscribe_files,\
                #files_getlink_files,\
                #files_uploads_files,\
                #files_versions_files,\
                #files_moveto_files,\
                #files_copyto_files,\
                #files_rename_files,\
				#files_restore_files,\
                #files_remove_files").css("display", "");

            jq("#files_download_files").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.download(obj.id);
            });

            if (ASC.Files.Folders.folderContainer == "corporate"
                || ASC.Files.Folders.folderContainer == "my"
                    && ASC.Files.UI.accessAdmin("file_" + objId)
                    && jq("#share_icn_file_" + objId + ":visible").length != 0) {

                jq("#files_getlink_files").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Folders.getLink(title, objId);
                });
            }
            else {
                jq("#files_getlink_files").hide();
            }

            jq("#files_moveto_files, #files_restore_files, #files_copyto_files").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Tree.showMoveToSelector(obj.id, this.id == "files_copyto_files");
                jq("#" + obj.id).parent().removeClass("no_combobox").addClass("combobox");
            });

            jq("#files_rename_files").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.rename(obj.id);
            });

            jq("#files_remove_files").unbind("click").click(function() {
                ASC.Files.Actions.hideAllActionPanels();
                ASC.Files.Folders.deleteItem("file_" + objId);
            });

            if (ASC.Files.Utils.FileCanBePreview(title)) {
                jq("#files_open_files").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Folders.clickOnFile(objId);
                    return false;
                });
            }
            else
                jq("#files_open_files").hide();

            var countVersion = jq("#file_version_" + objId).val();
            if (ASC.Files.UI.viewFolder == "g"
                || countVersion < 2
                || ASC.Files.Folders.folderContainer == "trash"
                || jq("#content_file_" + objId + " #content_versions").length != 0) {
                jq("#files_versions_files").hide();
            }
            else {
                jq("#files_versions_files span").html(countVersion);
                jq("#files_versions_files").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Folders.showVersions(obj.id);
                });
            }

            if (ASC.Files.UI.viewFolder == "g") {
                jq("#files_uploads_files").hide();
            } else {
                jq("#files_uploads_files").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Uploads.showUploadNewVersion(objId);
                });
            }

            if (jq("#file_editing_icn_" + objId + ":visible").length != 0) {
                jq("#files_edit_files,\
                    #files_editOO_files,\
                    #files_uploads_files,\
                    #files_versions_files,\
                    #files_moveto_files,\
                    #files_rename_files,\
                    #files_remove_files").hide();
            } else {
                if (ASC.Files.UI.editableFile("content_file_" + objId, true)
                    && !ASC.Files.UI.editingFile("content_file_" + objId)) {
                    jq("#files_edit_files").unbind("click").click(function() {
                        ASC.Files.Actions.hideAllActionPanels();
                        PopupKeyUpActionProvider.CloseDialog();
                        ASC.Files.Actions.checkEditFile(objId, title, true);
                    });
                } else {
                    jq("#files_edit_files").hide();
                }

                if (ASC.Files.UI.editableFile("content_file_" + objId, false)
                    && !ASC.Files.UI.editingFile("content_file_" + objId)) {
                    jq("#files_editOO_files").unbind("click").click(function() {
                        ASC.Files.Actions.hideAllActionPanels();
                        PopupKeyUpActionProvider.CloseDialog();
                        ASC.Files.Actions.checkEditFile(objId, title, false);
                    });
                } else {
                    jq("#files_editOO_files").hide();
                }
            }

            if (ASC.Files.Folders.folderContainer == "trash") {
                jq("#files_open_files,\
                    #files_edit_files,\
                    #files_editOO_files,\
                    #files_shareaccess_files,\
                    #files_getlink_files,\
                    #files_uploads_files,\
                    #files_versions_files,\
                    #files_moveto_files,\
                    #files_copyto_files,\
                    #files_rename_files").hide();

                jq("#files_remove_files,#files_restore_files").css("display", ""); ;
            }
            else {
                jq("#files_restore_files").hide();
            }

            if (ASC.Files.Folders.folderContainer == "project") {
                jq("#files_shareaccess_files").hide();
            }

            if (!ASC.Files.UI.accessibleItem("file_" + objId)) {
                jq("#files_edit_files,\
                    #files_editOO_files,\
                    #files_uploads_files,\
                    #files_rename_files").hide();
            }

            if (ASC.Files.UI.accessAdmin("file_" + objId)) {
                if (jq("#files_share_btn:visible").length != 0) {
                    jq("#files_shareaccess_files").unbind("click").click(function() {
                        ASC.Files.Actions.hideAllActionPanels();
                        ASC.Files.Share.clickShareButton(obj.id);
                    });
                } else {
                    jq("#files_shareaccess_files").hide();
                }
            }
            else {
                jq("#files_moveto_files,\
                    #files_shareaccess_files,\
                    #files_remove_files").hide();
            }

            if (jq("#content_file_" + objId + " div.fileInfo").hasClass("markForMe")
                || (ASC.Files.Folders.currentContainer == "corporate"
                    && ASC.Files.Constants.USER_ADMIN != true)) {
                jq("#files_unsubscribe_files").unbind("click").click(function() {
                    ASC.Files.Actions.hideAllActionPanels();
                    ASC.Files.Share.unSubscribeMe("file_" + objId);
                });
            } else {
                jq("#files_unsubscribe_files").hide();
            }

            jq("#files_actionPanel_files").show();
        }

        ASC.Files.Common.dropdownToggle(jq(obj).parent().parent(), 'files_actionPanel');

        jq("body").click(function(event) {
            if (ASC.Files.UI.currentCombo == "") return;

            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf()
                    .is("#files_actionPanel, div[id*='combo_folder_'], div[id*='combo_file_'], div.sub-folder, div.name a.name")) {
                ASC.Files.UI.currentCombo = "";
                ASC.Files.Actions.hideAllActionPanels();
            }
        });
    };

    var onContextMenu = function(objId, event) {
        var result = false;
        if (typeof event != "undefined") {
            if (jq(event.srcElement || event.target).is('[id="files_prompt_rename"]')) {
                result = true;
            }
        }
        result = result || jq("#content_" + objId).hasClass("newFolder") || jq("#content_" + objId).hasClass("rowRename");

        if (!result)
            jq("#combo_" + objId).click();

        return result;
    };

    var showFolderActions = function(obj, event) {
        showActionsPanel("folder", obj, event);
    };

    var showFileActions = function(obj, event) {
        showActionsPanel("file", obj, event);
    };

    var hideAllActionPanels = function() {
        jq("div.files_popup_win").hide();
        jq("#mainContentHeader .select").removeClass("select");
        jq("body").unbind("click");
        ASC.Files.Folders.curItemMoveTo = "";
        ASC.Files.UI.currentCombo = "";
        jq("#files_mainContent div.combobox").removeClass("combobox").addClass("no_combobox");
    };

    var checkEditFile = function(id, title, webEditor, isNew) {
        if (webEditor == true) {
            jq("#file_editing_icn_" + id).show();
            jq("#file_IsEditing_" + id).val('true');
            jq("#content_file_" + id + " div.fileEdit").hide();
            jq("#content_file_" + id + " div.fileEditing").show();

            isNew = (isNew === true);
            if (!isNew && jq("#content_file_" + id).hasClass("isNewForWebEditor")) {
                isNew = true;
                jq("#content_file_" + id).removeClass("isNewForWebEditor");
            }

            var url = ASC.Files.Utils.getWebEditUrl(title, id) + (isNew ? "&new=true" : "");
            var winEditor = window.open(url, "_blank");

            winEditor.onload = function() {
                if (id) {
                    jq("#file_editing_icn_" + id).show();
                    jq("#file_IsEditing_" + id).val('true');
                    jq("#content_file_" + id + " div.fileEdit").hide();
                    jq("#content_file_" + id + " div.fileEditing").show();
                }
            };

            winEditor.onunload = function() {
                if (id) {
                    jq("#file_editing_icn_" + id).hide();
                    jq("#file_IsEditing_" + id).val('false');
                    jq("#content_file_" + id + " div.fileEdit").show();
                    jq("#content_file_" + id + " div.fileEditing").hide();

                    ASC.Files.Folders.replaceVersion(id, false, true);
                }
            };

        } else {
            if (!ASC.Files.Constants.PLUGIN_ENABLE)
                return false;

            switch (ASC.Files.Plugin.GetOpenOfficeState()) {
                case ASC.Files.Plugin.OPEN_OFFICE_STATE_NOT_INSTALLED:
                    jq("#plugin_informer").slideDown();
                    jq(document).scrollTo(0);
                    break;
                case ASC.Files.Plugin.OPEN_OFFICE_STATE_INSTALLING:
                    //TODO
                    break;
                case ASC.Files.Plugin.OPEN_OFFICE_STATE_INSTALLED:
                    jq("#content_file_" + id + " div.fileEdit").hide();
                    jq("#content_file_" + id + " div.fileEditing").show();
                    serviceManager.checkEditFile(ASC.Files.TemplateManager.events.CheckEditFile, { fileID: id, title: title });
                    break;
            }
        }
    };

    return {
        init: init,

        showFilterTypePanel: showFilterTypePanel,
        showFilterValuePanel: showFilterValuePanel,
        showSortViewPanel: showSortViewPanel,
        showActionsViewPanel: showActionsViewPanel,
        showFolderActions: showFolderActions,
        showFileActions: showFileActions,

        setFilterCategory: setFilterCategory,
        onContextMenu: onContextMenu,

        checkEditFile: checkEditFile,

        hideAllActionPanels: hideAllActionPanels
    };
})(jQuery);

(function($) {
    ASC.Files.Actions.init();
    $(function() {

        jq("#files_actions_open").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Actions.showActionsViewPanel();
        });

        jq("#files_filter_category").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Actions.showFilterTypePanel(this);
        });

        jq("#files_filter_value").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Actions.showFilterValuePanel(this);
        });

        jq("[id^='files_filter_show_'] a").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Actions.setFilterCategory(this);
        });

        jq("#files_sort_open").click(function() {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Actions.showSortViewPanel(this);
        });

        jq(document).mousedown(function(event) {
            var result = (ASC.Files.UI.beginSelecting(event) == true);
            ASC.Files.UI.mouseBtn = true;
            return result;
        });

        jq(document).mouseup(function(event) {
            ASC.Files.UI.mouseBtn = false;
        });

        jq(document).mousemove(function(event) {
            if (ASC.Files.UI.mouseBtn == false)
                ASC.Files.UI.handleMove(event);
            return true;
        });

    });
})(jQuery);