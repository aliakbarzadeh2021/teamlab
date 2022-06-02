jQuery.extend(
  jQuery.expr[":"],
  { reallyvisible: function(a) { return !(jQuery(a).is(':hidden') || jQuery(a).parents(':hidden').length); } }
);

var fckPrevValue = new Array();

jq(document).ready(function() {
    //jq.ui.dialog.defaults.bgiframe = true;
    //jq.ui.dialog.defaults.closeOnEscape = true;

    //fix width
    jq("#studioFooter, div.studioTopNavigationPanel").css('width', jq(document).width() <= 1000 ? "1000px" : "100%");
    jq(window).resize(function() {
        jq("#studioFooter, div.studioTopNavigationPanel").css('width', jq(document).width() <= 1000 ? "1000px" : "100%");
    });


    if (typeof (ASC) == 'undefined')
        ASC = {};

    if (typeof (ASC.Controls) == 'undefined')
        ASC.Controls = {};

    if (typeof (ASC.Controls.Constants) == 'undefined')
        ASC.Controls.Constants = {};


    //fancy zoom settings
    jq('a.fancyzoom').each(function(i, el) {

        var url = jq(this).attr('href');
        jq(this).attr('href', '#studio_imgfancyzoom_' + i + '_scr');
        jq('form').append('<div id="studio_imgfancyzoom_' + i + '_scr" style="display:none;"><img style="max-height:800px;" id="studio_imgfancyzoom_' + i + '_img" src="' + url + '"></div>');

    });

    //NOTE: stolen from http://detectmobilebrowser.com/
    ASC.Controls.Constants.isMobileAgent = /android|avantgo|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od|ad)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(navigator.userAgent || navigator.vendor || window.opera);

    //uploadera settings
    if (typeof (ASC.Controls.FileUploaderGlobalConfig) != 'undefined') {
        ASC.Controls.FileUploaderGlobalConfig.DeleteText = StudioManager.RemoveMessage;
        ASC.Controls.FileUploaderGlobalConfig.ErrorFileSizeLimitText = StudioManager.ErrorFileSizeLimit;
        ASC.Controls.FileUploaderGlobalConfig.ErrorFileEmptyText = StudioManager.ErrorFileEmpty;
        
        ASC.Controls.FileUploaderGlobalConfig.DescriptionCSSClass = 'studioFileUploaderDescription';
        ASC.Controls.FileUploaderGlobalConfig.FileNameCSSClass = 'studioFileUploaderFileName';
        ASC.Controls.FileUploaderGlobalConfig.DeleteLinkCSSClass = 'linkAction';
        ASC.Controls.FileUploaderGlobalConfig.ProgressBarCSSClass = 'studioFileUploaderProgressBar';
        ASC.Controls.FileUploaderGlobalConfig.ErrorBarCSSClass = 'studioFileUploaderErrorProgressBar';
        ASC.Controls.FileUploaderGlobalConfig.ErrorTextCSSClass = 'studioFileUploaderErrorDescription';
    }

    //settings preloaded
    jq.blockUI.defaults.css = {};
    jq.blockUI.defaults.overlayCSS = {};
    jq.blockUI.defaults.fadeOut = 0;
    jq.blockUI.defaults.fadeIn = 0;
    jq.blockUI.defaults.message = '<img alt="" src="' + SkinManager.GetImage('mini_loader.gif') + '"/>';

    var isPromo = false;
    try {
        if (window.PromoMode && PromoMode != 'undefined' && PromoMode != undefined && PromoMode != null && PromoMode)
            isPromo = true;
    }
    catch (e) { isPromo = false }

    if (isPromo) {
        jq('.promoAction').each(function(i, el) {
            jq(this).unbind('click');
            jq(this).attr('onclick', 'void(0)');
            jq(this).attr('href', PromoActionURL);
        });
    }

    //cancel fckeditor changes button prepare
    jq('.cancelFckEditorChangesButtonMarker').each(function() {
        var fckEditorId = jq(this).attr('name');
        if (fckEditorId == null || fckEditorId == '') {
            return;
        }

        var cancelButtonClick = jq(this).attr('onclick');
        jq(this).attr('onclick', '');
        jq(this).click(
			function(e) {
			    if (!CancelButtonController.ConfirmCancellationWithPrevValue(fckEditorId, fckPrevValue[fckEditorId])) {
			        return false;
			    }
			    if (cancelButtonClick)
			        cancelButtonClick(e);
			}
		);

        addFckEditor_OnComplete(function(editorInstance, obj) {
            if (editorInstance.Name == jq(obj).attr('name')) {
                fckPrevValue[jq(obj).attr('name')] = editorInstance.GetXHTML();
            }
        }, this);

        window.onbeforeunload =
			function(e) {
			    if (fckEditorId) {
			        var curLink = jq('[name="' + fckEditorId + '"]');
			        if (!curLink.is(':reallyvisible')) {
			            return;
			        }

			        var prev = '';
			        if (fckPrevValue[fckEditorId]) {
			            prev = fckPrevValue[fckEditorId];
			        }

			        if (!CancelButtonController.IsEmptyFckEditorTextFieldWithPrevValue(fckEditorId, prev)) {
			            return CommonJavascriptResources.CancelConfirmMessage;
			        } else {
			            return;
			        }
			    }
			};

    });

    keepSessionAliveForFckEditor();

});


