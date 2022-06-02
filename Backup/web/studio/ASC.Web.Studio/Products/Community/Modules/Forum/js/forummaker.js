﻿var ForumMakerProvider = new function()
{
    this.All = "All";      
    this.ConfirmMessage = "Are you sure?";   
    this.NameEmptyString ="Enter title";
    this.SaveButton = "Save";
    this.CancelButton = "Cancel"    
    
    this.ModuleID = '853B6EB9-73EE-438d-9B09-8FFEEDF36234';
    this.Callback = '';
    this.IsRenderCategory = false;

    //-----------Forum Maker-----------------------------------------
    this.ShowForumMakerDialog = function(isRenderCategory, callback)
    {
        if(isRenderCategory!=null && isRenderCategory!=undefined)        
            this.IsRenderCategory = isRenderCategory;
        else
            this.IsRenderCategory = false;
            
        if(callback!=null && callback!=undefined)        
            this.Callback = callback;
        else
            this.Callback = '';
        
        AjaxPro.onLoading = function(b){};
        ForumMaker.GetCategoryList(function(result){
    
          jq('#forum_fmCategoryList').html(result.value);          
          
          jq('#forum_fmMessage').html('');          
          jq('#forum_fmMessage').hide();          
          
          jq('#forum_fmCategoryName').val('');
          jq('#forum_fmForumName').val('');
          jq('#forum_fmForumDescription').val('');
          try{
        
            jq.blockUI({message:jq("#forum_fmDialog"),
                    css: { 
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '400px',
                        height: '450px',
                        cursor: 'default',
                        textAlign : 'left',
                        'background-color': 'Transparent',
                        'margin-left': '-200px',
                        'top': '25%'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#aaaaaa',
                        cursor: 'default',
                        opacity: '0.3' 
                    },                  
                    focusInput : false,
                    fadeIn: 0,
                    fadeOut: 0
            });     
        }
        catch(e){}; 
        
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveThreadCategory();';
        
        if(jq('#forum_fmCategoryID').val()=='-1')        
            jq('#forum_fmCaregoryNameBox').show();
        
        jq("#forum_fmDialog").show();
        jq('#forum_fmContent').show();
        jq('#forum_fmInfo').hide();
        
      });
    };
    
    this.SelectCategory = function()
    {           
        if(jq('#forum_fmCategoryID').val()=='-1')        
            jq('#forum_fmCaregoryNameBox').show();
        else
            jq('#forum_fmCaregoryNameBox').hide();
    };
    
    this.SaveThreadCategory = function()
    {
         AjaxPro.onLoading = function(b){
            if(b)
            {
                jq('#forum_fm_panel_buttons').hide();
                jq('#forum_fm_action_loader').show();
                //jq('#forum_fmDialog').block();
            }
            else
            {
                jq('#forum_fm_action_loader').hide();
                jq('#forum_fm_panel_buttons').show();
                jq('#forum_fmDialog').unblock();
            }
         };         
         ForumMaker.SaveThreadCategory(jq('#forum_fmCategoryID').val(), jq('#forum_fmCategoryName').val(), 
                                                                        jq('#forum_fmThreadName').val(), 
                                                                        jq('#forum_fmThreadDescription').val(), 
                                                                        this.IsRenderCategory,
                                                                        function(result){
                    var res = result.value;
                    if(res.rs1=='1')
                    {                        
                        jq('#forum_fmContent').hide();
                        jq('#forum_fmInfo').show();
                        setTimeout('jq.unblockUI();',2000);
                        
                        if(res.rs3!='')
                        {
                            if(res.rs4=='0')
                            {
                                jq('#forum_threadCategories').append(res.rs3); 
                                ForumMakerProvider.InitSortCategory();
                            }
                            else
                            {
                                jq('#forum_categoryBox_'+res.rs2).replaceWith(res.rs3); 
                                ForumMakerProvider.InitSortCategory();
                            }
                        }
                        
                        if(ForumMakerProvider.Callback!='')                        
                            eval(ForumMakerProvider.Callback);
                            
                    }
                    else
                    {
                        
                        jq('#forum_fmMessage').html(res.rs2);
                        jq('#forum_fmMessage').show();
                    }
        });
        
    };
    
    this.CloseFMDialog = function()
    {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };
    
    
    this.ToggleThreadCategory = function(categoryID)
    {
        if(jq('#forum_threadListBox_'+categoryID).is(':visible'))
        {
            jq('#forum_threadListBox_'+categoryID).hide();
            jq('#forum_categoryState_'+categoryID).attr('src',SkinManager.GetImage('plus.png',ForumMakerProvider.ModuleID));
            
        }
        else
        {
            jq('#forum_threadListBox_'+categoryID).show();
            jq('#forum_categoryState_' + categoryID).attr('src', SkinManager.GetImage('minus.png', ForumMakerProvider.ModuleID));
        }
    };
    
    this.InitSortCategory = function()
    {   
        //category sort
        jq('#forum_threadCategories').sortable({       
            cursor: 'move',     
            items: '>div[id^="forum_categoryBox_"]',
            handle: 'td[id^="forum_categoryBoxHandle_"]',
            update: function(ev, ui){
                ForumMakerProvider.UpdateCategorySortOrder();
            },
            helper: function(e, el)
	        {
	            return jq(el).clone().width(jq(el).width());
	        },
	        
	        dropOnEmpty: false 	       
        });        
        
        //property sort
        jq('div[id^="forum_threadListBox_"]').sortable({       
            dropOnEmpty: false,
            cursor: 'move',
            items: '>div[id^="forum_threadBox_"]',
            connectWith: 'div[id^="forum_threadListBox_"]',
            handle: 'td[id^="forum_threadBoxHandle_"]',            
            update: function(ev, ui)
            { 
            },
            
            stop : function(event, ui) 
            {
                ForumMakerProvider.UpdateThreadSortOrder();
            },

            helper: function(e, el)
	        {
	            return jq(el).clone().width(jq(el).width());
	        }
        });
    };
    
    this.UpdateCategorySortOrder = function()
    {
        var sortOrders = new String();
        jq('div[id^="forum_categoryBox_"]').each(function(i,el){
           if(i>0) 
            sortOrders+=',';
            
           sortOrders+=jq(this).attr('name')+":"+i;
        });
        
        AjaxPro.onLoading = function(b) {};        
        ForumEditor.UpdateCategorySortOrder(sortOrders, function(result){});
    };
    
    this.UpdateThreadSortOrder = function()
    {
        var sortOrders = new String();
        jq('div[id^="forum_categoryBox_"]').each(function(i,el){
           if(i>0) 
            sortOrders+=';';
            
           var cid = jq(this).attr('name');
           sortOrders+=cid+"@";
           jq('#forum_threadListBox_'+cid+' > div[id^="forum_threadBox_"][name!="empty"]').each(function(j, obj){
                if(j>0)            
                    sortOrders+=',';
                        
                sortOrders+=jq(this).attr('name')+':'+j;
           });
           
        });
        
        AjaxPro.onLoading = function(b) {};        
        ForumEditor.UpdateThreadSortOrder(sortOrders, function(result){});
    };
    
    this.ShowEditCategoryDialog = function(categoryID, name, description)
    {
          jq('#forum_editCategoryMessage').html('');
          jq('#forum_editCategoryMessage').hide();
          
          jq('#forum_editCategoryID').val(categoryID);
          jq('#forum_editCategoryName').val(name);
          jq('#forum_editCategoryDescription').val(description);
          
           try{
        
            jq.blockUI({message:jq("#forum_editCategoryDialog"),
                    css: { 
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '400px',
                        height: '350px',
                        cursor: 'default',
                        textAlign : 'left',
                        'background-color': 'Transparent',
                        'margin-left': '-200px',
                        'top': '25%'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#aaaaaa',
                        cursor: 'default',
                        opacity: '0.3' 
                    },                  
                    focusInput : false,
                    fadeIn: 0,
                    fadeOut: 0
            });     
        }
        catch(e){}; 
        
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveCategory("edit");'
    };
    
    this.ShowNewCategoryDialog = function()
    {
          jq('#forum_newCategoryMessage').html('');
          jq('#forum_newCategoryMessage').hide('');
                    
          jq('#forum_newCategoryName').val('');
          jq('#forum_newCategoryDescription').val('');
          
          try{
        
            jq.blockUI({message:jq("#forum_newCategoryDialog"),
                    css: { 
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '400px',
                        height: '200px',
                        cursor: 'default',
                        textAlign : 'left',
                        'background-color': 'Transparent',
                        'margin-left': '-175px',
                        'top': '25%'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#aaaaaa',
                        cursor: 'default',
                        opacity: '0.3' 
                    },                  
                    focusInput : false,
                    fadeIn: 0,
                    fadeOut: 0
            });     
        }
        catch(e){}; 
        
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveCategory("new");'
    };
    
    this.SaveCategory = function(prefix)
    {
        var id= '';
        if(prefix=='edit')
            id = jq('#forum_editCategoryID').val();
    
        var name =  jq('#forum_'+prefix+'CategoryName').val();
        var description =  jq('#forum_'+prefix+'CategoryDescription').val();
        
        AjaxPro.onLoading = function(b) {
            if(b)
            {
                jq('#forum_' + prefix + '_categ_panel_buttons').hide();
                jq('#forum_' + prefix + '_categ_action_loader').show();
                //jq('#forum_'+prefix+'CategoryDialog').block();
            }
            else
            {
                jq('#forum_' + prefix + '_categ_panel_buttons').show();
                jq('#forum_' + prefix + '_categ_action_loader').hide();
                //jq('#forum_'+prefix+'CategoryDialog').unblock();
            }
        };
        
        if(prefix=='new')
        {
            ForumEditor.CreateCategory(name, description, function(result)
            {
                var res = result.value;
                if(res.rs1=='1')
                {
                    jq.unblockUI();
                    jq('#forum_threadCategories').append(res.rs2); 
                    ForumMakerProvider.InitSortCategory();
                }
                else
                {
                    jq('#forum_newCategoryMessage').html(res.rs2);
                    jq('#forum_newCategoryMessage').show();                
                }
            });
        }
        else
        {
            ForumEditor.SaveCategory(id, name, description, function(result)
            {
                var res = result.value;
                if(res.rs1=='1')
                {
                    jq.unblockUI();
                    jq('#forum_categoryBox_'+res.rs2).replaceWith(res.rs3); 
                    ForumMakerProvider.InitSortCategory();
                }
                else
                {
                    jq('#forum_editCategoryMessage').html(res.rs3);
                    jq('#forum_editCategoryMessage').show();
                }
            });
        }
        
    };
    
    this.ShowNewThreadDialog = function(categoryID)
    {
          jq('#forum_newThreadMessage').html('');                    
          jq('#forum_newThreadMessage').hide('');
            
          jq('#forum_newThreadName').val('');
          jq('#forum_newThreadDescription').val('');
          jq('#forum_newThreadCategoryID').val(categoryID);
          
          try{
        
            jq.blockUI({message:jq("#forum_newThreadDialog"),
                    css: { 
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '350px',
                        height: '350px',
                        cursor: 'default',
                        textAlign : 'left',
                        'background-color': 'Transparent',
                        'margin-left': '-175px',
                        'top': '25%'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#aaaaaa',
                        cursor: 'default',
                        opacity: '0.3' 
                    },                  
                    focusInput : false,
                    fadeIn: 0,
                    fadeOut: 0
            });     
        }
        catch(e){}; 
        
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveThread("new");'
    };
    
    this.ShowEditThreadDialog = function(threadID, categoryID, name, description)
    {
          jq('#forum_editThreadMessage').html('');
          jq('#forum_editThreadMessage').hide('');
          
          jq('#forum_editThreadCategoryID').val(categoryID);
          jq('#forum_editThreadID').val(threadID);
          jq('#forum_editThreadName').val(name);
          jq('#forum_editThreadDescription').val(description);          
          
          try{
        
            jq.blockUI({message:jq("#forum_editThreadDialog"),
                    css: { 
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '350px',
                        height: '350px',
                        cursor: 'default',
                        textAlign : 'left',
                        'background-color': 'Transparent',
                        'margin-left': '-175px',
                        'top': '25%'
                    },
                    
                    overlayCSS: { 
                        backgroundColor: '#aaaaaa',
                        cursor: 'default',
                        opacity: '0.3' 
                    },                  
                    focusInput : false,
                    fadeIn: 0,
                    fadeOut: 0
            });     
        }
        catch(e){}; 
        
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveThread("edit");'
          
    };
    
    this.SaveThread = function(prefix)
    {
        var threadID= '';
        if(prefix=='edit')
            id = jq('#forum_editThreadID').val();
    
        var categoryID = jq('#forum_'+prefix+'ThreadCategoryID').val();
        var name =  jq('#forum_'+prefix+'ThreadName').val();
        var description =  jq('#forum_'+prefix+'ThreadDescription').val();
        
         AjaxPro.onLoading = function(b) {
            if(b)
            {
                jq('#forum_' + prefix + '_thread_panel_buttons').hide();
                jq('#forum_' + prefix + '_thread_action_loader').show();
                //jq('#forum_'+prefix+'ThreadDialog').block();
            }
            else
            {
                jq('#forum_' + prefix + '_thread_action_loader').hide();
                jq('#forum_' + prefix + '_thread_panel_buttons').show();
                //jq('#forum_'+prefix+'ThreadDialog').unblock();
            }
        };
        
        if(prefix=='new')
        {
            ForumEditor.DoCreateThread(categoryID, name, description, function(result)
            {
                var res = result.value;
                if(res.rs1=='1')
                {
                    ForumMakerProvider.CloseDialogByID('forum_newThreadDialog');
                    jq('#forum_categoryBox_'+res.rs2).replaceWith(res.rs3); 
                    ForumMakerProvider.InitSortCategory();
                }
                else
                {
                    jq('#forum_newThreadMessage').html(res.rs2);
                    jq('#forum_newThreadMessage').show();
                }                   
            });
        }
        else
        {
            ForumEditor.SaveThread(id, categoryID, name, description, function(result)
            {
                var res = result.value;
                if(res.rs1=='1')
                {
                    ForumMakerProvider.CloseDialogByID('forum_editThreadDialog');
                    jq('#forum_categoryBox_'+res.rs2).replaceWith(res.rs3); 
                    ForumMakerProvider.InitSortCategory();
                }
                else
                {
                    jq('#forum_editThreadMessage').html(res.rs3);
                    jq('#forum_editThreadMessage').show();
                }
            });
        }
    };
    
    this.CloseDialogByID = function(dialogID)
    {
        PopupKeyUpActionProvider.ClearActions();
        jq('#'+dialogID).dialog('close');
        jq.unblockUI()
        
    };
    
    this.DeleteThread = function(threadID, categoryID)
    {
        if(!confirm(this.ConfirmMessage))
            return;
        
        AjaxPro.onLoading = function(b) {
            if(b)
                jq('#forum_threadCategories').block();
            else
                jq('#forum_threadCategories').unblock();
        }
        
        ForumEditor.DoDeleteThread(threadID, categoryID, function(result){
            var res = result.value;
            if(res.rs1=='1')            
                jq('#forum_threadBox_'+ res.rs2).remove();
            
            else                 
                jq('#forum_threadBox_'+ res.rs2).html('<div class="errorBox">'+res.rs4+'</div>');
        });
    };
  
    this.RemoveCategory = function(id)
    {
        if(!confirm(this.ConfirmMessage))
            return;
        
         AjaxPro.onLoading = function(b) {
            if(b)
                jq('#forum_threadCategories').block();
            else
                jq('#forum_threadCategories').unblock();
         };
         
        ForumEditor.DoDeleteThreadCategory(id, function(result){
            var res = result.value;
            if(res.rs1=='1')            
                jq('#forum_categoryBox_'+ res.rs2).remove();
            
            else                 
                jq('#forum_categoryBox_'+ res.rs2).html('<div class="errorBox">'+res.rs3+'</div>');
        });        
    };
    
    this.SelectModerators = function(id, isCategory)
    {
        jq('#forum_securityObjID').val(id);
        
        var userListElement =  'forum_moderators'+ (isCategory?'Category':'Thread')+'_'+id;
        var userIDs = jq('#'+userListElement).val().split(',');
        
        subjectSelector.ClearChecked();
        subjectSelector.OpenSearchTab();
        
        for(var i=0; i<userIDs.length; i++) 
        {   
            if(userIDs[i]!='')
            {
                subjectSelector.UserSelect(userIDs[i]);                
            }
        }
                
        if(isCategory)
        {            
            subjectSelector.OnSelectButtonClick = this.SelectCategoryModerators;
        }
        else    
        {
            subjectSelector.OnSelectButtonClick = this.SelectThreadModerators;
        }
    
        subjectSelector.ShowDialog();
    };
    
    this.SaveModerators = function(isCategory)
    {
        var userIDs = '';
        var userNames = ''
        var isFirst = true;
        jq(subjectSelector.Checked).each(function(i, el){
            
            if(el.Type == 'Group')
                return true;
                
            if(isFirst)            
                isFirst = false;
            else
            {
                userIDs+=',';
                userNames+=', ';
            }
            
            userIDs+=el.ID;
            userNames+=el.Name;
            
        });        
        
        jq('#forum_modNames'+(isCategory?'Category':'Thread')+'_'+jq('#forum_securityObjID').val()).html(userNames);
        jq('#forum_moderators'+(isCategory?'Category':'Thread')+'_'+jq('#forum_securityObjID').val()).val(userIDs);   
        
        AjaxPro.onLoading = function(b) {
            if(b)
                jq.blockUI();
            else
                jq.unblockUI();
        };  
        
        ForumEditor.SaveModerators(jq('#forum_securityObjID').val(),isCategory, userIDs, function(result){
            var res = result.value;
            if(res.rs1!='1')
                jq('#forum_modNames'+((res.rs4=='1')?'Category':'Thread')+'_'+jq('#forum_securityObjID').val()).html(res.rs3);
        });
    };
    
    this.SelectCategoryModerators = function()
    {
        ForumMakerProvider.SaveModerators(true);       
    };
    
    this.SelectThreadModerators = function()
    {
        ForumMakerProvider.SaveModerators(false);        
    };    
    
    this.SelectVisibleList = function(id, isCategory)
    {
        jq('#forum_securityObjID').val(id);
        
        var userListElement =  'forum_vl'+ (isCategory?'Category':'Thread')+'_'+id;
        var userIDs = jq('#'+userListElement).val().split(',');
        
        subjectSelector.ClearChecked();
        subjectSelector.OpenSearchTab();
        
        for(var i=0; i<userIDs.length; i++) 
        {   
            if(userIDs[i]!='')
            {
                subjectSelector.UserSelect(userIDs[i]);                
            }
        }
                
        if(isCategory)
        {            
            subjectSelector.OnSelectButtonClick = this.SelectCategoryVisibleList;
        }
        else    
        {
            subjectSelector.OnSelectButtonClick = this.SelectThreadVisibleList;
        }
    
        subjectSelector.ShowDialog();
    };
    
    this.SaveVisibleList = function(isCategory)
    {
        var userIDs = '';
        var userNames = ''
        var isFirst = true;
        jq(subjectSelector.Checked).each(function(i, el){
            
            if(el.Type == 'Group')
                return true;
                
            if(isFirst)            
                isFirst = false;
            else
            {
                userIDs+=',';
                userNames+=', ';
            }
            
            userIDs+=el.ID;
            userNames+=el.Name;
            
        });   
        
        if(userNames=='')     
            userNames = this.All;
                    
        jq('#forum_vlNames'+(isCategory?'Category':'Thread')+'_'+jq('#forum_securityObjID').val()).html(userNames);
        jq('#forum_vl'+(isCategory?'Category':'Thread')+'_'+jq('#forum_securityObjID').val()).val(userIDs);   
        
        AjaxPro.onLoading = function(b) {
            if(b)
                jq.blockUI();
            else
                jq.unblockUI();
        };  
        
        ForumEditor.SaveMembers(jq('#forum_securityObjID').val(),isCategory, userIDs, function(result){
            var res = result.value;
            if(res.rs1!='1')
                jq('#forum_vlNames'+((res.rs4=='1')?'Category':'Thread')+'_'+jq('#forum_securityObjID').val()).html(res.rs3);
        });
    };
    
    this.SelectCategoryVisibleList = function()
    {
        ForumMakerProvider.SaveVisibleList(true);       
    };
    
    this.SelectThreadVisibleList = function()
    {
        ForumMakerProvider.SaveVisibleList(false);        
    };    
    
    jq(document).ready(function(){
        ForumMakerProvider.InitSortCategory();
        
    });
    
    
///-----------------TagEditor-------------------------------------------------------------    
    
    this.ShowEditTag = function(id)
    {
        var sb = new String();    
        sb+='<input id="forum_tne_'+id+'" value="'+jq('#forum_tni_'+id).html()+'" class="textEdit" type="text" style="width:66%;"/>';    
        sb+='<div style="margin-top:3px;" class="clearFix">';
        sb+='<a class="baseLinkButton" style="float:left;" href="javascript:ForumMakerProvider.EditTag(\''+id+'\');">'+this.SaveButton+'</a>';        
        sb+='<a class="baseLinkButton" style="float:left; margin-left:5px;" href="javascript:ForumMakerProvider.HideEditTag(\''+id+'\');">'+this.CancelButton+'</a>';    
        sb+='</div>';
        
        jq('#forum_tag_name_edit_'+id).html(sb);
        jq('#forum_tag_name_info_'+id).hide();
        jq('#forum_tag_name_edit_'+id).show();
    };
    
    this.HideEditTag = function(id)
    {
        jq('#forum_tag_name_edit_'+id).hide();   
        jq('#forum_tag_name_info_'+id).show();
    };
    
    this.EditTag = function(id)
    {
        var tagName = jq.trim(jq('#forum_tne_'+id).val());  
        if(tagName=='')  
        {
            alert(this.NameEmptyString);
            jq('#forum_tne_'+id).focus();
            return;
        }
        
        AjaxPro.onLoading = function(b) {
            if(b)
                jq('#forum_tag_'+id).block();
            else
                jq('#forum_tag_'+id).unblock();
        };
        
        TagEditor.DoEditTagName(id,tagName, function(result){
            var res = result.value;
            var id = res.rs3;
            if(res.rs1=='1')
            {
                jq('#forum_tni_'+id).html(res.rs4);
                jq('#forum_tag_name_edit_'+id).hide();
                jq('#forum_tag_name_info_'+id).show();
            }
            else        
                jq('#forum_tag_name_edit_'+id).html('<div class="errorBox">'+res.rs2+'</div>');
        });
    };
    
    this.ApproveTag = function(id)
    {   
        AjaxPro.onLoading = function(b) {
            if(b)
                jq('#forum_tag_'+id).block();
            else
                jq('#forum_tag_'+id).unblock();
        };
        
        var isApproved = 1;
        if(jq('#forum_tag_approved_'+id).val()=='1')
            isApproved=0;        
        
        TagEditor.DoApproveTag(id,isApproved, function(result){
            var res = result.value;
            var idTag = res.rs3;
            if(res.rs1=='1')
            {
                jq('#forum_tag_approved_'+idTag).val(res.rs2);
                if(res.rs2=='1')            
                    jq('#forum_tag_'+idTag).removeClass('tintDangerous');
                else
                    jq('#forum_tag_'+idTag).addClass('tintDangerous');
            }
            else
            {
                jq('#forum_tag_'+idTag).attr('class','errorBox');
                jq('#forum_tag_'+idTag).html(res.rs4);
            }
        });
    };
    
    this.DeleteTag = function(id)
    {
        if(!confirm(this.ConfirmMessage))
            return;
        
        AjaxPro.onLoading = function(b) {
            if(b)
                jq('#forum_tag_'+id).block();
            else
                jq('#forum_tag_'+id).unblock();
        };
                
        TagEditor.DoDeleteTag(id, function(result){
            var res=result.value;
            if(res.rs1=='1')        
                jq('#forum_tag_'+res.rs3).remove();
            else
            {   
                jq('#forum_tag_'+res.rs3).attr('class','errorBox');
                jq('#forum_tag_'+res.rs3).html(res.rs2);
            }            
        });
    };
    
    this.DeleteTagFromTopic = function(idTag,idTopic)
    {
        AjaxPro.onLoading = function(b) {
            if(b)
                jq.blockUI();
            else
                jq.unblockUI();
        };
        TagEditor.DoDeleteTagFromTopic(idTag,idTopic, function(result){
            var res = result.value;
            var idTag = res.rs3;
            var idTopic = res.rs4;
            if(res.rs1=='1')        
                jq('#forum_tag_topic_'+idTag+'_'+idTopic).remove();
            else        
                jq('#forum_tag_topic_'+idTag+'_'+idTopic).html('<div class="errorBox">'+res.rs2+'</div>');
            
        });
    };    
};

