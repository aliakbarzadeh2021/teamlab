<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="fileList">
    <xsl:for-each select="entry">
      <li>
        <xsl:attribute name="class">
          shange_row
          <xsl:if test="position() mod 2 = 1">even</xsl:if>
        </xsl:attribute>
        <xsl:if test="position() = 1">
          <xsl:attribute name="style">
            border-top: 1px solid #D1D1D1;
          </xsl:attribute>
        </xsl:if>
        <xsl:attribute name="id">changed_file_<xsl:value-of select="id" /></xsl:attribute>
        <div class="name">
          <a>
            <xsl:attribute name="onclick">
              ASC.Files.Folders.clickOnFile(<xsl:value-of select="id" />);return false;
            </xsl:attribute>
            <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
            <xsl:value-of select="title" />
          </a>
        </div>
        <div class="author">
          <span class="userLink">
            <xsl:attribute name="title"><xsl:value-of select="create_by" /></xsl:attribute>
            <xsl:value-of select="create_by" />
          </span>
        </div>
        <div class="date">
          <xsl:attribute name="title"><xsl:value-of select="create_on" /></xsl:attribute>
          <xsl:value-of select="create_on" />
        </div>
      </li>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>