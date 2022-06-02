function viewSwitcherDropdownToggle(switcherUI, dropdownID) {

	var targetPos = jq(switcherUI).offset();
	var dropdownItem = jq("#" + dropdownID);

	var elemPosTop = targetPos.top + jq(switcherUI).outerHeight();
	var elemPosLeft = targetPos.left;

	var w = jq(window);
	var TopPadding = w.scrollTop();
	var LeftPadding = w.scrollLeft();
	var ScrWidth = w.width();
	var ScrHeight = w.height();

	if ((targetPos.left + dropdownItem.width()) > (LeftPadding + ScrWidth)) {
		elemPosLeft -= targetPos.left + dropdownItem.width() - (LeftPadding + ScrWidth);
	}

	var itemsCount = dropdownItem.children().length;
	dropdownItem.attr('class', '');
	if (itemsCount <= 15) {
		dropdownItem.addClass('viewSwitcherDropdownWithoutScroll');
	} else {
		dropdownItem.addClass('viewSwitcherDropdownWithScroll');
	}

	if ((targetPos.top + dropdownItem.outerHeight()) > (TopPadding + ScrHeight)) {
		elemPosTop = targetPos.top - dropdownItem.outerHeight();
	}

	dropdownItem.css(
		{
			'position': 'absolute',
			'top': elemPosTop,
			'left': elemPosLeft
		}
	);
	
	dropdownItem.toggle();
}

function viewSwitcherDropdownRegisterAutoHide(event, switcherID, dropdownID) {

	if (!jq((event.target) ? event.target : event.srcElement)
              .parents()
              .andSelf()
              .is("'#" + switcherID + ", #" + dropdownID + "'"))
		jq("#" + dropdownID).hide();
}

function viewSwitcherToggleTabs(tabID) {

	var tab = jq('#' + tabID);

	tab.parent().children().each(
		function() {
	var child = jq(this);
			viewSwitcherToggleCurrentTab(child, tabID);
		}
	);
}

function viewSwitcherToggleCurrentTab(tab, tabID) {
	var hideFlag = true;
	var currentTabID = tab.attr('id');	

	if (tab.hasClass('viewSwitcherTabSelected')) {
		tab.attr('class', 'viewSwitcherTab');
	}
	
	if (currentTabID == tabID) {
		tab.attr('class', 'viewSwitcherTabSelected');
		
		hideFlag = false;
	}

	var currentDivID = currentTabID.replace(/_ViewSwitcherTab$/gi, '');

	if (currentDivID != '') {
		if (hideFlag) {
			jq('#' + currentDivID).hide();
		} else {
			jq('#' + currentDivID).show();
		}
	}
}