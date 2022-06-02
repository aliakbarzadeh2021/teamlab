-- photo_album
CREATE TABLE `photo_album` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Caption` varchar(255) DEFAULT NULL,
  `Event` int(11) NOT NULL,
  `User` varchar(38) DEFAULT NULL,
  `FaceImage` int(11) NOT NULL DEFAULT '0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ImagesCount` int(11) NOT NULL DEFAULT '0',
  `ViewsCount` int(11) NOT NULL DEFAULT '0',
  `CommentsCount` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Photo_Album_Index1` (`Event`)
);
-- photo_comment
CREATE TABLE `photo_comment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Text` text NOT NULL,
  `User` varchar(38) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Image` int(11) NOT NULL,
  `Parent` int(11) NOT NULL DEFAULT '0',
  `Inactive` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Photo_Comment_Index1` (`Image`)
);
-- photo_event
CREATE TABLE `photo_event` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `User` varchar(38) DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
);
-- photo_image
CREATE TABLE `photo_image` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Album` int(11) NOT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `Location` varchar(1024) DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `User` varchar(38) DEFAULT NULL,
  `ThumbnailWidth` int(11) NOT NULL DEFAULT '0',
  `ThumbnailHeight` int(11) NOT NULL DEFAULT '0',
  `PreviewWidth` int(11) NOT NULL DEFAULT '0',
  `PreviewHeight` int(11) NOT NULL DEFAULT '0',
  `CommentsCount` int(11) NOT NULL DEFAULT '0',
  `ViewsCount` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Photo_Image_Index1` (`Album`)
);
-- photo_imageview
CREATE TABLE `photo_imageview` (
  `Image` int(11) NOT NULL,
  `User` varchar(38) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Image`,`User`,`Tenant`)
);

