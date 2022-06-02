<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="'ASC.Projects.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeSpendTimer.ascx.cs" Inherits="ASC.Web.Projects.Controls.TimeSpends.TimeSpendTimer" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script>
    var timer = null;
    var seconds = 0;
    var isFirstViewStyle = true;
    var isFirstStart = true;
    var defaultValue = "0<%= DecimalSeparator %>00"
    var buttonStyle = { 
                        "playButton": {"src": "<%= GetPlayButtonImg() %>", "title": "<%= ProjectsCommonResource.AutoTimerStart %>"},
                        "pauseButton": {"src": "<%= GetPauseButtonImg() %>", "title": "<%= ProjectsCommonResource.AutoTimerPause %>"},
                        "isPlay": true
                      };

    function playTimer()
    {
        lockElements();
        window.clearInterval(timer);
        
        
        if(isFirstStart)
        {
            firstStart();
            isFirstStart = false;
        }
        else
            startTimer();
        
    }
    
    function firstStart()
    {
        var startTime = jq("#inputTimeHours").val().trim();
        if(startTime!="")
        {
            AjaxPro.TimeTracking.GetSeconds(startTime,
            function(res)
            {
                if (res.error != null)
                {
                    alert(res.error.Message);
                    resetTimer();
                    return;
                }
                seconds = parseInt(res.value);
                startTimer();
            });
        }
    }
    
    function startTimer()
    {
        var timerHours = parseInt(seconds/3600);
        var timerMin = parseInt((seconds-(timerHours*3600))/60);
        var timerSec = seconds-(timerHours*3600)-(timerMin*60);
        
        timer = window.setInterval(function()
        {
            timerSec++;
            seconds++;
            if (timerSec == 60)
            { 
                timerSec=0;
                timerMin++;
                if(timerMin == 60)
                {
                    timerMin=0;
                    timerHours++;
                }
            }
            
            var time = "";
            
            if(timerHours<10)
                time += "0"+timerHours+":";
            else time += timerHours+":";
            
            if(timerMin<10)
                time += "0"+timerMin+":";
            else time += timerMin+":";
            
            if(timerSec<10)
                time += "0"+timerSec;
            else time += timerSec;

            jq("#firstViewStyle div:first").html(time);
            jq("#secondViewStyle div:first").html(((seconds/3600).toFixed(4)).replace(".","<%= DecimalSeparator %>"));
            
            if(isFirstViewStyle)
                jq("#viewStyle").html(((seconds/3600).toFixed(4)).replace(".","<%= DecimalSeparator %>"));
            else
                jq("#viewStyle").html(time);         
            
        }, 1000);
    }
    
    function pauseTimer()
    {
        window.clearInterval(timer);
    }
    
    function resetTimer()
    {
        unlockElements();
        window.clearInterval(timer);
        
        jq("#firstViewStyle div:first").html("00:00:00");
        jq("#secondViewStyle div:first").html(defaultValue);
        jq("#inputTimeHours").val(defaultValue);
        jq("#textareaTimeDesc").val("");
        seconds = 0;
        
        if(isFirstViewStyle)
            jq("#viewStyle").html(defaultValue);
        else
            jq("#viewStyle").html("00:00:00");
        
        jq("#play_pause_img").attr("src",buttonStyle.playButton.src).attr("alt",buttonStyle.playButton.title).attr("title",buttonStyle.playButton.title);
        buttonStyle.isPlay = true;
        isFirstStart = true;
    }
    
    function refreshTimer()
    {
        if(isFirstViewStyle)
        {
            isFirstViewStyle=false;
            jq("#secondViewStyle").show();
            jq("#firstViewStyle").hide();
            jq("#viewStyle").html(jq("#firstViewStyle div:first").html());
        }
        else
        {
            isFirstViewStyle=true;
            jq("#firstViewStyle").show();
            jq("#secondViewStyle").hide();
            jq("#viewStyle").html(jq("#secondViewStyle div:first").html());
        }
    }
    
    function play_pause_Timer()
    {
        if(buttonStyle.isPlay)
        {
            jq("#play_pause_img").attr("src",buttonStyle.pauseButton.src).attr("alt",buttonStyle.pauseButton.title).attr("title",buttonStyle.pauseButton.title);
            playTimer();
            buttonStyle.isPlay = false;
        }
        else
        {
            jq("#play_pause_img").attr("src",buttonStyle.playButton.src).attr("alt",buttonStyle.playButton.title).attr("title",buttonStyle.playButton.title);
            pauseTimer();
            buttonStyle.isPlay = true;
        }
    }
    function lockElements()
    {
        jq("#inputTimeHours").attr("disabled","true");
    }
    function unlockElements()
    {
        jq("#inputTimeHours").removeAttr("disabled");
    }
    function initSelectUserTasksByProject()
    {
        var prjID = parseInt(jq("#selectUserProjects option:selected").val());
        AjaxPro.TimeTracking.InitSelectUserTasksByProject(prjID,<%= Target %>,
            function(res) {
                if (res.error != null)
                {
                    alert(res.error.Message);
                    unlockElements();
                    return;
                }
                jq("#selectUserTasks").html(res.value);
            });
    }
    function logHours()
    {
        var prjID = parseInt(jq("#selectUserProjects option:selected").val());
        var taskID = parseInt(jq("#selectUserTasks option:selected").val());
        var desc = jq("#textareaTimeDesc").val().trim();
        var startValue = jq("#inputTimeHours").val().trim();
        
        if(seconds==0 && isFirstStart && startValue=="")
        {
            alert("<%= ProjectsCommonResource.AutoTimerNullTimeError %>");
            return;
        }
        
        AjaxPro.TimeTracking.SaveTaskTimeByTimer(prjID,taskID,seconds,desc,startValue,
            function(res) {
                if (res.error != null)
                {
                    alert(res.error.Message);
                    unlockElements();
                    return;
                }
                resetTimer();
            });
    }   
