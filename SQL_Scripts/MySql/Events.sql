-- events_comment
CREATE TABLE `events_comment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Feed` int(11) NOT NULL,
  `Comment` text NOT NULL,
  `Parent` int(11) NOT NULL DEFAULT '0',
  `Date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Creator` varchar(38) DEFAULT NULL,
  `Inactive` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
);
-- events_feed
CREATE TABLE `events_feed` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FeedType` int(11) NOT NULL DEFAULT '1',
  `Caption` text NOT NULL,
  `Text` text,
  `Date` datetime NOT NULL,
  `Creator` varchar(38) DEFAULT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `LastModified` (`Tenant`,`LastModified`)
);
-- events_poll
CREATE TABLE `events_poll` (
  `Id` int(11) NOT NULL,
  `PollType` int(11) NOT NULL DEFAULT '0',
  `StartDate` datetime NOT NULL,
  `EndDate` datetime NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
);
-- events_pollanswer
CREATE TABLE `events_pollanswer` (
  `Variant` int(11) NOT NULL,
  `User` varchar(64) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Variant`,`User`)
);
-- events_pollvariant
CREATE TABLE `events_pollvariant` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Poll` int(11) NOT NULL,
  `Name` varchar(1024) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Poll` (`Poll`)
);
-- events_reader
CREATE TABLE `events_reader` (
  `Feed` int(11) NOT NULL,
  `Reader` varchar(38) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Feed`,`Reader`)
);

