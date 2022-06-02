window.ASC.Files.Tree = (function($) {
    var isInit = false;
    var rootContainer = "files_trewViewContainer";
    var rootSContainer = "files_trewViewSelector";

    var init = function() {
        if (isInit === false) {
            isInit = true;

            serviceManager.bind(ASC.Files.TemplateManager.events.GetTreeSubFolders, onGetTreeSubFolders);
        }
    };

    var showTreeViewPanel = function() {
        ASC.Files.Tree.showTreePath();
        ASC.Files.Common.dropdownToggle(jq("#files_showTreeView"), "files_treeViewPanel");
    };

    var showMoveToSelector = function(obj_id, isCopy) {
        if (ASC.Files.Import)
            ASC.Files.Import.isImport = false;
        ASC.Files.Folders.isCopyTo = (isCopy === true);

        ASC.Files.Tree.showTreePath();
        ASC.Files.Tree.showSelect("stree_selector_" + ASC.Files.Folders.currentFolderId);

        ASC.Files.Actions.hideAllActionPanels();

        if (obj_id == -1) {
            var list = jq("#files_mainContent input:checked");

            if (list.length == 0)
                return undefined;
            ASC.Files.Folders.multiMoveTo = true;
            ASC.Files.Common.dropdownToggle(jq("#files_actions_open").parent().parent(), 'files_treeViewPanelSelector');
        }
        else {
            ASC.Files.Folders.curItemMoveTo = obj_id;
            ASC.Files.Folders.multiMoveTo = false;
            ASC.Files.Common.dropdownToggle(jq(jq("#" + obj_id)[0]).parent().parent(), 'files_treeViewPanelSelector');
        }

        jq("body").click(function(event) {
            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf()
                .is("#files_treeViewPanelSelector, #files_moveto_folders, #files_moveto_files, #files_copyto_folders, #files_copyto_files,\
                 #files_restore_folders, #files_restore_files, #files_movetoButton, #files_copytoButton, #files_restoreButton"))
                ASC.Files.Actions.hideAllActionPanels();
        });
    }

    var showTreePath = function() {
        var data = ASC.Files.Folders.pathParts;

        if (data == null || data == undefined)
            return;

        if (jq("#tree_node_" + data[0].Key).length != 0) {
            for (i = 0; i < data.length; i++) {
                var id = data[i].Key;

                if (jq("#tree_node_" + id + " ul").html() == null || jq("#tree_node_" + id + " ul").html() == "") {
                    ASC.Files.Tree.getTreeSubFolders(id);
                } else {
                    jq(jq("#tree_node_" + id + " ins")[0]).parent().addClass("jstree-open").removeClass("jstree-closed");
                    jq(jq("#stree_node_" + id + " ins")[0]).parent().addClass("jstree-open").removeClass("jstree-closed");
                }
            }
        }
        ASC.Files.Tree.select("#tree_selector_" + ASC.Files.Folders.currentFolderId);
        ASC.Files.Tree.showSelect("stree_selector_" + ASC.Files.Folders.currentFolderId);
    };

    var renderTreeView = function(idContainers, xmlData, xslData) {
        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getFoldersTree);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#tree_node_" + idContainers).addClass("jstree-open").removeClass("jstree-closed");
        jq("#stree_node_" + idContainers).addClass("jstree-open").removeClass("jstree-closed");

        if (htmlXML != "") {
            jq("#tree_node_" + idContainers).append("<ul>" + htmlXML + "</ul>");
            jq("#stree_node_" + idContainers).append("<ul>" + htmlXML.replace(/tree_/g, 'stree_') + "</ul>");
        }
    };

    var moveNode = function(idFolder, idTo) {
        if (jq("#tree_node_" + idTo).children().length == 3) {
            jq(jq("#tree_node_" + idTo).children()[2]).append(jq("#tree_node_" + idFolder));
        }
        else {
            jq("#tree_node_" + idFolder).remove();
        }

        if (jq("#stree_node_" + idTo).children().length == 3) {
            jq(jq("#stree_node_" + idTo).children()[2]).append(jq("#stree_node_" + idFolder));
        }
        else {
            jq("#stree_node_" + idFolder).remove();
        }
    };

    var select = function(obj) {
        jq("#" + rootContainer + " a").removeClass("selected");
        jq("#" + rootSContainer + " a").removeClass("selected");

        jq(obj).addClass("selected");
        if (obj.id != undefined)
            jq("#files_trewViewContainer li.jstree-closed").each(function() {
                if (jq("#" + obj.id.replace("tree_selector", "tree_node"), this).html() != null) {
                    jq(this).addClass("jstree-open").removeClass("jstree-closed")
                }
            });
    };

    var showSelect = function(id) {
        jq("#" + id).addClass("selected");
        if (id != undefined)
            jq("#files_trewViewSelector li.jstree-closed").each(function() {
                if (jq("#" + id.replace("stree_selector", "stree_node"), this).html() != null) {
                    jq(this).addClass("jstree-open").removeClass("jstree-closed")
                }
            });
    };

    var selectS = function(obj) {
        var id = obj.id.replace('stree_selector_', "");

        if (ASC.Files.Import != undefined && ASC.Files.Import.isImport == true) {
            ASC.Files.Import.setFolderImportTo(id, jq(obj).text());
        }
        else {
            var titleDf = jq("#stree_selector_" + id).html();
            titleDf = titleDf.split('</ins>')[1] || titleDf.split('</INS>')[1];

            ASC.Files.Folders.curItemFolderMoveTo(id, titleDf);
        }
    };

    var expand = function(obj) {
        var parent = jq(obj).parent();

        if (parent.children().length == 3) {
            parent.toggleClass("jstree-closed").toggleClass("jstree-open");
        }
        else {
            var id = jq(parent.children()[1]).attr("id").replace(/(s*)tree_selector_/, "");
            ASC.Files.Tree.getTreeSubFolders(id);
        }
    };

    var initEvents = function(idContainer) {
        if (idContainer == null || idContainer == undefined || idContainer == "")
            return;

        jq("#" + idContainer + " ins[rel='expander']").each(function() { jq(this).unbind("click").click(function() { ASC.Files.Tree.expand(this); return false; }); });
        jq("#" + idContainer + " a").each(function() { jq(this).unbind("click").click(function() { ASC.Files.Tree.select(this); return false; }); });
    };

    var attachEvents = function(idContainer) {
        if (idContainer == null || idContainer == undefined || idContainer == "")
            return;

        jq("#" + idContainer + " ul ins[rel='expander']").each(function() { jq(this).unbind("click").click(function() { ASC.Files.Tree.expand(this); return false; }); });
        jq("#" + idContainer + " ul a").each(function() { jq(this).unbind("click").click(function() { ASC.Files.Tree.select(this); return false; }); });
        jq(jq("#" + idContainer).children()).show();
    };

    var initSEvents = function(idContainer) {
        if (idContainer == null || idContainer == undefined || idContainer == "")
            return;

        jq("#" + idContainer + " ins[rel='expander']").each(function() { jq(this).unbind("click").click(function() { ASC.Files.Tree.expand(this); }); });
        jq("#" + idContainer + " a").each(function() {
            if (ASC.Files.UI.accessibleItem(this.id.replace("stree_selector", "folder"))) {
                jq(this).removeAttr("onclick").unbind("click").click(function() { ASC.Files.Tree.selectS(this); return false; });
            } else {
                jq(this).removeAttr("onclick").unbind("click").click(function() {
                    ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.ErrorMassage_SecurityException, true);
                    ASC.Files.Tree.expand(this); return false;
                });
            }
        });
    };

    var attachSEvents = function(idContainer) {
        if (idContainer == null || idContainer == undefined || idContainer == "")
            return;

        jq("#" + idContainer + " ul ins[rel='expander']").each(function() { jq(this).unbind("click").click(function() { ASC.Files.Tree.expand(this); return false; }); });
        jq("#" + idContainer + " ul a").each(function() {
            jq(this).removeAttr("onclick").unbind("click").click(function() { ASC.Files.Tree.selectS(this); return false; });
        });

        jq(jq("#" + idContainer).children()).show();
    };

    //request

    var getTreeSubFolders = function(id) {
        jq("#tree_node_" + id + " ins[rel='expander'], #stree_node_" + id + " ins[rel='expander']").addClass('jstreeLoadNode');

        serviceManager.request("post",
                                "xml",
                                ASC.Files.TemplateManager.events.GetTreeSubFolders,
                                { id: id },
                                { orderBy: ASC.Files.Folders.getOrderByDateAndTime(false) },
                                'folders', id, 'subfolders?from=0&count=0');
    };

    //event handler

    var onGetTreeSubFolders = function(xmlData, params, errorMessage, commentMessage) {
        jq("#tree_node_" + params.id + " ins[rel='expander'],#stree_node_" + params.id + " ins[rel='expander']").removeClass('jstreeLoadNode');
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var id = params.id;
        if (jq("#tree_node_" + id + " ul").html() == null || jq("#tree_node_" + id + " ul").html() == "") {
            renderTreeView(id, xmlData);
            ASC.Files.Tree.attachEvents("tree_node_" + id);
            ASC.Files.Tree.attachSEvents("stree_node_" + id);
        }
        ASC.Files.Tree.select("#tree_selector_" + ASC.Files.Folders.currentFolderId);
        ASC.Files.Tree.showSelect("stree_selector_" + ASC.Files.Folders.currentFolderId);
    };

    return {
        init: init,
        select: select,
        selectS: selectS,
        expand: expand,
        initEvents: initEvents,
        initSEvents: initSEvents,
        attachSEvents: attachSEvents,
        attachEvents: attachEvents,
        moveNode: moveNode,

        showSelect: showSelect,
        showTreeViewPanel: showTreeViewPanel,
        showMoveToSelector: showMoveToSelector,

        showTreePath: showTreePath,
        getTreeSubFolders: getTreeSubFolders

    };

})(jQuery);

(function($) {
    ASC.Files.Tree.init();
    $(function() {

        ASC.Files.Tree.initEvents("files_trewViewContainer");
        ASC.Files.Tree.initSEvents("files_trewViewSelector");

        jq("#files_showTreeView").click(function() {
            ASC.Files.Tree.showTreeViewPanel();
        });

        jq("#files_movetoButton, #files_copytoButton, #files_restoreButton").click(function() {
            ASC.Files.Tree.showMoveToSelector(-1, (this.id == "files_copytoButton"));
        });

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("div.files_treeViewPanel, div.jstree").css({ "max-width": "none", "max-height": "none" });
        }

        if (jq("#files_treeViewPanelSelector").length == 0) {
            jq("#files_copyto_folders, #files_copyto_files, #files_copytoButton").remove();
        }

    });
})(jQuery);