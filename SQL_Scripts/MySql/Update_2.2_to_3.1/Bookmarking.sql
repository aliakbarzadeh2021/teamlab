ALTER TABLE `bookmarking_bookmark`  ADD INDEX `Tenant` (`Tenant`);

create table bookmarking_tmp (c1 int, c2 int, c3 int);
insert into bookmarking_tmp select BookmarkID,TagID,Tenant from bookmarking_bookmarktag group by 1,2;
delete from bookmarking_bookmarktag;
ALTER TABLE `bookmarking_bookmarktag`
	ALTER `BookmarkID` DROP DEFAULT,
	ALTER `TagID` DROP DEFAULT,
	ALTER `Tenant` DROP DEFAULT;
ALTER TABLE `bookmarking_bookmarktag`
	CHANGE COLUMN `BookmarkID` `BookmarkID` INT(11) NOT NULL FIRST,
	CHANGE COLUMN `TagID` `TagID` INT(11) NOT NULL AFTER `BookmarkID`,
	CHANGE COLUMN `Tenant` `Tenant` INT(11) NOT NULL AFTER `TagID`,
	DROP COLUMN `BookmarkTagID`,
	DROP PRIMARY KEY,
	DROP INDEX `IndexTagID`,
	DROP INDEX `IndexBookmarkID`,
	ADD PRIMARY KEY (`BookmarkID`, `TagID`),
	ADD INDEX `Tenant` (`Tenant`);
insert into bookmarking_bookmarktag select c1,c2,c3 from bookmarking_tmp;
delete from bookmarking_tmp;

ALTER TABLE `bookmarking_tag`  CHANGE COLUMN `Name` `Name` VARCHAR(255) NOT NULL AFTER `TagID`;
ALTER TABLE `bookmarking_tag`  ADD INDEX `Name` (`Tenant`, `Name`);

ALTER TABLE `bookmarking_userbookmark`  CHANGE COLUMN `BookmarkID` `BookmarkID` INT(11) NOT NULL DEFAULT '0' AFTER `Description`;
ALTER TABLE `bookmarking_userbookmark`  CHANGE COLUMN `Raiting` `Raiting` INT(11) NOT NULL DEFAULT '0' AFTER `BookmarkID`;
ALTER TABLE `bookmarking_userbookmark`  ADD INDEX `LastModified` (`Tenant`, `LastModified`);

insert into bookmarking_tmp select UserBookmarkID,TagID,Tenant from bookmarking_userbookmarktag group by 1,2;
delete from bookmarking_userbookmarktag;
ALTER TABLE `bookmarking_userbookmarktag`
	ALTER `UserBookmarkID` DROP DEFAULT,
	ALTER `TagID` DROP DEFAULT,
	ALTER `Tenant` DROP DEFAULT;
ALTER TABLE `bookmarking_userbookmarktag`
	CHANGE COLUMN `UserBookmarkID` `UserBookmarkID` INT(11) NOT NULL FIRST,
	CHANGE COLUMN `TagID` `TagID` INT(11) NOT NULL AFTER `UserBookmarkID`,
	CHANGE COLUMN `Tenant` `Tenant` INT(11) NOT NULL AFTER `TagID`,
	DROP COLUMN `UserBookmarkTagID`,
	DROP PRIMARY KEY,
	DROP INDEX `IndexUserBookmarkTagUserBookmarkID`,
	DROP INDEX `IndexUserBookmarkTagUserTagID`,
	ADD PRIMARY KEY (`UserBookmarkID`, `TagID`),
	ADD INDEX `Tenant` (`Tenant`);
insert into bookmarking_userbookmarktag select c1,c2,c3 from bookmarking_tmp;
drop table bookmarking_tmp;
