<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />
  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="previewStatusHash/entry">
    <xsl:for-each select="key">
      <div class="preview_file_icn">
        <xsl:attribute name="id">preview_file_icn_<xsl:value-of select="id" /></xsl:attribute>
        <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
        <xsl:attribute name="onclick">ASC.Files.PreviewFile.downloadFile();return false;</xsl:attribute>
        <script type="text/javascript">
          var urlIcn=ASC.Utils.getFileTypeIcon('<xsl:value-of select="title" />');
          jq('#preview_file_icn_<xsl:value-of select="id" />').css('background', "url('"+urlIcn+"') no-repeat");
        </script>
      </div>
      <div class="clearFix" style="float:left;">
        <div id="PreviewFileTitle" class="preview_fileName">
          <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
          <xsl:value-of select="title" />
        </div>
        <div class="preview_fileInfo">
          <resource name="fres.TitleOwner" />
          <span class="span_space"></span>
          <span><xsl:value-of select="create_by" /></span>
          <span class="span_space"></span>
          <span>|</span>
          <span class="span_space"></span>
          <xsl:choose>
            <xsl:when test="version > 1">
              <resource name="fres.TitleModified" />
            </xsl:when>
            <xsl:otherwise>
              <resource name="fres.TitleUploaded" />
            </xsl:otherwise>
          </xsl:choose>
          <span class="span_space"></span>
          <span><xsl:value-of select="modified_on" /></span>
          <span class="span_space"></span>
          <span>|</span>
          <span class="span_space"></span>
          <span><xsl:value-of  select="content_length"/></span>
          <span class="span_space"></span>
        </div>
        <input type="hidden" value="false">
          <xsl:attribute name="id">file_IsEditing_<xsl:value-of select="id" /></xsl:attribute>
          <xsl:if test="contains(file_status,'IsEditing')">
            <xsl:attribute name="value">true</xsl:attribute>
          </xsl:if>
        </input>
        <input type="hidden">
          <xsl:attribute name="id">by_previewfile_<xsl:value-of select="id" /></xsl:attribute>
          <xsl:attribute name="value"><xsl:value-of select="create_by_id" /></xsl:attribute>
        </input>
        <input type="hidden" id="file_preview_folder_id">
          <xsl:attribute name="value"><xsl:value-of select="folder_id" /></xsl:attribute>
        </input>
      </div>
    </xsl:for-each>
    <input type="hidden" id="file_preview_physical_path">
      <xsl:attribute name="value"><xsl:value-of select="value/status" /></xsl:attribute>
    </input>
    <input type="hidden" id="file_preview_status">
      <xsl:choose>
        <xsl:when test="value/errors > 0">
          <xsl:attribute name="value">error</xsl:attribute>
        </xsl:when>
        <xsl:when test="contains(value/converted,'true')">
          <xsl:attribute name="value">converted</xsl:attribute>
        </xsl:when>
        <xsl:when test="contains(value/processing,'true') or contains(value/queue,'true')">
          <xsl:attribute name="value">wait</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="value">error</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </input>
  </xsl:template>

</xsl:stylesheet>