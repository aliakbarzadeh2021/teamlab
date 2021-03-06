<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text" encoding="utf-8" />

  <xsl:param name="p0"/>
  <xsl:param name="p1"/>
  <xsl:param name="p2"/>
  <xsl:param name="p3"/>
  <xsl:param name="p4"/>
  <xsl:param name="p5"/>

  <xsl:template match="/">

    <xsl:value-of select="$p0"/>
    <xsl:text>,</xsl:text>
    <xsl:value-of select="$p1"/>
    <xsl:text>,</xsl:text>
    <xsl:value-of select="$p2"/>
    <xsl:text>,</xsl:text>
    <xsl:value-of select="$p3"/>
    <xsl:text>,</xsl:text>
    <xsl:value-of select="$p4"/>
    <xsl:text>,</xsl:text>
    <xsl:value-of select="$p5"/>
    <xsl:text>&#10;</xsl:text>

    <xsl:apply-templates/>

  </xsl:template>

  <xsl:template match="r">

    <xsl:value-of select="@c1" />
    <xsl:text>,</xsl:text>
    <xsl:value-of select="@c7" />
    <xsl:text>,</xsl:text>
    <xsl:value-of select="@c3" />
    <xsl:text>,</xsl:text>
    <xsl:value-of select="@c4" />
    <xsl:text>,</xsl:text>
    <xsl:value-of select="@c5" />
    <xsl:text>,</xsl:text>
    <xsl:value-of select="@c6" />
    <xsl:text>&#10;</xsl:text>

  </xsl:template>

</xsl:stylesheet>