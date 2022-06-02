<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />
  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="ace_wrapperList">
    <xsl:for-each select="entry">
      <xsl:if test="is_group = 'false'">
        <li class="clearFix share_row">
          <xsl:attribute name="id"><xsl:value-of select="id" /></xsl:attribute>
          <div class="share_name">
            <span class="userLink">
              <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
              <xsl:value-of select="title" />
            </span>
          </div>
          <div class="share_actions">
            <xsl:if test="owner != 'true'">
              <div class="share_trash">
                <xsl:attribute name="title"><resource name="fres.ButtonDeleteFromShare" /></xsl:attribute>
                <xsl:attribute name="onclick">ASC.Files.Share.delUserInShare('<xsl:value-of select="id" />')</xsl:attribute>
              </div>
            </xsl:if>
          </div>
          <div class="share_rights">
            <div class="action_noactive">
              <xsl:choose>
                <xsl:when test="owner = 'true'">
                  <a class="owner">
                    <xsl:attribute name="id">action_<xsl:value-of select="id" /></xsl:attribute>
                    <resource name="fres.SelectRight" />
                  </a>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="onclick">ASC.Files.Share.showRightsPopup('<xsl:value-of select="id" />')</xsl:attribute>
                  <a>
                    <xsl:attribute name="id">action_<xsl:value-of select="id" /></xsl:attribute>
                    <xsl:attribute name="title"><resource name="fres.SelectRight" /></xsl:attribute>
                    <resource name="fres.SelectRight" />
                  </a>
                  <div class="combobox"></div>
                </xsl:otherwise>
              </xsl:choose>
            </div>
            <input type="hidden">
              <xsl:attribute name="id">selected_action_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value">
                <xsl:choose>
                  <xsl:when test="owner = 'true'">owner</xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="ace_status" />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </input>
            <input type="hidden" value="false">
              <xsl:attribute name="id">change_action_<xsl:value-of select="id" /></xsl:attribute>
            </input>
          </div>
        </li>
      </xsl:if>
    </xsl:for-each>
    <div id="share_groups_data">
      <xsl:for-each select="entry">
        <xsl:if test="is_group = 'true'">
          <input type="hidden">
            <xsl:attribute name="id">group_right_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="value"><xsl:value-of select="ace_status" /></xsl:attribute>
            <xsl:if test="locked = 'true'">
              <xsl:attribute name="value2">locked</xsl:attribute>
            </xsl:if>
          </input>
        </xsl:if>
      </xsl:for-each>
    </div>
  </xsl:template>

</xsl:stylesheet>