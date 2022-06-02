<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Request.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Common.Request" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Repeater runat="server" ID="rptContent">
    <ItemTemplate>
        <div class="<%# Container.ItemIndex==0 ? "" : "pm-hidden" %>" id="<%#Eval("Number") %>" value="<%#Eval("RequestID")%>" style="padding-top:10px;overflow:hidden;">
            <div class="pm-headerPanelSmall-splitter">
                <%# ASC.Core.Users.StudioUserInfoExtension.RenderProfileLink(Eval("CreateBy") as ASC.Core.Users.UserInfo, ProductEntryPoint.ID)%>
                <div class='textBigDescribe'><%#Eval("Action") %></div>
            </div>
            <div class="pm-headerPanelSmall-splitter" style='font-weight: bold;' id="<%#Eval("Number")%>_title" title="<%#Eval("Title") %>">
                <%#Eval("Title") %>
            </div>
            <div class="pm-headerPanelSmall-splitter <%# string.IsNullOrEmpty((string)Eval("Description")) ? "pm-hidden" : "" %>" id="id='<%#Eval("Number")%>_description'" title="<%#Eval("Description") %>">
                <%#GetDescription((string)Eval("Description")) %>
            </div>
            <div class="pm-headerPanelSmall-splitter">
                <span class='textBigDescribe'><%=ProjectResource.ProjectLeader %>:</span>
                <br/>
                <span  id="<%#Eval("Number")%>_responsible">
                    <%# ASC.Core.Users.StudioUserInfoExtension.RenderProfileLink(Eval("Responsible") as ASC.Core.Users.UserInfo, ProductEntryPoint.ID)%>
                </span>
            </div>
            <a href="javascript:void(0);" onclick="javascript:execShowRequestPanel()"><%=RequestResource.ViewRequest %></a>
            <div class='pm-h-line'><!– –></div>
        </div>
    </ItemTemplate>
</asp:Repeater>



<div>
        <img onclick="javascript:next('<%=GetPrevImageUrl() %>','<%=GetNextDisableImageUrl() %>')" align="absmiddle" id="nextImg" style="cursor: pointer;float:right"  src="<%=GetFirstImageUrl() %>"/>
        <div>
            <img onclick="javascript:prev('<%=GetNextImageUrl() %>','<%=GetPrevDisableImageUrl() %>')" align="absmiddle" id="prevImg" style="cursor: pointer;float:left" src="<%=GetPrevDisableImageUrl() %>"/>
            <div style="text-align:center"><%= GrammaticalHelperCountRequests()%></div>
        </div>
</div>

<input id="count" type="hidden" value="<%=RequestsCount %>" />

<div id="requestPanel" class="pm-hidden">
    <asp:PlaceHolder ID="phContent" runat="server"></asp:PlaceHolder>
</div>    



<script>
    
    var currentIndex = 0;
    
    function next(enableSRC,disableSRC)
    {
        if(currentIndex+1<jq('#count').val())
        {
            jq('#'+currentIndex).css('display','none');
            currentIndex=currentIndex+1;
        
            if(currentIndex==jq('#count').val()-1)
            {
                jq('#nextImg').attr('src',disableSRC);
            }
            if(currentIndex>0)
            {
                jq('#prevImg').attr('src',enableSRC);
            }

            jq('#'+currentIndex).css('display','block');
        }
    }
    function prev(enableSRC,disableSRC)
    {
        if(currentIndex-1>=0)
        {
            jq('#'+currentIndex).css('display','none');
            currentIndex=currentIndex-1;
            
            if(currentIndex==0)
            {
                jq('#prevImg').attr('src',disableSRC);
            }
            if(currentIndex<jq('#count').val()-1)
            {
                jq('#nextImg').attr('src',enableSRC);
            }

            jq('#'+currentIndex).css('display','block');
        }        
    }
    function execShowRequestPanel()
    {      
        var requestID = jq('#'+currentIndex).attr('value');
        jq('#requestPanel').html('');

        AjaxPro.Request.GetPopUpRequest(requestID,
        function(res)
        {
            if (res.value != null && res.value.Error != "") { alert(res.value.Error); return;}
            if (res.error != null) { alert(res.error); return;}
            
            var margintop =  jq(window).scrollTop()-15;
            margintop = margintop+'px';

            jq('#requestPanel').html(res.value.Request);
            jq.blockUI({message:jq("#requestPanel"),
                    css: { 
                        left: '50%',
                        top: '20%',
                        opacity: '1',
                        border: 'none',
                        padding: '0px',
                        width: '500px',
                       
                        cursor: 'default',
                        textAlign : 'left',
                        position: 'absolute',
                        'margin-left': '-300px',
                        'margin-top': margintop,                      
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
        });
}
</script>