function keepSessionAliveForFckEditor() {
	setTimeout(function() {
		var hasFckEditor = false;
		
		try {
			for (var fckInstance in FCKeditorAPI.Instances) {
				hasFckEditor = true;
				if (!CancelButtonController.IsEmptyFckEditorTextField(fckInstance)) {
					AjaxPro.onLoading = function(b) { };
					WebStudio.KeepSessionAlive(function(res) { });
				}
			}
		} catch (err) { }
		
		if (hasFckEditor) {
			keepSessionAliveForFckEditor();
		}
	}, 3 * 60 * 1000);
};


var PopupKeyUpActionProvider = new function() {
    //close dialog by esc
    jq(document).keyup(function(event) {

        if (!jq('.popupContainerClass').is(':visible'))
            return;

        var code;
        if (!e) var e = event;
        if (e.keyCode) code = e.keyCode;
        else if (e.which) code = e.which;

        if (code == 27 && PopupKeyUpActionProvider.EnableEsc) {
            PopupKeyUpActionProvider.CloseDialog();
        }

        else if ((code == 13) && e.ctrlKey) {
            if (PopupKeyUpActionProvider.CtrlEnterAction != null && PopupKeyUpActionProvider.CtrlEnterAction != '')
                eval(PopupKeyUpActionProvider.CtrlEnterAction);
        }
        else if (code == 13) {
            if (e.target.nodeName.toLowerCase() !== 'textarea' && PopupKeyUpActionProvider.EnterAction != null && PopupKeyUpActionProvider.EnterAction != '')
                eval(PopupKeyUpActionProvider.EnterAction);
        }

    });

    this.CloseDialog = function() {
        jq.unblockUI();

        if (PopupKeyUpActionProvider.CloseDialogAction != null && PopupKeyUpActionProvider.CloseDialogAction != '')
            eval(PopupKeyUpActionProvider.CloseDialogAction);

        PopupKeyUpActionProvider.ClearActions();
    };

    this.CloseDialogAction = '';
    this.EnterAction = '';
    this.CtrlEnterAction = '';
    this.EnableEsc = true;

    this.ClearActions = function() {
        this.CloseDialogAction = '';
        this.EnterAction = '';
        this.CtrlEnterAction = '';
        this.EnableEsc = true;
    };
};

