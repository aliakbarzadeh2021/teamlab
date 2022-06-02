window.ASC.Files.Share = (function($) {
    var isInit = false;
    var listItems = new Array();

    var init = function() {
        if (isInit === false) {
            isInit = true;

            //serviceManager.bind(ASC.Files.TemplateManager.events.GetNewItems, onGetNewItems);

            serviceManager.bind(ASC.Files.TemplateManager.events.GetSharedInfo, onGetSharedInfo);
            serviceManager.bind(ASC.Files.TemplateManager.events.SetAceObject, onSetAceObject);
            serviceManager.bind(ASC.Files.TemplateManager.events.UnSubscribeMe, onUnSubscribeMe);

            //            serviceManager.bind(ASC.Files.TemplateManager.events.GetGroups, onGetGroups);
            //            serviceManager.bind(ASC.Files.TemplateManager.events.GetUsersByGroup, onGetUsersByGroup);
            //            serviceManager.bind(ASC.Files.TemplateManager.events.SaveGroup, onSaveGroup);
            //            serviceManager.bind(ASC.Files.TemplateManager.events.DeleteGroup, onDeleteGroup);

        }
    };

    //    var displayCountNew = function(xmlData) {
    //        jq(".new_inshare").hide();

    //        xmlData = xmlData.childNodes;
    //        for (var i = 0; i < xmlData.length; i++) {
    //            var key = xmlData[i].childNodes[0].textContent || xmlData[i].childNodes[0].text;
    //            var value = xmlData[i].childNodes[1].textContent || xmlData[i].childNodes[1].text;
    //            if (value != 0) {
    //                jq("#newinshare_" + key).text(
    //                                            jq.format(ASC.Files.Resources.NewInShare, value))
    //                                        .css("display", "block");
    //            }
    //        }
    //    };

    var clickShareButton = function(objId) {

        if (typeof PremiumStubManager != "undefined") {
            if (ASC.Files.Folders) {
                switch (ASC.Files.Folders.folderContainer) {
                    case "my":
                        PremiumStubManager.SwitchMessageType(PremiumStubManager.FeatureType.SharedDocs);
                        break;
                    case "corporate":
                        PremiumStubManager.SwitchMessageType(PremiumStubManager.FeatureType.AccessRigths);
                        break;

                    default:
                        PremiumStubManager.SwitchMessageType(PremiumStubManager.FeatureType.AccessRigths);
                        break;
                }
            } else {
                PremiumStubManager.SwitchMessageType(PremiumStubManager.FeatureType.AccessRigths);
            }
            PremiumStubManager.ShowDialog();
            return;
        }

        if (ASC.Files.Folders) {
            if (ASC.Files.Folders.folderContainer != "my"
                && (ASC.Files.Folders.folderContainer != "corporate" || !ASC.Files.Constants.USER_ADMIN))
                return;
        }

        getSharedInfo(objId);
    };

    var paintRows = function(body) {
        jq("#" + body + " li.share_row").css("border-top", "");
        jq("#" + body + " li.share_row:not(.hidden):first").css("border-top", "1px solid #D1D1D1");
        jq("#" + body + " li.share_row.even").removeClass("even");
        jq("#" + body + " li.share_row:not(.hidden):even").addClass("even");
    };

    var selectAction = function(id, aceStatus) {
        switch (aceStatus) {
            case "owner":
                jq("#action_" + id).text(ASC.Files.Resources.AceStatusEnum_Owner);
                break;
            case ASC.Files.Constants.AceStatusEnum.Read:
                jq("#action_" + id).text(ASC.Files.Resources.AceStatusEnum_Read);
                break;
            case ASC.Files.Constants.AceStatusEnum.ReadWrite:
                jq("#action_" + id).text(ASC.Files.Resources.AceStatusEnum_ReadWrite);
                break;
            case ASC.Files.Constants.AceStatusEnum.Restrict:
                jq("#action_" + id).text(ASC.Files.Resources.AceStatusEnum_Restrict);
                break;
            case ASC.Files.Constants.AceStatusEnum.None:
            default:
                jq("#action_" + id).text(ASC.Files.Resources.SelectRight);

                //for merge ace
                //jq("#action_" + id).text(ASC.Files.Resources.AceStatusEnum_DiffRoles);

                break;
        }
        jq("#selected_action_" + id).val(aceStatus);

        jq("#share_main div.action_active").addClass("action_noactive").removeClass("action_active");
        jq("#files_rightPopup").hide();
    };

    var showAccessPanel = function() {
        ASC.Files.Share.initViewSwitcher(1);

        viewSwitcherToggleTabs("UserAccess_ViewSwitcherTab");

        ASC.Files.Common.blockUI(jq("#files_shareAccessDialogContainer"), 550, 500);
    };

    var initViewSwitcher = function(item) {
        if (item == 1) {
            shareUserSelector.DisplayAll();

            shareUserSelector.HideUser(ASC.Files.Constants.USER_ID);

            jq("#shareUserSelector #inputUserName").val("");

            jq("#files_usersAccessBody li.share_row:not(.hidden)").each(function() {
                shareUserSelector.HideUser(this.id);
            });

            jq("#shareUserSelector_add").unbind("click").click(function() {
                ASC.Files.Share.addUserToShare();
            });

            jq("#share_btnManage").hide();
        }
        else {
            jq("#share_btnManage").show();
        }

        backToPanel();
    };

    var backToPanel = function() {
        jq("#files_shareAccess_save").unbind("click").click(function() {
            ASC.Files.Share.setAceObject();
        });

        jq("#files_shareAccess_cancel").unbind("click").click(function() {
            PopupKeyUpActionProvider.CloseDialog();
        });

        jq("#share_main").show();

        jq("#share_manage").hide();
    }

    var newGroup = function() {
        shareUserSelector.DisplayAll();
        jq("#usersInGroup").empty();
        showShareManagerDialog();
    };

    var showShareManagerDialog = function(idGroup) {

        if (idGroup == undefined) {
            jq("#files_groupName").val("");
        }
        else {
            jq("#files_groupName").val(jq("#" + idGroup + " div.share_name").text());
        }

        jq("#files_shareAccess_save").unbind("click").click(function() {
            ASC.Files.Share.saveGroup(idGroup);
        });

        jq("#files_shareAccess_cancel").unbind("click").click(function() {
            ASC.Files.Share.initViewSwitcher(2);
        });

        jq("#shareUserSelector_add").unbind("click").click(function() {
            var uId = shareUserSelector.SelectedUserId;
            if (uId != null) {
                ASC.Files.Share.addUserToGroup(uId);
                jq("#shareUserSelector #inputUserName").val("");
                jq("#usersInGroup").scrollTo("#userLink_" + uId);
                jq("#usersInGroup #userLink_" + uId).css({ backgroundColor: "#ffffcc" })
                                                     .animate({ backgroundColor: "#ffffff" }, 2000, function() {
                                                         jq("#usersInGroup .share_row:even").css("backgroundColor", "").addClass("even");
                                                     });
                jq("#usersInGroup .share_row:first").css('border-top', '1px solid #D1D1D1');
            }
            return false;
        });

        //jq("#shareSelectUserPanel").show();
        jq("#share_btnManage").hide();
        jq("#share_main").hide();

        jq("#share_manage").show();
    };

    //    var showDeleteGroup = function(obj, event, groupId) {

    //        ASC.Files.Share.handlePopup(event);

    //        var title = jq("#files_groupsAccessBody li#" + groupId + " div.share_name").text();
    //        jq("#groupRemoveText").html(title);

    //        ASC.Files.Common.dropdownToggle(jq(obj), "group_remove_dialog");
    //        jq("#group_remove_dialog div.cancelButton").attr("onclick", "").unbind("click").click(ASC.Files.Actions.hideAllActionPanels);
    //        PopupKeyUpActionProvider.CloseDialogAction = "ASC.Files.Actions.hideAllActionPanels();";
    //        PopupKeyUpActionProvider.EnableEsc = false;

    //        jq("#groupRemoveBtn").unbind("click").click(function() {
    //            ASC.Files.Share.deleteGroup(groupId);
    //            ASC.Files.Actions.hideAllActionPanels();
    //        });

    //        jq("body").click(function(event) {
    //            if (!jq((event.target) ? event.target : event.srcElement).is("#group_remove_dialog, div.share_trash, #" + obj.id))
    //                ASC.Files.Actions.hideAllActionPanels();
    //        });
    //    };

    var addUserToGroup = function(UserId) {

        if (jq("#usersInGroup #userLink_" + UserId).length != 0) return;

        var curUser;
        for (var i = 0; i < shareUserSelector.Groups.length && !curUser; i++) {
            for (var j = 0; j < shareUserSelector.Groups[i].Users.length && !curUser; j++) {
                if (shareUserSelector.Groups[i].Users[j].ID == UserId)
                    curUser = shareUserSelector.Groups[i].Users[j];
            }
        }

        if (curUser == undefined)
            return;

        shareUserSelector.HideUser(UserId);

        var str = jq.format('\
            <li id="userLink_{0}" class="clearFix share_row">\
                <div class="user">\
	                <span class="userLink">{1}</span>\
                </div>\
                <div class="user_remove" onclick="ASC.Files.Share.delUserInGroup(\'{0}\');">\
                </div>\
            </li>\
            ', curUser.ID, curUser.Name);

        jq("#usersInGroup").html(jq("#usersInGroup").html() + str);
    };

    var delUserInGroup = function(uId) {
        shareUserSelector.HideUser(uId, false);
        jq("#usersInGroup #userLink_" + uId).remove();
        paintRows("usersInGroup");
    };

    var addUserToShare = function() {
        var UserId = shareUserSelector.SelectedUserId;

        if (jq("#files_usersAccessBody #" + UserId).length != 0) {
            jq("#files_usersAccessBody #" + UserId).show().removeClass("hidden");
            jq("#files_usersAccessBody").scrollTo(jq("#" + UserId));
        }
        else {
            var curUser;
            for (var i = 0; i < shareUserSelector.Groups.length && !curUser; i++) {
                for (var j = 0; j < shareUserSelector.Groups[i].Users.length && !curUser; j++) {
                    if (shareUserSelector.Groups[i].Users[j].ID == UserId)
                        curUser = shareUserSelector.Groups[i].Users[j];
                }
            }

            if (curUser == undefined)
                return;

            UserId = curUser.ID;
            var entry = {
                id: UserId,
                title: curUser.Name,
                is_group: false,
                owner: false,
                ace_status: ASC.Files.Constants.AceStatusEnum.Read
            };
            var stringData = serviceManager.jsonToXml({ ace_wrapperList: { entry: entry} });

            var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getSharedInfo);
            if (typeof xslData === "undefined") { return undefined; }
            var htmlXML = ASC.Controls.XSLTManager.translateFromString(stringData, xslData);

            jq("#files_usersAccessBody").append(htmlXML).scrollTo(jq("#files_usersAccessBody li.share_row:last"));
            jq("#share_groups_data").remove();
        }

        selectAction(UserId, ASC.Files.Constants.AceStatusEnum.Read);
        jq("#change_action_" + UserId).val(true);

        paintRows("files_usersAccessBody");

        shareUserSelector.HideUser(UserId);

        jq("#shareUserSelector #inputUserName").val("");
    };

    var delUserInShare = function(uId) {
        shareUserSelector.HideUser(uId, false);
        jq("#change_action_" + uId).val(true);
        ASC.Files.Share.selectAction(uId, ASC.Files.Constants.AceStatusEnum.None);
        jq("#files_usersAccessBody #" + uId).hide().addClass("hidden");
        paintRows("files_usersAccessBody");
    };

    var addGroupToShare = function() {
        var GroupId = jq("#selectGroup").val();
        if (GroupId == "emptyGroup")
            return;

        jq("#files_groupsAccessBody #" + GroupId).show().removeClass("hidden");
        jq("#GroupAccess .shareEmptyGroup").hide();
        jq("#files_groupsAccessBody").show().scrollTo(jq("#" + GroupId));

        selectAction(GroupId, ASC.Files.Constants.AceStatusEnum.Read);
        jq("#change_action_" + GroupId).val(true);

        paintRows("files_groupsAccessBody");

        jq("#sg_" + GroupId).hide().addClass("hidden").attr("disabled", "disabled");

        jq("#selectGroup option:first").attr('selected', 'selected');
    };

    var delGroupInShare = function(groupId) {
        jq("#sg_" + groupId).show().removeClass("hidden").attr("disabled", "").attr("selected", "selected");

        jq("#change_action_" + groupId).val(true);
        ASC.Files.Share.selectAction(groupId, ASC.Files.Constants.AceStatusEnum.None);

        jq("#files_groupsAccessBody #" + groupId).hide().addClass("hidden");

        if (jq("#files_groupsAccessBody li:visible").length == 0) {
            jq("#files_groupsAccessBody").hide();
            jq("#GroupAccess .shareEmptyGroup").show();
        }
        else {
            paintRows("files_groupsAccessBody");
        }
    };

    var handlePopup = function(event) {
        jq("#files_rightPopup").hide();
        jq("div.action_active").addClass("action_noactive").removeClass("action_active");
    };

    var showRightsPopup = function(id) {
        if (jq("#" + id).hasClass("locked"))
            return;

        jq("div.action_active").addClass("action_noactive").removeClass("action_active");
        jq("#action_" + id).parent().addClass("action_active").removeClass("action_noactive");

        var divShare = jq("#" + id + " div.share_rights");
        var targetPos = divShare.offset();
        var dialogPos = jq("#files_shareAccessDialogContainer").offset();
        var dropdownItem = jq("#files_rightPopup");

        var elemPosTop = targetPos.top - dialogPos.top + divShare.outerHeight();
        var elemPosLeft = targetPos.left - dialogPos.left;

        //        if (ASC.Controls.Constants.isMobileAgent) {
        //            elemPosTop += w.scrollTop();
        //        }

        dropdownItem.css(
               {
                   'position': 'absolute',
                   'top': elemPosTop - 1,
                   'left': elemPosLeft,
                   'visibility': ""
               }
            );
        dropdownItem.show();

        dropdownItem.css("left", elemPosLeft + divShare.outerWidth() - dropdownItem.outerWidth());

        jq("#files_read").unbind("click").click(function() { jq("#change_action_" + id).val(true); ASC.Files.Share.selectAction(id, ASC.Files.Constants.AceStatusEnum.Read); });
        jq("#files_readwrite").unbind("click").click(function() { jq("#change_action_" + id).val(true); ASC.Files.Share.selectAction(id, ASC.Files.Constants.AceStatusEnum.ReadWrite); });
        jq("#files_restrict").unbind("click").click(function() { jq("#change_action_" + id).val(true); ASC.Files.Share.selectAction(id, ASC.Files.Constants.AceStatusEnum.Restrict); });

        jq("body").click(function(event) {
            if (!jq((event.target) ? event.target : event.srcElement).parents().andSelf().is("#files_rightPopup, div.action_active")) {
                dropdownItem.hide();
                jq("div.action_active").addClass("action_noactive").removeClass("action_active");
            }
        });

    };

    //request

    //    var getNewItems = function(folderId) {
    //        serviceManager.request("get",
    //                                "xml",
    //                                ASC.Files.TemplateManager.events.GetNewItems,
    //                                { showLoading: true, folderId: folderId },
    //                                "newinshare"
    //                                , folderId);
    //    };

    //    var getGroups = function() {
    //        serviceManager.request("get",
    //                                "xml",
    //                                ASC.Files.TemplateManager.events.GetGroups,
    //                                { showLoading: true },
    //                                "groups");
    //    };

    //    var saveGroup = function(idGroup) {

    //        idGroup = idGroup || "0";
    //        var name = jq("#files_groupName").val();

    //        if (name == "") {
    //            jq("#files_groupName").yellowFade().focus();
    //            return;
    //        }

    //        var listUsersId = new Array();
    //        jq("#usersInGroup li.share_row").each(function() {
    //            listUsersId.push(this.id.replace("userLink_", ""));
    //        });
    //        var data = { entry: listUsersId };

    //        serviceManager.request("post",
    //                                "xml",
    //                                ASC.Files.TemplateManager.events.SaveGroup,
    //                                { id: idGroup },
    //                                { guidList: data },
    //                                'groups',
    //                                idGroup + '?name=' + encodeURIComponent(name));

    //    };

    //    var getUsersByGroup = function(idGroup) {
    //        serviceManager.request("get",
    //                                "json",
    //                                ASC.Files.TemplateManager.events.GetUsersByGroup,
    //                                { id: idGroup },
    //                                "groups",
    //                                idGroup,
    //                                "users");
    //    };

    //    var deleteGroup = function(idGroup) {
    //        serviceManager.request("delete",
    //                                "xml",
    //                                ASC.Files.TemplateManager.events.DeleteGroup,
    //                                { id: idGroup },
    //                                'groups',
    //                                idGroup);
    //    };

    var getSharedInfo = function(id) {
        var data = {};
        data.entry = new Array();

        if (id != undefined) {
            data.entry.push(id.replace("combo_", ""));
        }
        else {
            var list = jq("#files_mainContent input:checked");
            //            if (list.length == 0) {
            //                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoEmptyForShare, true);
            //                return;
            //            }

            if (list.length != 1) {
                ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoSelectForShare, true);
                return;
            }

            list.each(function() {
                var itemId = this.id.replace("check_", "");

                if (ASC.Files.UI.accessAdmin(itemId)) {
                    //NOTE: share only 1 element
                    if (data.entry.length < 1) {
                        ASC.Files.UI.checkSelectAll(false);
                        jq(this).click();

                        data.entry.push(this.id.replace("check_", ""));
                    }
                }
            });
        }

        if (data.entry.length == 0)
            return;

        listItems = data;

        serviceManager.request("post",
                                "xml",
                                ASC.Files.TemplateManager.events.GetSharedInfo,
                                { showLoading: true },
                                { stringList: data },
                                'sharedinfo');
    };

    var setAceObject = function() {
        var ace_wrapperList = "";
        var showIcn = false;

        jq("#share_main li.share_row input[id*='selected_action_']").each(function() {
            if (jq("#change_action_" + this.id.replace("selected_action_", "")).val() == 'true') {
                if (this.value != "" && this.value != null && this.value != "owner") {
                    var json =
                        {
                            id: this.id.replace("selected_action_", ""),
                            is_group: jq(this).parents().is("#files_groupsAccessBody"),
                            ace_status: this.value,
                            object_ids: listItems
                        };
                    ace_wrapperList += serviceManager.jsonToXml({ entry: json });
                    if (this.value == ASC.Files.Constants.AceStatusEnum.ReadWrite
                        || this.value == ASC.Files.Constants.AceStatusEnum.Read) {
                        showIcn = true;
                    }
                }
            }
        });
        if (ace_wrapperList == "") {
            PopupKeyUpActionProvider.CloseDialog();
            return;
        }

        var hideIcn = false;
        if (!showIcn) {
            if (jq("#share_main li.share_row input[id*='selected_action_']"
                + "[value!='owner']"
                + "[value!='" + ASC.Files.Constants.AceStatusEnum.None + "']"
                + "[value!='" + ASC.Files.Constants.AceStatusEnum.Restrict + "']").length == 0)
                hideIcn = true;
        }

        var data = serviceManager.jsonToXml({ ace_wrapperList: ace_wrapperList });

        serviceManager.request("post",
                                "xml",
                                ASC.Files.TemplateManager.events.SetAceObject,
                                { showLoading: true, showIcn: showIcn, hideIcn: hideIcn },
                                data,
                                'setaceobject');
    };

    var unSubscribeMe = function(objId) {

        serviceManager.request("get",
                                "xml",
                                ASC.Files.TemplateManager.events.UnSubscribeMe,
                                { showLoading: true, objId: objId },
                                "removeace",
                                objId);
    };

    //event handler

    //    var onGetNewItems = function(xmlData, params, errorMessage, commentMessage) {
    //        if (typeof errorMessage != "undefined") {
    //            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
    //            return undefined;
    //        }

    //        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getNewInShare);
    //        if (typeof xslData === "undefined") { return undefined; }
    //        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

    //        jq("#newInShareBody ul.change_table").html(htmlXML);
    //        jq("#newInShareBody ul.change_table div.name a").each(function() {
    //            var id = jq(this).parents("li").attr("id").replace("changed_file_", "");
    //            jq(this).attr("href", ASC.Files.Utils.fileDownloadUrl(id));
    //        });

    //        var countChange = xmlData.childNodes[0].childElementCount || xmlData.documentElement.childNodes.length

    //        var folderTitle = "";
    //        switch (params.folderId) {
    //            case ASC.Files.Constants.FOLDER_ID_COMMON_FILES:
    //                folderTitle = ASC.Files.Resources.CorporateFiles;
    //                break;
    //            case ASC.Files.Constants.FOLDER_ID_MY_FILES:
    //                folderTitle = ASC.Files.Resources.MyFiles;
    //                break;
    //        }

    //        jq("#newInShareDialogHeader").text(jq.format(ASC.Files.Resources.ChangedItemHeader,
    //                                            countChange,
    //                                            folderTitle));

    //        ASC.Files.Common.blockUI(jq("#files_newInShare"), 700, 460);
    //    };

    //    var onGetGroups = function(xmlData, params, errorMessage, commentMessage) {
    //        if (typeof errorMessage != "undefined") {
    //            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
    //            return undefined;
    //        }

    //        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getGroups);
    //        if (typeof xslData === "undefined") { return undefined; }
    //        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

    //        jq("#files_groupsAccessBody").html(htmlXML);
    //        paintRows("files_groupsAccessBody");

    //        if (!ASC.Files.Constants.USER_ADMIN) {
    //            jq("#share_btnManage, #files_groupsAccessBody div.share_actions *").remove();
    //        }
    //    };

    //    var onSaveGroup = function(xmlData, params, errorMessage, commentMessage) {
    //        if (typeof errorMessage != "undefined") {
    //            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
    //            return undefined;
    //        }

    //        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getGroups);
    //        if (typeof xslData === "undefined") { return undefined; }
    //        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

    //        var newElem;
    //        if (params.id == 0) {
    //            jq("#files_groupsAccessBody").append(htmlXML);
    //            newElem = jq("#files_groupsAccessBody li.share_row:last")
    //        } else {
    //            jq("#files_groupsAccessBody li.share_row#" + params.id).replaceWith(htmlXML);
    //            newElem = jq("#files_groupsAccessBody li.share_row#" + params.id)
    //        }

    //        paintRows("files_groupsAccessBody");

    //        ASC.Files.Share.initViewSwitcher(2);

    //        selectAction(newElem.attr("id"), ASC.Files.Constants.AceStatusEnum.Restrict);

    //        jq("#files_groupsAccessBody").scrollTo(newElem);
    //    };

    //    var onDeleteGroup = function(xmlData, params, errorMessage, commentMessage) {
    //        if (typeof errorMessage != "undefined") {
    //            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
    //            return undefined;
    //        }

    //        jq("#" + params.id).remove();
    //        paintRows("files_groupsAccessBody");
    //    };

    //    var onGetUsersByGroup = function(jsonData, params, errorMessage, commentMessage) {
    //        if (typeof errorMessage != "undefined") {
    //            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
    //            return undefined;
    //        }
    //        shareUserSelector.DisplayAll();
    //        jq("#usersInGroup").empty();

    //        if (jsonData != undefined)
    //            jq(jsonData).each(function() {
    //                ASC.Files.Share.addUserToGroup(this);
    //            });

    //        paintRows("usersInGroup");

    //        showShareManagerDialog(params.id);
    //    };

    var onGetSharedInfo = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        var xslData = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getSharedInfo);
        if (typeof xslData === "undefined") { return undefined; }
        var htmlXML = ASC.Controls.XSLTManager.translate(xmlData, xslData);

        jq("#files_usersAccessBody").html(htmlXML);

        paintRows("files_usersAccessBody");

        jq("#files_usersAccessBody li.share_row").each(function(i) {
            selectAction(this.id, jq("#selected_action_" + this.id).val());
        });

        if (jq("#files_groupsAccessBody li.share_row").length == 0) {
            var group = new Array();

            jq("#selectGroup option:not([value='emptyGroup'])").each(function() {
                group.push(serviceManager.jsonToXml(
                        {
                            id: this.id.replace("sg_", ""),
                            name: jq(this).html(),
                            ace_status: ASC.Files.Constants.AceStatusEnum.Restrict
                        }));
            });

            var stringData = serviceManager.jsonToXml({ group_infoList: { entry: group} });

            var xslDataGr = ASC.Files.TemplateManager.getTemplate(ASC.Files.TemplateManager.templates.getGroups);
            if (typeof xslDataGr === "undefined") { return undefined; }
            var htmlXMLGr = ASC.Controls.XSLTManager.translateFromString(stringData, xslDataGr);

            jq("#files_groupsAccessBody").html(htmlXMLGr);
        }

        jq("#files_groupsAccessBody").hide();
        jq("#GroupAccess .shareEmptyGroup").show();

        jq("#files_groupsAccessBody li.share_row").each(function(i) {
            selectAction(this.id, ASC.Files.Constants.AceStatusEnum.Restrict);
            jq(this).hide().addClass("hidden");
            jq(this).removeClass("locked");
        });

        jq("#selectGroup option").show().removeClass("hidden").attr("disabled", "");

        jq("#share_groups_data input[type='hidden']").each(function() {
            var idGr = this.id.replace("group_right_", "");

            jq("#" + idGr).show().removeClass("hidden");
            selectAction(idGr, this.value);

            if (jq(this).attr("value2") == "locked")
                jq("#" + idGr).addClass("locked");

            jq("#files_groupsAccessBody").show();
            jq("#GroupAccess .shareEmptyGroup").hide();

            jq("#sg_" + idGr).hide().addClass("hidden");
        });

        jq("#selectGroup option[value='emptyGroup']").attr('selected', 'selected');

        paintRows("files_groupsAccessBody");

        jq("#share_groups_data").remove();

        jq("#share_main input[id*='change_action_'][value='true']").each(function() {
            jq(this).val(false);
        });

        showAccessPanel();
    };

    var onSetAceObject = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            PopupKeyUpActionProvider.CloseDialog();
            return undefined;
        }

        if (params.showIcn == true) {
            for (var i = 0; i < listItems.entry.length; i++) {
                jq("#share_icn_" + listItems.entry[i]).css("display", "block");
            }
        }
        else {
            if (params.hideIcn == true) {
                for (var i = 0; i < listItems.entry.length; i++) {
                    jq("#share_icn_" + listItems.entry[i]).hide();
                }
            }
        }

        PopupKeyUpActionProvider.CloseDialog();
    };

    var onUnSubscribeMe = function(xmlData, params, errorMessage, commentMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            return undefined;
        }

        jq("#content_" + params.objId).remove();

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
    };

    return {
        init: init,

        //getNewItems: getNewItems,
        //displayCountNew: displayCountNew,

        handlePopup: handlePopup,
        clickShareButton: clickShareButton,
        getSharedInfo: getSharedInfo,
        initViewSwitcher: initViewSwitcher,
        selectAction: selectAction,

        addUserToGroup: addUserToGroup,
        delUserInGroup: delUserInGroup,

        addUserToShare: addUserToShare,
        delUserInShare: delUserInShare,

        setAceObject: setAceObject,
        unSubscribeMe: unSubscribeMe,

        //        newGroup: newGroup,
        //        getGroups: getGroups,
        //        saveGroup: saveGroup,
        //        deleteGroup: deleteGroup,
        //        getUsersByGroup: getUsersByGroup,
        //        showDeleteGroup: showDeleteGroup,

        addGroupToShare: addGroupToShare,
        delGroupInShare: delGroupInShare,

        showRightsPopup: showRightsPopup

    };
})(jQuery);
  
(function($) {
    ASC.Files.Share.init();
    $(function() {
        //ASC.Files.Share.getGroups();

        jq("#files_share_btn").click(function() {
            ASC.Files.Share.clickShareButton();
        });

        jq("#share_btnManage").click(function() {
            ASC.Files.Share.newGroup();
        });

        jq("#files_groupsAccessBody").scroll(function(event) {
            ASC.Files.Share.handlePopup(event);
            ASC.Files.Actions.hideAllActionPanels();
        });

        //        jq(".new_inshare").click(function(event) {
        //            ASC.Files.Share.getNewItems(this.id.replace("newinshare_", ""));
        //            ASC.Files.Common.cancelBubble(event);
        //            return false;
        //        });

        jq("#shareGroupSelector_add").click(function() {
            ASC.Files.Share.addGroupToShare();
        });

        if (ASC.Controls.Constants.isMobileAgent) {
            jq("#files_usersAccessBody, #files_groupsAccessBody").css(
                {

                    "min-height": "300px",
                    "height": "auto"
                });
        }

    });
})(jQuery);