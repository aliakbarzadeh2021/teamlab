ALTER TABLE `projects_comments`  DROP INDEX `create_by`;
ALTER TABLE `projects_comments`  DROP INDEX `parent_id`;
ALTER TABLE `projects_comments`  DROP INDEX `ix_target_uniq_id`;  
ALTER TABLE `projects_comments`  ADD INDEX `ix_target_uniq_id` (`tenant_id`, `target_uniq_id`);

ALTER TABLE `projects_following_project_participant`  DROP INDEX `participant_id`;

ALTER TABLE `projects_issues`  DROP INDEX `id`;
ALTER TABLE `projects_issues`  CHANGE COLUMN `description` `description` TEXT NULL AFTER `title`;
ALTER TABLE `projects_issues`  CHANGE COLUMN `create_on` `create_on` DATETIME NOT NULL AFTER `description`;
ALTER TABLE `projects_issues`  CHANGE COLUMN `corrected_in_version` `corrected_in_version` VARCHAR(64) NULL DEFAULT NULL AFTER `detected_in_version`;

ALTER TABLE `projects_messages`  DROP INDEX `create_by`;
ALTER TABLE `projects_messages`  DROP INDEX `last_modified_by`;
ALTER TABLE `projects_messages`  ADD INDEX `tenant_id` (`tenant_id`);

ALTER TABLE `projects_milestones`  DROP INDEX `create_by`;
ALTER TABLE `projects_milestones`  DROP INDEX `last_modified_by`;
ALTER TABLE `projects_milestones`  ADD INDEX `tenant_id` (`tenant_id`);

ALTER TABLE `projects_projects`  ADD COLUMN `private` INT(10) NOT NULL DEFAULT '0' AFTER `tenant_id`;
ALTER TABLE `projects_projects`  DROP INDEX `ix_tenant`,  ADD INDEX `tenant_id` (`tenant_id`);

ALTER TABLE `projects_project_change_request`  CHANGE COLUMN `is_edit_request` `is_edit_request` INT(11) NULL DEFAULT NULL AFTER `project_status`;
ALTER TABLE `projects_project_change_request`  ADD COLUMN `template_id` INT(11) NOT NULL DEFAULT '0' AFTER `project_id`;
ALTER TABLE `projects_project_change_request`  ADD COLUMN `private` INT(10) NOT NULL DEFAULT '0' AFTER `description`;
ALTER TABLE `projects_project_change_request`  DROP INDEX `responsible_id`;
ALTER TABLE `projects_project_change_request`  DROP INDEX `create_by`;

ALTER TABLE `projects_project_participant`  DROP INDEX `project_id`;
ALTER TABLE `projects_project_participant`  ADD COLUMN `security` INT(10) NOT NULL DEFAULT '0' AFTER `participant_id`;

create table projects_project_tag_tmp (c1 int, c2 int);
insert into projects_project_tag_tmp select tag_id,project_id from projects_project_tag group by 1,2;
delete from projects_project_tag;
ALTER TABLE `projects_project_tag`  ADD PRIMARY KEY (`project_id`, `tag_id`);
insert into projects_project_tag select c1,c2 from projects_project_tag_tmp;
delete from projects_project_tag_tmp;

insert into projects_project_tag_tmp select tag_id,project_id from projects_project_tag_change_request group by 1,2;
delete from projects_project_tag_change_request;
ALTER TABLE `projects_project_tag_change_request`  DROP INDEX `project_id`;
ALTER TABLE `projects_project_tag_change_request`  DROP INDEX `tag_id`;
ALTER TABLE `projects_project_tag_change_request`  ADD PRIMARY KEY (`project_id`, `tag_id`);
insert into projects_project_tag_change_request select c1,c2 from projects_project_tag_tmp;
drop table projects_project_tag_tmp;

ALTER TABLE `projects_review_entity_info`  DROP INDEX `user_id`,  ADD INDEX `entity_uniqID` (`entity_uniqID`);

ALTER TABLE `projects_tags`  ADD INDEX `tenant_id` (`tenant_id`);

ALTER TABLE `projects_tasks`  DROP INDEX `create_by`;
ALTER TABLE `projects_tasks`  DROP INDEX `last_modified_by`;
ALTER TABLE `projects_tasks`  ADD INDEX `tenant_id` (`tenant_id`);
ALTER TABLE `projects_tasks`  ADD COLUMN `deadline` DATETIME NULL DEFAULT NULL AFTER `sort_order`;

CREATE TABLE `projects_template_message` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NOT NULL,
	`project_id` INT(11) NOT NULL,
	`text` MEDIUMTEXT NULL,
	`create_by` CHAR(38) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`),
	INDEX `FK_Project` (`project_id`)
);

CREATE TABLE `projects_template_milestone` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NOT NULL,
	`project_id` INT(11) NOT NULL,
	`duration` INT(11) NOT NULL,
	`flags` INT(11) NOT NULL DEFAULT '0',
	`create_by` CHAR(38) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`),
	INDEX `FK_Project` (`project_id`)
);
CREATE TABLE `projects_template_project` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NOT NULL,
	`description` TEXT NULL,
	`responsible` CHAR(38) NULL DEFAULT NULL,
	`tags` VARCHAR(1024) NULL DEFAULT NULL,
	`team` TEXT NULL,
	`create_by` CHAR(38) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`)
);

CREATE TABLE `projects_template_task` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NOT NULL,
	`project_id` INT(11) NOT NULL,
	`description` TEXT NULL,
	`milestone_id` INT(11) NOT NULL DEFAULT '0',
	`priority` INT(11) NOT NULL DEFAULT '0',
	`sort_order` INT(11) NOT NULL DEFAULT '0',
	`responsible` CHAR(38) NULL DEFAULT NULL,
	`create_by` CHAR(38) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`),
	INDEX `FK_Project` (`project_id`)
);

CREATE TABLE `projects_todo` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`list_id` INT(11) NOT NULL,
	`text` MEDIUMTEXT NULL,
	`from_time` DATETIME NOT NULL,
	`to_time` DATETIME NOT NULL,
	`executed` INT(11) NOT NULL DEFAULT '0',
	`sortorder` INT(11) NOT NULL DEFAULT '0',
	`balloon_x` INT(11) NOT NULL DEFAULT '0',
	`balloon_y` INT(11) NOT NULL DEFAULT '0',
	`balloon_color` VARCHAR(10) NULL DEFAULT NULL,
	`balloon_border_color` VARCHAR(10) NULL DEFAULT NULL,
	`modified_on` DATETIME NOT NULL,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`)
);

CREATE TABLE `projects_todolist` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NULL DEFAULT NULL,
	`create_by` VARCHAR(38) NOT NULL,
	`modified_on` DATETIME NOT NULL,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`)
);