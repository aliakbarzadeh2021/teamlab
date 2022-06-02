<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />
  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="group_infoList">
    <xsl:for-each select="entry">
      <li class="clearFix share_row">
        <xsl:attribute name="id"><xsl:value-of select="id" /></xsl:attribute>
        <div class="share_name">
          <xsl:attribute name="title"><xsl:value-of select="name" /></xsl:attribute>
          <xsl:value-of select="name" />
        </div>
        <div class="share_actions">
          <div class="share_trash">
            <xsl:attribute name="title"><resource name="fres.ButtonDeleteFromShare" /></xsl:attribute>
            <xsl:attribute name="onclick">ASC.Files.Share.delGroupInShare('<xsl:value-of select="id" />')</xsl:attribute>
          </div>
        </div>
        <div class="share_rights">
          <div class="action_noactive">
            <xsl:attribute name="onclick">ASC.Files.Share.showRightsPopup('<xsl:value-of select="id" />')</xsl:attribute>
            <a>
              <xsl:attribute name="id">action_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="title"><resource name="fres.SelectRight" /></xsl:attribute>
              <resource name="fres.SelectRight" />
            </a>
            <div class="combobox">
            </div>
          </div>
          <input type="hidden">
            <xsl:attribute name="id">selected_action_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="value"><xsl:value-of select="ace_status" /></xsl:attribute>
          </input>
          <input type="hidden" value="false">
            <xsl:attribute name="id">change_action_<xsl:value-of select="id" /></xsl:attribute>
          </input>
        </div>
      </li>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>