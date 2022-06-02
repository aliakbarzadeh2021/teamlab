// implement JSON.parse de-serialization
var JSON = JSON || {};
JSON.parse = JSON.parse || function(str) {
    if (str === "") str = '""';
    eval("var p=" + str + ";");
    return p;
};

window.master = {
    hWnd: null,
    winName: 'ImportContacts',
    params: 'width=800,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no',
    init: function() {
        if (window.addEventListener) {
            window.addEventListener('message', master.listener, false);
        } else {
            window.attachEvent('onmessage', master.listener);
        }
    },
    listener: function(e) {
        try
        {
            var data = JSON.parse(e.data);
            master.callback(data.message, data.error);
        }
        catch(er){};
    },

    callback: function(data, error) {
        _callbackData(data, error);
    },
    open: function(addr) {
        this.hWnd = window.open(addr, this.winName, this.params);
    },
    opensafe: function(addr) {
        this.hWnd = window.open(addr, this.winName);
    }
};
master.init();

UserImporterManager = new function() {
    this.SelectImportTitle = '';
    this.CSVImportTitle = '';
    this.OutlookImportTitle = '';
    this.ADImportTitle = '';
    this.FileNotFoundMessage = '';

    jq(document).ready(function() {

        if (jq('#studio_importerFileUploader').length > 0) {
            new AjaxUpload('studio_importerFileUploader', {
                action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Users.ImportFileUploader,ASC.Web.Studio',
                onSubmit: function() { jq('#studio_fileImportHolder').block(); },
                onComplete: UserImporterManager.UploadImportFile,
                parentDialog: jq("#studio_impEmpDialog")[0]
            });
        } 

    });

    this.UploadImportFile = function(file, response) {
        jq('#studio_fileImportHolder').unblock();
        var result = eval("(" + response + ")");
        if (result.Success) {
            jq('#studio_importerMessage').html('');
            jq('#studio_impEmpFileName').html(result.Data);
        }
        else {
            jq('#studio_impEmpFileName').html('');
            jq('#studio_importerMessage').html('<div class="errorBox">' + result.Message + '</div>');
        }
    };

    this.ShowImportEmployeeDialog = function() {
        jq('#studio_importerMessage').html('');
        try {

            jq.blockUI({ message: jq("#studio_impEmpDialog"),
                css: {
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '800px',
                    height: '700px',
                    cursor: 'default',
                    textAlign: 'left',
                    'background-color': 'Transparent',
                    'margin-left': '-300px',
                    'top': '100px'
                },

                overlayCSS: {
                    backgroundColor: '#aaaaaa',
                    cursor: 'default',
                    opacity: '0.3'
                },
                focusInput: false,
                fadeIn: 0,
                fadeOut: 0
            });
        }
        catch (e) { };

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'UserImporterManager.SelectImporter();';

        jq('#studio_impEmpMessage').hide();
        jq('#studio_impEmpContent').show();
        this.DisplayStep('0');
    };

    this.DisplayStep = function(stepID) {
        jq('#studio_importerMessage').html('');
        jq('#studio_fileImportHolder').hide();
        jq('#studio_csvDescription').hide();
        jq('#studio_outlookDescription').hide();

        jq('#studio_adImportHolder').hide();
        jq('#studio_importerSelector').hide();
        jq('#studio_confirmImportHolder').hide();
        jq('#studio_impEmpFileName').html('');

        PopupKeyUpActionProvider.ClearActions();

        switch (stepID) {

            case '0':
                //selector                                
                jq('#studio_importerSelector').show();
                jq('#studio_impEmpTitle').html(this.SelectImportTitle);
                PopupKeyUpActionProvider.EnterAction = 'UserImporterManager.SelectImporter();';
                break;

            case '1':
                //csv         
                jq('#studio_fileImportHolder').show();
                jq('#studio_csvDescription').show();
                PopupKeyUpActionProvider.EnterAction = 'UserImporterManager.ImportFromFile();';
                //jq('#studio_impEmpTitle').html(this.CSVImportTitle);                     

                break;

            case '2':
                //outlook
                jq('#studio_fileImportHolder').show();
                jq('#studio_outlookDescription').show();

                PopupKeyUpActionProvider.EnterAction = 'UserImporterManager.ImportFromFile();';
                //jq('#studio_impEmpTitle').html(this.OutlookImportTitle);                

                break;

            case '3':
                //active directory                
                jq('#studio_adImportHolder').show();
                PopupKeyUpActionProvider.EnterAction = 'UserImporterManager.ImportFromAD();';
                //jq('#studio_impEmpTitle').html(this.ADImportTitle);
                break;

        }

    };

    this.CloseImportDialog = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };

    this.SelectImporter = function() {
        this.DisplayStep(jq('input:radio[name="studio_importerType"]:checked').val());
    };

    this.ImportFromFile = function() {
        if (jq('#studio_impEmpFileName').html() == '') {
            jq('#studio_importerMessage').html('<div class="errorBox">' + this.FileNotFoundMessage + '</div>');
            return;
        }

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_impEmpDialog').block();
            else
                jq('#studio_impEmpDialog').unblock();
        };

        UserImporter.ImportFromFile(jq('#studio_csvDescription').is(':visible'), function(result) {
            var res = result.value;
            if (res.rs1 == 1) {
                UserImporterManager.DisplayStep('-1');
                jq('#studio_importList').html(res.rs2);
                jq('#studio_confirmImportHolder').show();
                jq('#studio_impConfirmBack').attr('href', 'javascript:UserImporterManager.DisplayStep("' + res.rs3 + '");');
                PopupKeyUpActionProvider.ClearActions();
            }
            else {
                jq('#studio_importerMessage').html('<div class="errorBox">' + res.rs2 + '</div>');
            }
        });
    };

    this.ImportFromAD = function() {
        AjaxPro.timeoutPeriod = 300000;
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_impEmpDialog').block();
            else
                jq('#studio_impEmpDialog').unblock();
        };

        UserImporter.ImportFromAD(jq('#ad_usrname').val(), jq('#ad_pwd').val(), function(result) {
            var res = result.value;
            if (res.rs1 == 1) {
                UserImporterManager.DisplayStep('-1');
                jq('#studio_importList').html(res.rs2);
                jq('#studio_confirmImportHolder').show();
                jq('#studio_impConfirmBack').attr('href', 'javascript:UserImporterManager.DisplayStep("3");');
                PopupKeyUpActionProvider.ClearActions();
            }
            else {
                jq('#studio_importerMessage').html('<div class="errorBox">' + res.rs2 + '</div>');
            }
        });

    };

    this.ConfirmUserImport = function() {
        var userIDs = new Array();
        jq('input:checkbox[id^="studio_importusr_"]').each(function() {
            if (!jq(this).is(':disabled') && jq(this).is(':checked'))
                userIDs.push(jq(this).val());
        });

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_impEmpDialog').block();
            else
                jq('#studio_impEmpDialog').unblock();
        };

        UserImporter.SaveUserImport(userIDs, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                PopupKeyUpActionProvider.ClearActions();
                jq('#studio_impEmpContent').hide();
                jq('#studio_impEmpMessage').show();
                setTimeout("jq.unblockUI();", 3000)
            }
            else {
                jq('#studio_importerMessage').html('<div class="errorBox">' + res.rs2 + '</div>');
            }
        });
    };

}

