<%@ Assembly Name="ASC.Web.Files" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PluginBox.ascx.cs"
    Inherits="ASC.Web.Files.Controls.PluginBox" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<% #if (DEBUG) %>
    <link href="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/PluginBox/PluginBox.css" )%>" type="text/css" rel="stylesheet" />
<% #endif %>

<script type="text/javascript" language="javascript" src="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/PluginBox/pluginbox.js" ) %>"></script>

<div id="plugin_informer" class="plugin_informer_wrapper" >
    <div class="btn_close" title="<%=FilesUCResource.ButtonClose%>">
    </div>
    <div class="plugin_informer clearFix">
        <div class="plugin_informer_info">
            <span class="headerBase"></span>
            <p></p>
        </div>
        <span class="btn_install"></span>
    </div>
</div>

<div id="how_it_works_box" style="display: none">
    <ascwc:container ID="_howItWorks" runat="server">
        <header><%=FilesUCResource.HowItWorksHeader%> </header>
        <body>
            <p>
                <%=FilesUCResource.HowItWorksDescription%>
            </p>
            <div class="clearFix">
                <div class="video_box">
                    <span>
                        <%=FilesUCResource.HowItWorksWatchVideo %></span>
                    <object style="height: 240px; width: 320px">
                        <param name="movie" value="https://www.youtube.com/v/kgfwlG0ZAug?version=3">
                        <param name="allowFullScreen" value="true">
                        <param name="allowScriptAccess" value="always">
                        <embed src="https://www.youtube.com/v/kgfwlG0ZAug?version=3" type="application/x-shockwave-flash"
                            allowfullscreen="true" allowscriptaccess="always" width="320" height="240"></object>
                </div>
                <dl>
                    <dt><span class="headerBaseMedium">1</span></dt>
                    <dd>
                        <span class="headerBaseMedium">
                            <%=FilesUCResource.HowItWorksStepOneHeader %>
                        </span>
                        <p>
                            <%=string.Format(FilesUCResource.HowItWorksStepOneDescription, "<a href='http://www.ascensiosystem.com/plugin/' target='_blank'>", "</a>")%>
                        </p>
                    </dd>
                    <dt><span class="headerBaseMedium">2</span></dt>
                    <dd>
                        <span class="headerBaseMedium">
                            <%=FilesUCResource.HowItWorksStepTwoHeader %>
                        </span>
                        <p>
                            <%=FilesUCResource.HowItWorksStepTwoDescription %>
                        </p>
                    </dd>
                    <dt><span class="headerBaseMedium">3</span></dt>
                    <dd>
                        <span class="headerBaseMedium">
                            <%=FilesUCResource.HowItWorksStepThreeHeader%></span>
                        <p>
                            <%=FilesUCResource.HowItWorksStepThreeDescription%>
                            <span class="headerBaseMedium" style="color: #1A6DB3; margin-top: 20px; display: block;">
                                <%=FilesUCResource.HowItWorksOpportunitiesAvailable%>
                            </span>
                        </p>
                    </dd>
                </dl>
            </div>
            <a class="grayLinkButton" onclick="jq.unblockUI();">
                <%=FilesUCResource.ButtonClose%>
            </a>
        </body>
    </ascwc:container>
</div>

<div id="install_plugin_box" style="display: none">
    <ascwc:container ID="_installPlugin" runat="server">
        <header>
            <%=FilesUCResource.InstallPluginHeader%> 
        </header>
        <body>
            <div id="install_process">
                <div class="files_progress_box clearFix">
                    <span class="headerBaseMedium"></span>
                    <div class="progress_wrapper">
                        <div class="progress">
                        </div>
                        <span class="percent">0</span>
                    </div>
                    <span class="textSmallDescribe"></span>
                </div>
                <p>
                    <%=FilesUCResource.DoNotLeavePageLabel%>
                </p>
            </div>
            <div id="install_completed" style="display:none">
               
               <span class="headerBaseMedium"> </span>
               
               <a class="baseLinkButton" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;"><%=FilesUCResource.ButtonClose%></a>
            
            </div>            
            <div id="slider" class="tintMedium">
                <div class="slider-item">
                    <span class="headerBaseMedium">
                        <%=FilesUCResource.EditingFeatureHeader%>
                    </span>
                    <p>
                        <%=FilesUCResource.EditingFeatureDescription%>
                    </p>
                </div>
                <div class="slider-item">
                    <span class="headerBaseMedium">
                        <%=FilesUCResource.ManagementFeatureHeader%></span>
                    <p>
                        <%=FilesUCResource.ManagementFeatureDescription%>
                    </p>
                </div>
                <div class="slider-item">
                    <span class="headerBaseMedium">
                        <%=FilesUCResource.ImportFeatureHeader%></span>
                    <p>
                        <%=FilesUCResource.ImportFeatureDescription%>
                    </p>
                </div>
                <div class="slider-item">
                    <span class="headerBaseMedium">
                        <%=FilesUCResource.IntegrationFeatureHeader%></span>
                    <p>
                        <%=FilesUCResource.IntegrationFeatureDescription%>
                    </p>
                </div>
                <div class="slider-item">
                    <span class="headerBaseMedium">
                        <%=FilesUCResource.NotificationFeatureHeader%></span>
                    <p>
                        <%=FilesUCResource.NotificationFeatureDescription%>
                    </p>
                </div>
                <div class="slider-item">
                    <span class="headerBaseMedium">
                        <%=FilesUCResource.SortingFeatureHeader%></span>
                    <p>
                        <%=FilesUCResource.SortingFeatureDescription%>
                    </p>
                </div>
                <ul class="clearFix">
                    <li><a href="javascript:void(0)" class="slider-active" rel="0"></a></li>
                    <li><a href="javascript:void(0)" rel="1"></a></li>
                    <li><a href="javascript:void(0)" rel="2"></a></li>
                    <li><a href="javascript:void(0)" rel="3"></a></li>
                    <li><a href="javascript:void(0)" rel="4"></a></li>
                    <li><a href="javascript:void(0)" rel="5"></a></li>
                </ul>
            </div>
        </body>
    </ascwc:container>
</div>

<div id="objectDiv" style="width: 0px; height: 0px;">
</div>