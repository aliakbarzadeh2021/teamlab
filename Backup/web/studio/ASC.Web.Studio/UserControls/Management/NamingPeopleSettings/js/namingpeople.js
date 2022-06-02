
jq(function() {

    jq('#saveNamingPeopleBtn').click(NamingPeopleManager.SaveSchema);
    jq('#namingPeopleSchema').change(function() {
        NamingPeopleManager.LoadSchemaNames(false);
    })
    NamingPeopleManager.LoadSchemaNames(true);
})

NamingPeopleManager = new function() {

    this.SaveSchema = function() {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#studio_namingPeopleBox').block();
            else
                jq('#studio_namingPeopleBox').unblock();
        };

        if (NamingPeopleManager.MessageTimer != null)
            clearTimeout(NamingPeopleManager.MessageTimer);
            
        var schemaId = jq('#namingPeopleSchema').val();
        if (schemaId == 'custom') {
            NamingPeopleController.SaveCustomNamingSettings(jq('#usrcaption').val(), jq('#usrscaption').val(),
                                                       jq('#grpcaption').val(), jq('#grpscaption').val(),
                                                       jq('#usrstatuscaption').val(), jq('#regdatecaption').val(),
                                                       jq('#grpheadcaption').val(), jq('#globalheadcaption').val(),
                                                       NamingPeopleManager.SaveSchemaCallback);
        }
        else
            NamingPeopleController.SaveNamingSettings(schemaId, NamingPeopleManager.SaveSchemaCallback);
    }

    this.MessageTimer = null;

    this.SaveSchemaCallback = function(res) {
        var result = res.value;
        if (result.Status == 1)
            jq('#studio_namingPeopleInfo').html('<div class="okBox">' + result.Message + '</div>')
        else
            jq('#studio_namingPeopleInfo').html('<div class="errorBox">' + result.Message + '</div>')

        NamingPeopleManager.MessageTimer = setTimeout(function() { jq('#studio_namingPeopleInfo').html('') }, 4000);
    }

    this.LoadSchemaNames = function(empty) {

        if (!empty) {
            AjaxPro.onLoading = function(b) {
                if (b)
                    jq('#studio_namingPeopleBox').block();
                else
                    jq('#studio_namingPeopleBox').unblock();
            };
        }
        else
            AjaxPro.onLoading = function(b) { };


        var schemaId = jq('#namingPeopleSchema').val();
        NamingPeopleController.GetPeopleNames(schemaId, function(res) {
            var names = res.value;
            if (names.Id != 'custom')
                jq('input[id$="caption"]').attr('disabled', 'disabled');
            else
                jq('input[id$="caption"]').attr('disabled', false);



            jq('#usrcaption').val(names.UserCaption);
            jq('#usrscaption').val(names.UsersCaption);
            jq('#grpcaption').val(names.GroupCaption);
            jq('#grpscaption').val(names.GroupsCaption);
            jq('#usrstatuscaption').val(names.UserPostCaption);
            jq('#regdatecaption').val(names.RegDateCaption);
            jq('#grpheadcaption').val(names.GroupHeadCaption);
            jq('#globalheadcaption').val(names.GlobalHeadCaption);

        })
    }
}

