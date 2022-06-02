Array.prototype.remove = function(from, to) {
    var rest = this.slice((to || from) + 1 || this.length);
    this.length = from < 0 ? this.length + from : from;
    return this.push.apply(this, rest);
};

window.ASC.Files.Plugin = (function($) {
    var isInit = false;

    var curID = -1;

    var _startInstallApp = false;
    var _sliderTimer = null;

    var _openOfficeState = 0;

    var OPEN_OFFICE_STATE_NOT_INSTALLED = 0;
    var OPEN_OFFICE_STATE_INSTALLING = 1;
    var OPEN_OFFICE_STATE_INSTALLED = 2;

    var PLUGIN_STATUS_ERROR = 1;
    var PLUGIN_STATUS_IDLE = 2;
    var PLUGIN_STATUS_INITIALIZING = 3;
    var PLUGIN_STATUS_DOWNLOADING = 4;
    var PLUGIN_STATUS_STARTING = 5;
    var PLUGIN_STATUS_ATTACHING = 6;
    var PLUGIN_STATUS_ATTACHED = 7;
    var PLUGIN_STATUS_APP_ERROR = 8;
    var PLUGIN_STATUS_SDK_DOWNLOADING = 9;
    var PLUGIN_STATUS_SDK_UPDATED = 10;
    var PLUGIN_STATUS_UNPACKED = 11;
    var PLUGIN_STATUS_APP_UPD_DECIS = 13;

    var PLUGIN_APP_STATE_ABSENCE = 1;
    var PLUGIN_APP_STATE_BUSY = 2;
    var PLUGIN_APP_STATE_LATESTVERSION = 3;
    var PLUGIN_APP_STATE_HASUPDATE = 4;

    var PLUGIN_DECISION_USECURRENT = 1;
    var PLUGIN_DECISION_UPDATE = 2;
    var PLUGIN_DECISION_UPDATEANDSTOP = 3;
    var PLUGIN_DECISION_STOP = 4;

    var PLUGIN_ERROR_OK = 0;
    var PLUGIN_ERROR_UNEXPECTED = 1;
    var PLUGIN_ERROR_REINSTALL = 2;


    var GetOpenOfficeState = function() { return _openOfficeState; }
    var SetOpenOfficeState = function(state) { _openOfficeState = state; }

    var Init = function() {
        if (navigator.appVersion.indexOf("Win") == -1 || jq.browser.safari || jq.browser.opera) return;

        jq(document).unload(function() {

            jq("id=^virtObj_").each(function() {

                if (this.Status != PLUGIN_STATUS_IDLE) return;


                jq(this).remove();

            });
        });

        ASC.Files.UI.displayLoading(null, true);
        jq("#install_completed span").text(ASC.Files.Resources.InstallOpenOfficeCongratulation);
        jq("#install_process").show();
        jq("#install_completed").hide();

        var sliderItems = jq("#slider div.slider-item");

        sliderItems.hide();
        jq(sliderItems[0]).show();

        jq("#slider ul a").click(function() {
            SelectedSlide(jq(this).attr("rel"));
        });

        jq("#plugin_informer").click(function(event) {

            var isHowToElementEventFire = jq(event.target).parent().parent().hasClass("plugin_informer_info");
            var isButtonCloseElementEventFire = jq(event.target).hasClass("btn_close");
            var isButtonInstall = jq(event.target).hasClass("btn_install");

            if (!(isHowToElementEventFire || isButtonCloseElementEventFire || isButtonInstall))
                jq("#plugin_informer span.btn_install").click();
        });

        jq("#plugin_informer").insertAfter(jq("div.studioTopNavigationPanel"));
        jq("#plugin_informer div.btn_close").click(function() { jq("#plugin_informer").slideUp(); });

        if (!CheckPluginInstalled()) {

            ASC.Files.UI.hideLoading(true);

            InitPluginInformer(ASC.Files.Resources.InstallPluginHeader,
                              ASC.Files.Resources.InstallPluginDescription.format("<span>", "</span>"),
                              ASC.Files.Resources.InstallPluginButton,
                              DownloadPlugin,
                              false);

            _openOfficeState = OPEN_OFFICE_STATE_NOT_INSTALLED;
            return;
        }

        DetectOpenOfficeApp();
    };

    var CheckPluginInstalled = function() {
        if (jq("#virtObj_").length == 0)
            CreatePlugin("", true);

        var obj = document.getElementById("virtObj_");

        var Installed = obj != null && obj.Status != null;

        jq("#virtObj_").remove();

        return Installed;
    };

    var RunOpenOfficeApp = function(postUrl, fileID) {
        var obj = CreatePlugin(fileID, true);
        ASC.Files.UI.displayLoading(null, true);

        obj.Init(ASC.Files.Constants.OOO_ID, 4 | 8 | 16, ASC.Files.Common.getSitePath() + serviceManager.getDocPath(), postUrl);
        setTimeout("ASC.Files.Plugin.OnMonitorPluginStatus('" + fileID + "')", 250);

        try {
            var pageTracker = _gat._getTracker("UA-12442749-4");

            if (pageTracker != null)
                pageTracker._trackPageview('start_open_office');

        } catch (err) { }

        jq.ajax({
            async: true,
            type: "get",
            url: jq.format(ASC.Files.Constants.URL_HANDLER_TRACK, 2)
        });
    };

    var CreatePlugin = function(id, refreshFlag) {
        var ua = navigator.userAgent.toLowerCase();
        if (ua.indexOf("msie") != -1 && ua.indexOf("opera") == -1) {
            jq("#virtObj_" + id).remove();
            jq("#objectDiv").append('<object id="virtObj_' + id + '" style="width:0;height:0;" classid="clsid:' + ASC.Files.Constants.PLUGIN_ID + '"></object>');
        }
        if ((ua.indexOf("opera") != -1) || (ua.indexOf("gecko") != -1)) {
            if (refreshFlag)
                navigator.plugins.refresh(false);

            if (navigator.mimeTypes && navigator.mimeTypes["application/x-vnd-asc-virtualization"]) {
                jq("#virtObj_" + id).remove();
                jq("#objectDiv").append('<object id="virtObj_' + id + '" style="width:0;height:0;" type="application/x-vnd-asc-virtualization"></object>');
            }
        }

        return jq("#virtObj_" + id)[0];
    };

    var DownloadPlugin = function() {
        window.open(ASC.Files.Constants.URL_PLUGIN_PATH, '_blank');

        var checkPluginInstalledTimerID = setInterval(function() {

            if (!CheckPluginInstalled()) return;

            InstallOpenOfficeApp();

            clearInterval(checkPluginInstalledTimerID);

            try {
                var pageTracker = _gat._getTracker("UA-12442749-4");

                if (pageTracker != null)
                    pageTracker._trackPageview('install_plugin');

            } catch (err) { }


            jq.ajax({
                async: true,
                type: "get",
                url: jq.format(ASC.Files.Constants.URL_HANDLER_TRACK, 1)
            });

        }, 2000);
    };

    var isFakeObj = function(id) { return id == ""; };

    var OnMonitorPluginStatus = function(id) {

        try {

            var obj = document.getElementById("virtObj_" + id);

            var status = obj.Status;

            switch (status) {
                case PLUGIN_STATUS_ERROR:

                    switch (obj.Error) {

                        case PLUGIN_ERROR_UNEXPECTED:
                            break;

                        case PLUGIN_ERROR_REINSTALL:
                            InitPluginInformer(ASC.Files.Resources.InstallPluginHeader,
                                              ASC.Files.Resources.InstallPluginDescription.format("<span>", "</span>"),
                                              ASC.Files.Resources.InstallPluginButton,
                                              ASC.Files.Plugin.DownloadPlugin,
                                              true);

                            jq("#plugin_informer").slideDown();

                            _openOfficeState = OPEN_OFFICE_STATE_NOT_INSTALLED;

                            return;

                            break;
                    }
                    break;
                case PLUGIN_STATUS_IDLE:

                    jq(obj).remove();
                    jq("#file_editing_icn_" + id).hide();
                    jq("#file_IsEditing_" + id).val('false');
                    return;

                    break;
                case PLUGIN_STATUS_INITIALIZING:
                    break;
                case PLUGIN_STATUS_UNPACKED:
                    var progress = obj.Progress;
                    _openOfficeState = OPEN_OFFICE_STATE_INSTALLING;

                    jq("#install_process div.files_progress_box span.headerBaseMedium").text(ASC.Files.Resources.OpenOfficeUnpacking);
                    jq("#install_process div.files_progress_box span.textSmallDescribe").html("&nbsp;");
                    jq("#install_process div.files_progress_box div.progress").css('width', progress + '%');
                    jq("#install_process div.files_progress_box span.percent").text(progress + '%').css("color", progress > 45 ? "white" : "#1A6DB3");

                    if (progress > 95) {

                        clearInterval(_sliderTimer);

                        _openOfficeState = OPEN_OFFICE_STATE_INSTALLED;

                        jq("#install_process").hide();
                        jq("#install_completed").show();

                        return;
                    }
                    break;

                case PLUGIN_STATUS_DOWNLOADING:
                    var progress = obj.Progress;
                    if (progress == 0) break;

                    _openOfficeState = OPEN_OFFICE_STATE_INSTALLING;

                    jq("#install_process div.files_progress_box span.textSmallDescribe").text(jq.format(ASC.Files.Resources.OpenOfficeDownloadingInfo, 158 * progress / 100, 158));
                    jq("#install_process div.files_progress_box div.progress").css('width', progress + '%');
                    jq("#install_process div.files_progress_box span.percent").text(progress + '%').css("color", progress > 45 ? "white" : "#1A6DB3");

                    break;

                case PLUGIN_STATUS_STARTING:
                    break;
                case PLUGIN_STATUS_ATTACHING:
                    ASC.Files.UI.hideLoading(true);
                    break;
                case PLUGIN_STATUS_ATTACHED:
                    break;
                case PLUGIN_STATUS_APP_ERROR:
                    break;
                case PLUGIN_STATUS_SDK_DOWNLOADING:
                    break;
                case PLUGIN_STATUS_SDK_UPDATED:
                    break;
                case PLUGIN_STATUS_APP_UPD_DECIS:
                    if (!isFakeObj(id)) {
                        obj.SetDecision(PLUGIN_DECISION_USECURRENT);
                        break;
                    }

                    ASC.Files.UI.hideLoading(true);

                    switch (obj.AppState) {
                        case PLUGIN_APP_STATE_ABSENCE:

                            if (_startInstallApp) {

                                ShowInstallOpenOfficeBox();

                                obj.SetDecision(PLUGIN_DECISION_UPDATEANDSTOP);

                                break;
                            }

                            _openOfficeState = OPEN_OFFICE_STATE_NOT_INSTALLED;

                            InitPluginInformer(ASC.Files.Resources.InstallOpenOfficeHeader,
                                               "<span>" + ASC.Files.Resources.InstallOpenOfficeDescription + "</span>",
                                              ASC.Files.Resources.InstallOpenOfficeButton,
                                              ASC.Files.Plugin.InstallOpenOfficeApp,
                                              false);

                            jq("#plugin_informer").slideDown();

                            obj.SetDecision(PLUGIN_DECISION_STOP);

                            return;

                            break;
                        case PLUGIN_APP_STATE_LATESTVERSION:
                        case PLUGIN_APP_STATE_BUSY:
                            _openOfficeState = OPEN_OFFICE_STATE_INSTALLED;
                            obj.SetDecision(PLUGIN_DECISION_STOP);
                            return;
                        case PLUGIN_APP_STATE_HASUPDATE:
                            _openOfficeState = OPEN_OFFICE_STATE_INSTALLED;

                            if (_startInstallApp) {

                                ShowInstallOpenOfficeBox();

                                obj.SetDecision(PLUGIN_DECISION_UPDATEANDSTOP);
                                break;
                            }

                            InitPluginInformer(ASC.Files.Resources.UpdateOpenOfficeHeader,
                                              "<span>" + ASC.Files.Resources.InstallOpenOfficeDescription + "</span>",
                                              ASC.Files.Resources.InstallOpenOfficeButton,
                                              ASC.Files.Plugin.InstallOpenOfficeApp,
                                              true);

                            jq("#plugin_informer").slideDown();

                            obj.SetDecision(PLUGIN_DECISION_STOP);

                            return;

                            break;
                    }
                    break;
            }

            setTimeout("ASC.Files.Plugin.OnMonitorPluginStatus('" + id + "')", 250);
        }
        catch (err) {
            ASC.Files.UI.displayInfoPanel("OnProgress Error: " + err.number, true);
        }
    };

    var ShowInstallOpenOfficeBox = function() {
        jq("#install_process div.files_progress_box span.headerBaseMedium").text(ASC.Files.Resources.OpenOfficeDownloading);
        jq("#install_process div.files_progress_box span.textSmallDescribe").text(jq.format(ASC.Files.Resources.OpenOfficeDownloadingInfo, 158 * 0 / 100, 158));

        jq("#install_process div.files_progress_box div.progress").css('width', 0 + '%');
        jq("#install_process div.files_progress_box span.percent").text(0 + '%').css("color", "#1A6DB3");

        _sliderTimer = setInterval(function() {
            var index = (parseInt(jq("#slider ul a.slider-active").attr("rel")) || 0) + 1;

            if (index > 5) index = 0;
            SelectedSlide(index);

        }, 4000);

        ASC.Files.Common.blockUI(jq("#install_plugin_box"), 500, 385);
    };

    var InstallOpenOfficeApp = function() {
        _startInstallApp = true;

        ASC.Files.UI.displayLoading(null, true);

        if (jq("#virtObj_").length == 0)
            CreatePlugin("", true);

        var obj = document.getElementById("virtObj_");

        _openOfficeState = OPEN_OFFICE_STATE_INSTALLING;

        jq("#plugin_informer").slideUp();

        obj.Init(ASC.Files.Constants.OOO_ID, 4 | 8 | 16, "", "");

        setTimeout("ASC.Files.Plugin.OnMonitorPluginStatus('')", 250);
    };

    var DetectOpenOfficeApp = function() {
        if (jq("#virtObj_").length == 0)
            CreatePlugin("", true);

        var obj = document.getElementById("virtObj_");

        obj.Init(ASC.Files.Constants.OOO_ID, 4 | 8 | 16, "", "");

        setTimeout("ASC.Files.Plugin.OnMonitorPluginStatus('')", 250);
    };

    var SelectedSlide = function(index) {
        var sliderActive = jq("#slider ul a.slider-active");
        jq("#slider div.slider-item:eq(" + sliderActive.attr("rel") + ")").hide();
        sliderActive.removeClass("slider-active");
        jq("#slider ul a:eq(" + index + ")").addClass("slider-active");
        jq("#slider div.slider-item:eq(" + index + ")").show();
    };

    var InitPluginInformer = function(installPluginHeaderText,
                                    installPluginDescriptionText,
                                    installPluginButtonText,
                                    btnInstallHandler,
                                    hideProgress) {

        jq("#plugin_informer span.btn_install").unbind("click");

        jq("#plugin_informer span.btn_install").click(function() {
            btnInstallHandler();
            return false;
        });

        jq("#plugin_informer span.headerBase").html(installPluginHeaderText);
        jq("#plugin_informer p").html(installPluginDescriptionText);
        jq("#plugin_informer span.btn_install").text(installPluginButtonText);
        jq("#plugin_informer span.btn_install").attr('title', installPluginButtonText);

        jq("#plugin_informer div.plugin_informer_info p span").click(function() {

            ASC.Files.Common.blockUI(jq("#how_it_works_box"), 900, 385);
        });

        if (hideProgress)
            jq("#install_process div.files_progress_box").hide();
    };

    return {
        Init: Init,
        RunOpenOfficeApp: RunOpenOfficeApp,
        OnMonitorPluginStatus: OnMonitorPluginStatus,
        InstallOpenOfficeApp: InstallOpenOfficeApp,
        CheckPluginInstalled: CheckPluginInstalled,
        GetOpenOfficeState: GetOpenOfficeState,
        OPEN_OFFICE_STATE_NOT_INSTALLED: OPEN_OFFICE_STATE_NOT_INSTALLED,
        OPEN_OFFICE_STATE_INSTALLING: OPEN_OFFICE_STATE_INSTALLING,
        OPEN_OFFICE_STATE_INSTALLED: OPEN_OFFICE_STATE_INSTALLED,
        SelectedSlide: SelectedSlide
    };

})(jQuery);

(function($) {
    $(function() {
        ASC.Files.Plugin.Init();
    });
})(jQuery);