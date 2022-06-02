<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopNavigationPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.TopNavigationPanel" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Common" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<div class="studioTopNavigationPanel clearFix">
    <div class="systemSection mainPageLayout clearFix">    
        <ul>
        <li style="float:left; padding:0; margin-right:20px;">
            <a class="topLogo" target="_blank" href="http://www.teamlab.com">&nbsp;</a>
        </li>
        <asp:PlaceHolder ID="_productListHolder" runat="server">
            <li style="float:left;">
                <asp:Repeater runat="server" ID="_productRepeater">                            
                    <ItemTemplate>    
                        <%#(!_currentProductID.Equals(((IWebItem)Container.DataItem).ID)) ?
                            String.Format(@"<a href=""{0}"">{1}</a>", VirtualPathUtility.ToAbsolute(((IWebItem)Container.DataItem).StartURL), HttpUtility.HtmlEncode(((IWebItem)Container.DataItem).Name)) :
                                                        String.Format(HttpUtility.HtmlEncode(((IWebItem)Container.DataItem).Name))
                        %>                        
                    </ItemTemplate>                    
                    <SeparatorTemplate>
						<span class="spacer">|</span>
                    </SeparatorTemplate>
                    <FooterTemplate>
						<%if (NeedDrawMoreProducts)
                        { %>
							<span class="spacer">|</span>
						<%} %>
                    </FooterTemplate>
                </asp:Repeater>
            </li>
             
             <%if (NeedDrawMoreProducts)
               { %>
            <li onclick="PopupPanelHelper.ShowPanel(jq(this), 'MoreProductsPopupPanel'); return false;" class="myStaffBox" style="float: left; width: auto; text-align: left; margin-left: -10px; -moz-user-select: none;" onselectstart="return false;" onmousedown="return false;">                
                <span class="myStaff" style="margin-left: 7px; margin-right: 5px;">
                    <%=Resources.Resource.More%>
                </span>     
            </li>
            <%} %>
        </asp:PlaceHolder>       
        <li style="float:left;">&nbsp;</li> 
        
        <asp:PlaceHolder ID="_guestInfoHolder" runat="server">
            <li style="float:right;">                                
                <a href="<%=VirtualPathUtility.ToAbsolute("~/auth.aspx")%>"><%=Resources.Resource.LoginButton%></a>
            </li>
        </asp:PlaceHolder>
        
        <asp:PlaceHolder ID="_userInfoHolder" runat="server">
           
            <%--my staff--%>
             <%if (!ASC.Core.SecurityContext.DemoMode)
               {%>              
            <li id="studio_myStaffBox" onclick="PopupPanelHelper.ShowPanel(jq(this), 'studio_myStaffPopupPanel', -1);" class="myStaffBox" onselectstart="return false;" onmousedown="return false;">
                <span class="userLink">
                    <span class="usrProf">                      
                        <%=ASC.Core.Users.UserInfoExtension.DisplayUserName(_currentUser, true)%>
                    </span>     
                </span>
            </li>
            <%}
               else
               { %>  
               
                <li style="float:right;">
                    <span class="userProfileBox"> <%=Resources.UserControlsCommonResource.DemoUserName%></span>
               </li>
               
            <%} %>            
            
            <%=RenderCustomNavigation()%>
            
        </asp:PlaceHolder>
        </ul>
        
        <div id="studio_myStaffPopupPanel" class="myStaffPopupPanel" style="display:none; position:absolute;">
            <a class="myStaffItem" href="<%=CommonLinkUtility.GetMyStaff()%>"><%=Resources.Resource.Profile%></a>
            <a class="myStaffItem" href="<%=CommonLinkUtility.GetMyStaff(MyStaffType.Activity)%>"><%=Resources.Resource.RecentActivity%></a>
            <a class="myStaffItem" href="<%=CommonLinkUtility.GetMyStaff(MyStaffType.Subscriptions)%>"><%=Resources.Resource.Subscriptions%></a>
            <asp:Repeater runat="server" ID="myToolsItemRepeater">
                <ItemTemplate>
                    <a class="myStaffItem" href="<%#CommonLinkUtility.GetMyStaff((string)Eval("ParameterName"))%>"><%#HttpUtility.HtmlEncode((string)Eval("TabName"))%></a>        
                </ItemTemplate>
            </asp:Repeater>		
            
            <%--Logout--%>
            <%if (_canLogout){ %>
            <div class="borderBase" style="border-bottom:none;border-left:none;border-left:none;">
                 <a class="myStaffItem" href="<%=CommonLinkUtility.Logout%>"><%=Resources.UserControlsCommonResource.LogoutButton%></a>
            </div>
            <%} %>
        </div>
        
         <div id="MoreProductsPopupPanel" class="myStaffPopupPanel" style="display:none; position:absolute;">            
            <asp:Repeater runat="server" ID="MoreProductsRepeater">
                <ItemTemplate>
						<div>							
							<%#String.Format("<a href='{0}' class='myStaffItem'>{1}</a>", VirtualPathUtility.ToAbsolute(((IWebItem)Container.DataItem).StartURL), HttpUtility.HtmlEncode(((IWebItem)Container.DataItem).Name))	%>
                        </div>
                </ItemTemplate> 
                <FooterTemplate>
					<div style="border-bottom: solid 1px #D1D1D1"></div>
                </FooterTemplate>
            </asp:Repeater>            
            <a class="myStaffItem" href="<%=CommonLinkUtility.GetDefault()%>"><%=Resources.UserControlsCommonResource.AllProductsTitle%></a>
        </div>        
 
        <asp:PlaceHolder runat="server" ID="_customNavControls"></asp:PlaceHolder>
    </div>
    
    <asp:PlaceHolder runat="server" ID="_contentSectionHolder">
        <div class="contentSection">
        <div class="infoBox">
            <div class="mainPageLayout clearFix">
                
                <%--header--%>
                <a <%=String.IsNullOrEmpty(_titleURL) ? "" : "style=\"cursor:pointer;\" href=\""+_titleURL+"\""%> class="titleBox clearFix">
                    <%=String.IsNullOrEmpty(_titleIconURL) ? "" : "<div style='float:left;'><img alt=\"\" src=\"" + _titleIconURL + "\"/></div>"%>
                    <%=String.IsNullOrEmpty(_title) ? "" : "<div style='float:left;' class=\"title\">" + HttpUtility.HtmlEncode(_title) + "</div>"%>                    
                </a>
                
                <%--html injection--%>
                <%=String.IsNullOrEmpty(CustomInfoHTML) ? "" : "<div style='float:left;'>" + CustomInfoHTML + "</div>"%>
            
                <%--search--%>
                <asp:PlaceHolder ID="_searchHolder" runat="server">
                
                    <div class="searchBox" style="float:right;">
                        <div <%=_singleSearch?"class=\"singleLeftSearchPanel\"":"class=\"leftSearchPanel\" onclick=\"Searcher.ShowSearchPlaces();\""%> id="studio_searchSwitchButton">
                        <%if (!String.IsNullOrEmpty(_searchLogoUrl))
                          { %>
                            <img alt="" style="margin-top:2px; margin-left:2px;" id="studio_activeSearchHandlerLogo" align="absmiddle" src="<%=_searchLogoUrl%>"/>
                        <%} %>
                        </div>
                        <div class="mainSearchPanel">
                             <input type="text" id="studio_search" class="textEditMainSearch" value="<%=_searchText%>" style="width:130px;" />
                        </div>
                        
                        <div class="mainSearchButton" onclick="Searcher.Search();">
                            <input type="hidden" value=""/>
                        </div>
                        
                        <div id="studio_searchSwitchBox" class="switchBox" style="display:none; z-index:100; position:absolute;">
                            <%=RenderSearchHandlers()%>
                        </div>

                    </div>
                </asp:PlaceHolder>
            </div>       
            </div>
        
            <%--tabs with navigation--%>
            <asp:Repeater runat="server" ID="_navItemRepeater">
                <HeaderTemplate>    
                <div class="navigationBox">   
                    <div class=" mainPageLayout clearFix">            
                </HeaderTemplate>
                <ItemTemplate>
					<%#RenderNavigationItem(Container.DataItem as ASC.Web.Studio.Controls.Common.NavigationItem)%>
                </ItemTemplate>
                <FooterTemplate>            
                    </div></div>  
                </FooterTemplate>
            </asp:Repeater>
    
     </div>
    </asp:PlaceHolder>
</div>

