jq(function() {
    jq('#saveItemsSettingsBtn').click(WebItemsSettingsManager.SaveSettings);
    WebItemsSettingsManager.InitSortable();
})

var WebItemsSettingsManager = new function() {

    this.TimeoutHandler = null;

    this.SaveSettings = function() {

        var settings = new Array();
        jq('div[id^="studio_poptBox_"]').each(function(i, pel) {

            var pid = jq(this).attr('name');
            var iOpt = { SortOrder: i,
                ItemID: pid,
                Disabled: !jq('#studio_poptDisabled_' + pid).is(':checked'),
                CustomName: '',
                ChildItemIDs: new Array()
            };

            settings.push(iOpt);

            jq('#studio_poptBox_' + pid + ' > div[id^="studio_mopt_"]').each(function(n, mel) {
                var mid = jq(this).attr('name');

                iOpt.ChildItemIDs.push(mid);

                var mOpt = { SortOrder: n,
                    ItemID: mid,
                    CustomName: '',
                    Disabled: !jq('#studio_moptDisabled_' + mid).is(':checked'),
                    ChildItemIDs: new Array()
                };

                settings.push(mOpt);

            });

        });

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_productModulesSettingsBox').block();
            else
                jq('#studio_productModulesSettingsBox').unblock();
        };

        if (this.TimeoutHandler)
            clearTimeout(this.TimeoutHandler);

        WebItemsSettingsController.SaveSettings(settings, function(result) {
            var res = result.value;
            if (res.Status == 1)
                jq('#studio_productModulesSettingsInfo').html('<div class="okBox">' + res.Message + '</div>');
            else
                jq('#studio_productModulesSettingsInfo').html('<div class="errorBox">' + res.Message + '</div>');

            WebItemsSettingsManager.TimeoutHandler = setTimeout(function() { jq('#studio_productModulesSettingsInfo').html(''); }, 4000);

        });

    };

    this.InitSortable = function() {

        //		jq('div[id^="studio_mopt_"]').hover(
        //            function() {
        //            	var id = jq(this).attr('name');
        //            	jq('#studio_moptHandle_' + id).attr('class', 'moveHoverBackground borderBase');
        //            	jq('#studio_moptHandle_' + id).attr('style', 'width:18px; border-top:none;border-left:none; border-bottom:none;')
        //            },
        //            function() {
        //            	var id = jq(this).attr('name');
        //            	jq('#studio_moptHandle_' + id).attr('class', 'tintLight')
        //            	jq('#studio_moptHandle_' + id).attr('style', 'width:18px; padding-right:1px;')
        //            });

        //module sort
        jq('div[id^="studio_poptBox_"]').sortable({
            cursor: 'move',
            items: '>div[id^="studio_mopt_"][class*="sort"]',
            handle: 'td[id^="studio_moptHandle_"]',
            update: function(ev, ui) {
                //ForumMakerProvider.UpdateCategorySortOrder();
            },
            helper: function(e, el) {
                return jq(el).clone().width(jq(el).width());
            },

            dropOnEmpty: false
        });
    };

    this.ClickOnProduct = function(idProduct) {
        jq("#studio_poptBox_" + idProduct + " input[id^='studio_moptDisabled_']")
            .attr("checked", jq("#studio_poptDisabled_" + idProduct).is(":checked"));
    };

    this.ClickOnModule = function(module) {
        if (jq(module).is(":checked")) {
            var idProduct = jq(module).parents("[id^='studio_poptBox_']").attr("name");
            jq("#studio_poptDisabled_" + idProduct).attr("checked", true);
        }
    };
};