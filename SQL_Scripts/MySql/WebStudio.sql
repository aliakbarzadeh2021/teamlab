-- webstudio_fckuploads
CREATE TABLE `webstudio_fckuploads` (
  `TenantID` int(11) NOT NULL,
  `StoreDomain` varchar(50) NOT NULL,
  `FolderID` varchar(100) NOT NULL,
  `ItemID` varchar(100) NOT NULL,
  PRIMARY KEY (`TenantID`,`StoreDomain`,`FolderID`,`ItemID`)
);
-- webstudio_quicklinks
CREATE TABLE `webstudio_quicklinks` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `URL` text,
  `UserCreatorID` char(38) DEFAULT NULL,
  `Date` datetime DEFAULT NULL,
  `DisplayOnTopPanel` int(11) DEFAULT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`)
);
-- webstudio_settings
CREATE TABLE `webstudio_settings` (
  `TenantID` int(11) NOT NULL,
  `ID` varchar(64) NOT NULL,
  `UserID` varchar(64) NOT NULL,
  `Data` blob,
  PRIMARY KEY (`ID`,`UserID`,`TenantID`)
);
-- webstudio_useractivity
CREATE TABLE `webstudio_useractivity` (
  `ID` int(10) NOT NULL AUTO_INCREMENT,
  `ProductID` char(38) NOT NULL,
  `ModuleID` char(38) NOT NULL,
  `UserID` char(38) NOT NULL,
  `ContentID` varchar(256) NOT NULL,
  `Title` varchar(4000) NOT NULL,
  `URL` varchar(4000) NOT NULL,
  `BusinessValue` int(10) NOT NULL DEFAULT '0',
  `ActionType` int(10) NOT NULL,
  `ActionText` varchar(256) NOT NULL,
  `ActivityDate` datetime NOT NULL,
  `ImageFileName` varchar(1024) DEFAULT NULL,
  `PartID` varchar(38) DEFAULT NULL,
  `ContainerID` varchar(38) DEFAULT NULL,
  `AdditionalData` varchar(256) DEFAULT NULL,
  `TenantID` int(10) NOT NULL,
  `HtmlPreview` mediumtext,
  `SecurityId` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `UserID` (`UserID`),
  KEY `actiontype` (`TenantID`,`ActionType`,`ProductID`),
  KEY `ProductID` (`TenantID`,`ProductID`,`ModuleID`),
  KEY `containerid` (`ContainerID`),
  KEY `ActivityDate` (`ActivityDate`)
);
-- webstudio_widgetcontainer
CREATE TABLE `webstudio_widgetcontainer` (
  `ContainerID` varchar(64) NOT NULL,
  `UserID` varchar(64) NOT NULL,
  `SchemaID` int(11) NOT NULL,
  `Name` varchar(256) DEFAULT '',
  `SortOrder` int(11) NOT NULL DEFAULT '0',
  `ID` varchar(64) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`)
);
-- webstudio_widgetstate
CREATE TABLE `webstudio_widgetstate` (
  `WidgetID` varchar(64) NOT NULL,
  `IsCollapse` int(11) NOT NULL DEFAULT '0',
  `ColumnID` int(11) NOT NULL,
  `SortOrderInColumn` int(11) NOT NULL DEFAULT '0',
  `WidgetContainerID` varchar(64) NOT NULL,
  PRIMARY KEY (`WidgetContainerID`,`WidgetID`)
);

