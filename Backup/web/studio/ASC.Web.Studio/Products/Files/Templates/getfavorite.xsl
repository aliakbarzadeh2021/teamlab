<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <xsl:template match="favoriteList">
    <xsl:for-each select="entry">
      <li class='clearFix favoritesRow' onmouseout="ASC.Files.Favorites.displayRemoveBtn();">
        <xsl:attribute name="id">favoritesRow_<xsl:value-of select="id" /></xsl:attribute>
        <xsl:attribute name="onmouseover">ASC.Files.Favorites.displayRemoveBtn(<xsl:value-of select="id" />);</xsl:attribute>
        <a>
          <xsl:if test="starts-with(folder_path, 'file')">
            <xsl:attribute name="class">fav_file</xsl:attribute>
          </xsl:if>
          <xsl:if test="starts-with(folder_path, 'folder')">
            <xsl:attribute name="class">fav_folder</xsl:attribute>
          </xsl:if>
          <xsl:attribute name="onclick">ASC.Files.Favorites.getFavorite('<xsl:value-of select="id" />');return false;</xsl:attribute>
          <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
          <xsl:value-of select="title" />
        </a>
        <input type="hidden" class="fav_list">
          <xsl:attribute name="value"><xsl:value-of select="folder_path" /></xsl:attribute>
        </input>
        <div class='favoritesRemoveAction' style='display:none;'>
          <xsl:attribute name="onclick">ASC.Files.Favorites.removeFavorite(<xsl:value-of select="id" />);return false;</xsl:attribute>
          <xsl:attribute name="id">removeBtn_<xsl:value-of select="id" /></xsl:attribute>
        </div>
      </li>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>