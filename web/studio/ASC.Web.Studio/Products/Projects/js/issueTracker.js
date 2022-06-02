ASC.Projects.IssueTrackerActionPage = (function() {
    return {
        init: function(issueID, selectedParticipant) {
            jq("#addIssuePanel a.baseLinkButton:first").unbind("click").bind("click", function() { ASC.Projects.IssueTrackerActionPage.saveOrUpdate(issueID); });

            this.refresh(issueID);
        },
        show: function() {
            jq.blockUI({ message: jq("#addIssuePanel"),
                css: {
                    left: '50%',
                    top: '25%',
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '500px',

                    cursor: 'default',
                    textAlign: 'left',
                    'margin-left': '-300px',
                    'margin-top': '-135px',
                    'background-color': 'White'
                },

                overlayCSS: {
                    backgroundColor: '#AAA',
                    cursor: 'default',
                    opacity: '0.3'
                },
                focusInput: false,
                baseZ: 6666,

                fadeIn: 0,
                fadeOut: 0
            });

            jq("#addIssuePanel input[id$=tbTitle]").focus();
        },
        refresh: function(issueID) {
            jq("#addIssuePanel input, select, textarea").removeAttr("disabled");

            jq("#addIssuePanel  .pm-action-block").show();
            jq("#addIssuePanel  .pm-ajax-info-block").hide();

            var infoBlock = jq("#addIssuePanel  div[id$=issueContainer_InfoPanel]");

            infoBlock.hide();

            if (issueID == -1) {
                jq("#addIssuePanel input[id$=tbTitle]").val("");
                jq("#addIssuePanel input[id$=tbDetectedInVersion]").val("");
                jq("#priority_block input:radio[value=2]").attr("checked", "checked");
                jq("#addIssuePanel textarea[id$=tbDescription]").val("");                
                jq("#addIssuePanel input[id$=tbCorrectedInVersion]").val("");
            }
        },
        saveOrUpdate: function(issueID, fckClientID) {
            var tbTitle = jq("#addIssuePanel input[id$=tbTitle]");
            var tbDetectedInVersion = jq("#addIssuePanel input[id$=tbDetectedInVersion]");
            var cbPriority = jq("#priority_block input:radio:checked");
            var fckDescription = FCKeditorAPI.GetInstance(fckClientID);
            var cbAssignedOn = jq("#addIssuePanel select[id$=ddlParticipants] option:selected");            
            var tbCorrectedInVersion = jq("#addIssuePanel input[id$=tbCorrectedInVersion]");

            if (jq.trim(tbTitle.val()) == "") {

                alert(ASC.Projects.Resources.EmptyTaskTitle);

                return;

            }

            AjaxPro.onLoading = function(b) {

                if (b) {

                    jq("#addIssuePanel input, select, textarea").attr("disabled", "disabled");

                    jq("#addIssuePanel  .pm-action-block").hide();
                    jq("#addIssuePanel  .pm-ajax-info-block").show();


                }

            };

            AjaxPro.ListIssueTrackerView.SaveOrUpdateIssue(issueID,
                                           jq.getURLParam("prjID"),
                                           jq.trim(tbTitle.val()),
                                           jq.trim(tbDetectedInVersion.val()),
                                           cbPriority.val(),
                                           jq.trim(fckDescription.GetHTML(true)),
                                           cbAssignedOn.val(),
                                           jq.trim(tbCorrectedInVersion.val()),
                                           jq("#notify_assigned_on_issue").is(":checked"),
                function(res) {
                    if (res.error != null) {

                        alert(res.error.Message);

                        jq("#addIssuePanel input, select, textarea").removeAttr("disabled");

                        jq("#addIssuePanel  .pm-action-block").show();
                        jq("#addIssuePanel  .pm-ajax-info-block").hide();

                        return;

                    }

                    //var addedBlock = jq(res.value.getValue('HTML'));
                    //var tableBody = jq("#pm_issueContainer");
                    //var infoBlockText = jq.trim(res.value.getValue('InfoBlockText'));
                    //var infoBlock = jq("div[id$=_commonContainer_InfoPanel]");

                    /*if (infoBlockText != "") {

                        infoBlock.text(infoBlockText);
                    infoBlock.show();

                    }
                    else
                    infoBlock.hide();*/

                    jq("#addIssuePanel input, select, textarea").removeAttr("disabled");

                    jq("#addIssuePanel  .pm-action-block").show();
                    jq("#addIssuePanel  .pm-ajax-info-block").hide();

                    jq.unblockUI();

                    location.href = 'issueTracker.aspx?prjID=' + jq.getURLParam("prjID");

                    //addedBlock.css({ "background-color": "#ffffcc" });
                    //tableBody.prepend(addedBlock);
                });
        }
    }
})();

ASC.Projects.IssueTrackerPage = (function() { // Private Section

    return { // Public Section
        execShowIssueActionPanel: function(status, ui) {

            var tableRow = jq(ui).parents("tr").attr("id");

            jq("dl.pm-issueStatus").hide();

            AjaxPro.onLoading = function(b) { };

            AjaxPro.ListIssueTrackerView.IssueActionPanelHTML(tableRow.split('_')[1], status,
          function(res) {

              if (res.error != null) {

                  alert(res.error.Message);

                  return;

              }

              var pos = jq(ui).offset();

              var innerHTML = res.value;

              var item = jq(res.value).css(
                                {

                                    'position': 'absolute',
                                    'top': pos.top + jq(ui).height(),
                                    'left': pos.left
                                }
                             );


              item.find("dd").hover(
            function() {

                jq(this).css("color", "red");

            },
            function() {

                jq(this).css("color", "");

            }
           );


              if (jq(ui).siblings("dl").length != 0)
                  jq(ui).siblings("dl").replaceWith(item);
              else
                  jq(ui).parent().append(item);

          }
      );

        },
        execIssueChangeStatus: function(issue_id, new_status) {

            jq("dl.pm-issueStatus").hide();

            var infoBlock = jq("div[id$=_commonContainer_InfoPanel]");

            infoBlock.hide();

            var issueRow = jq("#issueRow_" + issue_id);


            AjaxPro.onLoading = function(b) {

                if (b & new_status != 3) {

                    var statusColumn = issueRow.find("td.pm-issue-status");

                    statusColumn.html(ASC.Projects.Constants.MINI_LOADER_IMG);

                }


            };

            AjaxPro.ListIssueTrackerView.IssueChangeStatus(issue_id, new_status,
                function(res) {

                    if (res.error != null) {

                        alert(res.error.Messages);

                        return;

                    }

                    if (new_status == 4)
                        issueRow.animate({ opacity: "hide" }, 750, function() { jq(this).remove() });
                    else
                        issueRow.find("td.pm-issue-status").html(res.value);

                    if (new_status == 3 && !ASC.Projects.Common.IAmIsManager) {
                        var issue_title = jq.trim(issueRow.find("td.pm-issue-title").text());

                        infoBlock.text(jq.format(ASC.Projects.Resources.NotifyManagerAboutIssueCompleted, issue_title));

                        infoBlock.show();
                    }

                }
            );
        }
    }
})();