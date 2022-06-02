-- webstudio_uservisit
CREATE TABLE `webstudio_uservisit` (
  `TenantID` int(10) NOT NULL,
  `VisitDate` datetime NOT NULL,
  `ProductID` varchar(38) NOT NULL,
  `ModuleID` varchar(38) NOT NULL,
  `UserID` varchar(38) NOT NULL,
  `VisitCount` int(10) NOT NULL DEFAULT '0',
  `FirstVisitTime` datetime DEFAULT NULL,
  `LastVisitTime` datetime DEFAULT NULL,
  PRIMARY KEY (`TenantID`,`VisitDate`,`ProductID`,`ModuleID`,`UserID`)
);