var existingUsersEmails;

var importUsersDomain;

var prevDocumentDomain = document.domain;
jq(window).unload(function() {
    document.domain = prevDocumentDomain;
});

function getContactsPopupWindowDisplay(url, popupTitle) {
    AjaxPro.onLoading = function(b) { };
    UserImporter.GetExistingUsersEmails(function(result) {
        existingUsersEmails = result.value;
    });
    
    if (document.domain.indexOf(importUsersDomain) != -1) {
        //open in standart way
        try {
            if (document.domain != importUsersDomain) {
                prevDocumentDomain = document.domain;
                document.domain = importUsersDomain;
            }
        }
        catch (err) { }

        try {
            master.open(url);
        }
        catch (err) {
            try
            {
                master.opensafe(url);
            }
            catch(e)
            {
                //All failed
                document.domain = prevDocumentDomain;
            }
        }
    }
    else {
        //Go through iframe version
        var iframeAddr = url.replace("default.", "frame.");
        var header = '<div class="containerHeaderBlock"><table cellspacing="0" cellpadding="0" border="0" style="width: 100%; height: 0px;"><tbody><tr valign="top"><td>' + popupTitle + '</td><td class="popupCancel"><div onclick="PopupKeyUpActionProvider.CloseDialog();" class="cancelButton"></div></td></tr></tbody></table></div>';
        jq('#studioPageContent').append('<div style="display:none" id="iframe_provider" class="popupContainerClass">'+header+'<iframe src="' + iframeAddr + '" frameborder="0" width="100%"></iframe></div>');
        //Create popup with iframe
        jq.blockUI({ message: jq("#iframe_provider"),
            css: {
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '300px',
                height: '300px',
                cursor: 'default',
                textAlign: 'left',
                'background-color': 'Transparent',
                'margin-left': '-150px',
                'top': '200px'
            },

            overlayCSS: {
                backgroundColor: '#aaaaaa',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            fadeIn: 0,
            fadeOut: 0
        });
    }
}

function _callbackData(e, error) {
    jq.unblockUI();
    jq("#iframe_provider").remove(); //if any
    userImporterCallback(e, error);
    //Set domain back
    document.domain = prevDocumentDomain;
}

var usersToImportCollection;

function inpurtUsersFromPopup() {
    //console.log(usersToImportCollection);
    UpdateContactInfoArray();
    var deptID = null;
    if (jq('#ImportContactsAddToDepChk').is(":checked")) {
        deptID = jq('#ImoportContactsDepartmentSelect').val();
    }
    if (jq('input[name="ContactAddChk"][type="checkbox"]').filter(':checked').length + jq('input[name="ContactInviteChk"][type="checkbox"]').filter(':checked').length == 0) {
        return;
    }
    var withFullAccessPrivileges = jq('#studio_inviteWithFullAcess').is(':checked');
    InviteEmployeeControl.InviteUsersFromWeb(usersToImportCollection, deptID, withFullAccessPrivileges, function(result) {
        var res = result.value;
        if (res.rs1 == '1') {
            for (var i in usersToImportCollection) {
                var curUser = usersToImportCollection[i];
                if (curUser.Add || curUser.Invite) {
                    jq('#ContactImportTableRow_' + i).remove();
                    curUser.Skip = true;
                    usersAlreadyAddedCount++;
                }
            }
            if (jq('#UsersImportTableContainer tr').length == 0) {
                PopupKeyUpActionProvider.ClearActions();
                jq('#studio_impEmpContent').hide();
                jq('#studio_impEmpMessage').show();
                setTimeout("jq.unblockUI();", 3000);
                return;
            }
            var rowNum = 0;
            jq('#UsersImportTableContainer tr').each(
				function() {
				    jq(this).attr('class', rowNum++ % 2 == 0 ? "tintLight" : "tintMedium");
				}
			);

            updateExistingUsersCountLabel();
        }
        else {
            jq('#studio_importerMessage').html('<div class="errorBox">' + res.rs2 + '</div>');
        }
    });
}




function userImporterCallback(e, error) {

    StudioManager.createCustomSelect('ImoportContactsDepartmentSelect');

    UserImporterManager.ShowImportEmployeeDialog();
    UserImporterManager.DisplayStep('-1');

    GenerateContactInfoArray(e);

    buildContactsTable();

    jq('#studio_confirmImportHolder').show();
    PopupKeyUpActionProvider.ClearActions();
    jq('#studio_impConfirmBack').hide();

    updateExistingUsersCountLabel();
}

function buildContactsTable() {
    var usersTable = [];
    usersTable.push("<table id='UsersImportTableContainer' cellpadding='3' cellspacing='0' style='width: 100%;'>");
    var userToAddCount = 0;
    var userToInviteCount = 0;

    var tempSpan = jq('<span id="testSpanToConstain" style="display:none;"></span>');
    tempSpan.appendTo('body');

    for (var i in usersToImportCollection) {
        var curUser = usersToImportCollection[i];
        if (!curUser.FirstName && !curUser.LastName && !curUser.Email) {
            continue;
        }

        var disableAdd = curUser.FirstName == '' || curUser.LastName == '';

        usersTable.push('<tr id="ContactImportTableRow_{1}" class="{0}">'.format(i % 2 == 0 ? "tintLight" : "tintMedium", curUser.ID));

        usersTable.push("<td style='width:85px;'>");
        usersTable.push('<input type="checkbox" name="ContactAddChk" id="ContactAddChk_{0}"'.format(curUser.ID));
        if (curUser.Add) {
            usersTable.push(' checked="checked"');
            userToAddCount++;
        }
        if (disableAdd) {
            usersTable.push(' disabled="disabled"');
            userToAddCount++;
        }
        usersTable.push("/>");
        usersTable.push("</td>");

        usersTable.push("<td style='width:100px;'>");
        usersTable.push('<input type="checkbox" name="ContactInviteChk" id="ContactInviteChk_{0}"'.format(curUser.ID));
        if (curUser.Invite) {
            usersTable.push(' checked="checked"');
            userToInviteCount++;
        }
        usersTable.push("/>");
        usersTable.push("</td>");

        usersTable.push("<td style='width:160px;'>");
        usersTable.push('<input type="text" class="studioEditableInput" value="{0}" id="ContactInviteFirstNameInput_{1}"/>'.format(curUser.FirstName, curUser.ID));
        usersTable.push("</td>");

        usersTable.push("<td style='width:160px;'>");
        usersTable.push('<input type="text" class="studioEditableInput" value="{0}" id="ContactInviteLastNameInput_{1}"/>'.format(curUser.LastName, curUser.ID));
        usersTable.push("</td>");

        usersTable.push(getCurUserEmailTd(curUser.Email, tempSpan));

        usersTable.push("</tr>");
    }
    usersTable.push("</table>");

    tempSpan.remove();

    if (userToAddCount >= usersToImportCollection.length) {
        jq('#UsersImporterSelectAllCheckbox').attr('checked', 'checked');
    }

    if (userToInviteCount >= usersToImportCollection.length) {
        jq('#UsersImporterInviteSelectAllCheckbox').attr('checked', 'checked');
    }

    jq('#studio_importList').html(usersTable.join(''));

    //select all checkboxes onclick bindings
    setTimeout(function() {
        bindSelectAllCapability('UsersImporterSelectAllCheckbox', 'ContactAddChk', 'ContactInviteChk_', 'UsersImporterInviteSelectAllCheckbox');
        bindSelectAllCapability('UsersImporterInviteSelectAllCheckbox', 'ContactInviteChk', 'ContactAddChk_', 'UsersImporterSelectAllCheckbox');
        disableAddContactsOnNameChange();
    }, 100);

    if (jq.browser.msie) {
        setTimeout(function() {
            jq('#UsersImportTableContainer .studioEditableInput').mouseover(function() {
                jq(this).addClass('studioEditableInputHover');
            }).mouseout(function() {
                jq(this).removeClass('studioEditableInputHover');
            }).focus(function() {
                jq(this).addClass('studioEditableInputFocus');
            }).blur(function() {
                jq(this).removeClass('studioEditableInputFocus');
            });
        }, 500);
    }
}


function bindSelectAllCapability(selectAllChkID, chkName, relativeChkPrefix, relativeSelectAllChkID) {
    SelectAllChkClick(selectAllChkID, chkName, relativeChkPrefix, relativeSelectAllChkID);
    SelectSingleChkClick(selectAllChkID, chkName, relativeChkPrefix, relativeSelectAllChkID);
}

function SelectSingleChkClick(selectAllChkID, chkName, relativeChkPrefix, relativeSelectAllChkID) {
    var relativeSelectAllChk = jq('#' + relativeSelectAllChkID);

    var chks = jq('input[name="{0}"][type="checkbox"]'.format(chkName));

    chks.click(
		function() {
		    var isChecked = jq(this).is(":checked");
		    if (isChecked) {
		        var id = this.id.split('_')[1];
		        jq('#' + relativeChkPrefix + id).attr('checked', false);
		        relativeSelectAllChk.removeAttr('checked');
		    }
		    var notDisabledChks = chks.not(':disabled');
		    jq('#' + selectAllChkID).attr('checked', notDisabledChks.length <= chks.filter(':checked').length);
		}
	);
}

function SelectAllChkClick(selectAllChkID, chkName, relativeChkPrefix, relativeSelectAllChkID) {
    var relativeSelectAllChk = jq('#' + relativeSelectAllChkID);
    jq('#' + selectAllChkID).click(
			function() {
			    var isChecked = jq(this).is(':checked');
			    jq('input[name="{0}"][type="checkbox"]'.format(chkName)).not(':disabled').each(
					function() {
					    jq(this).attr('checked', isChecked);

					    if (isChecked) {
					        var id = this.id.split('_')[1];
					        jq('#' + relativeChkPrefix + id).attr('checked', false);
					        relativeSelectAllChk.removeAttr('checked');
					    }
					}
				);
			}
		);
}

function disableAddContactsOnNameChange() {
    jq('#UsersImportTableContainer input[type="text"][id^="Contact"]').keyup(
		function(event) {
		    if (isEnterKeyPressed(event)) {
		        jq(this).blur();
		        animateUserImporterPanelChange(this.id);
		        return;
		    }
		    var id = this.id.split('_')[1];
		    var disableFlag = jq('#ContactInviteFirstNameInput_{0}'.format(id)).val() == '' || jq('#ContactInviteLastNameInput_{0}'.format(id)).val() == '';

		    var chk = jq('#ContactAddChk_{0}'.format(id));
		    chk.attr('disabled', disableFlag);
		}
	);
}

var usersAlreadyAddedCount = 0;

function GenerateContactInfoArray(usersCollection) {
    usersToImportCollection = new Array();
    usersAlreadyAddedCount = 0;
    var j = 0;
    for (var i in usersCollection) {
        var curUser = usersCollection[i];

        if (!curUser.Name && !curUser.LastName && !curUser.Email) {
            continue;
        }

        if (existingUsersEmails && existingUsersEmails.indexOf && existingUsersEmails.indexOf(",{0},".format(curUser.Email)) != -1) {
            usersAlreadyAddedCount++;
            continue;
        }

        var lastName = '';
        var firstName = curUser.FirstName;
        if (curUser.LastName) {
            lastName = curUser.LastName;
        } else {
            if (curUser.FirstName) {
                var names = curUser.FirstName.split(' ');
                if (names.length > 1) {
                    lastName = names[1];
                    firstName = names[0];
                }
            } else {
                firstName = '';
            }
        }

        if (firstName == curUser.Email) {
            firstName = '';
        }

        var add = firstName != '' && lastName != '' && curUser.Email != '';

        usersToImportCollection[j] = {
            FirstName: firstName,
            LastName: lastName,
            Email: curUser.Email,
            Add: add,
            Invite: !add,
            ID: j
        };

        j++;
    }
}

function UpdateContactInfoArray() {
    var inputs = jq('input[id^="Contact"]');

    for (var i in usersToImportCollection) {
        var curUser = usersToImportCollection[i];

        curUser.Add = inputs.filter('[id="ContactAddChk_{0}"][type = "checkbox"]'.format(i)).is(":checked");
        curUser.Invite = inputs.filter('[id="ContactInviteChk_{0}"][type = "checkbox"]'.format(i)).is(":checked");

        if (curUser.Add || curUser.Invite) {
            curUser.FirstName = inputs.filter('[id="ContactInviteFirstNameInput_{0}"]'.format(i)).val();
            curUser.LastName = inputs.filter('[id="ContactInviteLastNameInput_{0}"]'.format(i)).val();
        }
    }
}

function isEnterKeyPressed(event) {
    //Enter key was pressed
    if (event.keyCode == 13) {
        return true;
    }
    return false;
}


function animateUserImporterPanelChange(panelID) {
    if (panelID == null) {
        return;
    }
    try {
        var el = jq('#' + panelID);
        var bg = el.css("background-color");
        bg = bg.toLowerCase();
        el.css({ "background-color": "#ffffcc" });
        if (bg == "#edf6fd" || bg == "rgb(237, 246, 253)") {
            el.animate({ backgroundColor: '##EDF6FD' }, 2000);
        } else {
            el.animate({ backgroundColor: "#ffffff" }, 2000);
        }
    } catch (err) { el.css({ "background-color": "#ffffff" }); }
}

function updateExistingUsersCountLabel() {
    jq('#ExistingUsersCountLabel').html(jq('#ExistingUsersCountLabelHidden').val().format(usersAlreadyAddedCount));
}

function getCurUserEmailTd(email, tempSpan) {
    try {
        var ideal_width = 200;

        tempSpan.html(email);

        var item_width = tempSpan.width();
        var ideal = parseInt(ideal_width);
        var smaller_text = email;

        var title = '';

        if (item_width > ideal_width) {
            while (item_width > ideal) {
                smaller_text = smaller_text.substr(0, (smaller_text.length - 1));
                tempSpan.html(smaller_text);
                item_width = tempSpan.width();
            }
            smaller_text = smaller_text + '&hellip;';
            title = email;
        }
        return "<td title='{1}'>{0}</td>".format(smaller_text, title);
    } catch (err) {
        return "<td>{0}</td>".format(email);
    }
}