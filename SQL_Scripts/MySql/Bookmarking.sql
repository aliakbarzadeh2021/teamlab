-- bookmarking_bookmark
CREATE TABLE `bookmarking_bookmark` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `URL` text,
  `Date` datetime DEFAULT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Description` text,
  `UserCreatorID` char(38) DEFAULT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `Tenant` (`Tenant`)
);
-- bookmarking_bookmarktag
CREATE TABLE `bookmarking_bookmarktag` (
  `BookmarkID` int(11) NOT NULL,
  `TagID` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`BookmarkID`,`TagID`),
  KEY `Tenant` (`Tenant`)
);
-- bookmarking_comment
CREATE TABLE `bookmarking_comment` (
  `ID` char(38) NOT NULL,
  `UserID` char(38) DEFAULT NULL,
  `Content` text,
  `Datetime` datetime DEFAULT NULL,
  `Parent` char(38) DEFAULT NULL,
  `BookmarkID` int(11) DEFAULT NULL,
  `Inactive` int(11) DEFAULT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `IndexCommentBookmarkID` (`BookmarkID`)
);
-- bookmarking_tag
CREATE TABLE `bookmarking_tag` (
  `TagID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`TagID`),
  KEY `Name` (`Tenant`,`Name`)
);
-- bookmarking_userbookmark
CREATE TABLE `bookmarking_userbookmark` (
  `UserBookmarkID` int(11) NOT NULL AUTO_INCREMENT,
  `UserID` char(38) DEFAULT NULL,
  `DateAdded` datetime DEFAULT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Description` text,
  `BookmarkID` int(11) NOT NULL,
  `Raiting` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL,
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserBookmarkID`),
  KEY `LastModified` (`Tenant`,`LastModified`),
  KEY `BookmarkID` (`BookmarkID`)
);
-- bookmarking_userbookmarktag
CREATE TABLE `bookmarking_userbookmarktag` (
  `UserBookmarkID` int(11) NOT NULL,
  `TagID` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`UserBookmarkID`,`TagID`),
  KEY `Tenant` (`Tenant`)
);

