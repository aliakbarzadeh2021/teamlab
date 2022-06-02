ASC.Projects.Messages = (function() { 
// Private Section

return { // Public Section

deleteFile : function (fileID, fileVersion, fileName)
{
     
    if (!confirm(jq.format(ASC.Projects.Resources.ConfirmDelete,"'" +  fileName + "'"))) return false;   
       
   var prjID = jq.getURLParam("prjID");
   
   AjaxPro.MessageActionView.DeleteFile(prjID,fileID,fileVersion, 
    function(res)
    {
      
       if (res.error!= null)
       {
       
          alert(res.error.Message);
       
          return;
       
       } 
            
       
     }
  ); 
  
  return true;
     
},

unlockForm :function()
{

   jq("#page_content div.pm-headerPanel-splitter input[id$=tbxTitle]")
   .removeAttr("readonly")
   .removeClass("disabled");
   
   jq("#notify_participant  input,#another_notify_participant  input, #fileUploader, #notify_edit").removeAttr("disabled");
      
   jq("div.pm-action-block").show();
   jq("div.pm-ajax-info-block").hide();
   
   jq("#notify_participant_checked").val("");
   jq("#notify_edit_checked").val("");
   
},

lockForm :function()
{

   jq("#page_content div.pm-headerPanel-splitter input[id$=tbxTitle]")
   .attr("readonly", "readonly")
   .addClass("disabled");

   jq("#notify_participant  input,#another_notify_participant  input, #fileUploader, #notify_edit").attr("disabled", "disabled");

   jq("div.pm-action-block").hide();
   jq("div.pm-ajax-info-block").show();
   
   var ids = new Array();
   
   jq("#notify_participant input[id^=notify_user_]:checked").each(
     function(index)
     {
     
      ids.push(jq(this).attr("id"));
     
     }    
    );
   
    jq("#notify_participant_checked").val(ids.join(';'));
    
    
    ids = new Array();
   
   jq("#another_notify_participant input[id^=notify_user_]:checked").each(
     function(index)
     {
     
      ids.push(jq(this).attr("id"));
     
     }    
    );
   
    jq("#another_notify_participant_checked").val(ids.join(';'));  
    
    if (jq("#notify_edit").length != 0)     
       jq("#notify_edit_checked").val(jq("#notify_edit").is(":checked"));
    
}, 

uploadCompleteCallback_function : function (filesUploadedInfo)
{
   
    jq("#attachment_list").val(filesUploadedInfo.join(";"));
    
    jq("#page_content div.pm-ajax-info-block span").text(ASC.Projects.Resources.SavingMessage);
       
   __doPostBack('', '');    
 
}, 

savingMessage : function (permission)
{
    
    if (jq.trim(jq("input[id$=tbxTitle]").val()) == "")
    {
                
          alert(ASC.Projects.Resources.EmptyMessageTitle);
          
          return;
                        
    }
    
    var access = true;
    if(permission=="False") access = false;
    
    if((access && (!window.FileUploader || FileUploader.GetUploadFileCount() == 0)) || (!access))
    {
         
      
       this.lockForm();  
          
       __doPostBack('', '');   

        return;   
    
    }
    
    this.lockForm();   
       
    jq("#page_content div.pm-ajax-info-block span").text("");
    
    jq("#pageContent .pm-action-block").hide();
    jq("#pageContent .pm-ajax-info-block").show();
          
    FileUploader.Submit();
      
   
},
changeSubscribe : function(messageID)
{
  
    AjaxPro.onLoading = function(b)
    {
   
       if(b)  
         jq("#message_subscriber").block({message: null});
       else
         jq("#message_subscriber").unblock();     
     
    }

    AjaxPro.MessageDetailsView.ChangeSubscribe(messageID,
     function(res)
     {
      
       if (res.error!=null)
       {
       
         alert(res.error.Message);
         
         return;
       
       }
       
       jq("#message_subscriber a").text(jq.trim(res.value));
      
     }
    );       
    
},
addNewMessage: function(id)
{
    var title = jq("[id$=tbxTitle]").val();
    var prjID = jq.getURLParam("prjID");
    var ID = jq.getURLParam("ID");
    var type = jq.getURLParam("action");
    var content = FCKeditorAPI.GetInstance(id).GetHTML(true);

    if(title!="")
    {
        AjaxPro.MessageActionView.AddNewMessage(title,content,prjID,ID,type,
        function(res)
        {
        document.location.replace("messages.aspx?prjID="+prjID);
        });
    }
    else
    {
    jq('#error').html(ASC.Projects.Resources.EmptyMessageTitle);
    jq('#error').css('display','block');
    }
},

deleteMessage: function(messageID, isList)
{
    var prjID = jq.getURLParam("prjID");
    if (confirm(ASC.Projects.Resources.DeleteThisMessage))
    {
    AjaxPro.MessageActionView.DeleteMessage(messageID, function(res){
            if(isList==1)
                jq("#message_"+messageID).hide();
            else
                document.location.replace("messages.aspx?prjID="+prjID);
        });
    }
},
replace: function(href)
    {
        document.location.replace(href);
    }



}
})();