</script>

<style>
    #timerTime
    {
    	font-size: 45px;
    	padding: 10px 15px;
    	text-align: center;
    }
    a.pm-ts-timerButton
    {
        background-position: 0 center;
        background-repeat: no-repeat;
        cursor: pointer;
        padding: 0 0 0 20px;
        text-decoration: none;
    }
    a.pm-ts-timerButton span
    {
        border-bottom: 1px dotted #000000;
        color: #000000;
    }
</style>

<div style="width:250px;background-color: #FFFFFF;border: 1px solid #575757;">
    <div id="timerTime" class="tintMedium">
        <div id="firstViewStyle">
            <div>00:00:00</div>
            <div style="color:Gray;font-size:9px;"><%= ProjectsCommonResource.AutoTimerFirstViewType %></div>
        </div>
        <div id="secondViewStyle" style="display:none;">
            <div >0<%= DecimalSeparator %>00</div>
            <div style="color:Gray;font-size:9px;"><%= ProjectsCommonResource.AutoTimerSecondViewType %></div>
        </div>
    </div>
    <div style="padding: 15px;">
        <div class="pm-headerPanelSmall-splitter clearFix">
            <a style="background-image: url('<%=GetResetButtonImg()%>');float:right;" onclick="javascript:resetTimer();" title="<%= ProjectsCommonResource.AutoTimerReset %>" class="pm-ts-timerButton">
                <span><%= ProjectsCommonResource.AutoTimerReset %></span>
            </a>
            <a style="background-image: url('<%=GetRefreshButtonImg()%>');" onclick="javascript:refreshTimer();" title="<%= ProjectsCommonResource.AutoTimerChangeViewType %>" class="pm-ts-timerButton">
                <span id="viewStyle">0<%= DecimalSeparator %>00</span>
            </a>
        </div>
        <div class="pm-headerPanelSmall-splitter" style="text-align: center;">
            <a style="cursor: pointer;" onclick="javascript:play_pause_Timer();">
                <img id="play_pause_img" src="<%= GetPlayButtonImg() %>" alt="<%= ProjectsCommonResource.AutoTimerStart %>" title="<%= ProjectsCommonResource.AutoTimerStart %>" />
            </a>
        </div>
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.Project %>:</b>
            </div>
            <select id="selectUserProjects" style="width: 99%;" class="comboBox" onchange="javascript:initSelectUserTasksByProject();">
                <% foreach (var project in UserProjects) %>
                <% { %>
                    <% if (project.ID == ProjectFat.Project.ID) %>
                    <% { %>
                    <option selected="selected" value="<%= project.ID %>" id="option1"><%= project.Title%></option>
                    <% } %>
                    <% else %>
                    <% { %>
                    <option value="<%= project.ID %>" id="optionUserProject_<%= project.ID %>"><%= project.Title%></option>
                    <% } %>
                <% } %>
            </select>
        </div>
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= TaskResource.Task %>:</b>
            </div>
            <select id="selectUserTasks" style="width: 99%;" class="comboBox">
                <% foreach (var task in UserTasks) %>
                <% { %>
                    <% if (Target != -1 && task.ID == Target) %>
                    <% { %>
                    <option selected="selected" value="<%= task.ID %>" id="optionUserTask_<%= task.ID %>"><%= task.Title %></option>
                    <% } %>
                    <% else %>
                    <% { %>
                    <option value="<%= task.ID %>" id="optionUserTask_<%= task.ID %>"><%= task.Title %></option>
                    <% } %>
                <% } %>
            </select>
        </div>
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.HoursCount%>:</b>
            </div>
            <input id="inputTimeHours" type="text" style="width: 60px;text-align:right;" class="textEdit" value="0<%= DecimalSeparator %>00" maxlength="5" />
        </div>
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.ProjectDescription %>:</b>
            </div>
            <textarea id="textareaTimeDesc" style="width: 99%;resize:none;" class="pm-ntextbox "></textarea>
        </div>
        <div class="pm-headerPanelSmall-splitter">
            <a onclick="javascript:logHours();" class="baseLinkButton" style="width:99%;padding: 2px 0;">
                <%= ProjectsCommonResource.AutoTimerLogHours %>
            </a>
        </div>
    </div>
</div>