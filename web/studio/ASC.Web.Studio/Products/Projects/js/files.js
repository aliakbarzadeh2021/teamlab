ASC.Projects.FilePage = (function() { // Private Section

return { // Public Section
fileDelete : function(id, version, isLastVersion)
{

  var fileInfoUI = jq("#file_info_" + id + "_" + version);
  
  var fileName = "";
  
  if (isLastVersion)
     fileName = jq("#file_info_" + id + "_" + version + " .pm-file-info-header  div a:first").text();
  else
     fileName = jq("#file_info_" + id + "_" + version + " td:eq(1) a").text();
    
  if (!confirm(jq.format(ASC.Projects.Resources.DeleteThisFile, "'" + jq.trim(fileName) + "'"))) return;
  
  AjaxPro.onLoading = function(b)
  {

    if (b)
    {
      
       if (isLastVersion)
         fileInfoUI.block({
                           message: null,
                           overlayCSS: { backgroundColor: '#efefef',  opacity: 0.6  }
                          });
       else
         fileInfoUI.find("td:last a").hide();
       
       
    }  
  } 
    
  AjaxPro.FilePage.DeleteFile(id, version, 
    function(res)
    {
    
      if (res.error != null)
      {
        
        alert(res.error.Message);
        
        return;
      
      }
          
     if (isLastVersion)
     {
     
         fileInfoUI.unblock();     
     
         if (jq.trim(res.value) == "")
            fileInfoUI.remove();  
         else
         {
         
            var replacementHTML = jq(res.value);
         
            fileInfoUI.replaceWith(res.value);
            
            replacementHTML.yellowFade();
         
         }
                  
     } 
     else
     {
     
         var otherVersionContainer = fileInfoUI.parents('table');
     
         fileInfoUI.remove();    
         
         if (otherVersionContainer.find("tr").length == 0)
         {
         
           var otherVersionContainerWrapper = otherVersionContainer.parent();
           
           var otherVersionContainerInfoBlock = otherVersionContainerWrapper.prev();
           
           otherVersionContainerWrapper.remove();
           otherVersionContainerInfoBlock.remove();
                                 
           otherVersionContainer.parents("div.pm-file-info-other-version").remove();
                   
         }
     }
     ASC.Projects.FilePage.InitCategoryComboboxes();
    }
  );   
   
},
InitCategoryComboboxes : function() {
	jq('select[id^="DdlCategory_"]').each(
		function() {
			StudioManager.createCustomSelect(this.id, true);
		}
	);
},
ChangeCategory : function(TargetId, NewCat)
{
    jq("div[id^='file_info_" + TargetId + "']").block();
    AjaxPro.FilePage.ChangeCategory(jq.getURLParam("prjID"), TargetId, NewCat, function(res)
    {
        if(res.value === null)
            alert(ASC.Projects.Resources.ErrorCategoryNotChanged);
        else if(res.value !== "")
            alert(res.value);
            
        var ButtonHref = new String(jq("a#ButtonUpdVer" + TargetId).attr("href"));
        ButtonHref = ButtonHref.replace(new RegExp("cat=[0-9]*"), "cat=" + NewCat); 
        jq("a#ButtonUpdVer" + TargetId).attr("href", ButtonHref);                
        
        jq("div[id^='file_info_" + TargetId + "']").unblock();
    });
}

}

})();