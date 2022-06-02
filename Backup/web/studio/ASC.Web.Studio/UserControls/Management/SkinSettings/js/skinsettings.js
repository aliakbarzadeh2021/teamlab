jq(function() {
    jq('#saveSkinSettingBtn').click(SkinSettingsManager.SaveSettings);
    jq('input[name="studio_skin"]').click(function() { SkinSettingsManager.SkinPreview(jq(this).val()); });
})

SkinSettingsManager = new function() {

    this.TimeoutHandler = null;

    this.SaveSettings = function() {

        if (this.TimeoutHandler)
            clearTimeout(this.TimeoutHandler);

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_skinSettingsBox').block();
            else
                jq('#studio_skinSettingsBox').unblock();
        };

        SkinSettingsController.SaveSkinSettings(jq('input.studio_skin:checked').val(), function(result) {

            var res = result.value;
            if (res.Status == 1)
                window.location.reload(true);
            else {
                jq('#studio_skinSettingsInfo').html('<div class="errorBox">' + res.Message + '</div>');
                SkinSettingsManager.TimeoutHandler = setTimeout(function() { jq('#studio_skinSettingsInfo').html(''); }, 4000);
            }
        });
    }


    this.SkinPreview = function(skinID) {
        var preview = jq('#skin_prev_' + skinID).attr('src');
        jq('#studio_skinPreview').attr('src', preview);
    }

}