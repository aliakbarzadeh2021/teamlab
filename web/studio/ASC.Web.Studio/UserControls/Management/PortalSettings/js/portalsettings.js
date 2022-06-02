jq(function() {
    jq('#savePortalSettingsBtn').click(PortalSettingsManager.SaveSettings);
    jq('input[name="portalType"]').click(PortalSettingsManager.SwitchPortalType);
    
})

PortalSettingsManager = new function() {

    this.TimeoutHandler = null;

    this.SwitchPortalType = function() {

        if (jq('#publicPortal').is(':checked')) {

            jq('#privatePortalDescription').hide();
            jq('#publicPortalDescription').show();
        }
        else {
            jq('#publicPortalDescription').hide();
            jq('#privatePortalDescription').show();
        }
    }

    this.SaveSettings = function() {

        if (this.TimeoutHandler)
            clearTimeout(this.TimeoutHandler);

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_portalSettingsBox').block();
            else
                jq('#studio_portalSettingsBox').unblock();
        };

        var isPublic = jq('#publicPortal').is(':checked');
        var ids = new Array();
        if (isPublic) {
            jq('.publicItem').each(function(i, el) { 
                if(jq(this).is(':checked'))
                    ids.push(jq(this).val());
            })
        }

        PortalSettingsController.SavePortalSettings(isPublic, ids, function(result) {

            var res = result.value;
            if (res.Status == 1)
                jq('#studio_portalSettingsInfo').html('<div class="okBox">' + res.Message + '</div>');
            else
                jq('#studio_portalSettingsInfo').html('<div class="errorBox">' + res.Message + '</div>');

            PortalSettingsManager.TimeoutHandler = setTimeout(function() { jq('#studio_portalSettingsInfo').html(''); }, 4000);
        });
    }

}