<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />
  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="composite_data">

    <xsl:for-each select="folders/folder">
      <div class="clearFix fileRow folderRow">
        <xsl:attribute name="oncontextmenu">return ASC.Files.Actions.onContextMenu("folder_<xsl:value-of select="id" />",event);</xsl:attribute>
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
        <div class="sub-folder move-to">
          <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
          <xsl:attribute name="onclick">ASC.Files.Folders.clickOnFolder(<xsl:value-of select="id" />);return false;</xsl:attribute>
          <div class="share_file_icn">
            <xsl:if test="shared = 'true'">
              <xsl:attribute name="style">display:block;</xsl:attribute>
            </xsl:if>
            <xsl:attribute name="title"><resource name="fres.TitleShareFile" /></xsl:attribute>
            <xsl:attribute name="id">share_icn_folder_<xsl:value-of select="id" /></xsl:attribute>
          </div>
        </div>
        <div class="sub-info">
          <div class="fileName">
            <div class="name">
              <a class="name" onmousedown="return false;">
                <xsl:attribute name="onclick">ASC.Files.Folders.clickOnFolder(<xsl:value-of select="id" />);return false;</xsl:attribute>
                <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
                <xsl:attribute name="href">#<xsl:value-of select="id" /></xsl:attribute>
                <xsl:value-of select="title" />
              </a>
            </div>
          </div>
          <div class="fileInfo">
            <span class="ownerSpan">
              <resource name="fres.TitleOwner" />
              <span class="span_space"></span>
              <span class="create_by">
                <xsl:attribute name="title"><xsl:value-of select="create_by" /></xsl:attribute>
                <xsl:value-of select="create_by" />
              </span>
            </span>
            <span class="span_space"></span>
            <span>|</span>
            <span class="span_space"></span>
            <span>
              <span class="titleCreated"><resource name="fres.TitleCreated" /></span>
              <span class="titleRemoved" style="display:none"><resource name="fres.TitleRemoved" /></span>
              <span class="span_space"></span>
              <span class="create_date"><xsl:value-of select="create_on" /></span>
              <span class="modified_date" style="display:none"><xsl:value-of select="modified_on" /></span>
            </span>
            <span class="span_space"></span>
            <span>|</span>
            <span class="span_space"></span>
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
              <xsl:attribute name="id">access_folder_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value"><xsl:value-of select="access" /></xsl:attribute>
            </input>
            <input type="hidden">
              <xsl:attribute name="id">by_folder_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value"><xsl:value-of select="create_by_id" /></xsl:attribute>
            </input>
          </div>
        </div>
        <div class="fileFavorites">
          <a class="file_fav_notyet" style="margin-left:7px;">
            <xsl:attribute name="onclick">ASC.Files.Favorites.showToFavorite(this, event);return false;</xsl:attribute>
            <xsl:attribute name="id">favorite_button_<xsl:value-of select="id" /></xsl:attribute>
            <div class="fav"></div>
          </a>
        </div>
      </div>
    </xsl:for-each>

    <xsl:for-each select="files/file">
      <div class="clearFix fileRow">
        <xsl:attribute name="oncontextmenu">return ASC.Files.Actions.onContextMenu("file_<xsl:value-of select="id" />",event);</xsl:attribute>
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
        <div class="folder-file move-to">
          <xsl:attribute name="id">file_icn_<xsl:value-of select="id" /></xsl:attribute>
          <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
          <xsl:attribute name="onclick">ASC.Files.Folders.clickOnFile(<xsl:value-of select="id" />);return false;</xsl:attribute>
          <script type="text/javascript">
            var urlIcn = ASC.Files.Utils.getFileTypeIcon("<xsl:value-of select="title" />");
            jq("#file_icn_<xsl:value-of select="id" />").css("background", "url('"+urlIcn+"') no-repeat");
          </script>
          <div class="editing_file_icn">
            <xsl:if test="contains(file_status, 'IsEditing')">
              <xsl:attribute name="style">display:block;</xsl:attribute>
            </xsl:if>
            <xsl:attribute name="id">file_editing_icn_<xsl:value-of select="id" /></xsl:attribute>
            <xsl:attribute name="title"><resource name="fres.TitleEditingFile" /></xsl:attribute>
          </div>
          <div class="share_file_icn">
            <xsl:if test="shared = 'true'">
              <xsl:attribute name="style">display:block;</xsl:attribute>
            </xsl:if>
            <xsl:attribute name="title"><resource name="fres.TitleShareFile" /></xsl:attribute>
            <xsl:attribute name="id">share_icn_file_<xsl:value-of select="id" /></xsl:attribute>
          </div>
        </div>
        <div class="file-info">
          <div class="fileName">
            <div class="name">
              <a class="name" onmousedown="return false;">
                <xsl:attribute name="onclick">ASC.Files.Folders.clickOnFile(<xsl:value-of select="id" />);return false;</xsl:attribute>
                <xsl:attribute name="title"><xsl:value-of select="title" /></xsl:attribute>
                <xsl:attribute name="href">#<xsl:value-of select="folder_id" /></xsl:attribute>
                <xsl:value-of select="title" />
              </a>
            </div>
            <xsl:if test="version > 1">
              <div class="version">
                <xsl:attribute name="onclick">ASC.Files.Folders.showVersions("combo_file_" + <xsl:value-of select="id" />);return false;</xsl:attribute>
                <xsl:attribute name="title"><resource name="fres.Version" /><xsl:value-of select="version" /></xsl:attribute>
                <resource name="fres.Version" />
                <xsl:value-of select="version" />
              </div>
            </xsl:if>
            <xsl:if test="contains(file_status, 'IsNew')">
              <div class="is_new">
                <resource name="fres.IsNew" />
              </div>
            </xsl:if>
          </div>
          <div class="fileInfo">
            <span class="ownerSpan">
              <resource name="fres.TitleOwner" />
              <span class="span_space"></span>
              <span class="create_by">
                <xsl:choose>
                  <xsl:when test="version > 1">
                    <xsl:attribute name="title"><xsl:value-of select="modified_by" /></xsl:attribute>
                    <xsl:value-of select="modified_by" />
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="title"><xsl:value-of select="create_by" /></xsl:attribute>
                    <xsl:value-of select="create_by" />
                  </xsl:otherwise>
                </xsl:choose>
              </span>
              <span class="fortrash_create_by" style="display:none;">
                <xsl:attribute name="title"><xsl:value-of select="create_by" /></xsl:attribute>
                <xsl:value-of select="create_by" />
              </span>
            </span>
            <span class="span_space"></span>
            <span>|</span>
            <span class="span_space"></span>
            <span>
              <span class="titleCreated">
                <xsl:choose>
                  <xsl:when test="version > 1">
                    <resource name="fres.TitleModified" />
                  </xsl:when>
                  <xsl:otherwise>
                    <resource name="fres.TitleUploaded" />
                  </xsl:otherwise>
                </xsl:choose>
              </span>
              <span class="titleRemoved" style="display:none"><resource name="fres.TitleRemoved" /></span>
              <span class="span_space"></span>
              <span class="modified_date"><xsl:value-of select="modified_on" /></span>
            </span>
            <span class="span_space"></span>
            <span>|</span>
            <span class="span_space"></span>
            <span class="content_length"><xsl:value-of select="content_length" /></span>
            <input type="hidden">
              <xsl:attribute name="id">file_version_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value"><xsl:value-of select="version" /></xsl:attribute>
            </input>
            <input type="hidden">
              <xsl:attribute name="id">access_file_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value"><xsl:value-of select="access" /></xsl:attribute>
            </input>
            <input type="hidden">
              <xsl:attribute name="id">by_file_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value"><xsl:value-of select="create_by_id" /></xsl:attribute>
            </input>
            <input type="hidden">
              <xsl:attribute name="id">modified_by_file_<xsl:value-of select="id" /></xsl:attribute>
              <xsl:attribute name="value"><xsl:value-of select="modified_by_id" /></xsl:attribute>
            </input>
          </div>
        </div>
        <div class="fileFavorites">
          <a class="file_fav_notyet" style="margin-left:5px;">
            <xsl:attribute name="onclick">ASC.Files.Favorites.showToFavorite(this, event);return false;</xsl:attribute>
            <xsl:attribute name="id">favorite_button_<xsl:value-of select="id" /></xsl:attribute>
            <div class="fav"></div>
          </a>
        </div>        
        <div class="fileEdit">
          <xsl:if test="not(contains(file_status, 'IsEditing'))">
            <xsl:attribute name="style">display:block;</xsl:attribute>
          </xsl:if>
          <a>
            <xsl:attribute name="onclick">ASC.Files.Actions.checkEditFile(<xsl:value-of select="id" />, "<xsl:value-of select="title" />", true);return false;</xsl:attribute>
            <resource name="fres.ButtonEdit" />
          </a>
        </div>
        <div class="fileEditing">
          <xsl:if test="contains(file_status, 'IsEditing')">
            <xsl:attribute name="style">display:block;</xsl:attribute>
          </xsl:if>
          <a>
            <xsl:attribute name="onclick">return false;</xsl:attribute>
            <resource name="fres.ButtonEditing" />
          </a>
        </div>
      </div>
    </xsl:for-each>

  </xsl:template>

</xsl:stylesheet>