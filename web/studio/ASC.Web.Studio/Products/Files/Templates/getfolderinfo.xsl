<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="folder|folder_info">
    <table>
      <tbody>
        <tr>
          <xsl:if test="parent_folder_id > 0">
            <td>
              <div class="levelUp">
                <xsl:attribute name="id">levelUp_<xsl:value-of select="parent_folder_id" /></xsl:attribute>
                <a>
                  <xsl:attribute name="href">#<xsl:value-of select="parent_folder_id" /></xsl:attribute>
                  <xsl:attribute name="title"><resource name="fres.TitleFolderUp" /></xsl:attribute>
                  <xsl:attribute name="onclick">ASC.Files.UI.madeAnchor(<xsl:value-of select="parent_folder_id" />);return false;</xsl:attribute>
                </a>
              </div>
            </td>
          </xsl:if>
          <td>
            <div class="folderBigIcon">
              <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
              <div class="share_file_icn">
                <xsl:if test="shared = 'true'">
                  <xsl:attribute name="style">display:block;</xsl:attribute>
                </xsl:if>
                <xsl:attribute name="title"><resource name="fres.TitleShareFile" /></xsl:attribute>
                <xsl:attribute name="id">share_icn_folder_<xsl:value-of select="id" /></xsl:attribute>
              </div>
            </div>
          </td>
          <td>
            <div class="fade_title"></div>
            <div id="files_folderName" class="folderName">
              <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
              <xsl:value-of select="title" />
            </div>
            <div id="files_folderInfo" class="folderInfo">
              <xsl:if test="create_on != ''">
                <resource name="fres.TitleCreated" />
                <span class="span_space"></span>
                <span><xsl:value-of select="create_on" /></span>
                <span class="span_space"></span>
                <span>|</span>
                <span class="span_space"></span>
              </xsl:if>
              <xsl:if test="total_files != ''">
                <resource name="fres.TitleFiles" />
                <span class="span_space"></span>
                <span id="content_info_count_files"><xsl:value-of select="total_files" /></span>
              </xsl:if>
              <xsl:if test="total_sub_folder != ''">
                <span class="span_space"></span>
                <span>|</span>
                <span class="span_space"></span>
                <resource name="fres.TitleSubfolders" />
                <span class="span_space"></span>
                <span id="content_info_count_folders"><xsl:value-of select="total_sub_folder" /></span>
              </xsl:if>
              <input type="hidden">
                <xsl:attribute name="id">access_folder_<xsl:value-of select="id" /></xsl:attribute>
                <xsl:attribute name="value"><xsl:value-of select="access" /></xsl:attribute>
              </input>
              <input type="hidden">
                <xsl:attribute name="id">folder_shareable</xsl:attribute>
                <xsl:attribute name="value"><xsl:value-of select="shareable" /></xsl:attribute>
              </input>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </xsl:template>

</xsl:stylesheet>