var AuthManager = new function() {
    this.ConfirmMessage = 'Are you sure?';

    jq(document).ready(function() {
        var currentURL = new String(document.location);
        if (currentURL.indexOf('auth.aspx') != -1) {
            jq('#pwd').keydown(function(event) {
                var code;
                if (!e) var e = event;
                if (e.keyCode) code = e.keyCode;
                else if (e.which) code = e.which;

                if (code == 13)
                    AuthManager.Login();
                if (code == 27)
                    jq('#pwd').val('');

            });
            
        };
    });

    this.ShowPwdReminderDialog = function() {
        //jq('#studio_pwdReminderInfo').html('');
        jq('#' + jq('#studio_pwdReminderInfoID').val()).html('<div></div>');
        jq('#' + jq('#studio_pwdReminderInfoID').val()).hide();

        try {

            jq.blockUI({ message: jq("#studio_pwdReminderDialog"),
                css: {
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '400px',
                    height: '300px',
                    cursor: 'default',
                    textAlign: 'left',
                    'background-color': 'Transparent',
                    'margin-left': '-200px',
                    'top': '25%'
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
        PopupKeyUpActionProvider.EnterAction = 'AuthManager.RemindPwd();';

        jq('#studio_pwdReminderContent').show();
        jq('#studio_pwdReminderMessage').hide();

        var loginEmail = jq('#login').val();
        if (loginEmail != null && loginEmail != undefined && loginEmail.indexOf('@') >= 0) {
            jq('#studio_emailPwdReminder').val(loginEmail);
        }
    };


    this.RemindPwd = function() {
        AjaxPro.onLoading = function(b) {
            if (b) {
                //jq('#studio_pwdReminderDialog').block();
                jq('#pwd_rem_panel_buttons').hide();
                jq('#pwd_rem_action_loader').show();
            }
            else {
                //jq('#studio_pwdReminderDialog').unblock();
                jq('#pwd_rem_action_loader').hide();
                jq('#pwd_rem_panel_buttons').show();
            }
        };

        MySettings.RemaindPwd(jq('#studio_emailPwdReminder').val(), function(result) {
            var res = result.value;
            if (res.rs1 == "1") {
                jq('#studio_emailPwdReminder').val('');
                //jq('#studio_pwdReminderMessage').html(res.rs2);

                jq('#studio_pwdReminderContent').hide();
                //jq('#studio_pwdReminderMessage').show();

                jq('#' + jq('#studio_pwdReminderInfoID').val()).removeClass('alert')
                jq('#' + jq('#studio_pwdReminderInfoID').val()).addClass('info')
                jq('#' + jq('#studio_pwdReminderInfoID').val()).html("<div>" + res.rs2 + "</div>");
                jq('#' + jq('#studio_pwdReminderInfoID').val()).show();

                setTimeout("jq.unblockUI();", 2000)
            }
            else {
                jq('#' + jq('#studio_pwdReminderInfoID').val()).removeClass('info')
                jq('#' + jq('#studio_pwdReminderInfoID').val()).addClass('alert')
                jq('#' + jq('#studio_pwdReminderInfoID').val()).html(res.rs2);
                jq('#' + jq('#studio_pwdReminderInfoID').val()).show();
                //jq('#studio_pwdReminderInfo').html(res.rs2);
            }
        });
    };

    this.ClosePwdReminder = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };

    this.ConfirmInvite = function() {
        document.forms[0].submit();
    };

    this.Login = function(type) {
        if (type != undefined && type != null && type == 'demo')
            jq('#studio_authType').val('demo');
        else
            jq('#studio_authType').val('');

        if (type == undefined) {
            if (jq('#login').is('input[type="hidden"]'))
                jq('#studio_authType').val('name');
            else if (jq('#login').is('input[type="text"]'))
                jq('#studio_authType').val('email');
        }

        document.forms[0].submit();
    };

    this.ShowPwdChanger = function(userID) {
        //jq('#pwdchange_info').html('');
        jq('#' + jq('#pwdchange_info_id').val()).html('<div></div>');
        jq('#' + jq('#pwdchange_info_id').val()).hide();

        try {

            jq.blockUI({ message: jq("#studio_pwdChangerDialog"),
                css: {
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '400px',
                    height: '300px',
                    cursor: 'default',
                    textAlign: 'left',
                    'background-color': 'Transparent',
                    'margin-left': '-200px',
                    'top': '25%'
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
        PopupKeyUpActionProvider.EnterAction = 'AuthManager.ChangePwd();';

        jq('#studio_pwdChangerDialog_userID').val(userID);
        jq('#studio_pwdChangeContent').show();
        jq('#studio_pwdChangeMessage').hide();

    };

    this.ClosePwdChanger = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    }

    this.ChangePwd = function() {
        var userID = jq('#studio_pwdChangerDialog_userID').val();
        var pwd = jq('#user_newPwd').val();
        var repwd = jq('#user_rePwd').val();
        AjaxPro.onLoading = function(b) {
            if (b) {
                //jq('#studio_pwdChangerDialog').block();
                jq('#pwd_panel_buttons').hide();
                jq('#pwd_action_loader').show();
            }
            else {
                //jq('#studio_pwdChangerDialog').unblock();
                jq('#pwd_action_loader').hide();
                jq('#pwd_panel_buttons').show();
            }
        };

        MySettings.ChangePwd(userID, pwd, repwd, function(result) {
            var res = result.value;
            if (res.rs1 == "1") {
                jq('#user_newPwd').val('');
                jq('#user_rePwd').val('');

                //jq('#studio_pwdChangeMessage').html(res.rs2);

                jq('#studio_pwdChangeContent').hide();
                //jq('#studio_pwdChangeMessage').show();

                jq('#' + jq('#pwdchange_info_id').val()).removeClass('alert')
                jq('#' + jq('#pwdchange_info_id').val()).addClass('info')
                jq('#' + jq('#pwdchange_info_id').val()).html("<div>" + res.rs2 + "</div>");
                jq('#' + jq('#pwdchange_info_id').val()).show();

                setTimeout("jq.unblockUI();", 3000)
            }
            else {
                jq('#' + jq('#pwdchange_info_id').val()).removeClass('info')
                jq('#' + jq('#pwdchange_info_id').val()).addClass('alert')
                jq('#' + jq('#pwdchange_info_id').val()).html(res.rs2);
                jq('#' + jq('#pwdchange_info_id').val()).show();
                //jq('#pwdchange_info').html(res.rs2);
            }
        });
    };

    this.CloseInviteJoinDialog = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };


    this.ShowInviteJoinDialog = function(userID) {
        jq('#studio_invJoinInfo').html('');
        jq('#studio_joinEmail').val('');

        try {

            jq.blockUI({ message: jq("#studio_invJoinDialog"),
                css: {
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '400px',
                    height: '400px',
                    cursor: 'default',
                    textAlign: 'left',
                    'background-color': 'Transparent',
                    'margin-left': '-200px',
                    'top': '25%'
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
        PopupKeyUpActionProvider.CtrlEnterAction = 'AuthManager.SendInviteJoinMail();';

        jq('#studio_invJoinContent').show();
        jq('#studio_invJoinMessage').hide();
    };

    this.SendInviteJoinMail = function() {
        AjaxPro.onLoading = function(b) {
            if (b) {
                jq('#join_inv_panel_buttons').hide();
                jq('#join_inv_action_loader').show();
            }
            else {
                jq('#join_inv_action_loader').hide();
                jq('#join_inv_panel_buttons').show();

            }
        };

        InviteEmployeeControl.SendJoinInviteMail(jq('#studio_joinEmail').val(), function(result) {
            var res = result.value;
            if (res.rs1 == 1) {
                jq('#studio_invJoinMessage').html(res.rs2);
                jq('#studio_invJoinContent').hide();
                jq('#studio_invJoinMessage').show();
                setTimeout("jq.unblockUI();", 3000)

            }
            else
                jq('#studio_invJoinInfo').html('<div class="errorBox">' + res.rs2 + '</div>');

        });
    };


    this.RemoveInviteMailBox = function(number) {
        jq('#studio_invite_box_' + number).remove();
        var count = jq('div[id^="studio_invite_box_"]').length;
        if (count < 10)
            jq('#studio_invite_addButton').show();
    };

    this.AddInviteMailBox = function() {
        var maxNumb = -1;
        jq('div[id^="studio_invite_box_"]').each(function(i, pel) {

            var n = parseInt(jq(this).attr('name'));
            if (n > maxNumb)
                maxNumb = n + 1;
        });

        maxNumb++;
        var sb = new String();
        sb += '<div name="' + maxNumb + '" id="studio_invite_box_' + maxNumb + '" class="clearFix" style="margin-top:10px;">';
        sb += '<input tabindex="' + maxNumb + '" type="text" value="" id="studio_invite_mail_' + maxNumb + '" class="textEdit" maxlength="60" style="width:330px;"/>'
        sb += '<a style="margin-left:10px;" href="javascript:void(0);" onclick="AuthManager.RemoveInviteMailBox(\'' + maxNumb + '\');"><img alt="" align="absmiddle" border="0" src="' + SkinManager.GetImage("trash.png") + '"/></a>';
        sb += '</div>';

        jq('#studio_inviteMailBoxList').append(sb);

        var count = jq('div[id^="studio_invite_box_"]').length;
        if (count >= 10)
            jq('#studio_invite_addButton').hide();
    }


    this.ShowInviteEmployeeDialog = function(userID) {
        //jq('#studio_invEmpInfo').html('');
        jq('#studio_inviteUsersText').val(jq('#studio_inviteDefaultText').val());
        jq('div[id^="studio_invite_box_"][id!="studio_invite_box_1"]').remove();
        jq('#studio_invite_mail_1').val('');

        jq('#' + jq('#studio_invEmpContent_infoPanelID').val()).html('<div></div>');
        jq('#' + jq('#studio_invEmpContent_infoPanelID').val()).hide();



        try {

            jq.blockUI({ message: jq("#studio_invEmpDialog"),
                css: {
                    opacity: '1',
                    border: 'none',
                    padding: '0px',
                    width: '420px',
                    height: '600px',
                    cursor: 'default',
                    textAlign: 'left',
                    'background-color': 'Transparent',
                    'margin-left': '-200px',
                    'top': '25%'
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
        PopupKeyUpActionProvider.CtrlEnterAction = 'AuthManager.SendInviteMails();';

        jq('#studio_invEmpContent').show();
        jq('#studio_invEmpMessage').hide();

    };

    this.CloseInviteEmployeeDialog = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    }

    this.SendInviteMails = function() {

        jq('#' + jq('#studio_invEmpContent_infoPanelID').val()).hide();
        AjaxPro.onLoading = function(b) {
            if (b) {
                //jq('#studio_invEmpDialog').block();
                jq('#emp_inv_panel_buttons').hide();
                jq('#emp_inv_action_loader').show();
            }
            else {
                //jq('#studio_invEmpDialog').unblock();				
                jq('#emp_inv_action_loader').hide();
                jq('#emp_inv_panel_buttons').show();

            }
        };

        var mails = '';
        jq('input[id^="studio_invite_mail_"]').each(function(i, pel) {
            if (i != 0)
                mails += ',';

            mails += jq(this).val();
        });

        var isAdmin = jq('#studio_inviteFullAccessState').is(':checked');

        InviteEmployeeControl.SendInviteMails(jq('#studio_inviteUsersText').val(), mails, isAdmin, function(result) {
            var res = result.value;
            if (res.rs1 == 1) {


                jq('#studio_invEmpMessage').html(res.rs2);
                jq('#studio_invEmpContent').hide();
                jq('#studio_invEmpMessage').show();

                setTimeout("jq.unblockUI();", 2000)
            }
            else {
                jq('#' + jq('#studio_invEmpContent_infoPanelID').val()).html('<div>' + res.rs2 + '</div>');
                jq('#' + jq('#studio_invEmpContent_infoPanelID').val()).show();

                //jq('#studio_invEmpInfo').html('<div class="errorBox">' + res.rs2 + '</div>');
            }
        });
    };

    this.EditProfileCallback = function(makeResult) {
        if (makeResult.Action == 'edit_user') {
            if (makeResult.SelfProfile)
                window.location.reload(true);
        }
    };

    this.RemoveUser = function(userID) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        };

        UserProfileControl.RemoveUser(userID, function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                jq('.mainContainerClass .containerBodyBlock').html('<div class="okBox">' + res.rs2 + '</div>');

            else
                jq('#studio_userProfileCardInfo').html('<div class="errorBox">' + res.rs2 + '</div>');
        });
    };

    this.DisableUser = function(userID, disabled) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        };

        UserProfileControl.DisableUser(userID, disabled, function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                window.location.reload(true);
            else
                jq('#studio_userProfileCardInfo').html('<div class="errorBox">' + res.rs2 + '</div>');

        });
    };
    
    this.CreateAdmin = function() {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        };

        Wizard.CreateAdmin(jq('#studio_adminFirstName').val(),
                           jq('#studio_adminLastName').val(),
                           jq('#studio_adminEmail').val(),
                           jq('#studio_adminPwd').val(),
                           function(result) {

                               var res = result.value;
                               if (res.rs1 == '1')
                                   window.location.reload(true);
                               else
                                   jq('#studio_wizardMessage').html('<div class="errorBox">' + res.rs2 + '</div>');
                           });
    };
};

var StudioManager = new function() {
    this.ModuleQuickShortcutsMenuState = 0;

    this.SidePanelElementID = 'studio_sidePanel';
    this.SidePanelButtonElementID = 'studio_sidePanelButton';


    this.OnSideChangeState = null;


    this.ShowGettingStartedVideo = function() {
        jq.blockUI({
            message: jq("#studio_GettingStartedVideoDialog"),
            css: {
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '680px',
                height: '500px',
                cursor: 'default',
                textAlign: 'left',
                backgroundColor: 'transparent',
                marginLeft: '-340px',
                top: '25%'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            fadeIn: 0,
            fadeOut: 0
        });
    };

    this.SaveGettingStartedState = function() {
        var state = jq('#studio_gettingStartedState').is(':checked');

        AjaxPro.onLoading = function(b) { };

        WebStudio.SaveGettingStartedState(state, function(result) { });
    };


    this.createCustomSelect = function(selects, hiddenBorder, AdditionalBottomOption) {
        if (typeof selects === 'string') {
            selects = document.getElementById(selects);
        }
        if (!selects || typeof selects !== 'object') {
            return undefined;
        }
        if (typeof selects.join !== 'function' && !(selects instanceof String)) {
            selects = [selects];
        }

        for (var i = 0, n = selects.length; i < n; i++) {
            var select = selects[i];
            var selectValue = select.value;


            if (select.className.indexOf('originalSelect') !== -1) {
                continue;
            }

            var container = document.createElement('div');
            container.setAttribute('value', selectValue);
            container.className = select.className + (select.className.length ? ' ' : '') + 'customSelect';
            var position = (document.defaultView && document.defaultView.getComputedStyle) ? document.defaultView.getComputedStyle(select, '').getPropertyValue('position') : (select.currentStyle ? select.currentStyle['position'] : 'static');
            container.style.position = position === 'static' ? 'relative' : position;

            var title = document.createElement('div');
            title.className = 'title' + ' ' + selectValue;
            title.style.height = '100%';
            title.style.position = 'relative';
            title.style.zIndex = '1';
            container.appendChild(title);

            var selector = document.createElement('div');
            selector.className = 'selector';
            selector.style.position = 'absolute';
            selector.style.zIndex = '1';
            selector.style.right = '0';
            selector.style.top = '0';
            selector.style.height = '100%';
            container.appendChild(selector);

            var optionsList = document.createElement('ul');
            optionsList.className = 'options';
            optionsList.style.display = 'none';
            optionsList.style.position = 'absolute';
            optionsList.style.zIndex = '777';
            optionsList.style.width = '115%';
            optionsList.style.maxHeight = '200px';
            optionsList.style.overflow = 'auto';
            container.appendChild(optionsList);

            var optionHtml = '',
            optionValue = '',
            optionTitle = '',
            optionClassName = '',
            fullClassName = '',
            option = null,
            options = select.getElementsByTagName('option');
            for (var j = 0, m = options.length; j < m; j++) {
                option = document.createElement('li');
                optionValue = options[j].value;
                optionHtml = options[j].innerHTML;
                optionTitle = optionHtml.replace(/&amp;/gi, "&");
                optionsList.appendChild(option);
                fullClassName = 'option' + ' ' + optionValue + ((optionClassName = options[j].className) ? ' ' + optionClassName : '');
                option.setAttribute('title', optionTitle);
                option.setAttribute('value', optionValue);
                if (selectValue === optionValue) {
                    title.innerHTML = optionHtml;
                    fullClassName += ' selected';
                }
                option.selected = selectValue === optionValue;
                option.innerHTML = optionHtml;
                option.className = fullClassName;
            }
            if (AdditionalBottomOption !== undefined && AdditionalBottomOption !== "")
                optionsList.appendChild(AdditionalBottomOption);

            select.parentNode.insertBefore(container, select);
            container.appendChild(select);

            select.className += (select.className.length ? ' ' : '') + 'originalSelect';
            select.style.display = 'none';

            if (hiddenBorder) {
                jq(container).addClass('comboBoxHiddenBorder');
            }


            jq(optionsList).bind('selectstart', function() {
                return false;
            }).mousedown(function() {
                return false;
            }).click(function(evt) {
                var $target = jq(evt.target);
                if ($target.hasClass('option')) {
                    var 
                containerNewValue = evt.target.getAttribute('value'),
                    $container = $target.parents('div.customSelect:first'),
                    container = $container.get(0);
                    if (!container || container.getAttribute('value') === containerNewValue) {
                        return undefined;
                    }
                    container.setAttribute('value', containerNewValue);
                    $container.find('li.option').removeClass('selected').filter('li.' + containerNewValue + ':first').addClass('selected');
                    $container.find('div.title:first').html($target.html() || '&nbsp;').attr('className', 'title ' + containerNewValue);
                    $container.find('select.originalSelect:first').val(containerNewValue).change();
                }
            });
            if (jq.browser.msie && jq.browser.version < 7) {
                jq(optionsList).find('li.option').hover(

            function() {
                jq(this).addClass('hover')
            }, function() {
                jq(this).removeClass('hover')
            });
            }

            jq(selector).add(title).bind('selectstart', function() {
                return false;
            }).mousedown(function() {
                return false;
            }).click(function(evt) {
                var $options = jq(this.parentNode).find('ul.options:first');
                if ($options.is(':hidden')) {
                    $options.css({
                        top: jq(this.parentNode).height() + 1 + 'px'
                    }).slideDown(1, function() {
                        jq(document).one('click', function() {
                            jq('div.customSelect ul.options').hide();
                            jq(container).removeClass('comboBoxHiddenBorderFocused');
                        });
                    });

                    if (hiddenBorder) {
                        if (!jq(container).hasClass('comboBoxHiddenBorderFocused')) {
                            container.className = 'comboBoxHiddenBorderFocused ' + container.className;
                        }
                        var leftOffset = jq(container).width() - jq(select).width() - 1;
                        //						if (jQuery.browser.mozilla) { leftOffset -= 1;	}
                        $options.css({
                            'width': jq(select).width(),
                            'border-top': '1px solid #c7c7c7',
                            'left': leftOffset,
                            'top': jq(container).height()
                        });
                    }
                }
            });
        }
    };

    this.CloseAddContentDialog = function() {
        jq.unblockUI();
        return false;
    };

    this.ShowAddContentDialog = function() {
        jq.blockUI({
            message: jq("#studio_AddContentDialog"),
            css: {
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '400px',
                height: '350px',
                cursor: 'default',
                textAlign: 'left',
                backgroundColor: 'transparent',
                marginLeft: '-200px',
                top: '25%'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            fadeIn: 0,
            fadeOut: 0
        });
    };

    this.ToggleSidePanel = function() {
        var isVisible = jq('#' + this.SidePanelElementID).is(':visible');

        if (this.OnSideChangeState != null)
            this.OnSideChangeState(!isVisible);

        AjaxPro.onLoading = function(b) { };
        if (isVisible) {
            WebStudio.SaveSidePanelState(false, function(res) { });
            jq('#' + this.SidePanelElementID).hide('fast', function() {
                jq('#' + StudioManager.SidePanelButtonElementID).attr('src', SkinManager.GetImage('side_expand.png'));
            });
        }
        else {
            WebStudio.SaveSidePanelState(true, function(res) { });
            jq('#' + this.SidePanelElementID).show();
            jq('#' + this.SidePanelButtonElementID).attr('src', SkinManager.GetImage('side_collapse.png'));
        }
    };

    this.ToggleProductActivity = function(productID) {
        if (jq('#studio_product_activity_' + productID).is(':visible')) {
            jq('#studio_activityProductState_' + productID).attr('src', SkinManager.GetImage('collapse_right_dark.png'));
            jq('#studio_product_activity_' + productID).hide();
        }
        else {
            jq('#studio_activityProductState_' + productID).attr('src', SkinManager.GetImage('collapse_down_dark.png'));
            jq('#studio_product_activity_' + productID).show();
        }
    };

    this.Disable = function(obj_id) {
        jq('#' + obj_id).addClass("disableLinkButton");
        jq('#' + obj_id).removeClass("baseLinkButton");
    };

    this.Enable = function(obj_id) {
        jq('#' + obj_id).addClass("baseLinkButton");
        jq('#' + obj_id).removeClass("disableLinkButton");
    };
};

//------------fck uploads for comments-------------------
var FCKCommentsController = new function() {
    this.Callback = null;
    this.EditCommentHandler = function(commentID, text, domain, isEdit, callback) {
        this.Callback = callback;
        if (text == null || text == undefined)
            text = "";

        CommonControlsConfigurer.EditCommentComplete(commentID, domain, text, isEdit, this.CallbackHandler);
    };

    this.CancelCommentHandler = function(commentID, domain, isEdit, callback) {
        this.Callback = callback;
        CommonControlsConfigurer.CancelCommentComplete(commentID, domain, isEdit, this.CallbackHandler);
    };

    this.RemoveCommentHandler = function(commentID, domain, callback) {
        this.Callback = callback;
        CommonControlsConfigurer.RemoveCommentComplete(commentID, domain, this.CallbackHandler);
    };

    this.CallbackHandler = function(result) {
        if (FCKCommentsController.Callback != null && FCKCommentsController.Callback != '');
        eval(FCKCommentsController.Callback + '()');
    };
};

//------------subscriptions-------------------
var CommonSubscriptionManager = new function() {
    this.ConfirmMessage = 'Are you sure?';

    this.SubscribeToWhatsNew = function() {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.SubscribeToWhatsNew(function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                jq('#studio_newSubscriptionButton').html(res.rs2);

            else
                jq('#studio_newSubscriptionButton').html('<div class="errorBox">' + res.rs2 + '</div>');
        });
    };

    this.SubscribeToAdminNotify = function() {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.SubscribeToAdminNotify(function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                jq('#studio_adminSubscriptionButton').html(res.rs2);

            else
                jq('#studio_adminSubscriptionButton').html('<div class="errorBox">' + res.rs2 + '</div>');
        });
    };

    this.UnsibscribeModule = function(productID, moduleID) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.UnsubscribeModule(productID, moduleID, function(result) {
            var res = result.value;
            var productID = res.rs2;
            var moduleID = res.rs3;

            if (res.rs1 == '1') {
                jq('#studio_module_subscribeBox_' + productID + '_' + moduleID).remove();
                jq('#studio_module_subscriptions_' + productID + '_' + moduleID).remove();
            }
            else
                jq('#studio_module_subscriptions_' + productID + '_' + moduleID).html(res.rs4);
        });
    };

    this.UnsibscribeProduct = function(productID) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.UnsubscribeProduct(productID, function(result) {
            var res = result.value;
            var productID = res.rs2;

            if (res.rs1 == '1') {
                jq('#studio_product_subscribeBox_' + productID+' a.unsubscribe').remove();
                jq('#studio_product_subscriptions_' + productID).html('');
            }
            else
                jq('#studio_product_subscriptions_' + productID).html(res.rs4);
        });
    };

    this.UnsibscribeType = function(productID, moduleID, subscribeType) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + subscribeType).block();
            else
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + subscribeType).unblock();
        }

        SubscriptionManager.UnsubscribeType(productID, moduleID, subscribeType, function(result) {
            var res = result.value;
            var productID = res.rs2;
            var moduleID = res.rs3;
            var typeID = res.rs4;

            if (res.rs1 == '1') {
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID).remove();
                if (jq('div[id^="studio_subscribeType_' + productID + '_' + moduleID + '_"]').length == 0) {
                    jq('#studio_module_subscribeBox_' + productID + '_' + moduleID).remove();
                    jq('#studio_module_subscriptions_' + productID + '_' + moduleID).remove();
                }
            }
            else
                jq('#forum_subscribeType_' + productID + '_' + moduleID + '_' + typeID).html(res.rs5);
        });
    };


    this.UnsibscribeObjects = function(productID, moduleID, subscribeType) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + subscribeType).block();
            else
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + subscribeType).unblock();
        }

        var items = new Array();
        jq('input[id^="studio_subscribeItemChecker_' + productID + '_' + moduleID + '_' + subscribeType + '_"]:checked').each(function(i, n) {
            items.push(jq(this).val());
        });

        SubscriptionManager.UnsubscribeObjects(productID, moduleID, subscribeType, items, function(result) {
            var res = result.value;
            var productID = res.rs2;
            var moduleID = res.rs3;
            var typeID = res.rs4;
            var items = res.rs5.split(',');
            if (res.rs1 == '1') {
                for (var i = 0; i < items.length; i++)
                    jq('#studio_subscribeItem_' + productID + '_' + moduleID + '_' + typeID + '_' + items[i]).remove();


                if (jq('div[id^="studio_subscribeItem_' + productID + '_' + moduleID + '_' + typeID + '_"]').length == 0)
                    jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID).remove();
            }
            else
                jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID).html(res.rs6);

        });
    };

    this.UnsibscribeObject = function(productID, moduleID, subscribeType, objID) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_subscribeItem_' + productID + '_' + moduleID + '_' + subscribeType + '_' + objID).block();
            else
                jq('#studio_subscribeItem_' + productID + '_' + moduleID + '_' + subscribeType + '_' + objID).unblock();
        }

        SubscriptionManager.UnsubscribeObject(productID, moduleID, subscribeType, objID, function(result) {
            var res = result.value;
            var productID = res.rs2;
            var moduleID = res.rs3;
            var typeID = res.rs4;
            var objID = res.rs5;
            if (res.rs1 == '1') {
                jq('#studio_subscribeItem_' + productID + '_' + moduleID + '_' + typeID + '_' + objID).remove();
                if (jq('#studio_subscriptions_' + productID + '_' + moduleID + '_' + typeID).html() == '') {
                    jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID).remove();
                    jq('#studio_subscriptions_' + productID + '_' + moduleID + '_' + typeID).remove();
                }
            }
            else
                jq('#studio_subscribeItem_' + productID + '_' + moduleID + '_' + typeID + '_' + objID).html(res.rs6);

        });
    };

    this.ToggleProductList = function(productID) {
        if (jq('#studio_product_subscriptions_' + productID).is(':visible')) {
            jq('#studio_subscribeProductState_' + productID).attr('src', SkinManager.GetImage('collapse_right_dark.png'));
            jq('#studio_product_subscriptions_' + productID).hide();
        }
        else {
            jq('#studio_subscribeProductState_' + productID).attr('src', SkinManager.GetImage('collapse_down_dark.png'));
            jq('#studio_product_subscriptions_' + productID).show();
        }
    };

    this.ToggleModuleList = function(productID, moduleID) {
        if (jq('#studio_module_subscriptions_' + productID + '_' + moduleID).is(':visible')) {
            jq('#studio_subscribeModuleState_' + productID + '_' + moduleID).attr('src', SkinManager.GetImage('collapse_right_dark.png'));
            jq('#studio_module_subscriptions_' + productID + '_' + moduleID).hide();
        }
        else {
            jq('#studio_subscribeModuleState_' + productID + '_' + moduleID).attr('src', SkinManager.GetImage('collapse_down_dark.png'));
            jq('#studio_module_subscriptions_' + productID + '_' + moduleID).show();
        }
    }

    this.ToggleSubscriptionList = function(productID, moduleID, typeID) {
        var subscriptionState = jq('#studio_subscriptionsState_' + productID + '_' + moduleID + '_' + typeID);

        var subscriptionElementID = 'studio_subscriptions_' + productID + '_' + moduleID + '_' + typeID;
        var subscriptionElement = jq('#' + subscriptionElementID);
        if (subscriptionElement == null || subscriptionElement.attr('id') == null) {
            subscriptionState.attr('src', SkinManager.GetImage('mini_loader.gif'));


            AjaxPro.onLoading = function(b) {
                if (b)
                    jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID).block();
                else
                    jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID).unblock();
            }


            SubscriptionManager.RenderGroupItemSubscriptions(productID, moduleID, typeID, function(res) {
                var el = jq('#studio_subscribeType_' + productID + '_' + moduleID + '_' + typeID);
                var resultHTML = res.value;
                if (resultHTML == null || '' == resultHTML) {
                    resultHTML = "<div id='" + subscriptionElementID + "' style='height: 0px;'>&nbsp</div>";
                }
                el.html(el.html() + resultHTML);
            });
        }

        if (subscriptionElement.is(':visible')) {
            subscriptionState.attr('src', SkinManager.GetImage('collapse_right_light.png'));
            subscriptionElement.hide();
        }
        else {
            subscriptionState.attr('src', SkinManager.GetImage('collapse_down_light.png'));
            subscriptionElement.show();
        }
    };

    this.SaveNotifySenders = function(productID, moduleID, typeID) {
        var settings = ''
        jq('input[id^="studio_ns_' + productID + '_' + moduleID + '_' + typeID + '_"]:checked').each(function(i, n) {
            if (i != 0)
                settings += '$';

            settings += jq(this).val();
        });

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }

        SubscriptionManager.SaveNotifySenders(productID, moduleID, typeID, settings, function(result) {
            var res = result.value;
        });

    };


    this.InitNotifyByComboboxes = function() {
        jq('select[id^="NotifyByCombobox_"]').each(
			function() {
			    StudioManager.createCustomSelect(this.id, true);
			}
		);
    }


    this.SetNotifyByMethod = function(productID, notifyBy) {

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.SetNotifyByMethod(productID, notifyBy,
			function(result) {
			}
		);
    };

    this.SetWhatsNewNotifyByMethod = function(notifyBy) {

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.SetWhatsNewNotifyByMethod(notifyBy,
			function(result) {
			}
		);
    };
    this.SetAdminNotifyNotifyByMethod = function(notifyBy) {

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        }
        SubscriptionManager.SetAdminNotifyNotifyByMethod(notifyBy,
			function(result) {
			}
		);
    };
};

