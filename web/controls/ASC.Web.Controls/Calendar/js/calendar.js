var Calendar = new function() {

this.CurrentDate = {"Year": (new Date()).getFullYear() , "Month":(new Date()).getMonth()};

this.CurrentWeek = {"Year": (new Date()).getFullYear(), "Month":(new Date()).getMonth(), "Day":(new Date()).getDate()};

this.CurrentCursor = {X:0 , Y:0};

this.Init = function()
{
    if (window.Event && document.captureEvents)
    {
	    document.captureEvents(Event.MOUSEMOVE);
	}
	document.onmousemove = this.getCursorXY;
};

this.InitPopupContainer = function(dayNumber)
{
    jq("#eventTitle").val("");
    
    var selectedDate = new Date();
    selectedDate.setDate(1);
    selectedDate.setYear(Calendar.CurrentDate.Year);
    selectedDate.setMonth(Calendar.CurrentDate.Month+1);
    selectedDate.setDate(0);
    
    var countDaysInMonth = selectedDate.getDate();
    
    var tmpStartingDate="";
    var tmpEndingDate="";
    
    for(var i=Calendar.CurrentDate.Year-5;i<=Calendar.CurrentDate.Year+5;i++)
    {
        if(i==Calendar.CurrentDate.Year)
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateYear"+i+"' selected='selected'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateYear"+i+"' selected='selected'>"+i+"</option>";
        }
        else
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateYear"+i+"'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateYear"+i+"'>"+i+"</option>";
        }
    }
    
    jq("#eventStartingDateYear").html(tmpStartingDate);
    jq("#eventEndingDateYear").html(tmpEndingDate);
    
    var tmpStartingDate="";
    var tmpEndingDate="";
    
    for(var i=0;i<12;i++)
    {
        if(i==Calendar.CurrentDate.Month)
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateMonth"+i+"' selected='selected'>"+Months[i]+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateMonth"+i+"' selected='selected'>"+Months[i]+"</option>";
        }
        else
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateMonth"+i+"'>"+Months[i]+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateMonth"+i+"'>"+Months[i]+"</option>";
        }
    }
    
    jq("#eventStartingDateMonth").html(tmpStartingDate);
    jq("#eventEndingDateMonth").html(tmpEndingDate);
    
    var tmpStartingDate="";
    var tmpEndingDate="";
    
    for(var i=1;i<=countDaysInMonth;i++)
    {
        if(i==dayNumber.split(' ')[0])
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateDay"+i+"' selected='selected'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateDay"+i+"' selected='selected'>"+i+"</option>";
        }
        else
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateDay"+i+"'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateDay"+i+"'>"+i+"</option>";
        }
    }
    
    jq("#eventStartingDateDay").html(tmpStartingDate);
    jq("#eventEndingDateDay").html(tmpEndingDate);
    
    var tmpStartingDate="";
    var tmpEndingDate="";
    
    for(var i=0;i<=23;i++)
    {
        var prefix = "0";
        
        if(i<10)
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateHour"+i+"'>"+prefix+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateHour"+i+"'>"+prefix+i+"</option>";
        }
        else if(i==12)
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateHour"+i+"' selected='selected'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateHour"+i+"'>"+i+"</option>";
        }
        else if(i==13)
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateHour"+i+"'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateHour"+i+"' selected='selected'>"+i+"</option>";
        }
        else
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateHour"+i+"'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateHour"+i+"'>"+i+"</option>";
        }
    }
    
    jq("#eventStartingDateHour").html(tmpStartingDate);
    jq("#eventEndingDateHour").html(tmpEndingDate);
    
    var tmpStartingDate="<option value='0' id='eventStartingDateMinute0' selected='selected'>00</option>";
    var tmpEndingDate="<option value='0' id='eventEndingDateMinute0' selected='selected'>00</option>";
    
    for(var i=1;i<=59;i++)
    {
        var prefix = "0";
        
        if(i<10)
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateMinute"+i+"'>"+prefix+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateMinute"+i+"'>"+prefix+i+"</option>";
        }
        else
        {
            tmpStartingDate+="<option value='"+i+"' id='eventStartingDateMinute"+i+"'>"+i+"</option>";
            tmpEndingDate+="<option value='"+i+"' id='eventEndingDateMinute"+i+"'>"+i+"</option>";
        }
    }
    
    jq("#eventStartingDateMinute").html(tmpStartingDate);
    jq("#eventEndingDateMinute").html(tmpEndingDate);
};

this.InitPopupContainerByEvent = function(eventID)
{
    
    jq('#isEditEvent').val(1);
    var selectedIndex;
    for(var i=0;i<Events.length;i++)
    {
        if(Events[i].ID=='event'+eventID)
        {
            selectedIndex=i;
            break;
        }
    }
    
    startingDate = new Date(parseInt(Events[i].StartingDate.substring(6,19)));
    endingDate = new Date(parseInt(Events[i].EndingDate.substring(6,19)));
    
    Calendar.InitPopupContainer(1);
    
    jq("#eventTitle").val(Events[selectedIndex].Title);
    
    jq("#eventStartingDateYear"+startingDate.getFullYear()).attr("selected","selected");
    jq("#eventStartingDateMonth"+startingDate.getMonth()).attr("selected","selected");
    jq("#eventStartingDateDay"+startingDate.getDate()).attr("selected","selected");
    jq("#eventStartingDateHour"+startingDate.getHours()).attr("selected","selected");
    jq("#eventStartingDateMinute"+startingDate.getMinutes()).attr("selected","selected");
    
    jq("#eventEndingDateYear"+endingDate.getFullYear()).attr("selected","selected");
    jq("#eventEndingDateMonth"+endingDate.getMonth()).attr("selected","selected");
    jq("#eventEndingDateDay"+endingDate.getDate()).attr("selected","selected");
    jq("#eventEndingDateHour"+endingDate.getHours()).attr("selected","selected");
    jq("#eventEndingDateMinute"+endingDate.getMinutes()).attr("selected","selected");
    
    if(jq("#eventColor").length!=0)
    {
        jq("#eventColor").css('background-color',Events[selectedIndex].BackgroundColor).css('color',Events[selectedIndex].FontColor);
    }
    
};

