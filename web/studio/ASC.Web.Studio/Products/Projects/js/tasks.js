ASC.Projects.TaskActionPage = (function() {
    return {
        isAllMyTasks: false,
        isOneList: false,
        init: function(taskID, selectedMilestone, selectedParticipant) {

            jq("#addTaskPanel a.baseLinkButton:first").unbind("click").bind("click", function() {
                ASC.Projects.TaskActionPage.submitForm(taskID);
            });

            jq("#[id$=hfTaskId]").val(taskID);

            this.refresh(taskID);

            if (selectedMilestone != null) {
                if (selectedMilestone > 0) this.changeMilestone(selectedMilestone); else this.changeMilestone(0);
                jq("#milestoneSelector_switcherContainer, #milestoneSelector_switcher_mobile").hide();
                jq("#milestoneSelector_switcher small").remove();
                jq("#milestoneSelector_selectedMilestone").show()
                    .html(jq("#milestoneSelector_switcher_mobile option:selected").text() || jq("#milestoneSelector_switcher").html());

            }
            else {
                jq("#milestoneSelector_switcherContainer, #milestoneSelector_switcher_mobile").show();
                jq("#milestoneSelector_selectedMilestone").hide();
            }

            if (selectedParticipant != null) {
                this.changeResponsible(selectedParticipant);
            }
        },

        submitForm: function(taskID) {

            var tbTitle = jq("#addTaskPanel input[id$=tbxTitle]");

            if (jq.trim(tbTitle.val()) == "") {

                alert(ASC.Projects.Resources.EmptyTaskTitle);

                return;

            }

            jq("#addTaskPanel input, select, textarea").attr("disabled", "disabled");
            jq("#addTaskPanel  .pm-action-block").hide();
            jq("#addTaskPanel  .pm-ajax-info-block").show();

            if (!window.FileUploader || FileUploader.GetUploadFileCount() == 0) {
                ASC.Projects.TaskActionPage.saveOrUpdate(taskID);
            } else {
                FileUploader.Submit();
            }
        },

        changeResponsible: function(userID) {
            var userName = jq("#userName_" + userID).html();
            jq("#userSelector_switcher").html(userName + "&nbsp;<small style='color:#111'>▼</small>").css('color', '#111');
            jq("#userSelector_switcher_mobile option[value='" + userID + "']").attr("selected", "selected");
            jq("#taskResponsible").val(userID);
            jq("#userSelector_dropdown").hide();
            jq("#taskResponsibleTitle").show();

            if (userID == CurrUser.UserID || userID == CurrUser.EmptyGuid) {
                jq("#notifyResponsible").attr("disabled", "disabled").removeAttr("checked");
            }
            else {
                jq("#notifyResponsible").removeAttr("disabled").attr("checked", "checked");
            }
        },

        coloringRows: function() {
            var parents = jq("div.ui-sortable");
            for (var i = 0; i < parents.length; i++) {
                jq(parents[i]).children().removeClass("tintMedium tintLight");
                var children = jq("div.ui-sortable").children();
                for (var j = 0; j < children.length; j++) {
                    if (j % 2 == 0)
                        jq(children[j]).removeAttr("style").addClass("tintMedium");
                    else
                        jq(children[j]).removeAttr("style").addClass("tintLight");

                    if (typeof (jq(children[j]).attr("name")) != "undefined")
                        if (jq(children[j]).attr("name").split("_")[0] == "empty")
                        jq(children[j]).css("height", "3px").removeClass("tintMedium tintLight");
                }
            }
        },

        changeStatus: function(taskID, milestoneID, prjID) {
            jq("#taskStatus_" + taskID).hide();
            jq("#loaderImg_" + taskID).show();
            var isClosed = jq("#taskStatus_" + taskID).attr("checked");
            var container = isClosed ? jq("#pm_closedTaskContainer_" + milestoneID + "_" + prjID) : jq("#pm_openTaskContainer_" + milestoneID + "_" + prjID);

            AjaxPro.TaskBlockView.ChangeTaskStatus(taskID, isClosed, ASC.Projects.TaskActionPage.isAllMyTasks, ASC.Projects.TaskActionPage.isOneList, function(res) {
                if (res.error != null) {
                    jq("#taskStatus_" + taskID).show();
                    jq("#loaderImg_" + taskID).hide();
                    alert(res.error.Message);
                    return;
                }

                jq("#pmtask_" + taskID + "_" + milestoneID).remove();

                if (isClosed) {
                    var childrens = jq(container).children();
                    var nextTaskID = childrens.length > 1 ? parseInt(jq(childrens[0]).attr("id").split("_")[1]) : -1;
                    AjaxPro.TaskBlockView.SetTaskSortOrder(milestoneID, taskID, -1, nextTaskID, false);
                    jq(container).prepend(res.value);

                    ASC.Projects.TaskActionPage.coloringRows();

                    var addedBlock = jq(jq(container).children()[0]);
                    addedBlock.hide();
                    addedBlock.animate({ 'height': 'show' }, 300);
                    addedBlock.css({ "background-color": "#ffffcc" });
                    var endbackgroundColor = '#ffffff';
                    addedBlock.animate({ backgroundColor: endbackgroundColor }, 1000);

                }
                else {
                    var childrens = jq(container).children();
                    var prevTaskID = childrens.length > 1 ? parseInt(jq(childrens[childrens.length - 1]).attr("id").split("_")[1]) : -1;
                    AjaxPro.TaskBlockView.SetTaskSortOrder(milestoneID, taskID, prevTaskID, -1, false);
                    jq(container).append(res.value);

                    ASC.Projects.TaskActionPage.coloringRows();

                    var addedBlock = jq(jq(container).children()[jq(container).children().length - 1]);
                    addedBlock.hide();
                    addedBlock.animate({ 'height': 'show' }, 300);
                    addedBlock.css({ "background-color": "#ffffcc" });

                    if (jq(addedBlock).hasClass("tintMedium"))
                        endbackgroundColor = '#edf6fd';
                    else
                        endbackgroundColor = '#ffffff';

                    addedBlock.animate({ backgroundColor: endbackgroundColor }, 1000);
                }

                ASC.Projects.Common.tooltip("#taskTitle_" + taskID, "tooltip", false);

            });

        },

        changeMilestone: function(milestoneID) {
            var milestoneTitle = jq("#milestoneTitle_" + milestoneID).html();
            jq("#milestoneSelector_switcher").html(milestoneTitle + "&nbsp;<small style='color:#111 !important'>▼</small>").css('color', '#111');
            jq("#milestoneSelector_switcher_mobile option[value='" + milestoneID + "']").attr("selected", "selected");
            jq("#taskMilestone").val(milestoneID);
            jq("#milestoneSelector_dropdown").hide();
            jq("#taskMilestoneTitle").show();
        },

        changeDeadline: function(object) {
            jq("#taskDeadlineContainer").find("a").each(
            function(index) { jq(this).css("border-bottom", "1px dotted").css("border-color", "#111"); });
            jq(object).css("border-bottom", "none");
            var daysCount = parseInt(jq.trim(jq(object).attr('id').split('_')[1]));

            AjaxPro.onLoading = function(b) { if (b) { } };

            AjaxPro.TaskBlockView.ChangeTaskDaedline(daysCount, function(res) {
                if (res.error != null) { alert(res.error.Message); return; }
                jq("[id$=taskDeadline]").val(res.value);
            });
        },

        showLastCompletedTasks: function(milestoneID, prjID) {
            var tasks = jq("#hiddenContainer_" + milestoneID + '_' + prjID + " input[id$=hfTaskList]").val().split(',');
            var lastTask = parseInt(jq("#hiddenContainer_" + milestoneID + '_' + prjID + " input[id$=hfLastTask]").val());
            var count = parseInt(jq("#hiddenContainer_" + milestoneID + '_' + prjID + " input[id$=hfCountTask]").val());
            var tasksID = new Array();
            var newLastTask;

            for (var i = 0; i < tasks.length; i++) {
                tasksID.push(parseInt(tasks[i]));
                if (parseInt(tasks[i]) == lastTask)
                    if (tasks.length > i + count + 1)
                    newLastTask = parseInt(tasks[i + count]);
                else {
                    newLastTask = parseInt(tasks[tasks.length - 1]);
                    jq("#showLastCompletedTasks_" + milestoneID + '_' + prjID).hide();
                }
            }

            AjaxPro.TaskBlockView.GetLastCompletedTasks(tasksID, lastTask, count, function(res) {
                if (res.error != null) { alert(res.error.Message); return; }
                var taskContainer = jq('#pm_closedTaskContainer_' + milestoneID + '_' + prjID);
                var addedBlock = res.value;
                taskContainer.append(addedBlock);
                jq("#hiddenContainer_" + milestoneID + '_' + prjID + " input[id$=hfLastTask]").val(newLastTask);
            });
        },

        taskItem_mouseOver: function(obj) {
            jq(obj).addClass('pm-tasksItem-over');
            var taskID = jq(obj).attr('id').split('_')[1];

            jq("#imgTime_" + taskID).show();
            if (jq("#imgBell_" + taskID).length > 0) {
                jq("#imgProfile_" + taskID).hide();
                jq("#imgBell_" + taskID).show();
            }

            var offset = jq(obj).offset();
            var hack = 0; var hackBottom = 0;
            if (jq.browser.msie && jq.browser.version.split('.')[0] == '7')
            { hack = 1; hackBottom = 5; }

            if (CurrUser.HavePermission && !ASC.Projects.TaskActionPage.isAllMyTasks)
                jq("#drag_" + taskID).show().css('height', jq(obj).height() + hack).css('top', (8.5 - (jq(obj).height() / 2)));
            jq("#actionsPointer_" + taskID).css('height', jq(obj).height() + hack);
            jq("#imgZub_" + taskID).css('bottom', (jq(obj).height() / 2 + 5.5 + hackBottom));
            jq("#actionBlock_" + taskID).show().css('top', (offset.top - 1)).css('left', (offset.left + jq(obj).width()));

            //IE7 hack
            if (jq.browser.msie && jq.browser.version.split('.')[0] == '7') {
                jq("#actionBlock_" + taskID).css('width', '43px');
                jq("#actions_" + taskID).css('width', '20px');
            }
        },

        taskItem_mouseOut: function(obj) {
            jq(obj).removeClass('pm-tasksItem-over');
            var taskID = jq(obj).attr('id').split('_')[1];

            if (parseInt(jq("#imgTime_" + taskID).attr("hastime")) == 0)
                jq("#imgTime_" + taskID).hide();
            if (jq("#imgBell_" + taskID).length > 0) {
                jq("#imgProfile_" + taskID).show();
                jq("#imgBell_" + taskID).hide();
            }
            if (CurrUser.HavePermission)
                jq("#drag_" + taskID).hide();
            jq("#actionBlock_" + taskID).hide();
        },

        deleteTaskItem: function(task_id, milestone_id) {
            AjaxPro.onLoading = function(b) { };
            if (!confirm(jq.format(ASC.Projects.Resources.DeleteThisTask, ""))) return;
            AjaxPro.TaskDetailsView.ExecDeleteTask(task_id,
                function(res) {
                    if (res.error != null) { alert(res.error.Message); return; }
                    jq("#pmtask_" + task_id + "_" + milestone_id).animate({ opacity: "hide" }, 500);

                    //					if (window.ProjectTemplatesController) {
                    //                		setTimeout(function() {
                    //                			jq("#pmtask_" + task_id + "_" + milestone_id).remove();
                    //                			ProjectTemplatesController.HighlightItems('pmtask_');
                    //                		}, 500);
                    //                	}
                    //                	else
                    //                	{
                    setTimeout(function() {
                        jq("#pmtask_" + task_id + "_" + milestone_id).remove();
                        ASC.Projects.TaskActionPage.coloringRows();
                    }, 500);
                    //                	}

                });

        },

        notifyResponsible: function(taskID, imgSrc) {
            AjaxPro.TaskBlockView.NotifyTaskResponsible(taskID,
                function(res) {
                    if (res.error != null) { alert(res.error.Message); return; }
                    jq("#imgBell_" + taskID).attr('src', imgSrc);
                    jq("[id$=_commonContainer_InfoPanel]").html(res.value).show();
                });

        },

        deleteFile: function(fileID, fileVersion, fileName) {

            if (!confirm(jq.format(ASC.Projects.Resources.ConfirmDelete, "'" + fileName + "'"))) return false;

            var prjID = jq.getURLParam("prjID");
            
            AjaxPro.TaskDetailsView.DeleteFile(prjID, fileID, fileVersion,
                function(res) {

                    if (res.error != null) {

                        alert(res.error.Message);

                        return;

                    }


                }
              );

            return true;

        },
        changeSubscribe: function(taskID) {

            AjaxPro.onLoading = function(b) {

                if (b)
                    jq("#task_subscriber").block({ message: null });
                else
                    jq("#task_subscriber").unblock();

            }

            AjaxPro.TaskDetailsView.ChangeSubscribe(taskID,
     function(res) {

         if (res.error != null) {

             alert(res.error.Message);

             return;

         }

         jq("#task_subscriber a").text(jq.trim(res.value));

     }
    );

        },

        refresh: function(taskID) {

            jq("#addTaskPanel input, select, textarea").removeAttr("disabled");

            jq("#addTaskPanel  .pm-action-block").show();
            jq("#addTaskPanel  .pm-ajax-info-block").hide();

            var infoBlock = jq("#addTaskPanel  div[id$=taskContainer_InfoPanel]");

            infoBlock.hide();

            if ((typeof (CurrTask) === 'undefined') && (taskID != -1)) {
                var CurrTask = AjaxPro.TaskBlockView.GetCurrUser(taskID).value;
            }

            if ((typeof (CurrTask) !== 'undefined') && (taskID != -1)) {
                jq("#addTaskPanel input[id$=tbxTitle]").val(CurrTask.Title);
                jq("#addTaskPanel textarea[id$=tbxDescribe]").val(CurrTask.Description);

                if (CurrTask.MilestoneID > 0) this.changeMilestone(CurrTask.MilestoneID); else this.changeMilestone(0);
                if (jq("#userName_" + CurrTask.ResponsibleID).length > 0
                    || jq("#userSelector_switcher_mobile option[value='" + CurrTask.ResponsibleID + "']").length > 0) {
                    this.changeResponsible(CurrTask.ResponsibleID);
                }
                jq("[id$=taskDeadline]").val(CurrTask.Deadline);

                jq("#addTaskPanel div.popupContainerClass div.containerHeaderBlock table tbody tr td:first").text(ASC.Projects.Resources.EditThisTask);
                jq("#addTaskPanel a.baseLinkButton:first").text(ASC.Projects.Resources.SaveChanges);
            }
            else {
                jq("#task_uploadContainer").html("");

                if (!ASC.Projects.Common.IAmIsManager) {
                    infoBlock.find("div").text(ASC.Projects.Resources.TaskActionCreateInfoBlock);
                    infoBlock.show();
                }

                jq("#addTaskPanel input[id$=tbxTitle]").val("");
                jq("#addTaskPanel textarea[id$=tbxDescribe]").val("");
                jq("[id$=taskDeadline]").val("");
                jq("#addTaskPanel div.popupContainerClass div.containerHeaderBlock table tbody tr td:first").text(ASC.Projects.Resources.AddNewTask);
                jq("#addTaskPanel a.baseLinkButton:first").text(ASC.Projects.Resources.AddThisTask);
            }
        },
        show: function(taskID) {
            taskID = taskID || -1;
            var margintop = jq(window).scrollTop() - 135;
            margintop = margintop + 'px';

            PopupKeyUpActionProvider.EnableEsc = false;
            jq.blockUI({ message: jq("#addTaskPanel"),
                css: {
                    left: '50%',
                    top: '25%',
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '650px',

                    cursor: 'default',
                    textAlign: 'left',
                    position: 'absolute',
                    'margin-left': '-300px',
                    'margin-top': margintop,
                    'background-color': 'White'
                },

                overlayCSS: {
                    backgroundColor: '#AAA',
                    cursor: 'default',
                    opacity: '0.3'
                },
                focusInput: false,
                baseZ: 666,

                fadeIn: 0,
                fadeOut: 0,

                onBlock: function() {
                    var $blockUI = jq("#addTaskPanel").parents('div.blockUI:first'), blockUI = $blockUI.removeClass('blockMsg').addClass('blockDialog').get(0), cssText = '';
                    if (jq.browser.msie && jq.browser.version < 9 && $blockUI.length !== 0) {
                        var prefix = ' ', cssText = prefix + blockUI.style.cssText, startPos = cssText.toLowerCase().indexOf(prefix + 'filter:'), endPos = cssText.indexOf(';', startPos);
                        if (startPos !== -1) {
                            if (endPos !== -1) {
                                blockUI.style.cssText = [cssText.substring(prefix.length, startPos), cssText.substring(endPos + 1)].join('');
                            } else {
                                blockUI.style.cssText = cssText.substring(prefix.length, startPos);
                            }
                        }
                    }
                }
            });

            if ((typeof (FileHtml5Uploader) != 'undefined')) {
                jq("#task_uploadContainer").html("");
                jq("object[id*='SWFUpload']").before('<span id="asc_fileuploaderSWFObj"></span>');
                jq("object[id*='SWFUpload']").remove();
                var uploadFiles = typeof (uploadedFiles) === 'undefined' ? AjaxPro.TaskBlockView.GetUploadedFiles(taskID).value : uploadedFiles;

                if (taskID == -1) uploadFiles = new Array();
                initFileUploaderInPopup(uploadFiles);
            }

            jq("#addTaskPanel input[id$=tbxTitle]").focus();

            if (jq("#taskResponsible").val() == CurrUser.UserID || jq("#taskResponsible").val() == CurrUser.EmptyGuid) {
                jq("#notifyResponsible").attr("disabled", "disabled").removeAttr("checked");
            }
            else {
                jq("#notifyResponsible").removeAttr("disabled").attr("checked", "checked");
            }
        },

        showMoveTaskDialog: function(taskID, milestoneID) {

            jq("#moveTaskPanel b[id=moveTaskTitles]").html(jq("#taskTitle_" + taskID).html());
            if (jq("input[name=milestones]").length > 0)
                jq("input[id=milestone_" + milestoneID + "]").attr('checked', 'checked');
            else return;

            jq("#moveTaskPanel a.baseLinkButton:first").unbind("click").bind("click", function() {
                ASC.Projects.TaskActionPage.moveTaskToMilestone(taskID);
            });

            var margintop = jq(window).scrollTop() - 135;
            margintop = margintop + 'px';

            jq.blockUI({ message: jq("#moveTaskPanel"),
                css: {
                    left: '50%',
                    top: '25%',
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '550px',

                    cursor: 'default',
                    textAlign: 'left',
                    position: 'absolute',
                    'margin-left': '-300px',
                    'margin-top': margintop,
                    'background-color': 'White'
                },

                overlayCSS: {
                    backgroundColor: '#AAA',
                    cursor: 'default',
                    opacity: '0.3'
                },
                focusInput: false,
                baseZ: 666,

                fadeIn: 0,
                fadeOut: 0

            });
        },

        moveTaskToMilestone: function(taskID, milstoneID) {
            var checkedMilestoneID = parseInt(jq("input[name=milestones]:checked").attr("id").split("_")[1]);

            AjaxPro.TaskBlockView.MoveTaskToMilestone(taskID, checkedMilestoneID, function(res) {
                if (res.error != null) { alert(res.error.Message); }
                document.location.reload();
            });
        },

        toggleClosedTask: function(interval) {

            var basePath = 'tasks.aspx?prjID=' + jq.getURLParam("prjID");

            if (jq.trim(jq.getURLParam("action")) != "")
                basePath += "&action=" + jq.getURLParam("action");

            if (jq.trim(jq.getURLParam("userID")) != "")
                basePath += "&userID=" + jq.getURLParam("userID");

            if (jq.getURLParam("view") == "all")
                location.href = basePath;
            else
                location.href = basePath + "&view=all" + "&interval=" + interval;

        },

        initTaskSort: function() {
            if (typeof CurrUser == 'undefined')
                CurrUser = {};

            jq('div[id^="pm_openTaskContainer_"]').sortable({
                cursor: 'move',
                items: '>div.pm_task_item',
                connectWith: CurrUser.HavePermission ? 'div[id^="pm_openTaskContainer_"]' : '',
                handle: 'div.moveHoverBackground',

                update: function(ev, ui) {
                    var moveToAnotherMilestone = false;
                    var milestoneID = jq(jq(ui.item).parent()).attr("id").split('_')[2];
                    var projectID = jq(jq(ui.item).parent()).attr("id").split('_')[3];
                    var taskID = jq(ui.item).attr("id").split('_')[1];

                    if (milestoneID != jq(ui.item).attr("id").split('_')[2])
                        moveToAnotherMilestone = true;

                    jq(ui.item).attr("id", jq(ui.item).attr("id").split('_')[0] + '_' + taskID + '_' + milestoneID);
                    jq("#taskStatus_" + taskID).attr('onchange', '').unbind("change").bind("change", function() {
                        ASC.Projects.TaskActionPage.changeStatus(parseInt(taskID), parseInt(milestoneID), parseInt(projectID));
                    });

                    var prevID = jq(ui.item).prev().attr("id");
                    var nextID = -1;
                    if (prevID == 'undefined' || prevID == null || prevID == -1) {
                        prevID = -1;
                        if (jq(ui.item).next().length > 0)
                            nextID = jq(ui.item).next().attr("id").split('_')[1];
                    }
                    else
                        prevID = prevID.split('_')[1];

                    jq('div[id^="pm_openTaskContainer_"]').each(
                    function(index) {
                        var milID = jq(this).attr("id").split('_')[2];

                        if (jq(this).children().length == 0) {
                            var empty = document.createElement('div');
                            jq(empty).attr("id", "pmtask_-1_" + milID).addClass("pm_task_item").attr("name", "empty_" + milID).css("height", "3px");
                            jq(this).append(empty);
                        }
                        else {
                            if (jq('div[name="empty_' + milID + '"]').length != 0 && jq(this).children().length > 1)
                                jq('div[name="empty_' + milID + '"]').remove();
                        }

                        ASC.Projects.TaskActionPage.coloringRows();
                    });

                    jq("[id^=drag_]").each(function() { jq(this).hide(); })
                    jq("[id^=actionBlock_]").each(function() { jq(this).hide(); })

                    AjaxPro.onLoading = function(b) { };

                    AjaxPro.TaskBlockView.SetTaskSortOrder(milestoneID, taskID, prevID, nextID, moveToAnotherMilestone, function(result) {
                    });
                },
                helper: function(e, el) {
                    return jq(el).clone().width(jq(el).width());
                },
                start: function(event, ui) {
                    jq("[id^=actionBlock_]").each(function() { jq(this).hide(); })
                },

                dropOnEmpty: false
            });
        },

        unlockForm: function() {

            jq("#page_content div.pm-headerPanel-splitter input[id$=tbxTitle]")
               .removeAttr("readonly")
               .removeClass("disabled");

            jq("#notify_participant  input,#another_notify_participant  input, #fileUploader, #notify_edit").removeAttr("disabled");

            jq("div.pm-action-block").show();
            jq("div.pm-ajax-info-block").hide();

            jq("#notify_participant_checked").val("");
            jq("#notify_edit_checked").val("");

        },

        saveOrUpdate: function(taskID) {

            var titleUI = jq("#addTaskPanel input[id$=tbxTitle]");
            var descriptionUI = jq("#addTaskPanel textarea[id$=tbxDescribe]");
            var responsibleUI = jq("#taskResponsible");
            var milestoneUI = jq("#taskMilestone");
            var deadlineUI = jq("[id$=taskDeadline]");
            var prjID = jq.getURLParam("prjID");
            var notifyResponsible = jq("#notifyResponsible").attr("checked");

            AjaxPro.onLoading = function(b) {

                if (b) {

                    jq("#addTaskPanel input, select, textarea").attr("disabled", "disabled");

                    jq("#addTaskPanel  .pm-action-block").hide();
                    jq("#addTaskPanel  .pm-ajax-info-block").show();


                }

            };

            AjaxPro.TaskBlockView.SaveOrUpdateTask(prjID,
                                           taskID,
                                           milestoneUI.val(),
                                           jq.trim(titleUI.val()),
                                           jq.trim(descriptionUI.val()),
                                           responsibleUI.val(),
                                           deadlineUI.val(),
                                           notifyResponsible,
                                           jq("#attachment_list").val(),
     function(res) {

         if (res.error != null) {

             alert(res.error.Message);

             jq("#addTaskPanel input, select, textarea").removeAttr("disabled");

             jq("#addTaskPanel  .pm-action-block").show();
             jq("#addTaskPanel  .pm-ajax-info-block").hide();

             return;

         }

         var alreadyExistElement = jq("#pmtask_" + taskID + "_" + milestoneUI.val());

         if (taskID != -1 && alreadyExistElement.length == 0) {

             location.href = "tasks.aspx?prjID=" + jq.getURLParam("prjID") + "&ID=" + taskID;

             return;

         }

         var milestoneWithTasksBlock = jq("#milestoneWithTasksBlock_" + milestoneUI.val());

         if (milestoneWithTasksBlock.length == 0) {

             location.href = "tasks.aspx?prjID=" + jq.getURLParam("prjID") + "&ID=" + res.value.getValue('ID');

             return;

         }

         jq('#emptyTasksBlock').css('display', 'none');
         jq('#TasksBlock').css('display', 'block');

         if (milestoneUI.parent().css('display') != 'none') {

             var scroll_to = milestoneWithTasksBlock.position();

             jq.scrollTo(scroll_to.top, { speed: 500 });

         }

         var addedBlock = jq(res.value.getValue('HTML'));




         var infoBlockText = jq.trim(res.value.getValue('InfoBlockText'));
         var infoBlock = jq("div[id$=_commonContainer_InfoPanel]");


         if (infoBlockText != "") {

             infoBlock.text(infoBlockText);
             infoBlock.show();

         }
         else
             infoBlock.hide();




         jq("#addTaskPanel input, select, textarea").removeAttr("disabled");

         jq("#addTaskPanel  .pm-action-block").show();
         jq("#addTaskPanel  .pm-ajax-info-block").hide();

         jq.unblockUI();

         var taskContainer = jq('#pm_openTaskContainer_' + milestoneUI.val() + '_' + prjID);

         if (alreadyExistElement.length == 0) {
             taskContainer.prepend(addedBlock);
             ASC.Projects.Common.tooltip("#taskTitle_" + res.value.getValue('ID'), "tooltip", true);
         }
         else {
             jq(alreadyExistElement).replaceWith(addedBlock);
             ASC.Projects.Common.tooltip("#taskTitle_" + res.value.getValue('ID'), "tooltip", false);
         }

         ASC.Projects.TaskActionPage.initTaskSort();
         ASC.Projects.TaskActionPage.coloringRows();

         //		if (window.ProjectTemplatesController) {
         //     		addedBlock.addClass('tintMedium');
         //     		addedBlock.hide();
         //     		addedBlock.animate({ 'height': 'show' }, 300);
         //     		setTimeout(function() {
         //     			ProjectTemplatesController.HighlightItems('pmtask_');
         //     		}, 300);
         //     	} else {
         addedBlock.css({ "background-color": "#ffffcc" });

         if (alreadyExistElement.length == 0) {
             if (taskContainer.find(':first-child ').hasClass('tintMedium'))
                 endbackgroundColor = '#edf6fd';
             else
                 endbackgroundColor = '#ffffff';
         }
         else {
             if (jq(addedBlock).hasClass('tintMedium'))
                 endbackgroundColor = '#edf6fd';
             else
                 endbackgroundColor = '#ffffff';
         }

         addedBlock.animate({ backgroundColor: endbackgroundColor }, 1000);
         setTimeout(function() {
             ASC.Projects.TaskActionPage.coloringRows();
         }, 1000);
         //     	}

     });
        }

    }
})();

