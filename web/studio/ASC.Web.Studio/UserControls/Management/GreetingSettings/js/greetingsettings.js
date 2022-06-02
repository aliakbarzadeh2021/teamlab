﻿jq(function() {
    jq('#saveGreetSettingsBtn').click(GreetingSettingsManager.SaveGreetingOptions);
    jq('#restoreGreetSettingsBtn').click(GreetingSettingsManager.RestoreGreetingOptions);

    if (jq('#studio_logoUploader').length > 0) {
        new AjaxUpload('studio_logoUploader', {
            action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.LogoUploader,ASC.Web.Studio',
            onSubmit: jq.blockUI,
            onComplete: GreetingSettingsManager.SaveGreetingLogo
        });
    } 

})

var GreetingSettingsManager = new function() {

    this.TimeoutHandler = null;

    this.SaveGreetingLogo = function(file, response) {
        jq.unblockUI();

        if (GreetingSettingsManager.TimeoutHandler)
            clearTimeout(GreetingSettingsManager.TimeoutHandler);

        var result = eval("(" + response + ")");
        if (result.Success) {
            jq('#studio_greetingLogo').attr('src', result.Message);
            jq('#studio_greetingLogoPath').val(result.Message);
        }
        else {
            jq('div[id^="studio_setInf"]').html('');
            jq('#studio_setInfGreetingSettingsInfo').html('<div class="errorBox">' + result.Message + '</div>');
            GreetingSettingsManager.TimeoutHandler = setTimeout(function() { jq('#studio_setInfGreetingSettingsInfo').html(''); }, 4000);
        }
    };

    this.SaveGreetingOptions = function() {
        if (this.TimeoutHandler)
            clearTimeout(this.TimeoutHandler);

        jq('#studio_greetingSettingsInfo').html('');
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_greetingSettingsBox').block();
            else
                jq('#studio_greetingSettingsBox').unblock();
        };

        GreetingSettingsController.SaveGreetingSettings(jq('#studio_greetingLogoPath').val(),
                                                jq('#studio_greetingHeader').val(),
                                                function(result) {

                                                    //clean logo path input
                                                    jq('#studio_greetingLogoPath').val('');
                                                    jq('div[id^="studio_setInf"]').html('');

                                                    var classCSS = (result.value.Status == 1 ? "okBox" : "errorBox");
                                                    jq('#studio_setInfGreetingSettingsInfo').html('<div class="' + classCSS + '">' + result.value.Message + '</div>');
                                                    GreetingSettingsManager.TimeoutHandler = setTimeout(function() { jq('#studio_setInfGreetingSettingsInfo').html(''); }, 4000);
                                                });
    };

    this.RestoreGreetingOptions = function() {
        if (this.TimeoutHandler)
            clearTimeout(this.TimeoutHandler);

        jq('#studio_greetingSettingsInfo').html('');
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_greetingSettingsBox').block();
            else
                jq('#studio_greetingSettingsBox').unblock();
        };

        GreetingSettingsController.RestoreGreetingSettings(function(result) {

            //clean logo path input
            jq('#studio_greetingLogoPath').val('');
            jq('div[id^="studio_setInf"]').html('');

            var classCSS = (result.value.Status == 1 ? "okBox" : "errorBox");
            jq('#studio_setInfGreetingSettingsInfo').html('<div class="' + classCSS + '">' + result.value.Message + '</div>');

            if (result.value.Status == 1) {
                jq('#studio_greetingHeader').val(result.value.CompanyName);
                jq('#studio_greetingLogo').attr('src', result.value.LogoPath);
            }

            GreetingSettingsManager.TimeoutHandler = setTimeout(function() { jq('#studio_setInfGreetingSettingsInfo').html(''); }, 4000);
        });
    }
}