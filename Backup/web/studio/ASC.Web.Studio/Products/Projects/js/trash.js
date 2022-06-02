ASC.Projects.TrashPage = (function(){

return {

executeAction: function(actionID)
{     
   var ids = new Array();
   var items = jq("#tasks_block tbody input:checked").parents("tr");
   items.each(
     function(index)
     {                             
       ids.push(jq(this).attr("id").split('_')[1]);
     }
   );
   if (ids.length == 0) 
   {
     alert(ASC.Projects.Resources.SelectedTasksInfo);
     return;
   }
   var milestoneID = -1;
   AjaxPro.TrashPage.ExecuteAction(ids,actionID,milestoneID, 
     function(res)
     {
       if (res.error != null)
       {
          alert(res.error.Message);
          return;       
       }
       items.animate({ opacity: "hide" }, 750, function(){jq(this).remove()});      
       if (jq("#tasks_block tbody tr").length == 0)
       {          
          jq("#tasks_block").remove();               
          jq("#empty_screen_control").show();
       }              
     }
   );                           
      
},
unlockForm :function(popupOnly)
{
    if(popupOnly)
    {
        jq("#group_manager_popup  .pm-action-block").show();
        jq("#group_manager_popup  .pm-ajax-info-block").hide();
    }
    else
    {
        jq('#actions').show();
        jq("#info-block").hide();    
    }
},

lockForm :function(popupOnly)
{
    if(popupOnly)
    {
        jq("#group_manager_popup  .pm-action-block").hide();
        jq("#group_manager_popup  .pm-ajax-info-block").show();
    }
    else
    {
        jq('#actions').hide();
        jq("#info-block").show();
    } 
},

viewTrashActionPanel: function(actionTitle, type)
{
   var ids = new Array();
   var innerHTML = "";
   jq(".pm-tablebase input:checked").each(
     function(index)
     {
       var taskRow = jq(this).parents("tr");
       ids.push(taskRow.attr("id").split('_')[1]);
       innerHTML  +=  "<li>" + jq.trim(taskRow.find("td").eq(1).text()) + "</li>";
     }
   );
    if (ids.length == 0)
    {
       alert(ASC.Projects.Resources.SelectedTasksInfo);
       return;   
    }
    jq("#group_manager_popup dd ul").html(innerHTML);
    jq("#group_manager_popup dd:last").html(actionTitle);
    var executeGroupOperationsButton =  jq("#group_manager_popup .pm-action-block a:first");
    executeGroupOperationsButton.unbind('click');
    executeGroupOperationsButton.bind('click', 
     function()
     {  
         ASC.Projects.TrashPage.taskManipulation(ids,type);           
     }     
    );
    jq('#1_1').show();
    jq('#1_2').show();
    jq('#2_1').show();
    jq('#2_2').show();
    if(parseInt(type)==2)
        if(document.getElementById('milestones')!=null)
            if(document.getElementById('milestones').length==0)
            {
                jq('#1_1').hide();
                jq('#1_2').hide();
            }
    if(parseInt(type)==1)
        {
                jq('#1_1').hide();
                jq('#1_2').hide();
        }
    
    jq.blockUI({message:jq("#group_manager_popup"),
                    css: { 
                        left: '50%',
                        top: '50%',
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '600px',
                       
                        cursor: 'default',
                        textAlign : 'left',
                        'margin-left': '-300px',
                        'margin-top': '-135px',                      
                        'background-color':'White'
                    },
                    overlayCSS: { 
                        backgroundColor: '#AAA',
                        cursor: 'default',
                        opacity: '0.3' 
                    },
                    focusInput : false,
                    baseZ : 6666,
                    fadeIn: 0,
                    fadeOut: 0
                }); 
                
                
},

taskManipulation: function(ids,type)
{
   AjaxPro.onLoading = function(b) 
   {    
        if (b){ ASC.Projects.TrashPage.lockForm(true);}
   };  
   var actionType;
   var milestoneID=0;
   if(document.getElementById('milestones')!=null)
            if(document.getElementById('milestones').length!=0)
            {
                milestoneID=jq('#milestones option:selected').val();
            }
   actionType = type +'_'+ milestoneID;
   AjaxPro.TrashPage.TrashTaskManager(ids,actionType, 
    function (res)
    { 
      if (res.error != null)
      {
        alert(res.error.Messages);
        ASC.Projects.TrashPage.unlockForm(true);
        ASC.Projects.TrashPage.unlockForm(false);          
        return;   
      }    
      var items = jq("#tasks_block tbody input:checked").parents("tr");
      items.animate({ opacity: "hide" }, "slow", function(){
      ASC.Projects.TrashPage.unlockForm(true);
      ASC.Projects.TrashPage.unlockForm(false);
      jq.unblockUI();
      items.remove()
      if (jq("#tasks_block tbody tr").length == 0)
       {          
          jq("#tasks_block").remove();
          jq("#actions").remove();               
          jq("#empty_screen_control").show();  
       }
       });       
    }
   );
  
},

clearTrash: function()
{
if(confirm(ASC.Projects.Resources.ConfirmClearTrash))
{jq('#tasks_block tbody input').attr('checked', 'checked');
 jq('#tasks_block tbody input').attr('disabled', 'disabled');
 ASC.Projects.TrashPage.lockForm(false);
 var ids = new Array();
   jq(".pm-tablebase input:checked").each(
     function(index)
     {
       var taskRow = jq(this).parents("tr");
       ids.push(taskRow.attr("id").split('_')[1]);
     }
   );
    if (ids.length == 0)
    {
        ASC.Projects.TrashPage.unlockForm(false);
        alert(ASC.Projects.Resources.TrashEmpty);
        return;   
    }
ASC.Projects.TrashPage.taskManipulation(ids,'1_0'); 
}
}


}

})();