ASC.Projects.TaskDetailsView = (function(){

function _saveOrUpdate (taskID)
{
  var titleUI = jq("input[id$=tbxTitle]");
  var descriptionUI = jq("textarea[id$=tbxDescribe]");
  var responsibleUI = jq("#taskResponsible");
  var milestoneUI = jq("#taskMilestone");

  if (jq.trim(titleUI.val()) == "")
  {
    alert(ASC.Projects.Resources.EmptyTaskTitle);
    return;
  }

  AjaxPro.onLoading = function(b) { };  

  AjaxPro.TaskDetailsView.SaveOrUpdateTask(jq.getURLParam("prjID"), 
                                           taskID, 
                                           milestoneUI.val(), 
                                           jq.trim(titleUI.val()), 
                                           jq.trim(descriptionUI.val()), 
                                           responsibleUI.val(), 
     function (res)
     {
       if (res.error != null)
       {
          alert(res.error.Message);
          return;
       }     
       if (res.value != -1)
       {
         location.href = "tasks.aspx?prjID=" + jq.getURLParam("prjID") + "&ID=" + res.value ;             
       }
       else
       {
         location.reload(true);
       }
     });
}
return  {
execSaveOrUpdate :  function(taskID)
{

  ASC.Projects.TaskActionView.initControl(taskID);
  ASC.Projects.TaskActionView.execShowTaskAddPanel();
  jq("#addTaskPanel a.baseLinkButton:first").unbind("click").bind("click", function(){ _saveOrUpdate(taskID);});
   
},
execTaskChangeStatus :  function(task_id, new_status)
{
 
  AjaxPro.onLoading = function(b) { };  

  AjaxPro.TaskDetailsView.ExecChangeStatus(task_id, new_status, 
  function (res)
  {
    
     if (res.error != null)
     {
     
       alert(res.error.Message);
     
       return;
     
     }    
               
     location.reload(true);  
  
  });
  

},
execTaskBeResponsible :  function(task_id)
{
 
  AjaxPro.onLoading = function(b) { };  

  AjaxPro.TaskDetailsView.ExecBeResponsible(task_id, 
  function (res)
  {
    
     if (res.error != null)
     {
     
       alert(res.error.Message);
     
       return;
     
     }    
               
     location.reload(true);  
  
  });
  

},
execTaskDelete : function(task_id)
{

 AjaxPro.onLoading = function(b) { };  

 if (!confirm(jq.format(ASC.Projects.Resources.DeleteThisTask , ""))) return;
 
 AjaxPro.TaskDetailsView.ExecDeleteTask(task_id, 
  function(res)
  {
  
     if (res.error != null)
     {
     
       alert(res.error.Message);
     
       return;
     
     }  
     
     location.href = 'tasks.aspx?prjID=' + jq.getURLParam('prjID');    
  
  }
   
 
 );

}

}
})();

