ASC.Projects.TimeSpendActionPage = (function() {
    return {


        AddTimeLog: function(prjID, taskID, imagePath) {

            var logDate = jq("#addLogPanel [id$=addLogPanel_tbxDate]").val();
            var personID = jq("#addLogPanel [id$=addLogPanel_ddlPerson] option:selected").val();
            var hours = jq("#addLogPanel [id$=addLogPanel_tbxHours]").val();
            var note = jq("#addLogPanel [id$=addLogPanel_tbxNote]").val();

            if (hours <= 0) { alert(ASC.Projects.Resources.EmptyHoursCount); return; }

            AjaxPro.onLoading = function(b) {
                if (b)
                    ASC.Projects.TimeSpendActionPage.lockAddLogPanel();
                else
                    ASC.Projects.TimeSpendActionPage.unlockAddLogPanel();
            }

            AjaxPro.TimeSpendActionView.Save(taskID, logDate, personID, hours, note, prjID,
     function(res) {

         if (res.error != null) {
             alert(res.error.Message);
             ASC.Projects.TimeSpendActionPage.unlockAddLogPanel();
             return;
         }

         jq("#imgTime_" + taskID).attr('src', imagePath).attr("hastime", "1").show();
         jq("#timeTrackingImg").attr('src', imagePath);
         jq("#timeTrackingCountHours").html(res.value);

         if (jq("#timeLogImg").length != 0) jq("#timeLogImg").attr('src', imagePath);
         if (jq("#timeLogHours").length != 0) jq("#timeLogHours").html(res.value);

         ASC.Projects.TimeSpendActionPage.unlockAddLogPanel();
         jq.unblockUI();

     }

  );


        },

        ViewTimeLogPanel: function(prjID, taskID) {

            AjaxPro.onLoading = function(b) {

                if (b)
                    ASC.Projects.TimeSpendActionPage.lockAddLogPanel();
                else
                    ASC.Projects.TimeSpendActionPage.unlockAddLogPanel();

            }

            AjaxPro.TimeSpendActionView.InitPopUpContainer(prjID, taskID,
     function(res) {
         if (res.error != null) {
             alert(res.error.Message);
             ASC.Projects.TimeSpendActionPage.unlockAddLogPanel();
             return;
         }
         jq("#addLogPanel a.baseLinkButton:first").unbind("click").bind("click", function() { ASC.Projects.TimeSpendActionPage.AddTimeLog(prjID, taskID, res.value.ImgClockActive); });
         jq("#TotalHoursCount").html(res.value.CountHours);
         jq("#addLogPanel [id$=addLogPanel_tbxDate]").val(res.value.DateTimeNow);
         jq("#addLogPanel [id$=addLogPanel_ddlPerson]").html(res.value.HTML);
         jq("#addLogPanel [id$=addLogPanel_ddlPerson] option").each(function() { if (jq(this).val() == res.value.ResponsibleID) jq(this).attr('selected', 'selected'); });
         jq("#addLogPanel [id$=addLogPanel_tbxHours]").val("1" + jq("#NumberDecimalSeparator").val() + "00");
         jq("#addLogPanel [id$=addLogPanel_tbxNote]").val("");

         jq('[id$=addLogPanel_tbxDate]').datepick({ selectDefaultDate: true, showAnim: '', popupContainer: "#addLogPanel" });

         AjaxPro.TimeSpendActionView.GetTaskTitle(taskID, function(res) {
             jq("#TimeLogTaskTitle").html(res.value);
             ASC.Projects.TimeSpendActionPage.unlockAddLogPanel();

            var margintop =  jq(window).scrollTop()-135;
            margintop = margintop+'px';
            
             jq.blockUI({ message: jq("#addLogPanel"),
                 css: {
                     left: '50%',
                     top: '40%',
                     opacity: '1',
                     border: 'none',
                     padding: '0px',
                     width: '500px',

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

         });


     }
    );

        },

        quickAdd: function() {
            var date = jq("[id$=quickAdd_tbxDate]").val();
            var personID = jq("[id$=quickAdd_ddlPerson] option:selected").val();
            var hours = jq("[id$=quickAdd_tbxHours]").val();
            var note = jq("[id$=quickAdd_tbxNote]").val();
            var prjID = jq.getURLParam("prjID");
            var cssClass = jq("[id$=hiddenCssClass]").val();

            if (hours <= 0) { alert(ASC.Projects.Resources.EmptyHoursCount); return; }
            if (note == "") { alert(ASC.Projects.Resources.EmptyDescription); return; }


            AjaxPro.TimeTracking.QuickAdd(date, personID, hours, note, prjID, cssClass,
            function(res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    return;
                }

                jq("[id$=quickAdd_tbxHours]").val("1" + jq("#NumberDecimalSeparator").val() + "00");
                jq("[id$=quickAdd_tbxNote]").val("");
                jq("#editLogPanel input, select, textarea").removeAttr("disabled");

                jq("#quickAddTimeSpendsList").prepend(res.value);

                if (cssClass == "tintLight") {
                    jq("[id$=hiddenCssClass]").val("tintMedium");
                    jq("#quickAddTimeSpendsList div:first").animate({ backgroundColor: "#ffffcc" }, 500).animate({ backgroundColor: "#ffffff" }, 500);
                }
                else {
                    jq("[id$=hiddenCssClass]").val("tintLight");
                    jq("#quickAddTimeSpendsList div:first").animate({ backgroundColor: "#ffffcc" }, 500).animate({ backgroundColor: "#edf6fd" }, 500);
                }

                AjaxPro.TimeTracking.GetTotalHoursCount(prjID, function(res) { jq("#totalHoursCount").html(res.value); jq("input, select, textarea").removeAttr("disabled"); });
                AjaxPro.TimeTracking.GetTotalHoursCountOnPage(jq("#totalHoursCountOnPage").html(), "0", hours,
                function(res) {
                    jq("#totalHoursCountOnPage").html(res.value)
                }
                );

            });
        },

        saveTimeSpend: function(id) {
            var date = jq("#editLogPanel [id$=editLogPanel_tbxDate]").val();
            var personID = jq("#editLogPanel [id$=editLogPanel_ddlPerson] option:selected").val();
            var hours = jq("#editLogPanel [id$=editLogPanel_tbxHours]").val();
            var note = jq("#editLogPanel [id$=editLogPanel_tbxNote]").val();
            var prjID = jq.getURLParam("prjID");

            if (hours <= 0) { alert(ASC.Projects.Resources.EmptyHoursCount); return; }
            if (note == "" && jq("#TimeLogTaskTitle").html() == "") { alert(ASC.Projects.Resources.EmptyDescription); return; }

            AjaxPro.onLoading = function(b) {
                if (b) {
                    ASC.Projects.TimeSpendActionPage.lockEditLogPanel();
                }
            }

            AjaxPro.TimeSpendEditView.Save(date, personID, hours, note, id, prjID,
            function(res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                    return;
                }

                var result = res.value.split("||");

                AjaxPro.TimeTracking.GetTotalHoursCountOnPage(jq("#totalHoursCountOnPage").html(), jq("#hours_ts" + id).html(), hours,
                function(res) {
                    jq("#totalHoursCountOnPage").html(res.value)
                }
                );

                jq("#date_ts" + id).html(date);
                jq("#person_ts" + id).html(result[0]);
                jq("#hours_ts" + id).html(result[1]);
                jq("#totalHoursCount").html(result[2]);

                if (jq("#timeSpendRecord" + id).attr("class") == "tintLight") {
                    jq("#timeSpendRecord" + id).animate({ backgroundColor: "#ffffcc" }, 500).animate({ backgroundColor: "#ffffff" }, 500);
                }
                else {
                    jq("#timeSpendRecord" + id).animate({ backgroundColor: "#ffffcc" }, 500).animate({ backgroundColor: "#edf6fd" }, 500);
                }

                AjaxPro.TimeSpendEditView.GetNote(id, function(res) { jq("#note_ts" + id).html(res.value); });
                AjaxPro.TimeSpendEditView.GetTitle(id, function(res) { jq("#timeSpendRecord" + id + " td.pm-ts-noteColumn").attr('title', res.value); jq("input, select, textarea").removeAttr("disabled"); });

                ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                jq.unblockUI();

            });
        },

        deleteTimeSpend: function(id) {
            AjaxPro.TimeTracking.DeleteTimeSpend(id,
            function(res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    jq("input, select, textarea").removeAttr("disabled");
                    return;
                }
                jq("#timeSpendRecord" + id).animate({ opacity: "hide" }, 500);
                jq("input, select, textarea").removeAttr("disabled");
                jq("#totalHoursCount").html(res.value);
                setTimeout(function() {
                    jq("#timeSpendRecord" + id).remove();
                    if (jq("#timeSpendsList").children().length == 0) {
                        jq("[id$=MainPageContainer]").hide();
                        jq("[id$=EmptyScreenContainer]").show();
                    }
                    else {
                        jq("#timeSpendsList").children().removeClass("tintMedium tintLight");
                        var children = jq("#timeSpendsList").children();
                        for (var j = 0; j < children.length; j++) {
                            if (j % 2 == 0)
                                jq(children[j]).addClass("tintMedium");
                            else
                                jq(children[j]).addClass("tintLight");
                        }
                    }

                }, 500);

                AjaxPro.TimeTracking.GetTotalHoursCountOnPage(jq("#totalHoursCountOnPage").html(), jq("#hours_ts" + id).html(), "0",
                function(res) {
                    jq("#totalHoursCountOnPage").html(res.value)
                }
                );
            });
        },

        ViewEditTimeLogPanel: function(id) {
            ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
            jq("#editLogPanel [id$=editLogPanel_tbxNote]").val("");
            jq("#TimeLogTaskTitle").html("");
            AjaxPro.TimeSpendEditView.InitPopUpContainer(id,
        function(res) {
            if (res.error != null) {
                alert(res.error.Message);
                ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                return;
            }

            jq("#editLogPanel a.baseLinkButton:first").unbind("click").bind("click", function() { ASC.Projects.TimeSpendActionPage.saveTimeSpend(id); });

            var tmp = res.value.split("||");


            jq("#editLogPanel [id$=editLogPanel_tbxHours]").val(tmp[0]);
            jq("#editLogPanel [id$=editLogPanel_tbxDate]").val(tmp[1]);
            jq("#editLogPanel [id$=editLogPanel_ddlPerson] option").each(function() { if (jq(this).val() == tmp[2]) jq(this).attr('selected', 'selected'); });


            AjaxPro.TimeSpendEditView.GetTimeSpendNote(id, function(res) {

                jq("#editLogPanel [id$=editLogPanel_tbxNote]").val(jq('<div/>').html(res.value).text());
                ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                if (tmp[3] != null) {

                    if (parseFloat(tmp[3]) != 0) {
                        jq("#TotalHoursCount").html(tmp[3]);
                        jq("#TimeLogInfoPanel").show();
                    }

                    AjaxPro.TimeSpendEditView.GetTimeSpendRelativeTaskTitle(id, function(res) {

                        jq("#TimeLogTaskTitle").html(res.value);
                        ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                        jq("#TimeLogTaskTitle").show();
                        ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                        
                        var margintop =  jq(window).scrollTop()-135;
                        margintop = margintop+'px';
                        
                        jq.blockUI({ message: jq("#editLogPanel"),
                            css: {
                                left: '50%',
                                top: '40%',
                                opacity: '1',
                                border: 'none',
                                padding: '0px',
                                width: '500px',

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
                    });

                }
                else {
                    jq("#TimeLogTaskTitle").html("");
                    jq("#TotalHoursCount").html("");
                    jq("#TimeLogInfoPanel").hide();
                    jq("#TimeLogTaskTitle").hide();

                    ASC.Projects.TimeSpendActionPage.unlockEditLogPanel();
                    
                    var margintop =  jq(window).scrollTop()-135;
                        margintop = margintop+'px';
                        
                    
                    jq.blockUI({ message: jq("#editLogPanel"),
                        css: {
                            left: '50%',
                            top: '40%',
                            opacity: '1',
                            border: 'none',
                            padding: '0px',
                            width: '500px',

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
                }

            });
        }
    );

        },


        lockAddLogPanel: function() {
            jq("#addLogPanel input").attr("disabled", "disabled");
            jq("#addLogPanel textarea").attr("disabled", "disabled");
            jq("#addLogPanel select").attr("disabled", "disabled");
            jq("#addLogPanel .pm-action-block").hide();
            jq("#addLogPanel .pm-ajax-info-block").show();
        },

        unlockAddLogPanel: function() {
            jq("#addLogPanel input, select, textarea").removeAttr("disabled");
            jq("#addLogPanel .pm-action-block").show();
            jq("#addLogPanel .pm-ajax-info-block").hide();
        },

        lockEditLogPanel: function() {
            jq("#editLogPanel input").attr("disabled", "disabled");
            jq("#editLogPanel textarea").attr("disabled", "disabled");
            jq("#editLogPanel select").attr("disabled", "disabled");
            jq("#editLogPanel .pm-action-block").hide();
            jq("#editLogPanel .pm-ajax-info-block").show();
        },

        unlockEditLogPanel: function() {
            jq("#editLogPanel input").removeAttr("disabled");
            jq("#editLogPanel textarea").removeAttr("disabled");
            jq("#editLogPanel select").removeAttr("disabled");
            jq("#editLogPanel .pm-action-block").show();
            jq("#editLogPanel .pm-ajax-info-block").hide();
        },

        lockQuickAddPanel: function() {
            jq("#quickAddPanel input").attr("disabled", "disabled");
            jq("#quickAddPanel select").attr("disabled", "disabled");
            jq("#quickAddPanel [id$=imgCalendar]").attr("disabled", "disabled");
            jq("#quickAddPanel a").removeAttr('onclick');
        },

        showActions: function(id) {
            jq("#ts_actions" + id).css('display', 'block');
        },

        hideActions: function(id) {
            jq("#ts_actions" + id).css('display', 'none');
        },

        keyPress: function(event, elementID)//keyPress
        {

            var separator = jq("#NumberDecimalSeparator").val();

            var code;
            if (!e) var e = event;
            if (!e) var e = window.event;
            if (e.keyCode) {
                code = e.keyCode;
            }
            else if (e.which) {
                code = e.which;
            }

            if (separator == '.') {
                if (code == 46) {
                    if (jq("[id$=" + elementID + "]").val().length == 0) {
                        return false;
                    }

                    if (jq("[id$=" + elementID + "]").val().indexOf('.') == -1) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }

            if (separator == ',') {
                if (code == 44) {
                    if (jq("[id$=" + elementID + "]").val().length == 0) {
                        return false;
                    }

                    if (jq("[id$=" + elementID + "]").val().indexOf(',') == -1) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }

            if (code >= 48 && code <= 57) {
                return true;
            }

            if (code == 8 || code == 37 || code == 39)//backspace left right
            {
                return true;
            }

            return false;
        },

        keyPress: function(event) {
            var code;
            if (!e) var e = event;
            if (!e) var e = window.event;
            if (e.keyCode) {
                code = e.keyCode;
            }
            else if (e.which) {
                code = e.which;
            }

            if (code >= 48 && code <= 57) {
                ASC.Projects.TimeSpendActionPage.changeDateRange();
            }
        },

        changeDateRange: function() {
            jq("#8").attr('selected', 'selected');
            jq("[id$=hiddenCurrentTimeRange]").val(8);
        },

        changeTimeRange: function() {
            var index = jq('#selectDate option:selected').val();
            var currentTime = new Date();

            var from_date = jq.datepick.today();
            var to_date = jq.datepick.today();

            switch (parseInt(index)) {
                case 0:   // Today
                    from_date = jq.datepick.formatDate(currentTime);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 1:  // Yesterday                         
                    currentTime.setDate(currentTime.getDate() - 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 2:  // This week
                    currentTime.setDate(-currentTime.getDay() + currentTime.getDate() + 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(currentTime.getDate() + 6);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 3:  // Last Week
                    currentTime.setDate(-currentTime.getDay() + 1 + currentTime.getDate() - 7);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(currentTime.getDate() + 6);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 4: // This month
                    currentTime.setMonth(currentTime.getMonth() + 1);
                    currentTime.setDate(-1);
                    currentTime.setDate(currentTime.getDate() + 1);
                    to_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(1);
                    from_date = jq.datepick.formatDate(currentTime);
                    break;

                case 5: // Last month
                    currentTime.setMonth(currentTime.getMonth());
                    currentTime.setDate(-1);
                    currentTime.setDate(currentTime.getDate() + 1);
                    to_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(1);
                    from_date = jq.datepick.formatDate(currentTime);
                    break;

                case 6: // This Year            
                    currentTime.setFullYear(currentTime.getFullYear(), 0, 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setFullYear(currentTime.getFullYear() + 1, 0, 1);
                    currentTime.setDate(0);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 7: // Last Year
                    currentTime.setFullYear(currentTime.getFullYear() - 1, 0, 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setFullYear(currentTime.getFullYear() + 1, 0, 1);
                    currentTime.setDate(0);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 8: // Other
                    from_date = jq.datepick.parseDate(jq.datepick.dateFormat, jq('[id$=tbxStartDate]').val());
                    to_date = jq.datepick.parseDate(jq.datepick.dateFormat, jq('[id$=tbxFinishDate]').val());
                    break;
            }

            jq('[id$=tbxStartDate],[id$=tbxFinishDate]').datepick('option', 'minDate', null);
            jq('[id$=tbxStartDate],[id$=tbxFinishDate]').datepick('option', 'maxDate', null);

            jq("[id$=tbxStartDate]").datepick('setDate', from_date);
            jq("[id$=tbxFinishDate]").datepick('setDate', to_date);
            jq("[id$=hiddenCurrentTimeRange]").val(index);
            jq("#" + index).attr('selected', 'selected');
        },
        
        showTimer: function(url)
        {
            var width = 250;
            var height = 473;
            
            if(jq.browser.safari && jq.browser.webkit)
            {
                height = 468;
            }
            
            if(jq.browser.msie)
            {
                height = 455;
            }
            
            open(url, "displayTimerWindow", "width="+width+",height="+height+",resizable=yes");
        }
    }
})();