var CancelButtonController = new function() {

    this.ConfirmCancellationWithPrevValue = function(fckEditorID, prevValue) {
        try {
            if (!this.IsEmptyFckEditorTextFieldWithPrevValue(fckEditorID, prevValue)) {
                return window.confirm(CommonJavascriptResources.CancelConfirmMessage);
            }
        } catch (err) { }
        return true;
    };

    this.IsEmptyFckEditorTextFieldWithPrevValue = function(fckEditorID, prevValue) {
        try {
            var curValue = FCKeditorAPI.GetInstance(fckEditorID).GetXHTML();
            return this.IsEmptyTextField(curValue) || (prevValue && curValue == prevValue);
        } catch (err) { }
        return true;
    };

    this.IsEmptyTextField = function(text) {
        if (text.replace(/<\/?\s*(br|p|div)[\s\S]*?>|&nbsp;?|\s+/gi, '') == '') {
            return true;
        }
        return false;
    };

    this.IsEmptyFckEditorTextField = function(fckEditorID) {
        try {
            return this.IsEmptyTextField(FCKeditorAPI.GetInstance(fckEditorID).GetXHTML());
        } catch (err) { }
        return true;
    };
};

String.prototype.format = function() {
    var txt = this,
        i = arguments.length;

    while (i--) {
        txt = txt.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return txt;
};

var EventTracker = new function() {
    this.Track = function(event) {

        try {
            var pageTracker = _gat._getTracker("UA-12442749-4");

            if (pageTracker != null)
                pageTracker._trackPageview(event);

        } catch (err) { }    
    }
}