this.getCursorXY = function(e)
{
	var x = (window.Event && e) ? e.pageX : event.clientX + (document.documentElement.scrollLeft ? document.documentElement.scrollLeft : document.body.scrollLeft);
	var y = (window.Event && e) ? e.pageY : event.clientY + (document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop);
	
	var pageWidth = ((document.width) ? document.width : document.body.offsetWidth)-100;
	var pageHeight = ((document.height) ? document.height : document.body.offsetHeight)-100;
	
	var popupWidth = jq("#popup").width();
	var popupHeight = jq("#popup").height();
	
	if((pageHeight-y-popupHeight)<0)
	{
	    Calendar.CurrentCursor.Y = y - popupHeight;
	}
	else
	{
	    Calendar.CurrentCursor.Y = y;
	}
	
	if((pageWidth-x-popupWidth)<0)
	{
	    Calendar.CurrentCursor.X = x - popupWidth;
	}
	else
	{
	    Calendar.CurrentCursor.X = x;
	}
};

this.closePopup = function()
{
    jq('#popup').css('display','none');
    jq('.selected').removeClass('selected');
};

this.writeNavigationPanelTitle = function()
{  
    var action = jq.getURLParam("action");
    
    if(action==2)
    {
        var tmp = window.location.href.split("#");
    
        if(tmp.length>1)
        {
            if(tmp[1]!="")
                {
                    var array = tmp[1].split("/");
                    this.CurrentWeek.Year = parseInt(array[0]);
                    var week = parseInt(array[1])
                    var d = new Date(this.CurrentWeek.Year,0,1+week*7);
                    this.CurrentWeek.Month = d.getMonth();
                    this.CurrentWeek.Day = d.getDate();
                }
            else 
            window.location.href =  tmp[0];   
        }
        
        var newYear = new Date(this.CurrentWeek.Year, 0, 0).getTime();
        var now = new Date(this.CurrentWeek.Year,this.CurrentWeek.Month,this.CurrentWeek.Day).getTime();
        var week = Math.round((now - newYear) / (1000 * 60 * 60 * 24 * 7)); 
        jq("#title").html(" #"+week+" "+this.CurrentWeek.Year);
    }
    else
    {
        var tmp = window.location.href.split("#");
    
        if(tmp.length>1)
        {
            if(tmp[1]!="")
                {
                    var array = tmp[1].split("/");
                    this.CurrentDate.Month = parseInt(array[1]-1);
                    this.CurrentDate.Year = parseInt(array[0]);
                }
            else 
            window.location.href =  tmp[0];   
        }
    
        jq("div.calendar-navigation-panel-title").html(Months[this.CurrentDate.Month]+" "+this.CurrentDate.Year);
    }
};

