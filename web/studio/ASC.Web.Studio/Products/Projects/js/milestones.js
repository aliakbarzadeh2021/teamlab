ASC.Projects.Milestones = (function() { 
// Private Section

return { // Public Section

list: new Array(),
        

slideActiveMilestones: function(first,last)
{
jq("#ActiveMilestones").slideToggle("slow");

var src = jq("#slideActiveMilestones").attr('src');
    if (src.indexOf("expand") > -1)
    {
        var newSrc = last;
        jq("#slideActiveMilestones").attr('src',newSrc);
        jq("#slideActiveMilestones").attr('title',ASC.Projects.Resources.Expand);
        jq("#slideActiveMilestones").attr('alt',ASC.Projects.Resources.Expand);
    }
    
    if (src.indexOf("collapse") > -1) 
    {
        var newSrc = first;
        jq("#slideActiveMilestones").attr('src',newSrc);
        jq("#slideActiveMilestones").attr('title',ASC.Projects.Resources.Collapse);
        jq("#slideActiveMilestones").attr('alt',ASC.Projects.Resources.Collapse);
    }
},

slideClosedMilestones: function(first,last)
{
jq("#ClosedMilestones").slideToggle("slow");

var src = jq("#slideClosedMilestones").attr('src');
    if (src.indexOf("expand") > -1)
    {
        var newSrc = last;
        jq("#slideClosedMilestones").attr('src',newSrc);
        jq("#slideClosedMilestones").attr('title',ASC.Projects.Resources.Expand);
        jq("#slideClosedMilestones").attr('alt',ASC.Projects.Resources.Expand);
    }
    
    if (src.indexOf("collapse") > -1)
    {
        var newSrc = first;
        jq("#slideClosedMilestones").attr('src',newSrc);
        jq("#slideClosedMilestones").attr('title',ASC.Projects.Resources.Collapse);
        jq("#slideClosedMilestones").attr('alt',ASC.Projects.Resources.Collapse);
    }
},
unlockForm :function()
{
   jq("#pm-calendar-container").unblock();
   jq("#pm-calendar-container").removeAttr("readonly").removeClass("disabled");
   jq("[id$=tbxTitle]").removeAttr("readonly").removeClass("disabled");
   jq("#notify_manager").removeAttr("disabled");
   jq("#is_key").removeAttr("disabled");
   jq("#actions").show();
   jq("#info-block").hide();
   jq("#shift").removeAttr("disabled");
   jq("#moveOutWeekend").removeAttr("disabled");
},

lockForm :function()
{
   jq("#pm-calendar-container").block({message: null});
   jq("#pm-calendar-container").attr("readonly", "readonly").addClass("disabled");
   jq("[id$=tbxTitle]").attr("readonly", "readonly").addClass("disabled");
   jq("#notify_manager").attr("disabled", "disabled");
   jq("#is_key").attr("disabled", "disabled");
   jq("#actions").hide();
   jq("#info-block").show();
   jq("#shift").attr("disabled", "disabled");
   jq("#moveOutWeekend").attr("disabled", "disabled");
}, 

addNewMilestone: function()
{
    var title = jq.trim(jq("[id$=tbxTitle]").val());
    var deadline = jq.trim(jq("[id$=Date]").text());
    var prjID = jq.getURLParam("prjID");
    var milestoneID = jq.getURLParam("ID");
    var notifyManager = jq("#notify_manager").is(":checked");
    var isKey =  jq("#is_key").is(":checked");
    var shift = jq("#shift").is(":checked");
    var moveOutWeekend = jq("#moveOutWeekend").is(":checked");
    
    if(jq.getURLParam("action")=="add")   
    {
        shift = false;
    }
    if(title=="")
        {
        jq("[id$=tbxTitle]").val(ASC.Projects.Resources.UntitledMilestone);
        title = ASC.Projects.Resources.UntitledMilestone;
        }  
    AjaxPro.onLoading = function(b)
    {
       if(b){ASC.Projects.Milestones.lockForm();}
    }    
    AjaxPro.MilestoneDetailsView.AddNewMilestone(prjID,milestoneID,title,deadline,notifyManager,isKey,shift,moveOutWeekend,
            function(res)
            {
            if (res.error != null)
                {
                alert(res.error.Message);
                ASC.Projects.Milestones.unlockForm();                 
                return;
                }
            if(res.value=="error")
                {
                alert(ASC.Projects.Resources.IncorrectDate);
                ASC.Projects.Milestones.unlockForm();
                return;
                }
            else
                {
                document.location.replace("milestones.aspx?prjID="+prjID+"&ID="+res.value);                
                }    
            }
            );    
},

finishMilestone: function()
{
    var milestoneID = jq.getURLParam("id");
    var prjID = jq.getURLParam("prjID");
    var countOpenTasks = jq("#pm_openTaskContainer_"+milestoneID+"_"+prjID).children().length;
    
    if(countOpenTasks==0)
    {

    if(milestoneID!="")
        {
            AjaxPro.MilestoneDetailsView.FinishMilestone(milestoneID);    
        }

    location.reload(true);
    }
    else alert(ASC.Projects.Resources.ClosedStatusFunctionIsLocked);   
},

resumeMilestone: function()
{
    var milestoneID = jq.getURLParam("ID");
    
    if(milestoneID!="")
        {
            AjaxPro.MilestoneDetailsView.ResumeMilestone(milestoneID);    
        }

    location.reload(true);   
},

editMilestone: function(prjID,ID,countTasks)
{
if(countTasks==0)
    document.location.replace("milestones.aspx?prjID="+prjID+"&ID="+ID+"&action=edit");
else
    alert(ASC.Projects.Resources.EditFunctionIsLocked);
},

deleteMilestone: function()
{
if(confirm(ASC.Projects.Resources.DeleteThisMilectone))
{
var milestoneID = jq.getURLParam("ID");
var prjID = jq.getURLParam("prjID");
AjaxPro.MilestoneDetailsView.DeleteMilestone(milestoneID);
document.location.replace("milestones.aspx?prjID="+prjID);
}
},

restoreFromTrash: function()
{
var milestoneID = jq.getURLParam("ID");
var prjID = jq.getURLParam("prjID");
AjaxPro.MilestoneDetailsView.RestoreFromTrash(milestoneID);
document.location.replace("milestones.aspx?prjID="+prjID+"&ID="+milestoneID);
},

_prev: function()
    {
        var date = ASC.Projects.Milestones._getCurrentPageDate();
        
        date.setMonth(date.getMonth()-1);

        ASC.Projects.Milestones._initMonthYear(date);
        
        ASC.Projects.Milestones.writeNewMonth();
        
        ASC.Projects.Milestones.drawMonth(date.getFullYear(),date.getMonth());

    },

_next: function()
    {
        var date = ASC.Projects.Milestones._getCurrentPageDate();
        
        date.setMonth(date.getMonth()+1);
        
        ASC.Projects.Milestones._initMonthYear(date);
        
        ASC.Projects.Milestones.writeNewMonth();
        
        ASC.Projects.Milestones.drawMonth(date.getFullYear(),date.getMonth());

    },

_click1: function(object)
    {
        var id;
        var year;
        var month;
        var day;
        
        jq("td.choose").removeClass("choose");
        
        var cell = parseInt(jq(object).attr('id'))-1;
        
        day = Days[cell].dayNumber;
        month = Days[cell].monthNumber;
        year = Days[cell].yearNumber;
        
        if(jq.getURLParam("action")=="edit")
        {
            jq("#shiftMilestonesBox").css("display","block");
        }
            
        jq(object).addClass("choose");

        str=ASC.Projects.Milestones._DateToString(day,month,year);
        date=new Date(month+"/"+day+"/"+year);

        ASC.Projects.Milestones._initHidden(date);
            
        jq("[id$=Date]").text(str);   
            
        if(jq("#oldDeadline").val()==str)
        {
            jq("#shiftMilestonesBox").css("display","none");
            jq("#shift").removeAttr("checked");
            jq("#moveOutWeekend").removeAttr("checked");
                
         }

    },

_click: function(object)
    {
        var id;
        var year;
        var month;
        var day;
        
        jq("td.choose").removeClass("choose");
        
        var cell = parseInt(jq(object).attr('id'))-1;
        
        day = ASC.Projects.Milestones.list[cell].dayNumber;
        month = ASC.Projects.Milestones.list[cell].monthNumber+1;
        year = ASC.Projects.Milestones.list[cell].yearNumber;
        
        if(jq.getURLParam("action")=="edit")
        {
            jq("#shiftMilestonesBox").css("display","block");
        }
            
        jq(object).addClass("choose");

        str=ASC.Projects.Milestones._DateToString(day,month,year);
        date=new Date(month+"/"+day+"/"+year);

        ASC.Projects.Milestones._initHidden(date);
            
        jq("[id$=Date]").text(str);   
            
        if(jq("#oldDeadline").val()==str)
        {
            jq("#shiftMilestonesBox").css("display","none");
            jq("#shift").removeAttr("checked");
            jq("#moveOutWeekend").removeAttr("checked");
                
         }

    },

_getDate: function()
    {
	    jq("#day").val(ChoosenDay[2]);
        jq("#month").val(ChoosenDay[1]);
        jq("#year").val(ChoosenDay[0]);
        var date = new Date(jq("#month").val()+"/"+jq("#day").val()+"/"+jq("#year").val());
	    ASC.Projects.Milestones._initMonthYear(date);
    },
    
_initHidden: function(date)
{
    jq("#day").val(date.getDate());
    jq("#month").val(date.getMonth()+1);
    jq("#year").val(date.getFullYear());
},



_setDate: function(date,day,month,year)
{
    
    date.setYear(parseInt(year));
    date.setDate(parseInt(1));
    date.setMonth(parseInt(month));
    date.setDate(parseInt(day));
},

_initMonthYear: function(date)
{
    jq("#month_year").html(month_names[date.getMonth()] + " " + date.getFullYear());
},

_init: function()
  {
  jq(function () {
	jq('.date_has_event').each(function () {
		// options
		var distance = 7;
		
		var s = navigator.userAgent.toLowerCase();
        var isIE7 = /*@cc_on!@*/false && (parseInt(s.match(/msie (\d+)/)[1], 10) == 7);
		
		if(isIE7) distance = 2;
		
		var time = 250;
		var hideDelay = 500;
 
		var hideDelayTimer = null;
 
		// tracker
		var beingShown = false;
		var shown = false;
 
		var trigger = jq(this);
		var popup = jq('#popup', this).css('opacity', 0);
 
		// set the mouseover and mouseout on both element
		jq([trigger.get(0), popup.get(0)]).mouseover(function () {
			// stops the hide event if we move from the trigger to the popup element
			if (hideDelayTimer) clearTimeout(hideDelayTimer);
 
			// don't trigger the animation again if we're being shown, or already visible
			if (beingShown || shown) {
				return;
			} else {
				beingShown = true;
 
				// reset position of popup box
				popup.css({
					bottom: 3,
					left: -10,
					display: 'block' // brings the popup back in to view
				})
 
				// (we're using chaining on the popup) now animate it's opacity and position
				.animate({
					bottom: '+=' + distance + 'px',
					opacity: 1
				}, time, 'swing', function() {
					// once the animation is complete, set the tracker variables
					beingShown = false;
					shown = true;
				});
			}
		}).mouseout(function () {
			// reset the timer if we get fired again - avoids double animations
			if (hideDelayTimer) clearTimeout(hideDelayTimer);
 
			// store the timer so that it can be cleared in the mouseover if required
			hideDelayTimer = setTimeout(function () {
				hideDelayTimer = null;
				popup.animate({
					bottom: '-=' + distance + 'px',
					opacity: 0
				}, time, 'swing', function () {
					// once the animate is complete, set the tracker variables
					shown = false;
					// hide the popup entirely after the effect (opacity alone doesn't do the job)
					popup.css('display', 'none');
				});
			}, hideDelay);
		});
	});
});
  },
  
_getCurrentPageDate: function()
{
    var tmp = jq("#month_year").html().split(" ");
    var date = new Date();
    var month = date.getMonth()+1;
    for(var i=0;i<12;i++)
        if(month_names[i]==tmp[0])
            month=i;
            
   if(jq.getURLParam("action")=="edit" && isFirst==true)
        {
            ASC.Projects.Milestones._setDate(date,1,parseInt(jq("#month").val())-1,parseInt(jq("#year").val()));
            isFirst=false;
        }
   else
        {
            ASC.Projects.Milestones._setDate(date,1,month,parseInt(tmp[1]));
        }        
    
    return date;
},

    
_DateToString: function(day,month,year)
{
    var date = new Date();
    var str;
    ASC.Projects.Milestones._setDate(date,day,month-1,year);
    str = date.format(dateFormat);
    return str;
},

changeSubscribe : function(milestoneID)
{
  
    AjaxPro.onLoading = function(b)
    {
   
       if(b)  
         jq("#milestone_subscriber").block({message: null});
       else
         jq("#milestone_subscriber").unblock();     
     
    }

    AjaxPro.MilestoneDetailsView.ChangeSubscribe(milestoneID,
     function(res)
     {
      
       if (res.error!=null)
       {
       
         alert(res.error.Message);
         
         return;
       
       }
       
       jq("#milestone_subscriber a").text(jq.trim(res.value));
      
     }
    );       
    
},

writeNewMonth: function()
{
    var prjID = jq.getURLParam("prjID");
    var date = ASC.Projects.Milestones._getCurrentPageDate();
    var year = date.getFullYear();
    var month = date.getMonth()+1;
    
    AjaxPro.MilestoneActionView.GetMonthDays(prjID,year,month,jq("#year").val(), jq("#month").val(), jq("#day").val(),
    function(res)
    {
        Days = eval("("+res.value+")");
        var mydata = {table:Days};
        jq("#divJTemplate").processTemplate(mydata);
        ASC.Projects.Milestones._init();
    
    });
},

drawMonth: function(year,month)
    {

        var date = new Date(year,month+1,1);
        date.setDate(0);
        var countDaysInMonth = date.getDate();

        date = new Date(year,month,1);
        var dayOfWeek = date.getDay() - firstDayOfWeekInCurrentCulture;
        var countDaysInWeekBeforeDate = (-1)*(6 - (7 - dayOfWeek));
        var tmp = countDaysInWeekBeforeDate;
        
        ASC.Projects.Milestones.list = new Array();
        
        for(var i=1; i<43 ; i++)
        {
            date.setDate(tmp);
            
            var css = '';
            for(var j=0;j<Events.length;j++)
            {
                if(date.getFullYear()==Events[j].yearNumber && date.getMonth()==Events[j].monthNumber && date.getDate()==Events[j].dayNumber)
                    css='date_has_event ';
            }
            
            var today = new Date();
            
            if(date>=new Date(year,month,1) && date<=new Date(year,month,countDaysInMonth))
            {
                if(date.getFullYear()==today.getFullYear() && date.getMonth()==today.getMonth() && date.getDate()==today.getDate())
                {
                    ASC.Projects.Milestones.list.push({dayNumber:date.getDate(),monthNumber:date.getMonth(),yearNumber:date.getFullYear(),index:i,cssClass: css+'current'});
                }
                else
                {
                    ASC.Projects.Milestones.list.push({dayNumber:date.getDate(),monthNumber:date.getMonth(),yearNumber:date.getFullYear(),index:i,cssClass:css});
                }
            }
            else
            {
                if(date.getFullYear()==today.getFullYear() && date.getMonth()==today.getMonth() && date.getDate()==today.getDate())
                {
                    ASC.Projects.Milestones.list.push({dayNumber:date.getDate(),monthNumber:date.getMonth(),yearNumber:date.getFullYear(),index:i,cssClass: css+'pm-grayText current'});
                }
                else
                {
                    ASC.Projects.Milestones.list.push({dayNumber:date.getDate(),monthNumber:date.getMonth(),yearNumber:date.getFullYear(),index:i,cssClass:css+'pm-grayText'});
                }
            }
            
            tmp=date.getDate()+1;
            
        }
        
        
        var day = jq("#day").val();
        var month = parseInt(jq("#month").val())-1;
        var year = jq("#year").val();
        
        for(var i=0; i<42; i++)
        {
            if(ASC.Projects.Milestones.list[i].dayNumber==day && ASC.Projects.Milestones.list[i].monthNumber==month && ASC.Projects.Milestones.list[i].yearNumber == year)
                ASC.Projects.Milestones.list[i].cssClass = ASC.Projects.Milestones.list[i].cssClass+" choose";
        }
        
        var data = {
                    daysOfMonth: ASC.Projects.Milestones.list,
                    events: Events
                    };
        jq("#divTemplateContainer").setTemplateElement("templateContainer", null, {filter_data: false});
        jq("#divTemplateContainer").processTemplate(data);
    }         
                


}
})();