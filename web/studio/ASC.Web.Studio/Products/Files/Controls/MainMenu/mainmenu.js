window.ASC.Files.MainMenu = (function($) {
    var isInit = false;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            jq(document).click(function(event) {
                ASC.Files.Common.dropdownRegisterAutoHide(event, "#files_newdoc_btn", "#files_newDocumentPanel");
            });
        }
    };

    var toggleMainMenu = function() {

        var mainMenuHolderSpacer = jq("#mainMenuHolderSpacer");
        var mainMenuHolder = jq("#mainMenuHolder");
        var newDocBtn = jq("#files_newdoc_btn img");

        var tempTop = 32;

        if (mainMenuHolderSpacer.css("display") == "none")
            tempTop += mainMenuHolder.offset().top;
        else
            tempTop += mainMenuHolderSpacer.offset().top;

        if (jq(window).scrollTop() > tempTop) {
            mainMenuHolderSpacer.show();
            mainMenuHolder.addClass("fixed");

            if (newDocBtn.length != 0) {
                jq("#files_newDocumentPanel").css({
                    'position': 'fixed',
                    'top': newDocBtn.offset().top + newDocBtn.outerHeight() - jq(window).scrollTop()
                });
            }

            if (ASC.Controls.Constants.isMobileAgent) {
                mainMenuHolder.css("top", jq(document).scrollTop() + "px");
            }
        }
        else {
            mainMenuHolderSpacer.hide();
            mainMenuHolder.removeClass("fixed");
            if (newDocBtn.length != 0) {
                jq("#files_newDocumentPanel").css({
                    'position': 'absolute',
                    'top': newDocBtn.offset().top + newDocBtn.outerHeight()
                });
            }
        }
    };

    var showCreateNewDoc = function() {
        var listTypes = "";
        listTypes += ASC.Files.Utils.FileCanBeEdit(ASC.Files.Utils.typeNewDoc.text) ? "" : "#files_create_text,";
        listTypes += ASC.Files.Utils.FileCanBeEdit(ASC.Files.Utils.typeNewDoc.spreadsheet) ? "" : "#files_create_spreadsheet,";
        listTypes += ASC.Files.Utils.FileCanBeEdit(ASC.Files.Utils.typeNewDoc.presentation) ? "" : "#files_create_presentation,";

        if (listTypes.length != 0) {
            switch (ASC.Files.Plugin.GetOpenOfficeState()) {
            case ASC.Files.Plugin.OPEN_OFFICE_STATE_NOT_INSTALLED:
            case ASC.Files.Plugin.OPEN_OFFICE_STATE_INSTALLING:
                jq(listTypes).css(
                    {
                        'opacity': 0.5,
                        'background-color': '#FFFFFF'
                    });
                break;
            case ASC.Files.Plugin.OPEN_OFFICE_STATE_INSTALLED:
                jq(listTypes).css(
                    {
                        'opacity': 1,
                        'background-color': ""
                    });
                break;
            }
        }

        ASC.Files.Common.dropdownToggle(jq("#files_newdoc_btn img"), "files_newDocumentPanel");
    };

    return {
        init: init,
        showCreateNewDoc: showCreateNewDoc,
        toggleMainMenu: toggleMainMenu
    };
})(jQuery);

(function($) {
    ASC.Files.MainMenu.init();

    $(function() {

        jq("#mainMenuHolderSpacer").css({ "height": jq("ul#mainMenuHolder").outerHeight() });

        jq("#files_newdoc_btn").click(function(event) {
            ASC.Files.MainMenu.showCreateNewDoc();
        });

        if (navigator.appVersion.indexOf("Win") == -1
            || jq.browser.safari
            || jq.browser.opera
            || !ASC.Files.Constants.PLUGIN_ENABLE) {
            if (!ASC.Files.Utils.FileCanBeEdit(ASC.Files.Utils.typeNewDoc.text))
                jq("#files_create_text").remove();
            if (!ASC.Files.Utils.FileCanBeEdit(ASC.Files.Utils.typeNewDoc.spreadsheet))
                jq("#files_create_spreadsheet").remove();
            if (!ASC.Files.Utils.FileCanBeEdit(ASC.Files.Utils.typeNewDoc.presentation))
                jq("#files_create_presentation").remove();

            if (jq("#files_newDocumentPanel ul li").length == 0) {
                jq("#files_newDocumentPanel").remove(); 
                jq("#files_newdoc_btn").parents("li").remove();
            }
        }

        jq(window).scroll(function() {
            ASC.Files.MainMenu.toggleMainMenu();
            return true;
        });

        jq(".comming_soon").unbind("click").click(function() {
            ASC.Files.UI.displayInfoPanel(ASC.Files.Resources.InfoCommingSoon, true);
            return false;
        });

    });
})(jQuery);