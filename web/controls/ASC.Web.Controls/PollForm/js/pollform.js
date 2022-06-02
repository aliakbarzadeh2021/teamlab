AnswerVariantPrototype = function(id, name, voteCount)
{
    this.ID = id;
    this.Name = name;
    this.VoteCount = voteCount;
};

VotingPollPrototype = function(varName, voteHandlerType, pollID, emptySelectText, 
                                statBarCSSClass, liderBarCSSClass, variantNameCSSClass, voteCountCSSClass,
                                additionalParams)
{
    this.VarName = varName;
    this.VoteHandlerType = voteHandlerType;
    this.PollID = pollID;
    this.EmptySelect = emptySelectText;
    this.AnswerVariants = new Array();
    this.AdditionalParams = additionalParams;
    
    this.StatBarCSSClass = statBarCSSClass;
    this.LiderBarCSSClass = liderBarCSSClass;
    this.VariantNameCSSClass = variantNameCSSClass;
    this.VoteCountCSSClass = voteCountCSSClass;
    
    this.RegistryVariant = function(variant)
    {
        for(var i=0; i<this.AnswerVariants.length; i++)
        {
            if(this.AnswerVariants[i].ID == variant.ID)
                return;
        }
        this.AnswerVariants.push(variant);
    };

    this.Vote = function()
    {  
        var ids = new String();
        jq.each( jq("input:checked[name='__pollForm_"+this.PollID+"_av']"), function(i, n)
        {            
            if(i!=0)
                ids+=",";
            
            ids+=jq(this).val();
        });
        
        jq('#__pollForm_'+this.PollID+'_result').html('');  
                
        if(ids=='')
        {   
            jq('#__pollForm_'+this.PollID+'_result').html('<div class="errorBox">'+this.EmptySelect+'</div>');   
            return;
        }       
            
        AjaxPro.onLoading = function(b)
        {
            if(b)
                jq('#__pollForm_'+this.PollID+'_voteBox').block();
            else
                jq('#__pollForm_'+this.PollID+'_voteBox').unblock();
        }
        
        PollFormControl.Vote(this.VoteHandlerType, this.PollID,ids, this.AnswerVariants,
                             this.StatBarCSSClass, this.LiderBarCSSClass, this.VariantNameCSSClass, this.VoteCountCSSClass,
                             this.AdditionalParams, function(result){
            var res = result.value;
            if(res.Success=='1')
            {   
                jq('#__pollForm_'+res.PollID+'_answButtonBox').hide();
                jq('#__pollForm_'+res.PollID+'_voteBox').html(res.HTML); 
            }
            else
            {   
                jq('#__pollForm_'+res.PollID+'_result').html('<div class="errorBox">'+res.Message+'</div>');
            }
        });
    };
}

PollMaster = new function()
{
    this.AddAnswerVariant = function(uniqueID, variantClass, labelClass, editClass)        
    {
        var items=jq('#__poll_'+uniqueID+'_qbox').children();    
        var numb = items.length + 1;
        var sb = new String();
        sb = '<div class="'+variantClass+'"><label for="__poll_'+uniqueID+'_q' + numb + '" style="float:left;" class="'+labelClass+'">' + 
              jq('#__poll_'+uniqueID+'_variantCaption').val() + ' ' + numb +
             ': </label><input id="__poll_'+uniqueID+'_q' + numb + '" name="q' + numb + '" type="text" maxlength="100" class="'+editClass+'" style="margin-top:2px; float:right; width:90%"/>'+
             '<input id="__poll_'+uniqueID+'_qid' + numb + '" name="qid' + numb + '" type="hidden" value=""/>'+
             '</div>';
        
        jq('#__poll_'+uniqueID+'_qbox').append(sb);
        
        jq('#__poll_'+uniqueID+'_removeButton').show();    
        if(numb>=15)    
           jq('#__poll_'+uniqueID+'_addButton').hide();           
        else
            jq('#__poll_'+uniqueID+'_addButton').show();
    };
    
    this.RemoveAnswerVariant = function(uniqueID)
    {
        var items=jq('#__poll_'+uniqueID+'_qbox').children();    
        jq('#__poll_'+uniqueID+'_addButton').show();    
        if(items.length>2)
        {      
            jq(items[items.length-1]).remove();
        }
        items=jq('#__poll_'+uniqueID+'_qbox').children();
        if(items.length<=2)                   
           jq('#__poll_'+uniqueID+'_removeButton').hide();           
        else
           jq('#__poll_'+uniqueID+'_removeButton').show();           
    };
}