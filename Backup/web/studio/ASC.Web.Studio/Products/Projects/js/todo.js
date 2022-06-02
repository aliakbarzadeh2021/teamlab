window.ASC.Projects.ToDo = (function($) {
    var isInit = false;
    
    var closeCount = 5;
    var closeFrom = 3;
    
    var whatDoYouText = "";
    
    var currentListID = "";
    
    var init = function() {

        if (isInit === false) {
            isInit = true;
        }
    };
    
    var showAddNewList = function()
    {
    
    };
    
    var addNewList = function()
    {
        var title = jq("#pm_new_todo_title").val();
        
        if(title != "")
            ToDoList.AddTodoList(title, function(response)
            {
                if(response.value != null)
                {
                    jq("#pm_listslist").append(response.value.rs2);
                    jq("#pm_todolist").append(response.value.rs3);
                    jq("#pm_selected_list").append("<option value=" + response.value.rs1 + ">" + response.value.rs4 + "</option>");
                    
                    bindEvents();
                }
            });
    };
    
    var addToDoByWatch = function(text, from, to, BalloonBackColor, BalloonBorderColor, BalloonPositionX, BalloonPositionY)
    {
        var listID = jq("#pm_selected_list").val();
        if(listID == null || listID == "")
                return;
    
        ToDoList.AddToDoByWatch(text, listID, from, to, BalloonBackColor, BalloonBorderColor, Math.round(BalloonPositionX), Math.round(BalloonPositionY), function(response)
        {
            if(response.value != null)
            {
                //initWaterMark("pm_new_todo", whatDoYouText)
                
                if(jq("#pm_todolist_" + response.value.rs3 + " div[id*='pm_todo_'][class!='pm_edit_done']").length == 0)
                {
                    jq("#pm_todolist_" + response.value.rs3).append(response.value.rs2);
                    jq("#pm_todo_" + response.value.rs1).insertBefore(jq("#pm_todolist_" + response.value.rs3 + " .pm_add_todo_box"));
                }
                else
                {
                    jq("#pm_todolist_" + response.value.rs3).append(response.value.rs2);
                    var list = jq("#pm_todolist_" + response.value.rs3 + " div[id*='pm_todo_'][class!='pm_edit_done']");
                    jq("#pm_todo_" + response.value.rs1).insertBefore(jq(list[0]));
                }
                
                animateToDo(response.value.rs1);
                
                bindEvents();
                jq("#pm_empty_tasks").hide();
            }
        });
    
    };
    
    var addToDo = function() {
    
        var text = jq("#pm_new_todo").val();
        var listID = ASC.Projects.ToDo.currentListID;
        if(text == "" || text == whatDoYouText)
            return;
        
        if(listID == null || listID == "")
            return;
        
        jq(".pm_add_todo_box").show();
        jq(".pm_add_container").hide();
            
        ToDoList.AddToDo(text,listID, function(response){
            jq("#pm_new_todo").val("");
            
            if(response.value != null)
            {
                initWaterMark("pm_new_todo", whatDoYouText)
                
                clockManager.insertToDoItemByInfo(response.value.rs1, text, new Date(), new Date());
                
                if(jq("#pm_todolist_" + response.value.rs3 + " div[id*='pm_todo_'][class!='pm_edit_done']").length == 0)
                {
                    jq("#pm_todolist_" + response.value.rs3).append(response.value.rs2);
                    jq("#pm_todo_" + response.value.rs1).insertBefore(jq("#pm_todolist_" + response.value.rs3 + " .pm_add_todo_box"));
                }
                else
                {
                    jq("#pm_todolist_" + response.value.rs3).append(response.value.rs2);
                    var list = jq("#pm_todolist_" + response.value.rs3 + " div[id*='pm_todo_'][class!='pm_edit_done']");
                    jq("#pm_todo_" + response.value.rs1).insertBefore(jq(list[0]));
                }
                
                animateToDo(response.value.rs1);
                
                bindEvents();
                jq("#pm_empty_tasks").hide();
            }
        });
    };
    
    var removeToDo = function(id) {
    
        AjaxPro.onLoading = function(b) {
            if(b)
            {
                jq("#pm_close_todo_" + id).hide();
                jq("#pm_loader_todo_" + id).show();
            }
            else
            {            
                jq("#pm_loader_todo_" + id).hide();
                jq("#pm_close_todo_" + id).show();
            }
        }; 
        
        ToDoList.RemoveToDo(id, function(response){jq("#pm_todo_" + response.value).remove(); 
            if(jq("#pm_todolist div[id*='pm_todo_'][class!='pm_edit_done']").length == 0){jq("#pm_empty_tasks").show();}
        });
    };
    
    var saveToDo = function(id) {
        jq("#pm_todo_" + id + " div.todo_edit").attr("style", "");
        
        AjaxPro.onLoading = function(b) {
            if(b)
            {
                jq("#pm_close_todo_" + id).hide();
                jq("#pm_loader_todo_" + id).show();
            }
            else
            {            
                jq("#pm_loader_todo_" + id).hide();
                jq("#pm_close_todo_" + id).show();
            }
        }; 
        
        ToDoList.EditToDo(id, jq("#pm_todo_text_edit_" + id).val(), function(response){if(response == null)return; jq("#pm_todo_" + id).replaceWith(response.value.rs2);bindEvents();});
    };
    
    var editToDo = function(id) {
        jq("#pm_todo_text_" + id).hide();
        jq("#pm_todo_text_edit_" + id).val(jq("#pm_todo_text_" + id).text());
        jq("#pm_todo_" + id + " div.todo_edit").hide();
        jq("#pm_todo_text_edit_" + id).show();
        jq("#pm_todo_edit_done_" + id).show();
        
        jq("#pm_todo_text_edit_" + id)[0].focus();
        
        jq("#pm_todo_edit_done_" + id).unbind("click").click(function(){ 
            saveToDo(id);            
        });
        
        jq("#pm_todo_text_edit_" + id).unbind("blur").blur(function(event){
        jq("#pm_todo_" + id + " div.todo_edit").attr("style", "");
        
            setTimeout("ASC.Projects.ToDo.hideEdit(" + id + ")", 100);
        });
        
       
        jq("#pm_todo_text_edit_" + id).unbind("keyup").keyup(function(event){ 
            var code;
            if (!e) var e = event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;    

            if(code==13)
                saveToDo(id);
	        if(code==27) 
	            hideEdit(id);
        });
        
    };
    
    var hideEdit = function(id)
    {
        jq("#pm_todo_text_edit_" + id).hide();
        jq("#pm_todo_edit_done_" + id).hide();
        jq("#pm_todo_text_" + id).show();
    };
    
    var browseClosed = function() {
        AjaxPro.onLoading = function(b) {};
        ToDoList.BrowseClosed(closeFrom, closeCount, function(response) {
        
            if(response.value != null)
            {
                jq("#pm_todolist").append(response.value.rs1);
                
                if(closeCount > response.value.rs2*1)
                    jq(jq("#pm_browse_closed").parent()).hide();
                    
                closeFrom += response.value.rs2*1;
                bindEvents();
            }
        });
    };
    
    var animateToDo = function(id)
    {
        var obj = jq("#pm_todo_" + id);                
		obj.css({ "background-color": "#ffffcc" });
		obj.animate({ backgroundColor: "#ffffff" }, 2000, function(){jq(this).attr("style", "")});                
    }
    
    var bindEvents = function() {
    
        jq("#pm_todolist div[id*='pm_todo_'][class!='pm_edit_done']").unbind("mouseover").unbind("mouseout").
            mouseover(function(){
                //if(!jq(this).hasClass("pm_td_closed"))
                    jq(this).addClass("selected");
            }).
            mouseout(function(){
                jq(this).removeClass("selected");
            });
            
        jq("div[id*='pm_todo_list_']").unbind("click").
            click(function(){
                     jq("div[id*='pm_todo_list_']").removeClass("selected");
                     jq(this).addClass("selected");
            });
            
        jq("input[id*='pm_close_todo_'][type='checkbox']").unbind("change").
            change(function(){
            
            var id = jq(this).attr("id").replace("pm_close_todo_", "");
            var checked = jq(this).attr("checked");
                changeStatus(id, checked)
            });
        
        jq("div[id*='pm_list_title_']").unbind("click").click(function ()
        {
            jq(this).toggleClass("pm_expand_list").toggleClass("pm_collapce_list");
            var id = jq(this)[0].id.replace("pm_list_title_", "")
            
            if(jq(this).hasClass("pm_expand_list"))
            {
                jq("#pm_todolist_" + id).show()            
            }
            else if(jq(this).hasClass("pm_collapce_list"))
            {
                jq("#pm_todolist_" + id).hide()            
            }
        });
        
        jq(".pm_add_link_list").unbind("click").click(function ()
        {
            jq(".pm_add_list, .pm_add_link_list").hide();
            jq("#pm_new_todo_title").show().focus().unbind("keyup").keyup(function(e)
            {
                var code;
                if (!e) var e = event;
                if (e.keyCode) code = e.keyCode;
                else if (e.which) code = e.which;    
                
                if(code==13)
                    ASC.Projects.ToDo.addNewList();
                if(code==13 || code==27)        
                {
                    jq(".pm_add_list, .pm_add_link_list").show();
	                jq("#pm_new_todo_title").hide()
	                jq('#pm_new_todo_title').val('');
                }
                
                
            });
        });

        jq(".pm_add_link_todo").unbind("click").click(function ()
        {
            var obj = jq(this).parent();
            obj.hide();
            jq(".pm_add_container").insertAfter(obj);
            jq(".pm_add_container").show();
            ASC.Projects.ToDo.currentListID = jq(this).parent().parent()[0].id.replace("pm_todolist_", "");
        });
    };
    
    changeStatus = function(id, checked)
    {
        AjaxPro.onLoading = function(b) 
        {
          if(b)
          {
                jq("#pm_close_todo_" + id).hide();
                jq("#pm_loader_todo_" + id).show();
          }
          else
          {
              jq("#pm_loader_todo_" + id).hide();
              jq("#pm_close_todo_" + id).show();
          }
      }; 
            
      ToDoList.ChangeStatusToDo(id, checked, function(response)
      { 
          if(checked)
          {
              jq("#pm_todo_" + response.value.rs1).addClass("pm_td_closed"); 
              var list = jq("#pm_todolist_" + response.value.rs2 + " div[id*='pm_todo_'][class!='pm_edit_done']");
              
              if(list.length > 1 && jq("#pm_todo_" + response.value.rs1)[0].id != list[list.length - 1].id )
                  jq("#pm_todo_" + response.value.rs1).insertAfter(jq(list[list.length - 1]));
              
              jq("#pm_todo_" + response.value.rs1).removeClass("selected");
              jq("#pm_close_todo_" + response.value.rs1).attr("checked", true);
              
          }
          else
          {
              var list = jq("#pm_todolist_" + response.value.rs2 + " div[id*='pm_todo_'][class!='pm_edit_done']").not("div.pm_td_closed");
              
              if(list.length > 1 && jq("#pm_todo_" + response.value.rs1)[0].id != list[list.length - 1].id )
                  jq("#pm_todo_" + response.value.rs1).insertAfter(jq(list[list.length - 1]));
              
              jq("#pm_todo_" + response.value.rs1).removeClass("selected");
              jq("#pm_todo_" + response.value.rs1).removeClass("pm_td_closed");
              jq("#pm_close_todo_" + response.value.rs1).removeAttr("checked");

          }
          
          animateToDo(response.value);
       });
    };
    
    var removeToDoList = function(id){
      ToDoList.RemoveTodoList(id, function(response)
      {
        if(response.value != null)
        {    
            jq("#pm_todo_list_" + response.value + ", #pm_todolist_" + response.value).remove();
            jq("#pm_list_title_" + response.value).parent().remove();
            jq("#pm_selected_list option[value='" + response.value + "']").remove();
                
        }
      });
    };
    
    var saveToDoList = function(id){
        //jq("#pm_todo_list_" + id + " div.todo_edit_list").attr("style", "");
        
        AjaxPro.onLoading = function(b) {
            if(b){}else{}
        }; 
        var text = jq("#pm_todo_text_edit_list_" + id).val();
        ToDoList.EditTodoList(id, text, function(response){
        if(response.value== null)return;
        jq("#pm_todo_list_" + id).replaceWith(response.value.rs2);
        
        jq("#pm_selected_list option[value='" + id + "']").html(text);
        
        var newTitle= jq("#pm_todo_text_edit_list_" + id).val();
        var oldTitle = jq("#pm_list_title_" + id).parent().text();
        var html =  jq("#pm_list_title_" + id).parent().html();
        
        jq("#pm_list_title_" + id).parent().html(html.replace(oldTitle, newTitle));
        
        bindEvents();});
    
    };
    
    var hideEditList = function(id){
        jq("#pm_todo_text_edit_list_" + id).hide();
        jq("#pm_todo_edit_done_list_" + id).hide();
        jq("#pm_todo_text_list_" + id).show();
        jq("#pm_todo_list_" + id + " div.todo_edit_list").attr("style", "");
        
    };
    
    var editToDoList = function(id){
        jq("#pm_todo_text_list_" + id).hide();
        jq("#pm_todo_text_edit_list_" + id).val(jq("#pm_todo_text_list_" + id).text());
        jq("#pm_todo_list_" + id + " div.todo_edit_list").hide();
        jq("#pm_todo_text_edit_list_" + id).show();
        jq("#pm_todo_edit_done_list_" + id).show();
        
        jq("#pm_todo_text_edit_list_" + id)[0].focus();
        
        jq("#pm_todo_edit_done_list_" + id).unbind("click").click(function(){ 
            saveToDoList(id);            
        });
        
        jq("#pm_todo_text_edit_list_" + id).unbind("blur").blur(function(event){
            
            setTimeout("ASC.Projects.ToDo.hideEditList(" + id + ")", 100);
        });
      
        jq("#pm_todo_text_edit_list_" + id).unbind("keyup").keyup(function(event){ 
            var code;
            if (!e) var e = event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;    

            if(code==13)
                saveToDoList(id);
	        if(code==27) 
	            hideEditList(id);
        });
    };
    
    
    var initWaterMark = function(objID, text)
    {
        whatDoYouText = text;
        
        if(jq('#' + objID).val() == '' || jq('#' + objID).val() == text) 
        {   
           jq('#' + objID).val(text);
           jq('#' + objID).addClass('empty_todo');
        }
        
        jq('#' + objID).unbind("keyup").unbind("focus").unbind("blur").keyup(function(event){ 
            var code;
            if (!e) var e = event;
            if (e.keyCode) code = e.keyCode;
            else if (e.which) code = e.which;    

            if(code==13)        
                addToDo();
	        if(code==27) 
	        {
	            jq(".pm_add_todo_box").show();
                jq(".pm_add_container").hide();
                jq('#' + objID).val('');
	        }	        
        }).focus(function()
        { 
            jq('#' + objID).removeClass('empty_todo');
                if(jq('#' + objID).val()==text)
                    jq('#' + objID).val('')            
        }).blur(function(){ 
            if(jq('#' + objID).val()=='')
               setTimeout("jq('#" + objID + "').addClass('empty_todo'); jq('#" + objID + "').val('" + text + "');",200);            
       }); 
    };
    
    var changePosition = function(id, balloonX, balloonY)
    {
        AjaxPro.onLoading = function(b) {if(b){}else{}};
        ToDoList.ChangePosition(id, Number(balloonX), Number(balloonY), function(){});
    }
    
    var initToDoSort  = function() {
        
        jq('div.[id*="pm_todolist_"]').sortable({
                cursor: 'move',
                items: '>div.pm_todo_item',
                handle: 'div.moveHoverBackground',
                update: function(ev, ui) {                    
                    var id = jq(ui.item).attr("id").replace("pm_todo_", "");
                    var prev = (jq(ui.item).prev().attr("id") != null && jq(ui.item).prev().attr("id") != undefined ? jq(ui.item).prev().attr("id").replace("pm_todo_", "") : -1);
                    //var next = (jq(ui.item).next().attr("id") != null && jq(ui.item).next().attr("id") != undefined ? jq(ui.item).next().attr("id").replace("pm_todo_", "") : -1);
                    ToDoList.ChangeSortOrder(id, prev, function(result) {});
                },
                helper: function(e, el) {
                    return jq(el).clone().width(jq(el).width());
                },                
                dropOnEmpty: false
            });
        };
    
    
    return {
        init            : init,        
        addToDo         : addToDo,
        removeToDo      : removeToDo,
        editToDo        : editToDo,
        browseClosed    : browseClosed,
        bindEvents      : bindEvents,
        initWaterMark   : initWaterMark,
        hideEdit        : hideEdit,
        initToDoSort    : initToDoSort,
        addToDoByWatch  : addToDoByWatch,
        changeStatus    : changeStatus,
        addNewList      : addNewList,
        currentListID   : currentListID,
        removeToDoList  : removeToDoList,
        editToDoList    : editToDoList,
        hideEditList    : hideEditList,
        changePosition  : changePosition
    };

})(jQuery);


(function($) {
	ASC.Projects.ToDo.init();
	$(function() {
    
    jq("#pm_todo_add").click(function(){ASC.Projects.ToDo.addToDo();});
    
    jq(document).ready(function() {
    
    ASC.Projects.ToDo.initToDoSort();
    ASC.Projects.ToDo.bindEvents();
    });
    
    jq("#pm_browse_closed").click(function(){ASC.Projects.ToDo.browseClosed();});
	
	});
})(jQuery);