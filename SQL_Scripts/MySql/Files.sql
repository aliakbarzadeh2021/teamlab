-- files_bunch_objects
CREATE TABLE `files_bunch_objects` (
  `left_node` varchar(255) NOT NULL,
  `right_node` varchar(255) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`right_node`,`tenant_id`),
  KEY `left_node` (`left_node`)
);
-- files_file
CREATE TABLE `files_file` (
  `id` int(11) NOT NULL,
  `version` int(11) NOT NULL,
  `current_version` int(11) NOT NULL DEFAULT '0',
  `folder_id` int(11) NOT NULL DEFAULT '0',
  `title` varchar(400) NOT NULL,
  `content_type` varchar(255) DEFAULT NULL,
  `content_length` bigint(25) NOT NULL DEFAULT '0',
  `file_status` int(11) NOT NULL DEFAULT '0',
  `category` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `modified_by` char(38) NOT NULL,
  `modified_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `converted_type` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`tenant_id`,`id`,`version`),
  KEY `folder_id` (`folder_id`,`title`(255)),
  KEY `modified_on` (`modified_on`)
);
-- files_folder
CREATE TABLE `files_folder` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parent_id` int(11) NOT NULL DEFAULT '0',
  `title` varchar(400) NOT NULL,
  `folder_type` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `modified_by` char(38) NOT NULL,
  `modified_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `foldersCount` int(10) NOT NULL DEFAULT '0',
  `filesCount` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `parent_id` (`parent_id`,`title`(255))
);
-- files_folder_tree
CREATE TABLE `files_folder_tree` (
  `folder_id` int(11) NOT NULL,
  `parent_id` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  PRIMARY KEY (`parent_id`,`folder_id`),
  KEY `folder_id` (`folder_id`)
);
-- files_security
CREATE TABLE `files_security` (
  `tenant_id` int(10) NOT NULL,
  `entry_id` int(10) NOT NULL,
  `entry_type` int(10) NOT NULL,
  `subject` char(38) NOT NULL,
  `owner` char(38) NOT NULL,
  `security` int(11) NOT NULL,
  PRIMARY KEY (`tenant_id`,`entry_id`,`entry_type`,`subject`)
);
-- files_tag
CREATE TABLE `files_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `owner` varchar(38) NOT NULL,
  `flag` int(11) NOT NULL DEFAULT '0',
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `name` (`owner`,`name`,`flag`)
);
-- files_tag_link
CREATE TABLE `files_tag_link` (
  `tag_id` int(10) NOT NULL,
  `entry_id` int(10) NOT NULL,
  `entry_type` int(10) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`tenant_id`,`tag_id`,`entry_id`,`entry_type`)
);

