var ProjectTemplatesController = new function() {
	this.CreateProjectTemplate = function(id) {
		var title = jq('#CreateProjectTemplateInput').val();
		if (!title || jq.trim(title) == '') {
			ProjectTemplatesController.ShowErrorMessage(true, jq('#InvalidProjectTemplateTitleInputHidden').val());
			return;
		}
		ProjectTemplatesController.ShowErrorMessage(false);
		ProjectTemplates.CreateProjectTemplate(title,
			function(result) {
				if(result.error != null)
				{
				    ProjectTemplatesController.ShowErrorMessage(true, res.error.Message);
				    showHideActionButtonPanel(id, false);
				    jq('input[type="text"]').attr('disabled', false);
				}
				var res = result.value;
				if (res && res.Url) {
					document.location = res.Url;
					return;
				}
				ProjectTemplatesController.ShowErrorMessage(true, res.ErrorMessage);
				showHideActionButtonPanel(id, false);
				jq('input[type="text"]').attr('disabled', false);
			});
	};

	this.InputOnKeyDown = function(event, buttonID) {
		//Enter key was pressed
		if (event.keyCode == 13) {
			jq('#' + buttonID).click();
			return false;
		}
		return true;
	};

	this.TextAreaOnKeyDown = function(event, buttonID) {
		//Ctrl + Enter was pressed
		if (event.ctrlKey && event.keyCode == 13) {
			jq('#' + buttonID).click();
			return false;
		}
		return true;
	};

	this.ShowErrorMessage = function(showFlag, message) {
		var infoPanel = jq('div[id$="_InfoPanel"]');
		if (!showFlag) {
			infoPanel.hide();
			return;
		}
		infoPanel.parent().addClass('alert');
		if (message) {
			infoPanel.html(message).show();
		}
	};

	this.CreateMilestone = function() {
		var title = jq('#MilestoneTitleInput').val();
		if (!title || title == '') {
			title = ASC.Projects.Resources.UntitledMilestone;
			jq('#MilestoneTitleInput').val(title);
		}
		ProjectTemplatesController.ShowErrorMessage(false);
		MilestoneTemplates.CreateMilestone(title, jq('#ProjectIdHiddenInput').val(), jq('#notify_manager').is(':checked'), jq('#is_key').is(':checked'), jq('#MilestoneIdHiddenInput').val(),
			jq('#MilestoneWeekCombobox').val(),
			jq('#MilestoneDayCombobox').val(),
			function(result) {
				var res = result.value;
				if (!res || res == '') {
					document.location = jq('#CreateMilestoneCancelButton').attr('href');
					return;
				}
				ProjectTemplatesController.ShowErrorMessage(true, res);
			});
	};

	this.InitMilestone = function(isNotiry, isKey, week, day) {
		jq('#MilestoneTitleInput').val(jq("[id$=hfMilestoneTitle]").val());
		jq('#notify_manager').attr('checked', isNotiry);
		jq('#is_key').attr('checked', isKey);
		jq('#MilestoneWeekCombobox').val(week);
		jq('#MilestoneDayCombobox').val(day);
		jq('#MilestoneTitleInput').focus();
	}

	this.DeleteProjectTemplate = function(projectID, hideFlag) {
		if (!confirm(ASC.Projects.Resources.DeleteThisProjectTemplate)) {
			return;
		}
		jq.blockUI();
		TemplateOverview.DeleteProjectTemplate(projectID,
			function(result) {
				var res = result.value;
				if (hideFlag) {
					jq('#ProjectTemplateHolder_' + projectID).animate({ height: 'hide', opacity: 'hide' }, 200);
					setTimeout(function() {
						jq('#ProjectTemplateHolder_' + projectID).remove();
						ProjectTemplatesController.HighlightItems('ProjectTemplateHolder_');
						ProjectTemplatesController.ShowProjectTemplatesEmptyScreen();
					}, 250);
					jq.unblockUI();
				} else {
					document.location = res;
				}
			});
	};

	this.DeleteMilestoneTemplate = function() {
		if (!confirm(ASC.Projects.Resources.DeleteThisMilectone)) {
			return;
		}
		var milestoneID = jq('#MilestoneIdHiddenInput').val();
		var projectID = jq('#ProjectIdHiddenInput').val();

		MilestoneTemplates.DeleteMilestoneTemplate(milestoneID, projectID,
			function(result) {
				var res = result.value;
				document.location = res;
			});
	};

	this.CreateProjectFromTemplate = function(projectID) {
		jq.blockUI();
		TemplatesUtil.CreateProjectFromTemplate(projectID, function(result) {
			var url = result.value;
			if (url && url != '') {
				document.location = result.value;
				return;
			}
			jq.unblockUI();
		});
	};

	this.HighlightItems = function(itemUniquePrefix) {
		var i = 0;
		jq('[id^="{0}"]'.format(itemUniquePrefix)).each(function() {
			if (i++ % 2 == 0) {
				jq(this).addClass('tintMedium');
			} else {
				jq(this).removeClass('tintMedium');
			}
		});
	};

	this.ShowPopupItem = function(itemID, isOver, containerID, css) {
		var item = jq('#' + itemID);
		if (isOver) {
			item.show();
			if (containerID) {
				jq('#' + containerID).addClass(css);
			}
		} else {
			item.hide();
			if (containerID) {
				jq('#' + containerID).removeClass(css);
			}
		}
	};

	this.ChangeDropdownItem = function(templateID) {
		
		AjaxPro.ProjectActionView.GetProjectTemplateTitle(templateID,
		    function(res)
		    {
		        if(res.error!=null)
		        {
		            alert(res.error.Message);
		            return;
		        } 
		        
		        var title = res.value;
		        jq('#SelectedTemplateID').val(templateID);
		        jq('[id$="_TemplatesDropdownContainer"]').hide();
		        jq('#SelectedTemplateTitle').show();
		        var t = jq('#SelectedTemplateTitleHidden').val();
		        t = t.format("<span style='color: Black;'>«" + title + "»</span>");
		        jq('#SelectedTemplateTitle').html(t);
		        jq('#ClearSelectedTemplate').show();
		    });
	};

	this.ClearSelectedTemplate = function() {
		jq('#SelectedTemplateID').val(0);

		jq('[id$="_TemplatesDropdownContainer"]').show();
		jq('#SelectedTemplateTitle').hide();
		jq('#ClearSelectedTemplate').hide();
	};

	this.MoveProjectTemplatesCombo = function() {
		var offset_top = 54;
		var offset_bottom = 50;
		
		if(jq('div[id$=_commonContainer_InfoPanel]').css('display')=="block")
		{
		    offset_top = 90;
		    offset_bottom = 90;
		}
		
		var el = jq('div[id$="{0}"]'.format('_InfoPanel'));
		var flag = jq('div[id$="{0}"]'.format('_InfoPanel')).is(':visible');
		if (flag) {
			offset_top += el.outerHeight();
			offset_bottom += el.outerHeight();
		}
		jq('#TemplatesComboboxContainer').css({ 'margin-top': '-{0}px'.format(offset_top), 'margin-bottom': '{0}px'.format(offset_bottom) });
	};

	this.ShowProjectTemplatesEmptyScreen = function() {
		if (jq('div[id$=_ProjectTemplatesListContainer]').children().length == 0) {
			jq('#ProjectTemplatesMainContainer').children().each(function() { jq(this).hide(); });
			jq('div[id$=_EmpryScreenContainer]').show(false);
		} else {
			jq('div[id$=_EmpryScreenContainer]').hide();
		}
	};

	this.InitOverviewPage = function() {
		jq('.containerHeaderBlock').remove();
		jq('.infoPanel').remove();
		jq('.containerBodyBlock').css({ 'padding': 0 });
		jq('.studioLeftContent').css({ 'width': 'auto', 'float': 'none' });
	}
}