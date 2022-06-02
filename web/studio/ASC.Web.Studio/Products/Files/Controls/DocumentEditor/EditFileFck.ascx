<%@ Assembly Name="ASC.Web.Files" %>
<%@ Assembly Name="ASC.Web.Controls" %>
<%@ Assembly Name="ASC.Web.UserControls.Files" %>
<%@ Assembly Name="FredCK.FCKeditorV2" %>
<%@ Import Namespace="ASC.Web.UserControls.Files.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditFileFck.ascx.cs" Inherits="ASC.Web.UserControls.Files.EditFileFck" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>

<div id="EditFileFckAscx" <%--style='display:none;'--%>>
    <FCKeditorV2:FCKeditor id='_fckEditor' runat='server' Height='400px' ></FCKeditorV2:FCKeditor>
</div>