ASC.Projects.TaskPage =(function() { // Private Section

return { // Public Section

execTaskChangeStatus : function(task_id, new_status)
{
  
    jq("dl.pm-taskStatus").hide();
    
    var infoBlock =  jq("div[id$=_commonContainer_InfoPanel]");

    infoBlock.hide();

    var taskRow = jq("#taskRow_" + task_id);
    
   
    AjaxPro.onLoading = function(b)  
    {
    
      if (b & new_status!=3)
      {
                   
          var statusColumn = taskRow.find("td.pm-task-status");
      
          statusColumn.html(ASC.Projects.Constants.MINI_LOADER_IMG);        
          
      }
      
    
    };
            
    AjaxPro.TaskBlockView.TaskChangeStatus(task_id, new_status, 
     function(res)
     {
       
        if (res.error != null)
        {
      
           alert(res.error.Messages);  
        
           return;
               
        } 
                               
        if (new_status == 3)
          taskRow.animate({ opacity: "hide" }, 750, function(){jq(this).remove()});        
        else                
          taskRow.find("td.pm-task-status").html(res.value);      
          
        if (new_status == 2 && !ASC.Projects.Common.IAmIsManager)
        {
           var task_title = jq.trim(taskRow.find("td.pm-task-title").text());
                      
           infoBlock.text(jq.format(ASC.Projects.Resources.NotifyManagerAboutTaskCompleted, task_title));
           
           infoBlock.show();                    
        }  
                            
     }
    ); 
},

execTaskBeResponsible :  function(taskID, milestoneID)
{
 
  AjaxPro.onLoading = function(b) { };  

  AjaxPro.TaskBlockView.ExecBeResponsible(taskID, 
  function (res)
  {
     if (res.error != null){alert(res.error.Message);return;}    
     jq("#pmtask_"+taskID+"_"+milestoneID).replaceWith(res.value);
     ASC.Projects.TaskActionPage.coloringRows();          

  });
  

},

execTaskManipulation : function (ids, actionType)
{
  
   AjaxPro.onLoading = function(b) 
   {    
        if (b)
        {
                    
            jq("#group_manager_popup  .pm-action-block").hide();
            jq("#group_manager_popup  .pm-ajax-info-block").show();

        }
   };  
    
   AjaxPro.TaskBlockView.TaskManager(ids, actionType, 
    function (res)
    {
    
      if (res.error != null)
      {
      
        alert(res.error.Messages);  
                          
        jq("#group_manager_popup  .pm-action-block").show();
        jq("#group_manager_popup  .pm-ajax-info-block").hide();
        
        return;
              
      }       
      
      location.reload(true);        
    }
   );   
}, 
execShowTaskAddPanel : function(milestoneID)
{

   if(jq("#milestoneTitle_"+milestoneID).length>0) ASC.Projects.TaskActionPage.changeMilestone(milestoneID);
   
   jq("input[id$=tbxTitle]").val("");
   jq("textarea[id$=tbxDescribe]").val("");
           
   jq("#addTaskPanel a.baseLinkButton:first").unbind("click").bind("click", 
     function()
     {
      
       ASC.Projects.TaskPage.execTaskSaveOrUpdate(-1);
        
     }   
   );
   
   var margintop =  jq(window).scrollTop()-135;
   margintop = margintop+'px';
      
   jq.blockUI({message:jq("#addTaskPanel"),
                    css: { 
                        left: '50%',
                        top: '40%',
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '600px',
                       
                        cursor: 'default',
                        textAlign : 'left',
                        position: 'absolute',
                        'margin-left': '-300px',
                        'margin-top': margintop,                      
                        'background-color':'White'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#AAA',
                        cursor: 'default',
                        opacity: '0.3' 
                    },
                    focusInput : false,
                    baseZ : 666,
                    
                    fadeIn: 0,
                    fadeOut: 0
                }); 
        jq("input[id$=tbxTitle]").focus();
 
},
execViewTaskListCheckedPanel : function (actionTitle,actionType, milestoneWithTaskBlockID)
{
  
   var ids = new Array();
   var innerHTML = "";

   jq("#" + milestoneWithTaskBlockID +  " input:checked").each(
     function(index)
     {
     
       var taskRow = jq(this).parents("tr");
              
       ids.push(taskRow.attr("id").split('_')[1]);
              
       innerHTML  +=  "<li>" + jq.trim(taskRow.find("td").eq(3).text()) + "</li>";

     }
   );
   
    if (ids.length == 0)
    {
   
       alert(ASC.Projects.Resources.SelectedTasksInfo);
      
       return;
         
    }
    
    jq("#group_manager_popup dd ul").html(innerHTML);
  
    jq("#group_manager_popup dd:last").html(actionTitle);
   
    var executeGroupOperationsButton =  jq("#group_manager_popup .pm-action-block a:first");
   
    executeGroupOperationsButton.unbind('click');
    executeGroupOperationsButton.bind('click', 
     function()
     {  
        
         ASC.Projects.TaskPage.execTaskManipulation(ids, actionType);
                           
     }     
    );
    
    if (jq("#milestone_dropdown").length!=0)
       jq("#milestone_dropdown").hide();
   
   jq(".pm-dropdown").hide();
   
   var margintop =  jq(window).scrollTop()-135;
   margintop = margintop+'px';
   
    jq.blockUI({message:jq("#group_manager_popup"),
                    css: { 
                        left: '50%',
                        top: '50%',
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '600px',
                       
                        cursor: 'default',
                        textAlign : 'left',
                        position: 'absolute',
                        'margin-left': '-300px',
                        'margin-top': margintop,                      
                        'background-color':'White'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#AAA',
                        cursor: 'default',
                        opacity: '0.3' 
                    },
                    focusInput : false,
                    baseZ : 666,
                    
                    fadeIn: 0,
                    fadeOut: 0
                }); 
                
                
},



execShowTaskActionPanel : function(status, ui)
{

  var tableRow = jq(ui).parents("tr").attr("id");
  
  jq("dl.pm-taskStatus").hide();

  AjaxPro.onLoading = function(b) { };  
   
  AjaxPro.TaskBlockView.TaskActionPanelHTML(tableRow.split('_')[1], status, 
      function(res)
      {
      
        if (res.error != null)
        {
        
           alert(res.error.Message);
        
           return;
        
        }  
        
        var pos = jq(ui).offset();
        
        var innerHTML = res.value;
        
        var item =  jq(res.value).css(
                            {
                              
                              'position' : 'absolute',
                              'top' : pos.top + jq(ui).height(),
                              'left' : pos.left                              
                            }                            
                         );  
                         
                            
        item.find("dd").hover(
        function()
        {
            
            jq(this).css("color", "red");
            
        }, 
        function()
        {
                
            jq(this).css("color", "");
        
        }       
       );                   
                                  
                                  
        if (jq(ui).siblings("dl").length != 0)
           jq(ui).siblings("dl").replaceWith(item);
        else
           jq(ui).parent().append(item);                    
      
      }
  );
  
}


}

})();

//task sort
jq(function() {

    ASC.Projects.TaskActionPage.initTaskSort();

});