this.initPrevMonthButton = function()
{
    document.getElementById("prevMonthButton").onclick = function()
    {
    
    Calendar.CurrentDate.Month--;
            
    Calendar.closePopup();
    
    if(Calendar.CurrentDate.Month<0)
    {
        Calendar.CurrentDate.Month=11;
        Calendar.CurrentDate.Year--;    
    }
    
    window.location.href = window.location.href.replace(/#\S+/g, '') + '#' + Calendar.CurrentDate.Year + '/' + (Calendar.CurrentDate.Month+1);
    
    Calendar.drawMonth();
            
    return false;
    
    }
};

this.initPrevWeekButton = function()
{
    document.getElementById("prevWeekButton").onclick = function()
    {
    var day = Calendar.CurrentWeek.Day - 7;
    
    var date = new Date(Calendar.CurrentWeek.Year, Calendar.CurrentWeek.Month, day);
    
    Calendar.CurrentWeek.Year = date.getFullYear();
    Calendar.CurrentWeek.Month = date.getMonth();
    Calendar.CurrentWeek.Day = date.getDate();
    
    var newYear = new Date(Calendar.CurrentWeek.Year, 0, 0).getTime();
    var now = new Date(Calendar.CurrentWeek.Year,Calendar.CurrentWeek.Month,Calendar.CurrentWeek.Day).getTime();
    var additional = firstDayOfWeekInCurrentCulture==1 ? 0 : 1; 
    var week = Math.round((now - newYear) / (1000 * 60 * 60 * 24 * 7))+additional; 
    window.location.href = window.location.href.replace(/#\S+/g, '') + '#' + Calendar.CurrentWeek.Year + '/' + week;
    
    Calendar.drawWeek();
            
    return false;
    
    }
};

this.initNextMonthButton = function()
{
    document.getElementById("nextMonthButton").onclick = function()
    {
    Calendar.CurrentDate.Month++;
    
    Calendar.closePopup();
    
    if(Calendar.CurrentDate.Month>11)
    {
        Calendar.CurrentDate.Month=0;
        Calendar.CurrentDate.Year++;    
    }
    
    window.location.href = window.location.href.replace(/#\S+/g, '') + '#' + Calendar.CurrentDate.Year + '/' + (Calendar.CurrentDate.Month+1);
    
    Calendar.drawMonth();
            
    return false;
    
    }
};

this.initNextWeekButton = function()
{
    document.getElementById("nextWeekButton").onclick = function()
    {
    var day = Calendar.CurrentWeek.Day + 7;
    
    var date = new Date(Calendar.CurrentWeek.Year, Calendar.CurrentWeek.Month, day);
    
    Calendar.CurrentWeek.Year = date.getFullYear();
    Calendar.CurrentWeek.Month = date.getMonth();
    Calendar.CurrentWeek.Day = date.getDate();
    
    var newYear = new Date(Calendar.CurrentWeek.Year, 0, 0).getTime();
    var now = new Date(Calendar.CurrentWeek.Year,Calendar.CurrentWeek.Month,Calendar.CurrentWeek.Day).getTime();
    var additional = firstDayOfWeekInCurrentCulture==1 ? 0 : 1; 
    var week = Math.round((now - newYear) / (1000 * 60 * 60 * 24 * 7))+additional; 
    window.location.href = window.location.href.replace(/#\S+/g, '') + '#' + Calendar.CurrentWeek.Year + '/' + week;
    
    Calendar.drawWeek();
            
    return false;
    
    }
};

this.write = function()
{     
        Calendar.writeNavigationPanelTitle();
        
        var currentMonth = new Date();
        
        currentMonth.setDate(1);
        currentMonth.setYear(parseInt(this.CurrentDate.Year));
        currentMonth.setMonth(parseInt(this.CurrentDate.Month));
        
        Events.sort(Calendar.sortfunction);
        
        for(var i=0;i<IsAlreadyWrite.length;i++)
        {
            IsAlreadyWrite[i]="0,16";
        }
        
        Calendar.writeMonth(currentMonth);
        
};

this.writeDay = function(weekContainer,dayNumber)
{
    var elementDayNumber = document.createElement('div');
    var elementDay = document.createElement('div');
    
    jq(elementDayNumber).addClass('wbs_number');
        if(dayNumber!=0 && dayNumber!=null)
        {
            jq(elementDayNumber).append(dayNumber);
            
            jq(elementDay).unbind("click").bind("click", function()
                    {
                        jq("div.selected").removeClass('selected');
                        jq(this).addClass('selected');

                        Calendar.InitPopupContainer(parseInt(this.childNodes[0].innerHTML));
                        
                        var style = 'display:block;top:'+Calendar.CurrentCursor.Y+'px;left:'+Calendar.CurrentCursor.X+'px;';
                        jq("#popup").attr('style',style);
                    });
        }
    
    var today = new Date();
    var cssClassToday = '';
    if(today.getFullYear()==this.CurrentDate.Year && today.getMonth() == this.CurrentDate.Month && today.getDate() == dayNumber)
        cssClassToday = 'today';
    
    jq(elementDay).addClass('wbs_day').addClass(cssClassToday).append(elementDayNumber);
    
    jq(weekContainer).append(elementDay);
};

this.writeMonth = function(date)
{
    date.setDate(1);
    
    var firstDayOfWeekInMonth = date.getDay();
    
    firstDayOfWeekInMonth = (firstDayOfWeekInMonth - firstDayOfWeekInCurrentCulture) < 0 ? 6 : (firstDayOfWeekInMonth - firstDayOfWeekInCurrentCulture);
    
    var nextMonth = date.getMonth()+1;
    
    date.setMonth(nextMonth);
    date.setDate(0);
    
    var countDaysInMonth = date.getDate();
    
    var lastDayOfWeekInMonth = date.getDay();
    
    lastDayOfWeekInMonth = (lastDayOfWeekInMonth - firstDayOfWeekInCurrentCulture) < 0 ? 6 : (lastDayOfWeekInMonth - firstDayOfWeekInCurrentCulture);
    
    var monthContainer = jq("#month");
    
    jq(monthContainer).html("");
    
    var weekContainer = document.createElement('div');
    
    jq(weekContainer).addClass('wbs_week');
    
    var moreDays = 7 - (lastDayOfWeekInMonth+1);
    
    var dayNumber = 0;
    
    for(var i=0;i<(firstDayOfWeekInMonth+1)+countDaysInMonth+moreDays;i++)
    {
        if(i>=firstDayOfWeekInMonth && dayNumber<countDaysInMonth)
        {
            dayNumber++;
            Calendar.writeDay(weekContainer,dayNumber);
            date.setDate(dayNumber);
            Calendar.writeEvents(weekContainer,date);
        }
        else
        {
            Calendar.writeDay(weekContainer,0);
        }
        
        if(i!=0 && ((i+1)%7==0))
        {   
            jq(monthContainer).append(weekContainer);
            weekContainer = document.createElement('div');
            jq(weekContainer).addClass('wbs_week');
        }
    }
 
};

this.writeEvents = function(weekContainer,date)
{
    date.setHours(0);
    date.setMinutes(0);
    date.setSeconds(0);
    date.setMilliseconds(0);
    
    var ID;
    var startingDate = new Date();
    var endingDate = new Date();
    var title;
    var description;
    var eventURL;
    var bgColor;
    var fontColor;
    
    var countEventsInOneDay = 0;
    
    var left;
    var top;
    var width;
    
    var lastEventTop = 16;

    for(var i=0;i<Events.length;i++)
    {
        startingDate = new Date(parseInt(Events[i].StartingDate.substring(6,19)));
        startingDate.setHours(0);
        startingDate.setMinutes(0);
        startingDate.setSeconds(0);
        startingDate.setMilliseconds(0); 
        
        endingDate = new Date(parseInt(Events[i].EndingDate.substring(6,19)));
        endingDate.setHours(0);
        endingDate.setMinutes(0);
        endingDate.setSeconds(0);
        endingDate.setMilliseconds(0); 
        
        ID = Events[i].ID;
        title = Events[i].Title;
        description = Events[i].Description;
        eventURL = Events[i].EventURl;
        bgColor = Events[i].BackgroundColor;
        fontColor = Events[i].FontColor;
        
        if(date>=startingDate && date<=endingDate)
        {
            countEventsInOneDay++;
            
            if(IsAlreadyWrite[i].split(',')[0]==1)
            {
                lastEventTop = parseInt(IsAlreadyWrite[i].split(',')[1])+16;
            }
            
            //if(countEventsInOneDay>5)
            //{
            var height = parseInt(jq(weekContainer).css('height'));
            if(height<(countEventsInOneDay+1)*16)
                jq(weekContainer).css('height',height+16);
            //}
            
                var dayOfWeek = ( (date.getDay()-firstDayOfWeekInCurrentCulture<0) ? 6 : date.getDay()-firstDayOfWeekInCurrentCulture );
                
                if(dayOfWeek==0) {IsAlreadyWrite[i]="0,16";lastEventTop=16;}
                
                if(IsAlreadyWrite[i].split(',')[0]==0)
                {
                    var hack = 0;
                    if(jq.browser.mozilla) hack = 0.2;
                    if(jq.browser.chrome) hack = -0.1+(dayOfWeek>=5 ? -0.3 : 0);
                    if(jq.browser.msie) hack = 0.1+(dayOfWeek>5 ? 0.1 : 0);
                    left = 14.2 * dayOfWeek + hack;
                    
                    //duration 
                    var duration = (endingDate - date)/(24*60*60*1000)+1;
                    
                    var space;
                    var tmpDate = new Date(date);
                    tmpDate.setDate(1);
                    tmpDate.setMonth(date.getMonth()+1);
                    tmpDate.setDate(0);

                    var countDaysInMonth = tmpDate.getDate();
                    var lastDaysInWeeks = new Array();
                    var tmp = date.getDay()==0 ? 7 : date.getDay();
                    tmpDate.setDate(date.getDate()+6-tmp+firstDayOfWeekInCurrentCulture)
                    if(tmpDate.getMonth()+1==date.getMonth()) tmpDate.setDate(tmpDate.getDate()+7);
                    var lastDayInWeek = date.getMonth()== tmpDate.getMonth() ? tmpDate.getDate():countDaysInMonth;

                    while(lastDayInWeek<countDaysInMonth)
                    {
                        lastDaysInWeeks.push(lastDayInWeek);
                        lastDayInWeek+=7;
                    }
                    lastDaysInWeeks.push(countDaysInMonth);
                    
                    for(var k=0;k<lastDaysInWeeks.length;k++)
                    {
                        if(date.getDate()<=lastDaysInWeeks[k])
                        {
                            space=lastDaysInWeeks[k]-date.getDate()+1;
                            break;
                        }
                    }
                    
                    width = 14.1*(duration<=space ? duration : space);
                    
                    if(jq.browser.mozilla || jq.browser.msie)
                    {
                    var tmp = (duration<=space ? duration : space)-2;
                    width+= tmp>0 ? tmp/10 : 0;
                    }
                    
                    //if(duration>1)
                    //{
                    //   top = Calendar.getEventTopShift(date,duration) 
                    //}
                    //else
                    //{
                        top = 16 * countEventsInOneDay;
                        
                        if(lastEventTop>top)
                            top = lastEventTop;
                        
                        lastEventTop+=16;    
                    //}

                    var eventContainer = document.createElement('div');
                    jq(eventContainer).attr('id', ID);
                    jq(eventContainer).addClass('wbs_event');
                    jq(eventContainer).attr('title', title);
                    eventContainer.eventURL =  eventURL;
                    jq(eventContainer).html(title);
                    jq(eventContainer).unbind("click").bind("click", function()
                    {
                       if(this.eventURL!="" && this.eventURL!=null)
                        location.assign(this.eventURL);
                       else void(0); 
                    }); 
                    
                    var style = 'left:'+left+'%; top:'+top+'px; width:'+width+'%;background-color:'+bgColor+';color:'+fontColor+';';
                    
                    if((dayOfWeek==0 && date>startingDate)||(date.getDate()==1 && date>startingDate)) style += '-moz-border-radius-topleft: 0px;-moz-border-radius-bottomleft: 0px;';
                    if(duration>space) style += '-moz-border-radius-topright: 0px;-moz-border-radius-bottomright: 0px;';
                    
                    jq(eventContainer).attr('style',style);
                    
                    jq(weekContainer).append(eventContainer);
                    
                    height = parseInt(jq(weekContainer).css('height'));
                    if(height<top+16)
                    jq(weekContainer).css('height',height+16);
                    
                    IsAlreadyWrite[i]="1,"+String(top);
                }
            //}
        }
    }
};

this.writeEventsOnWeekView = function(weekContainer,date)
{
    date.setHours(0);
    date.setMinutes(0);
    date.setSeconds(0);
    date.setMilliseconds(0);
    
    var ID;
    var startingDate = new Date();
    var endingDate = new Date();
    var title;
    var description;
    var eventURL;
    var bgColor;
    var fontColor;
    
    var countEventsInOneDay = 0;
    
    var left;
    var top;
    var width;
    
    var lastEventTop = 16;

    for(var i=0;i<Events.length;i++)
    {
        startingDate = new Date(parseInt(Events[i].StartingDate.substring(6,19)));
        startingDate.setHours(0);
        startingDate.setMinutes(0);
        startingDate.setSeconds(0);
        startingDate.setMilliseconds(0); 
        
        endingDate = new Date(parseInt(Events[i].EndingDate.substring(6,19)));
        endingDate.setHours(0);
        endingDate.setMinutes(0);
        endingDate.setSeconds(0);
        endingDate.setMilliseconds(0); 
        
        ID = Events[i].ID;
        title = Events[i].Title;
        description = Events[i].Description;
        eventURL = Events[i].EventURl;
        bgColor = Events[i].BackgroundColor;
        fontColor = Events[i].FontColor;
        
        if(date>=startingDate && date<=endingDate)
        {
            countEventsInOneDay++;
            
            if(IsAlreadyWrite[i].split(',')[0]==1)
            {
                lastEventTop = parseInt(IsAlreadyWrite[i].split(',')[1])+16;
            }
            
            //if(countEventsInOneDay>5)
            //{
            var height = parseInt(jq(weekContainer).css('height'));
            if(height<(countEventsInOneDay+1)*16)
                jq(weekContainer).css('height',height+16);
            //}
            
                var dayOfWeek = ( (date.getDay()-firstDayOfWeekInCurrentCulture<0) ? 6 : date.getDay()-firstDayOfWeekInCurrentCulture );
                
                if(dayOfWeek==0) {IsAlreadyWrite[i]="0,16";lastEventTop=16;}
                
                if(IsAlreadyWrite[i].split(',')[0]==0)
                {
                    var hack = jq.browser.msie ? 12.4 : 12.5;
                    var hack2 = jq.browser.msie ? 0.1 : 0;
                    left = hack * dayOfWeek + 12.3 + hack2*dayOfWeek;
                    if(jq.browser.chrome) left=left+0.1;
                    
                    var duration = (endingDate - date)/(24*60*60*1000)+1;
                    
                    var space;
                    var tmpDate = new Date(date);
                    tmpDate.setDate(1);
                    tmpDate.setMonth(date.getMonth()+1);
                    tmpDate.setDate(0);
                    var countDaysInMonth = tmpDate.getDate();

                    var lastDaysInWeeks = new Array();
                    var tmp = date.getDay()==0 ? 7 : date.getDay();

                    tmpDate.setDate(date.getDate()+6-tmp+firstDayOfWeekInCurrentCulture)
                    if(tmpDate.getMonth()+1==date.getMonth()) tmpDate.setDate(tmpDate.getDate()+7);
                    var lastDayInWeek = date.getMonth()== tmpDate.getMonth() ? tmpDate.getDate():countDaysInMonth+tmpDate.getDate();

                    while(lastDayInWeek<=countDaysInMonth)
                    {
                        lastDaysInWeeks.push(lastDayInWeek);
                        lastDayInWeek+=7;
                    }

                    if(date.getMonth()!= tmpDate.getMonth())
                        lastDaysInWeeks.push(countDaysInMonth+tmpDate.getDate());
                    

                    for(var k=0;k<lastDaysInWeeks.length;k++)
                    {
                        if(date.getDate()<=lastDaysInWeeks[k])
                        {
                            space=lastDaysInWeeks[k]-date.getDate()+1;
                            break;
                        }
                    }
                    

                    width = hack*(duration<=space ? duration : space);
                    if(jq.browser.chrome) width=width-0.2;
                    if(jq.browser.mozilla) width=width-0.1;

                    //if(duration>1)
                    //{
                    //   top = Calendar.getEventTopShift(date,duration) 
                    //}
                    //else
                    //{
                        top = 16 * countEventsInOneDay;
                        
                        if(lastEventTop>top)
                            top = lastEventTop;
                        
                        lastEventTop+=16;    
                    //}

                    var eventContainer = document.createElement('div');
                    jq(eventContainer).attr('id', ID);
                    jq(eventContainer).addClass('wbs_event');
                    jq(eventContainer).attr('title', title);
                    eventContainer.eventURL =  eventURL;
                    jq(eventContainer).html(title);
                    jq(eventContainer).unbind("click").bind("click", function()
                    {
                       if(this.eventURL!="" && this.eventURL!=null)
                        location.assign(this.eventURL);
                       else void(0); 
                    }); 
                    
                    var style = 'left:'+left+'%; top:'+top+'px; width:'+width+'%;background-color:'+bgColor+';color:'+fontColor+';';
                    
                    if((dayOfWeek==0 && date>startingDate)||(date.getDate()==1 && date>startingDate)) style += '-moz-border-radius-topleft: 0px;-moz-border-radius-bottomleft: 0px;';
                    if(duration>space) style += '-moz-border-radius-topright: 0px;-moz-border-radius-bottomright: 0px;';
                    
                    jq(eventContainer).attr('style',style);
                    
                    jq(weekContainer).append(eventContainer);
                    
                    height = parseInt(jq(weekContainer).css('height'));
                    if(height<top+16)
                    jq(weekContainer).css('height',height+16);
                    
                    IsAlreadyWrite[i]="1,"+String(top);
                }
            //}
        }
    }
};

this.AddSplitter = function(start,events,parent)
{
    var parentID = jq(parent).attr('id').split('_');
    var parentsIDs = new Array();
    
    for(var i=0;i<events.length;i++)
    {
        startingDate = new Date(parseInt(events[i].StartingDate.substring(6,19)));
        endingDate = new Date(parseInt(events[i].EndingDate.substring(6,19)));
        
        var flag = false;
        var id = startingDate.getHours()+"_"+parentID[2];
        
        if(startingDate<=start && endingDate>=start)
        {    
            for(var j=0; j<parentsIDs.length; j++)
            {
                if(parentsIDs[j]==id)
                {
                    flag = true;
                    break;
                }
            }
            if(!flag)
            {
                parentsIDs.push(id);
                var length = jq("#div_"+id).children().length;
                if(length>0)
                {
                    var childs = jq("#div_"+id).children();
                    if(childs.length!=0)
                    {
                        for(var j=0;j<childs.length;j++)
                        {
                            var left = (100/(childs.length+1))*j;   
                            var width = 100/(childs.length+1);
                            jq(childs[j]).css('left',left).css('width',width);
                        } 
                        
                        var splitter = document.createElement('div');
                        jq(splitter).addClass('eventSplitter');
                        jq(splitter).css('left',(100/(childs.length+1))*(childs.length)).css('width',width);
                        jq("#div_"+id).append(splitter);
                    }
                }
                
                length = jq("#div_"+id).children().length;
                
                var count = length - jq(parent).children().length;
                
                for(var j=0;j<count;j++)
                {
                    var splitter = document.createElement('div');
                    jq(splitter).addClass('eventSplitter');
                    jq(parent).prepend(splitter);
                }
                
                for(var j=0;j<length;j++)
                {
                    var left = (100/(length))*j;   
                    var width = 100/(length);
                    jq(jq(parent).children()[j]).css('left',left).css('width',width);
                }
            }
        }
    }
    if(parentsIDs.length==0) return;
};

this.writeEventsByHours = function(cell,date)
{
    var events = new Array();
    var cellID = jq(cell).attr('id').split('_');
    
    var ID;
    var startingDate = new Date();
    var endingDate = new Date();
    var title;
    var description;
    var eventURL;
    var bgColor;
    var fontColor;

    var countEvents = 0;

    var left;
    var top;
    var width;
    var height;
    
    var lastEventTop = 16;
    
    for(var i=0;i<Events.length;i++)
    {

        startingDate = new Date(parseInt(Events[i].StartingDate.substring(6,19)));
        startingDate.setHours(0);
        startingDate.setMinutes(0);
        startingDate.setSeconds(0);
        startingDate.setMilliseconds(0); 

        endingDate = new Date(parseInt(Events[i].EndingDate.substring(6,19)));
        endingDate.setHours(0);
        endingDate.setMinutes(0);
        endingDate.setSeconds(0);
        endingDate.setMilliseconds(0);

        if(date.getFullYear()==startingDate.getFullYear() && date.getMonth()==startingDate.getMonth() && date.getDate()==startingDate.getDate())
        {
            if(endingDate.getFullYear()==startingDate.getFullYear() && endingDate.getMonth()==startingDate.getMonth() && endingDate.getDate()==startingDate.getDate())
            {
                events.push(Events[i]);
            }
        }
    }
    
    events.sort(Calendar.sortfunctionForWeekView);
    
    var parent = document.createElement('div');
    jq(parent).attr('id', 'div_'+cellID[1]+'_'+cellID[2]);

    for(var i=0;i<events.length;i++)
    {

        startingDate = new Date(parseInt(events[i].StartingDate.substring(6,19)));
        endingDate = new Date(parseInt(events[i].EndingDate.substring(6,19)));
        ID = events[i].ID;
        title = events[i].Title;
        description = events[i].Description;
        eventURL = events[i].EventURl;
        bgColor = events[i].BackgroundColor;
        fontColor = events[i].FontColor;
        
        var duration = endingDate.getHours()-startingDate.getHours();
        
        if(date.getFullYear()==startingDate.getFullYear() && date.getMonth()==startingDate.getMonth() && date.getDate()==startingDate.getDate() && cellID[1]==startingDate.getHours())
        {
            countEvents++;        
             
            height = 24; if(duration>0) height+=(24*(duration-1));
            if(jq.browser.msie || jq.browser.chrome) height+=duration-1;
            
            Calendar.AddSplitter(startingDate,events,parent);
            
            var childs = jq(parent).children();
            left = (100/(childs.length+1))*childs.length;   
            width = 100/(childs.length+1);
                
            for(var j=0;j<childs.length;j++)
            {
                jq(childs[j]).css('left',j*(100/(childs.length+1))).css('width',100/(childs.length+1));
            }

             var eventContainer = document.createElement('div');
             jq(eventContainer).addClass('tagged_item');
             jq(eventContainer).attr('title', title);
             jq(eventContainer).html(title);
             eventContainer.eventURL =  eventURL;
             jq(eventContainer).unbind("click").bind("click", function()
             {
                if(this.eventURL!="" && this.eventURL!=null)
                    location.assign(this.eventURL);
                else void(0); 
             }); 
                    
             var style = 'left:'+left+'%; height:'+height+'px; width:'+width+'%;background-color:'+bgColor+';color:'+fontColor+';';
             jq(eventContainer).attr('style',style);
             jq(parent).append(eventContainer);          

        }
    }
    if(countEvents>0)jq(cell).append(parent);
};

this.writeWeekDays = function()
{
    for (var i = 0; i < 7; i++)
        {
            if(firstDayOfWeekInCurrentCulture==1)
                {

                    if(i==5 || i==6)
                        jq("div.calendar-week-days").append("<div class='wbs_weekend'>"+daysOfWeek[i].title+"</div>");
                    else
                        jq("div.calendar-week-days").append("<div>"+daysOfWeek[i].title+"</div>");
                }
            else
                {
                    if(i==0 || i==6)
                        jq("div.calendar-week-days").append("<div class='wbs_weekend'>"+daysOfWeek[i].title+"</div>");
                    else
                        jq("div.calendar-week-days").append("<div>"+daysOfWeek[i].title+"</div>");
                }    
        }
};

this.cerateEvent = function()
{

    var prjID = jq.getURLParam("prjID").split("#")[0];
    var title = jq("#eventTitle").val();
    
    var startingYear = jq('#eventStartingDateYear option:selected').val();
    var startingMonth = parseInt(jq('#eventStartingDateMonth option:selected').val())+1;
    var startingDay = jq('#eventStartingDateDay option:selected').val();
    var startingHour = jq('#eventStartingDateHour option:selected').val();
    var startingMinute = jq('#eventStartingDateMinute option:selected').val();
    
    var endingYear = jq('#eventEndingDateYear option:selected').val();
    var endingMonth = parseInt(jq('#eventEndingDateMonth option:selected').val())+1;
    var endingDay = jq('#eventEndingDateDay option:selected').val();
    var endingHour = jq('#eventEndingDateHour option:selected').val();
    var endingMinute = jq('#eventEndingDateMinute option:selected').val();
    
    AjaxPro.Calendar.CreateNewEvent_BadVersion(prjID, title, startingYear, startingMonth, startingDay, startingHour, startingMinute, endingYear, endingMonth, endingDay, endingHour, endingMinute,
    function(res)
    {
        if (res.error != null)
        {
            alert(res.error.Message);
        }
        

        Events.push(eval("("+res.value+")"));
        IsAlreadyWrite.push("0,16");
        

        Calendar.write();
        Calendar.closePopup();
    }
    );
};

this.createEventRequest = function()
{
    var urlParam = jq.getURLParam("prjID");
    var prjID = urlParam!=null ? urlParam.split("#")[0] : '0';
    var title = jq("#eventTitle").val();
    
    while(title.indexOf("\"")>-1)
    {
        title = title.replace("\"","&quot;");
    }
    
    var startingYear = jq('#eventStartingDateYear option:selected').val();
    var startingMonth = parseInt(jq('#eventStartingDateMonth option:selected').val())+1;
    var startingDay = jq('#eventStartingDateDay option:selected').val();
    var startingHour = jq('#eventStartingDateHour option:selected').val();
    var startingMinute = jq('#eventStartingDateMinute option:selected').val();
    
    var endingYear = jq('#eventEndingDateYear option:selected').val();
    var endingMonth = parseInt(jq('#eventEndingDateMonth option:selected').val())+1;
    var endingDay = jq('#eventEndingDateDay option:selected').val();
    var endingHour = jq('#eventEndingDateHour option:selected').val();
    var endingMinute = jq('#eventEndingDateMinute option:selected').val();
    
    var bgColor = jq("#eventColor").length!=0 ? jq("#eventColor").css('background-color') : "";
    var fontColor = jq("#eventColor").length!=0 ? jq("#eventColor").css('color') : "";
    
    var request = "{";
    request += "'prjID':"+prjID+",";
    request += "'title':\""+title+"\",";
    request += "'startingYear':"+startingYear+",";
    request += "'startingMonth':"+startingMonth+",";
    request += "'startingDay':"+startingDay+",";
    request += "'startingHour':"+startingHour+",";
    request += "'startingMinute':"+startingMinute+",";
    request += "'endingYear':"+endingYear+",";
    request += "'endingMonth':"+endingMonth+",";
    request += "'endingDay':"+endingDay+",";
    request += "'endingHour':"+endingHour+",";
    request += "'endingMinute':"+endingMinute+",";
    request += "'bgColor':\""+bgColor+"\",";
    request += "'fontColor':\""+fontColor+"\"";
    request += "}";
    
    return request;
};

this.getEventTopShift = function(startDate,duration)
{
    var date = new Date(startDate);
    var maxCount = 0;
    var startingDate = new Date();
    var endingDate = new Date();
    
    for(var i=0; i<duration; i++)
    {
        var dateNumber = date.getDate();
        var count = 0;
        date.setDate(dateNumber+1);
        
        for(var i=0;i<Events.length;i++)
        {
        if(IsAlreadyWrite[i].split(',')[0]==0)
        {
        startingDate = new Date(parseInt(Events[i].StartingDate.substring(6,19)));
        startingDate.setHours(0);
        startingDate.setMinutes(0);
        startingDate.setSeconds(0);
        startingDate.setMilliseconds(0); 
        
        endingDate = new Date(parseInt(Events[i].EndingDate.substring(6,19)));
        endingDate.setHours(0);
        endingDate.setMinutes(0);
        endingDate.setSeconds(0);
        endingDate.setMilliseconds(0); 

        if(date>=startingDate && date<=endingDate)
        {
            count++;
        }
        }
        }
        
        if(count>maxCount) maxCount=count;
    }
    
    return maxCount*16;
};

this.sortfunction = function (a, b)
{
    aStartingDate = new Date(parseInt(a.StartingDate.substring(6,19)));
        aStartingDate.setHours(0);
        aStartingDate.setMinutes(0);
        aStartingDate.setSeconds(0);
        aStartingDate.setMilliseconds(0); 
    aEndingDate = new Date(parseInt(a.EndingDate.substring(6,19)));
        aEndingDate.setHours(0);
        aEndingDate.setMinutes(0);
        aEndingDate.setSeconds(0);
        aEndingDate.setMilliseconds(0);
    
    bStartingDate = new Date(parseInt(b.StartingDate.substring(6,19)));
        bStartingDate.setHours(0);
        bStartingDate.setMinutes(0);
        bStartingDate.setSeconds(0);
        bStartingDate.setMilliseconds(0);
    bEndingDate = new Date(parseInt(b.EndingDate.substring(6,19)));
        bEndingDate.setHours(0);
        bEndingDate.setMinutes(0);
        bEndingDate.setSeconds(0);
        bEndingDate.setMilliseconds(0);
    
    var aDuration = parseInt((aEndingDate - aStartingDate)/(24*60*60*1000))+1;
    var bDuration = parseInt((bEndingDate - bStartingDate)/(24*60*60*1000))+1;
    
    if(aStartingDate>bStartingDate) return 1;
    if(aStartingDate<bStartingDate) return -1;
    
    if((aStartingDate-bStartingDate) == 0)
    {
        if(aDuration>bDuration) return -1;
        if(aDuration<bDuration) return 1;
    }
    
    return 0;
    
};

this.sortfunctionForWeekView = function (a, b)
{
    aStartingDate = new Date(parseInt(a.StartingDate.substring(6,19)));
    aEndingDate = new Date(parseInt(a.EndingDate.substring(6,19)));
    
    bStartingDate = new Date(parseInt(b.StartingDate.substring(6,19)));
    bEndingDate = new Date(parseInt(b.EndingDate.substring(6,19)));
    
    var aDuration = (aEndingDate.getHours() - aStartingDate.getHours())+1;
    var bDuration = (bEndingDate.getHours() - bStartingDate.getHours())+1;
    
    if(aStartingDate>bStartingDate) return 1;
    if(aStartingDate<bStartingDate) return -1;
    
    if((aStartingDate-bStartingDate) == 0)
    {
        if(aDuration>bDuration) return -1;
        if(aDuration<bDuration) return 1;
    }
    
    return 0;
    
};

this.drawMonth = function ()
    {

        Calendar.writeNavigationPanelTitle();

        Events.sort(Calendar.sortfunction);

        for(var i=0;i<IsAlreadyWrite.length;i++)
        {
            IsAlreadyWrite[i]="0,16";
        }

        var date = new Date(this.CurrentDate.Year,this.CurrentDate.Month+1,1);
        date.setDate(0);
        var countDaysInMonth = date.getDate();

        date = new Date(this.CurrentDate.Year,this.CurrentDate.Month,1);
        var dayOfWeek = date.getDay() - firstDayOfWeekInCurrentCulture;
        if(dayOfWeek==-1) dayOfWeek=6;
        var countDaysInWeekBeforeDate = (-1)*(6 - (7 - dayOfWeek));
        var tmp = countDaysInWeekBeforeDate;

        var index = 1;
        var list = new Array();
        var stop = false;

        while(!stop)
        {
            date.setDate(tmp);
            
            if(date>=new Date(this.CurrentDate.Year,this.CurrentDate.Month,1) && date<=new Date(this.CurrentDate.Year,this.CurrentDate.Month,countDaysInMonth))
            {
                var today = new Date();
                
                if(date.getFullYear()==today.getFullYear() && date.getMonth()==today.getMonth() && date.getDate()==today.getDate())
                {
                    list.push({dayNumber:date.getDate(),index:index,cssToday: jq.browser.msie?'week_today':'today'});
                }
                else
                {
                    list.push({dayNumber:date.getDate(),index:index,cssToday: jq.browser.msie?'day':''});
                }
            }
            else list.push({dayNumber:'',index:index,cssToday:''});
            tmp=date.getDate()+1;
            
            if(index%7==0)
            {
                if(index>(countDaysInMonth+(-1)*countDaysInWeekBeforeDate))
                {
                    stop=true;
                }
            }
            
            index++;
        }

        var data = {daysOfWeek_table:daysOfWeek,
                    firstDayOfWeekInCurrentCulture:firstDayOfWeekInCurrentCulture,
                    daysOfMonth:list
                    };

        jq("#jTemplatesContainer").setTemplateElement("calendarTemlate", null, {filter_data: false});
        jq("#jTemplatesContainer").processTemplate(data);
        

        if(jq.browser.msie)
        {
        jq('.wbs_day').each( function() {jq(this).css('width','14.1%');} );
        jq('.wbs_number').each(function() {jq(this).css('border-left','none');} );
        jq('.wbs_number_empty').each(function()
            {
                var parent = jq(this).parent();
                jq(parent).addClass('day');
            } );   
        }
    };

this.drawWeek = function ()
    {

        Calendar.writeNavigationPanelTitle();

        Events.sort(Calendar.sortfunction);

        for(var i=0;i<IsAlreadyWrite.length;i++)
        {
            IsAlreadyWrite[i]="0,16";
        }

        var date = new Date(this.CurrentWeek.Year,this.CurrentWeek.Month,this.CurrentWeek.Day);
        var dayOfWeek = date.getDay();
        
        var countDaysInWeekBeforeDate;
        if(firstDayOfWeekInCurrentCulture==0) countDaysInWeekBeforeDate = dayOfWeek;
        else countDaysInWeekBeforeDate = dayOfWeek-1;
        if(countDaysInWeekBeforeDate==-1) countDaysInWeekBeforeDate=6;
        
        date = new Date(this.CurrentWeek.Year,this.CurrentWeek.Month,this.CurrentWeek.Day-countDaysInWeekBeforeDate);
        
        Calendar.CurrentWeek.Year = date.getFullYear();
        Calendar.CurrentWeek.Month = date.getMonth();
        Calendar.CurrentWeek.Day = date.getDate();
        
        var today = new Date();
        var list = new Array();
        
        for(var i=0; i<7; i++)
        {
            if(i!=0)
                date.setDate(date.getDate()+1);
            
            if(date.getFullYear()==today.getFullYear() && date.getMonth()==today.getMonth() && date.getDate()==today.getDate())
                list.push({monthName:Months[date.getMonth()],month:date.getMonth(),year:date.getFullYear(),dayNumber:date.getDate(),index:i+1,cssToday:jq.browser.msie?'week_today':'today'});
            else
                list.push({monthName:Months[date.getMonth()],month:date.getMonth(),year:date.getFullYear(),dayNumber:date.getDate(),index:i+1,cssToday:jq.browser.msie?'day':''});

        }
        
        var hack = 'width:12.5%;';
        if(jq.browser.msie) hack = 'width:12.36%;';
        if(jq.browser.chrome) hack =  'width:12.6%;';
        
        var data = {daysOfWeek_table:daysOfWeek,
                    daysOfWeek:list,
                    hackWidth:hack
                    };

        jq("#jTemplatesContainer").setTemplateElement("calendarTemlate", null, {filter_data: false});
        jq("#jTemplatesContainer").processTemplate(data);
        
    };

this.click = function(object)
                    {
                        jq('#selectedEvent').val(-1);
                        jq('#createEventButton').show();
                        jq('#createHeader').show();
                        jq('#editEventButton').hide();
                        jq('#deleteEventButton').hide();
                        jq('#editHeader').hide();

                        jq("div.selected").removeClass('selected');

                        jq(object).addClass('selected');

                        Calendar.InitPopupContainer(object.childNodes[0].innerHTML);

                        var style = 'display:block;top:'+Calendar.CurrentCursor.Y+'px;left:'+Calendar.CurrentCursor.X+'px;';
                        jq("#popup").attr('style',style);
                    };

this.clickOnEvent = function(id)
                    {

                        jq('#selectedEvent').val(id);
                        jq('#createEventButton').hide();
                        jq('#createHeader').hide();
                        jq('#editEventButton').show();
                        jq('#deleteEventButton').show();
                        jq('#editHeader').show();

                        jq(".selected").removeClass('selected');

                        Calendar.InitPopupContainerByEvent(id);

                        var style = 'display:block;top:'+Calendar.CurrentCursor.Y+'px;left:'+Calendar.CurrentCursor.X+'px;';
                        jq("#popup").attr('style',style);
                    };
                    
this.clickOnWeekView = function(object, year, month, day)
                    {
                        if( jq.browser.msie && jq('#isEditEvent').val()==1 && object.nodeName=="TD" )
                        {jq(".selected").removeClass('selected');}
                        else
                        {

                        jq('#selectedEvent').val(-1);
                        jq('#createEventButton').show();
                        jq('#createHeader').show();
                        jq('#editEventButton').hide();
                        jq('#deleteEventButton').hide();
                        jq('#editHeader').hide();

                        jq(".selected").removeClass('selected');

                        jq(object).addClass('selected');

                        Calendar.InitPopupContainer(1);
                        
                        var array = jq(object).attr('id').split('_');
                        
                        jq("#eventStartingDateYear"+year).attr("selected","selected");
                        jq("#eventStartingDateMonth"+month).attr("selected","selected");
                        jq("#eventStartingDateDay"+day).attr("selected","selected");

                        jq("#eventEndingDateYear"+year).attr("selected","selected");
                        jq("#eventEndingDateMonth"+month).attr("selected","selected");
                        jq("#eventEndingDateDay"+day).attr("selected","selected");
                        
                        if(array.length>1)
                        {
                            jq("#eventStartingDateHour"+array[1]).attr("selected","selected");
                            if(parseInt(array[1])==23)
                                jq("#eventEndingDateHour0").attr("selected","selected");
                            else 
                                jq("#eventEndingDateHour"+(parseInt(array[1])+1)).attr("selected","selected");    
                        }    
                        
                        var style = 'display:block;top:'+Calendar.CurrentCursor.Y+'px;left:'+Calendar.CurrentCursor.X+'px;';
                        jq("#popup").attr('style',style);
                        }
                        jq('#isEditEvent').val(-1);
                    };                    

this.listView = function ()
    {
        var startingDate;
        var endingDate;
        var title;
        var eventURL;
        var now = new Date();
        var ongoingEvents = new Array();
        var upcomingEvents = new Array();
        var pastEvents = new Array();
        var start; var end;

        for(var i=0;i<Events.length;i++)
        {
            startingDate = new Date(parseInt(Events[i].StartingDate.substring(6,19)));
            endingDate = new Date(parseInt(Events[i].EndingDate.substring(6,19)));
            title = Events[i].Title;
            eventURL = Events[i].EventURl;
            
            start = startingDate.getDate()+' '+Months[startingDate.getMonth()]+' '+startingDate.format('yyyy HH:mm');
            end = endingDate.getDate()+' '+Months[endingDate.getMonth()]+' '+endingDate.format('yyyy HH:mm');
            
            if(startingDate <= now && endingDate >= now)
                ongoingEvents.push({Title:title,EventURL:eventURL,StartingDate:start,EndingDate:end});
            if(startingDate > now)
                upcomingEvents.push({Title:title,EventURL:eventURL,StartingDate:start,EndingDate:end});   
            if (endingDate < now)
                pastEvents.push({Title:title,EventURL:eventURL,StartingDate:start,EndingDate:end});
        }
        
        var data = {Events:ongoingEvents};

        jq("#ongoingEvents").setTemplateElement("listViewTemlate", null, {filter_data: false});
        jq("#ongoingEvents").processTemplate(data);

        data = {Events:upcomingEvents};

        jq("#upcomingEvents").setTemplateElement("listViewTemlate", null, {filter_data: false});
        jq("#upcomingEvents").processTemplate(data);

        data = {Events:pastEvents};

        jq("#pastEvents").setTemplateElement("listViewTemlate", null, {filter_data: false});
        jq("#pastEvents").processTemplate(data);
    };                         

this.ValidDate = function()
    {
        var startingDate = new Date(jq("#eventStartingDateYear option:selected").val(),jq("#eventStartingDateMonth option:selected").val(),jq("#eventStartingDateDay option:selected").val(),jq("#eventStartingDateHour option:selected").val(),jq("#eventStartingDateMinute option:selected").val(),0);
        var endingDate = new Date(jq("#eventEndingDateYear option:selected").val(),jq("#eventEndingDateMonth option:selected").val(),jq("#eventEndingDateDay option:selected").val(),jq("#eventEndingDateHour option:selected").val(),jq("#eventEndingDateMinute option:selected").val(),0);
        
        if(endingDate<startingDate) return false;
        
        return true;
    };
 this.InitViewSwitcher = function()
    {
        var action = jq.getURLParam('action');
        if(action==null) action = 0;
        jq("#vsItem_"+action).removeClass('viewSwitcherItem').addClass('viewSwithcerSelectedItem');
        var title = jq("#vsItem_"+action+" a").html();
        jq("#vsItem_"+action).html("<b>"+title+"</b>");
    };   

}

