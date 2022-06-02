ASC.Projects.MyTaskPage = (function() {
    return {

        isOneList: false,

        coloringRows: function()
        {
            if(ASC.Projects.MyTaskPage.isOneList)
            {
                if (jq("#myTasksBody table.pm-tasks-block tbody tr").length > 0)
                {
                    jq("#myTasksBody table.pm-tasks-block tbody tr").each(
                    function(index)
                    {
                        jq(this).removeClass("tintLight").removeClass("tintMedium");
                        if (index % 2 == 0)
                        {
                            jq(this).addClass("tintMedium");
                        }
                        else
                        {
                            jq(this).addClass("tintLight");
                        }
                    });
                }
            }
            else
            {
                if (jq("#myTasksBody table.pm-tasks-block tbody").length > 0)
                {
                    jq("#myTasksBody table.pm-tasks-block tbody").each(
                    function(index)
                    {
                        var rows = jq(this).children();
                        for (var j = 0; j < rows.length; j++)
                        {
                            jq(rows[j]).removeClass("tintLight").removeClass("tintMedium");
                            if (j % 2 == 0)
                            {
                                jq(rows[j]).addClass("tintMedium");
                            }
                            else
                            {
                                jq(rows[j]).addClass("tintLight");
                            }
                        }
                    });
                }
            }
        }, 
        
        changeStatus: function(taskID)
        {   
            jq("#taskStatus_"+taskID).hide();
            jq("#loaderImg_"+taskID).show();
            var isClosed = jq("#taskStatus_"+taskID).attr("checked");
            var container = jq("#pmtask_"+taskID).parent();
            
            AjaxPro.MyTasksPage.ChangeTaskStatus(taskID, isClosed, function(res)
            {
                if (res.error != null)
                {
                    jq("#taskStatus_"+taskID).show();
                    jq("#loaderImg_"+taskID).hide();
                    alert(res.error.Message);
                    return;
                }

                jq("#pmtask_"+taskID).animate({ opacity: "hide" }, 500);
                
                setTimeout(function() {
                			jq("#pmtask_"+taskID).remove();
                			ASC.Projects.MyTaskPage.coloringRows();
                			if(jq("tr[id*='pmtask_']").length == 0)
                            {
                                jq("#pm_exists_tasks").hide();            
                                jq("#pm_title_tasks_one_list").hide();
                                jq("#pm_empty_tasks").show();
                            }
                		}, 500);	

            });
                
        },

        
        taskItem_mouseOver: function(obj)
        {
            jq(obj).addClass('pm-tasksItem-over');
            var taskID = jq(obj).attr('id').split('_')[1];
            
            jq("#imgTime_"+taskID).show();
            jq("#imgTimer_"+taskID).show();
        },

        taskItem_mouseOut: function(obj)
        {
            jq(obj).removeClass('pm-tasksItem-over');
            var taskID = jq(obj).attr('id').split('_')[1];
            
            if(parseInt(jq("#imgTime_"+taskID).attr("hastime"))==0)
                jq("#imgTime_"+taskID).hide();
                
            jq("#imgTimer_"+taskID).hide();    
        }

    }
})();