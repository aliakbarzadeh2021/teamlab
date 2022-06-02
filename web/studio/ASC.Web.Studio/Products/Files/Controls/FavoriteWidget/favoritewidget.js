window.ASC.Files.Favorites = (function($) {
    var isInit = false;

    var init = function() {
        if (isInit === false) {
            isInit = true;
            serviceManager.bind(ASC.Files.TemplateManager.events.GetFavorites, onGetFavorites);
            serviceManager.bind(ASC.Files.TemplateManager.events.SaveFavorite, onSaveFavorite);
            serviceManager.bind(ASC.Files.TemplateManager.events.RemoveFavorite, onRemoveFavorite);
        }
    };

    var setIfFavorites = function(obj) {
        var favId = favoriteId(obj.attr("id").replace("content_", ""));

        if (favId != null) {
            jq("a.file_fav_notyet", obj).removeClass("file_fav_notyet")
                                            .addClass("file_fav_already")
                                            .attr("alt", ASC.Files.Resources.RemoveFavorites)
                                            .attr("title", ASC.Files.Resources.RemoveFavorites)
                                            .removeAttr("onclick")
                                            .unbind("click")
                                            .click(function(event) { ASC.Files.Favorites.showRemoveFavorite(this, event, favId) });
            return;
        }
    };

    var favoriteId = function(objId) {
        var row = jq("#files_favoritesBody input.fav_list[value='" + objId + "']");
        if (row.length == 0) return null;

        return row.parent().attr("id").replace("favoritesRow_", "");
    };

    var getFavorite = function(fav_id) {
        var path = jq("#favoritesRow_" + fav_id + " input[type='hidden']").val();

        if (path.search("file_") == 0) {
            var id = path.replace("file_", "");
            ASC.Files.Folders.clickOnFile(id, null, jq("#favoritesRow_" + fav_id + " a").html(), true);
        }
        else if (path.search("folder_") == 0) {
            ASC.Files.Folders.clickOnFolder(path.replace("folder_", ""), true);
        }
    };

    var displayRemoveBtn = function(id) {
        jq(".favoritesRemoveAction").hide();
        jq("#removeBtn_" + id).show();
    };

    var showRemoveFavorite = function(obj, event, id) {
        ASC.Files.UI.finishMoveTo({}, event);

        ASC.Files.Actions.hideAllActionPanels();
        ASC.Files.UI.checkSelectAll(false);
        ASC.Files.UI.selectRow(jq(obj).parents('div.fileRow'), true);

        var title = jq("div.fileName a.name", jq(obj).parents('.fileRow')).attr("title");
        jq("#favoriteRemoveText").html(jq.format(ASC.Files.Resources.FavoritesRemove, '<br/><b>' + title + '</b><br/>'));

        ASC.Files.Common.dropdownToggle(jq(obj), 'favorites_remove_dialog');
        PopupKeyUpActionProvider.CloseDialogAction = 'jq("#favoriteRemoveBtn").unbind("click");ASC.Files.Actions.hideAllActionPanels();';
        PopupKeyUpActionProvider.EnterAction = 'jq("#favoriteRemoveBtn").click();';

        jq("#favoriteRemoveBtn").unbind("click").click(function() {
            removeFavorite(id);
            PopupKeyUpActionProvider.CloseDialog();
        });

        jq("body").click(function(event) {
            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf().is("#favorites_remove_dialog, div.fav, #" + obj.id))
                PopupKeyUpActionProvider.CloseDialog();
        });
    };

    var showToFavorite = function(obj, event, side) {
        ASC.Files.UI.finishMoveTo({}, event);

        ASC.Files.Actions.hideAllActionPanels();

        if (ASC.Files.Folders.folderContainer == "trash") {
            return;
        }
        
        var type = "";
        var id = "";
        var title = "";

        if (side) {
            type = 'folder';
            id = ASC.Files.Folders.currentFolderId;
            if (id == 0)
                id = ASC.Files.Folders.folderContainer;
            title = jq("#files_folderName").html();
        }
        else {

            ASC.Files.UI.checkSelectAll(false);
            ASC.Files.UI.selectRow(jq(obj).parents('div.fileRow'), true);

            var objId = jq(obj).parents('div.fileRow').attr("id");
            type = objId.split('_')[1];
            id = objId.split('_')[2];
            title = jq("#" + objId + " div.fileName a.name").attr("title");
            if (type == 'file') {
                var lenExt = ASC.Files.Utils.getFileExts(title).length;
                title = title.substring(0, title.length - lenExt);
            }
        }

        var exstItem = jq("#files_favoritesBody input.fav_list[value='" + type + "_" + id + "']");
        if (exstItem.length != 0) {
            exstItem.parent(".favoritesRow").yellowFade();
            return;
        }

        ASC.Files.Common.dropdownToggle(jq(obj), "favorites_prompt_name");
        PopupKeyUpActionProvider.CloseDialogAction = 'jq("#favoriteOkBtn").unbind("click");ASC.Files.Actions.hideAllActionPanels();';
        PopupKeyUpActionProvider.EnterAction = 'jq("#favoriteOkBtn").click();';

        jq("#favoriteNewName").val(title).focus().select();

        jq("#favoriteOkBtn").unbind("click").click(function() {
            var newName = ASC.Files.Utils.replaceSpecCharacter(jq("#favoriteNewName").val().trim());
            if (newName == "" || newName == null) {
                PopupKeyUpActionProvider.CloseDialog();
                return;
            }

            if (type == 'file') {
                var oldTitle = jq("#" + objId + " div.fileName a.name").attr("title");
                var lenExt = ASC.Files.Utils.getFileExts(oldTitle).length;
                newName += oldTitle.substring(oldTitle.length - lenExt);
            }

            saveFavorite({ path: (type + "_" + id), title: newName });
            PopupKeyUpActionProvider.CloseDialog();
        });

        jq("body").click(function(event) {
            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf().is("#favorites_prompt_name, div.fav, #" + obj.id))
                PopupKeyUpActionProvider.CloseDialog();
        });
    };

    //request
    var showFavorite = function() {
        serviceManager.request("get",
                                "xml",
                                ASC.Files.TemplateManager.events.GetFavorites,
                                { ajaxsync: true, showLoading: true },
                                'favorites');
    };

    var saveFavorite = function(params) {
        serviceManager.request("get",
                                "xml",
                                ASC.Files.TemplateManager.events.SaveFavorite,
                                params,
                                'favorites', 'add?title=' + encodeURIComponent(params.title) + '&folderPath=' + params.path);
    };

    var removeFavorite = function(id) {
        serviceManager.request("delete",
                                "xml",
                                ASC.Files.TemplateManager.events.RemoveFavorite,
                                { favoriteID: id, path: jq("#favoritesRow_" + id + " input").val() },
                                'favorites', id);
    };

    //event handler
    var onGetFavorites = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }
        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFavorite);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#files_favoritesBody").html(htmlXML);

        jq("#files_favoritesEmpty").show();
        if (htmlXML.length == 0)
            return;

        jq("#files_favoritesEmpty").hide();
        jq("#files_favoritesBody").show();

        jq("#files_favoritesBody .favoritesRow").each(function() {
            var path = jq(this).children(".fav_list").val();

            if (path.search("file_") == 0) {
                title = jq(this).children(".fav_file").html();
                var id = path.replace("file_", "");
                var link = jq(this).children('.fav_file')
                if (ASC.Files.Utils.FileCanBePreview(title)) {
                    var url = ASC.Files.Utils.getPreviewUrl(title, id);
                    if (ASC.Files.Utils.itIsDocument(title)) {
                        link.attr("href", url).attr("target", "_blank");
                    } else {
                        link.attr("href", url);
                    }
                }
                else {
                    link.attr("href", ASC.Files.Utils.fileDownloadUrl(id));
                }
            }
            else if (path.search("folder_") == 0) {
                var id = path.replace("folder_", "");
                jq(this).children(".fav_folder").attr("href", "#" + id);
            }
        });

        jq("div.fileRow").each(function() {
            ASC.Files.Favorites.setIfFavorites(jq(this));
        });
    };

    var onSaveFavorite = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFavorite);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#files_favoritesBody").append(htmlXML);

        jq("#files_favoritesEmpty").hide();
        jq("#files_favoritesBody").show();

        jq("#files_favoritesBody .favoritesRow:last").each(function() {
            var path = jq(this).children(".fav_list").val();

            if (path.search("file_") == 0) {
                title = jq(this).children(".fav_file").html();
                var id = path.replace("file_", "");
                var link = jq(this).children('.fav_file')
                if (ASC.Files.Utils.FileCanBePreview(title)) {
                    var url = ASC.Files.Utils.getPreviewUrl(title, id);
                    if (ASC.Files.Utils.itIsDocument(title)) {
                        link.attr("href", url).attr("target", "_blank");
                    } else {
                        link.attr("href", url);
                    }
                }
                else {
                    link.attr("href", ASC.Files.Utils.fileDownloadUrl(id));
                }
            }
            else if (path.search("folder_") == 0) {
                var id = path.replace("folder_", "");
                jq(this).children(".fav_folder").attr("href", "#" + id);
            }
        });

        var id = jq("#files_favoritesBody .favoritesRow:last").attr("id").replace('favoritesRow_', "");

        jq("#content_" + params.path + " a.file_fav_notyet").removeClass("file_fav_notyet")
                                                            .attr("alt", ASC.Files.Resources.RemoveFavorites)
                                                            .attr("title", ASC.Files.Resources.RemoveFavorites)
                                                            .removeClass("file_fav_action")
                                                            .addClass("file_fav_already")
                                                            .removeAttr("onclick")
                                                            .unbind("click")
                                                            .click(function(event) { ASC.Files.Favorites.showRemoveFavorite(this, event, id); });

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoAddFavorite, params.title));

    };

    var onRemoveFavorite = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }
        var title = jq("#favoritesRow_" + params.favoriteID + " a").attr("title");
        jq("#favoritesRow_" + params.favoriteID).remove();

        if (jq("#files_favoritesBody").html() != null && jq("#files_favoritesBody").html().length == 0) {
            jq("#files_favoritesBody").hide();
            jq("#files_favoritesEmpty").show();
        }

        jq("#content_" + params.path + " a.file_fav_already").removeClass("file_fav_already").addClass("file_fav_notyet")
            .attr("alt", ASC.Files.Resources.AddToFavorites).attr("title", ASC.Files.Resources.AddToFavorites)
            .unbind("click").click(function(event) { ASC.Files.Favorites.showToFavorite(this, event); });

        ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.Resources.InfoRemoveFavorites, title));
    };

    return {
        init: init,

        setIfFavorites: setIfFavorites,
        favoriteId: favoriteId,
        getFavorite: getFavorite,
        displayRemoveBtn: displayRemoveBtn,

        showFavorite: showFavorite,
        showToFavorite: showToFavorite,
        showRemoveFavorite: showRemoveFavorite,

        saveFavorite: saveFavorite,
        removeFavorite: removeFavorite
    };
})(jQuery);

(function($) {
    ASC.Files.Favorites.init();

    $(function() {

        ASC.Files.Favorites.showFavorite();

        jq("#favorites_addcurrent").click(function(event) {
            ASC.Files.Favorites.showToFavorite(this, event, true);
            return false;
        });

    });
})(jQuery);