//-------------------------------------------
ForumSubscriber = new function()
{   
    this.SubscribeOnTopic = function(topicID, state)
    {
        AjaxPro.onLoading = function(b)
        {
            if(b)           
               jq('#forum_subcribeOnTopicBox').block();
            else
               jq('#forum_subcribeOnTopicBox').unblock();
        };
                
        Subscriber.SubscribeOnTopic(topicID, state,function(result)
        {   
            var res = result.value;
            jq('#forum_subcribeOnTopicBox').replaceWith(res.rs2);
        });
    };
    
    this.SubscribeOnThread = function(threadID, state)
    {
        AjaxPro.onLoading = function(b)
        {
            if(b)
                jq('#forum_subcribeOnThreadBox').block();
            else
                jq('#forum_subcribeOnThreadBox').unblock();
        }   
                 
        Subscriber.SubscribeOnThread(threadID,state,function(result){            
            var res = result.value;
            jq('#forum_subcribeOnThreadBox').replaceWith(res.rs2);
        });
    };    
    
    this.SubscribeOnTag = function(tagID, state)
    {
        AjaxPro.onLoading = function(b)
        {    
            if(b)      
                jq('#forum_subcribeOnTagBox').block();
            else
                jq('#forum_subcribeOnTagBox').unblock(); 
        }    
                
        Subscriber.SubscribeOnTag(tagID,state,function(result)
        {
            var res = result.value;
            jq('#forum_subcribeOnTagBox').replaceWith(res.rs2);
        });
    };
    
    this.SubscribeOnNewTopics = function(state)
    {
        AjaxPro.onLoading = function(b)
        {    
            if(b)      
                jq('#forum_subcribeOnNewTopicBox').block();
            else
                jq('#forum_subcribeOnNewTopicBox').unblock(); 
        }    
                
        Subscriber.SubscribeOnNewTopic(state,function(result)
        {
            var res = result.value;
            jq('#forum_subcribeOnNewTopicBox').replaceWith(res.rs2);
        });     
    };
}