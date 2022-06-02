<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />
  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="folder">
    <div class="clearFix fileRow folderRow newFolder" name="addRow">
      <xsl:attribute name="oncontextmenu">return ASC.Files.Actions.onContextMenu('folder_<xsl:value-of select="id" />',event);</xsl:attribute>
      <xsl:attribute name="onclick">ASC.Files.UI.clickRow(this,event);</xsl:attribute>
      <xsl:attribute name="id">content_folder_<xsl:value-of select="id" /></xsl:attribute>
      <div class="checkbox">
        <div class="no_combobox cornerAll">
          <input type="checkbox" style="float:left">
            <xsl:attribute name="onclick">
              ASC.Files.UI.selectRow(jq("#content_folder_" + <xsl:value-of select="id" />), this.checked == true);
              ASC.Files.Common.cancelBubble(event);
            </xsl:attribute>
            <xsl:attribute name="id">check_folder_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="title"><resource name="fres.TitleSelectFile" /></xsl:attribute>
          </input>
          <div class="combobtn">
            <xsl:attribute name="id">combo_folder_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="title"><resource name="fres.TitleActionPanel" /></xsl:attribute>
          </div>
        </div>
      </div>
      <div class="folder_thumbnail">
        <xsl:attribute name="onclick">ASC.Files.UI.madeAnchor(<xsl:value-of select="id" />);return false;</xsl:attribute>
        <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
      </div>
      <div class="sub-info" style="float:left">
        <div class="fileName">
          <div class="name">
            <a class="name" onmousedown="return false;">
              <xsl:attribute name="onclick">ASC.Files.UI.madeAnchor(<xsl:value-of select="id" />);return false;</xsl:attribute>
              <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
              <xsl:attribute name="href">#<xsl:value-of select="id" /></xsl:attribute>
              <xsl:value-of select="title" />
            </a>
          </div>
        </div>
        <div class="fileInfo">
          <resource name="fres.TitleFiles" />
          <span class="span_space"></span>
          <span>
            <xsl:attribute name="id">content_folder_countFiles_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:value-of select="total_files" />
          </span>
          <span class="span_space"></span>
          <span>|</span>
          <span class="span_space"></span>
          <resource name="fres.TitleSubfolders" />
          <span class="span_space"></span>
          <span>
            <xsl:attribute name="id">content_folder_countFolders_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:value-of select="total_sub_folder" />
          </span>
          <input type="hidden">
            <xsl:attribute name="id">by_folder_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="value"><xsl:value-of select="create_by_id" /></xsl:attribute>
          </input>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="file">
    <xsl:variable name="height" select="content_length*1024*1024"/>
    <div class="clearFix fileRow newFile" name="addRow">
      <xsl:attribute name="oncontextmenu">return ASC.Files.Actions.onContextMenu('file_<xsl:value-of select="id" />',event);</xsl:attribute>
      <xsl:attribute name="onclick">ASC.Files.UI.clickRow(this,event);</xsl:attribute>
      <xsl:attribute name="id">content_file_<xsl:value-of select="id" /></xsl:attribute>
      <div class="checkbox">
        <div class="no_combobox cornerAll">
          <input type="checkbox" style="float:left">
            <xsl:attribute name="onclick">
              ASC.Files.UI.selectRow(jq("#content_file_" + <xsl:value-of select="id" />), this.checked == true);
              ASC.Files.Common.cancelBubble(event);
            </xsl:attribute>
            <xsl:attribute name="id">check_file_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="title"><resource name="fres.TitleSelectFile" /></xsl:attribute>
          </input>
          <div class="combobtn">
            <xsl:attribute name="id">combo_file_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="title"><resource name="fres.TitleActionPanel" /></xsl:attribute>
          </div>
        </div>
      </div>
      <div class="file_thumbnail">
        <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
        <xsl:attribute name="onclick">ASC.Files.Folders.clickOnFile(<xsl:value-of select="id" />);return false;</xsl:attribute>
        <script type="text/javascript">
          var validUrl = ASC.Utils.setThumbnailTypeIcon('<xsl:value-of select="title" />','<xsl:value-of select="thumbnail_url" />');
          jq("#content_file_<xsl:value-of select="id" /> div.file_thumbnail").css('background', "url('" + validUrl + "') no-repeat 50% 50%");
        </script>
        <div class="editing_file_icn">
          <xsl:if test="contains(file_status, 'IsEditing')">
            <xsl:attribute name="style">display:block;</xsl:attribute>
          </xsl:if>
          <xsl:attribute name="id">file_editing_icn_<xsl:value-of select="id" /></xsl:attribute>
          <xsl:attribute name="title"><resource name="fres.TitleEditingFile" /></xsl:attribute>
        </div>
        <div class="subscribe_file_icn">
          <xsl:if test="is_subscribe = 'true'">
            <xsl:attribute name="style">display:block</xsl:attribute>
          </xsl:if>
          <xsl:attribute name="title"><resource name="fres.TitleSubscribe" /></xsl:attribute>
          <xsl:attribute name="id">file_subscribe_icn_<xsl:value-of select="id" /></xsl:attribute>
        </div>
        <!--<div class="share_file_container">
						<xsl:if test="is_shared = 'false'">
							<xsl:attribute name="style">
								display:none
							</xsl:attribute>
						</xsl:if>
						<xsl:attribute name="title">
							<resource name="fres.TitleShareFile" />
						</xsl:attribute>
						<xsl:attribute name="id">content_file_share_<xsl:value-of select="id" /></xsl:attribute>
					</div>-->
      </div>
      <div class="file-info" style="float:left">
        <div class="fileName">
          <div class="name">
            <a class="name" onmousedown="return false;">
              <xsl:attribute name="onclick">ASC.Files.Folders.clickOnFile(<xsl:value-of select="id" />);return false;</xsl:attribute>
              <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
              <xsl:attribute name="href">#<xsl:value-of select="folder_id" /></xsl:attribute>
              <xsl:value-of select="title" />
            </a>
          </div>
        </div>
        <div class="fileInfo">
          <resource name="fres.TitleOwner" />
          <span class="span_space"></span>
          <span><xsl:value-of select="create_by" /></span>
          <span class="span_space"></span>
          <span>|</span>
          <span class="span_space"></span>
          <span><xsl:value-of select="content_length" /></span>
          <input type="hidden">
            <xsl:attribute name="id">file_version_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="value"><xsl:value-of select="version" /></xsl:attribute>
          </input>
          <input type="hidden">
            <xsl:attribute name="id">by_file_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="value"><xsl:value-of select="create_by_id" /></xsl:attribute>
          </input>
        </div>
      </div>
    </div>
  </xsl:template>

</xsl:stylesheet>