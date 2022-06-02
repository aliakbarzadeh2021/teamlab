-- wiki_categories
CREATE TABLE `wiki_categories` (
  `Tenant` int(11) NOT NULL,
  `CategoryName` varchar(240) NOT NULL,
  `PageName` varchar(240) NOT NULL,
  PRIMARY KEY (`Tenant`,`CategoryName`,`PageName`),
  KEY `PageName` (`Tenant`,`PageName`)
);
-- wiki_comments
CREATE TABLE `wiki_comments` (
  `Id` char(38) NOT NULL,
  `ParentId` char(38) NOT NULL,
  `PageName` varchar(255) NOT NULL,
  `Body` text NOT NULL,
  `UserId` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `Inactive` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `PageName` (`Tenant`,`PageName`)
);
-- wiki_files
CREATE TABLE `wiki_files` (
  `FileName` varchar(255) NOT NULL,
  `UploadFileName` text NOT NULL,
  `Version` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `FileLocation` text NOT NULL,
  `FileSize` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Tenant`,`FileName`)
);
-- wiki_files_history
CREATE TABLE `wiki_files_history` (
  `FileName` varchar(255) NOT NULL,
  `UploadFileName` text NOT NULL,
  `Version` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `FileLocation` text NOT NULL,
  `FileSize` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Tenant`,`FileName`,`Version`)
);
-- wiki_pages
CREATE TABLE `wiki_pages` (
  `PageName` varchar(255) NOT NULL,
  `Version` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `Body` mediumtext,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Tenant`,`PageName`)
);
-- wiki_pages_history
CREATE TABLE `wiki_pages_history` (
  `PageName` varchar(255) NOT NULL,
  `Version` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `Body` mediumtext,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Tenant`,`PageName`,`Version`)
);

