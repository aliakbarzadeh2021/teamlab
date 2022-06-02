<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <xsl:template match="folderList">
    <xsl:for-each select="entry">
      <li>
        <xsl:attribute name="class">
          jstree-closed
          <xsl:if test="access = 'Read'">
            access_read
          </xsl:if>
        </xsl:attribute>
        <xsl:attribute name="id">tree_node_<xsl:value-of select="id" /></xsl:attribute>
        <xsl:attribute name="name"><xsl:value-of select="parent_folder_id" /></xsl:attribute>
        <ins class="jstree-icon" rel="expander"> </ins>
        <a>
          <xsl:attribute name="onclick">ASC.Files.UI.madeAnchor(<xsl:value-of select="id" />);return false;</xsl:attribute>
          <xsl:attribute name="name"><xsl:value-of select="container_id" /></xsl:attribute>
          <xsl:attribute name="id">tree_selector_<xsl:value-of select="id" /></xsl:attribute>
          <xsl:attribute name="href">#<xsl:value-of select="id" /></xsl:attribute>
          <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
          <ins class="jstree-icon" rel="folder"> </ins>
          <xsl:value-of select="title" />
        </a>
      </li>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>