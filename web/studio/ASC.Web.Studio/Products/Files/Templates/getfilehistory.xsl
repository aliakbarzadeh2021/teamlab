<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="fileList">

    <xsl:if test="not(@withoutheader)">
    <div id="content_versions" class="clearFix versionsRow notPreview">
      <span class="clearFix versionsTitle">
        <resource name="fres.TitleVersions" />:
      </span>
      <div class="clearFix">
        <div class="clearFix headerTableVersions">
          <div class="VersionNum"><resource name="fres.VersionNum" /></div>
          <div class="DateTime"><resource name="fres.DateTime" /></div>
          <div class="Size"><resource name="fres.Size" /></div>
          <div class="Author"><resource name="fres.Author" /></div>
          <div class="Action"><resource name="fres.Action" /></div>
        </div>
      </div>
      <div class="versions_btn">
        <a id="content_versions_btn" class="baseLinkButton" style="float:left; margin-right:8px;" onclick="ASC.Files.Uploads.uploadFromHistory(this);return false;">
          <resource name="fres.ButtonUploadNewVersions" />
        </a>
        <a class="grayLinkButton" style="float:left;" onclick="ASC.Files.Folders.closeVersions();return false;">
          <resource name="fres.ButtonClose" />
        </a>
      </div>
    </div>
    </xsl:if>

    <div class="tableVersionRow">
        <xsl:for-each select="entry">
          <div onmouseover="jq(this).addClass('actionRow')" onmouseout="jq(this).removeClass('actionRow')">
            <xsl:attribute name="class">
              clearFix versionRow
              <xsl:if test="position() mod 2 = 1">even</xsl:if>
            </xsl:attribute>
            <xsl:attribute name="id">content_fileversion_<xsl:value-of select="id" />_<xsl:value-of select="version" /></xsl:attribute>
            <div class="version_num">
              <xsl:value-of select="version" />
            </div>
            <div class="version_datetime">
              <b style="margin-right:8px;">
                <xsl:value-of select="substring-before(modified_on, ' ')" />
              </b>
              <xsl:value-of select="substring-after(modified_on, ' ')" />
            </div>
            <div class="version_size">
              <xsl:value-of select="content_length" />
            </div>
            <div class="version_author" >
              <xsl:attribute name="title"><xsl:value-of select="modified_by" /></xsl:attribute>
              <xsl:value-of select="modified_by" />
            </div>
            <div class="version_restore">
              <div class="previewVersion">
                <xsl:attribute name="title"><resource name="fres.OpenFile" /></xsl:attribute>
                <xsl:attribute name="onclick">
                  ASC.Files.Folders.clickOnFile(<xsl:value-of select="id" />, <xsl:value-of select="version" />);return false;
                </xsl:attribute>
              </div>
              <div class="downloadVersion">
                <xsl:attribute name="title"><resource name="fres.ButtonDownload" /></xsl:attribute>
                <xsl:attribute name="onclick">
                  ASC.Files.Folders.download("file_" + <xsl:value-of select="id" />, <xsl:value-of select="version" />);return false;
                </xsl:attribute>
              </div>
              <a>
                <xsl:attribute name="onclick">ASC.Files.Folders.makeCurrentVersion(<xsl:value-of select="id" />, <xsl:value-of select="version" />);return false;</xsl:attribute>
                <resource name="fres.MakeCurrent" />
              </a>
            </div>
          </div>
        </xsl:for-each>
    </div>
    
  </xsl:template>

</xsl:stylesheet>
