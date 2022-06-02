-- projects_comments
CREATE TABLE `projects_comments` (
  `id` char(38) NOT NULL DEFAULT '',
  `content` text,
  `inactive` tinyint(1) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `parent_id` char(38) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `target_uniq_id` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `target_uniq_id` (`tenant_id`,`target_uniq_id`)
);
-- projects_events
CREATE TABLE `projects_events` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `from_date` datetime NOT NULL,
  `to_date` datetime NOT NULL,
  `project_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
);
-- projects_files
CREATE TABLE `projects_files` (
  `version` int(11) NOT NULL,
  `id` int(11) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `create_by` char(38) NOT NULL,
  `content_type` varchar(255) DEFAULT NULL,
  `content_length` bigint(20) DEFAULT NULL,
  `create_on` datetime NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `project_id` int(11) NOT NULL,
  `target_type` varchar(255) DEFAULT NULL,
  `target_id` int(11) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `category_id` int(10) DEFAULT NULL,
  PRIMARY KEY (`version`,`id`),
  KEY `last_modified_by` (`last_modified_by`),
  KEY `create_by` (`create_by`),
  KEY `target_id` (`target_id`),
  KEY `project_id` (`project_id`)
);
-- projects_files_category
CREATE TABLE `projects_files_category` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `create_by` varchar(40) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` varchar(40) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `project_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
);
-- projects_following_project_participant
CREATE TABLE `projects_following_project_participant` (
  `project_id` int(11) NOT NULL,
  `participant_id` char(38) NOT NULL,
  PRIMARY KEY (`participant_id`,`project_id`),
  KEY `project_id` (`project_id`)
);
-- projects_issues
CREATE TABLE `projects_issues` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `issue_id` varchar(64) NOT NULL,
  `project_id` int(11) NOT NULL,
  `title` varchar(255) NOT NULL,
  `description` text,
  `create_on` datetime NOT NULL,
  `create_by` varchar(38) NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` varchar(40) DEFAULT NULL,
  `detected_in_version` varchar(64) NOT NULL,
  `corrected_in_version` varchar(64) DEFAULT NULL,
  `priority` int(11) NOT NULL,
  `assigned_on` varchar(38) NOT NULL,
  `status` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
);
-- projects_messages
CREATE TABLE `projects_messages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `content` mediumtext,
  `project_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `project_id` (`project_id`)
);
-- projects_milestones
CREATE TABLE `projects_milestones` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `deadline` datetime NOT NULL,
  `status` int(11) NOT NULL,
  `create_by` char(38) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `is_notify` tinyint(1) NOT NULL DEFAULT '0',
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `project_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `is_key` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `project_id` (`project_id`)
);
-- projects_project_change_request
CREATE TABLE `projects_project_change_request` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `project_status` int(11) DEFAULT NULL,
  `is_edit_request` int(11) DEFAULT NULL,
  `project_id` int(11) DEFAULT NULL,
  `template_id` int(11) NOT NULL DEFAULT '0',
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `private` int(10) NOT NULL DEFAULT '0',
  `responsible_id` char(38) NOT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
);
-- projects_project_participant
CREATE TABLE `projects_project_participant` (
  `project_id` int(11) NOT NULL,
  `participant_id` char(38) NOT NULL,
  `security` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`project_id`,`participant_id`),
  KEY `participant_id` (`participant_id`)
);
-- projects_project_tag
CREATE TABLE `projects_project_tag` (
  `tag_id` int(11) NOT NULL,
  `project_id` int(11) NOT NULL,
  PRIMARY KEY (`project_id`,`tag_id`)
);
-- projects_project_tag_change_request
CREATE TABLE `projects_project_tag_change_request` (
  `tag_id` int(11) NOT NULL,
  `project_id` int(11) NOT NULL,
  PRIMARY KEY (`project_id`,`tag_id`)
);
-- projects_projects
CREATE TABLE `projects_projects` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `status` int(11) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `responsible_id` char(38) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `private` int(10) NOT NULL DEFAULT '0',
  `create_on` datetime DEFAULT NULL,
  `create_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `responsible_id` (`responsible_id`),
  KEY `tenant_id` (`tenant_id`)
);
-- projects_report_template
CREATE TABLE `projects_report_template` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `type` int(11) NOT NULL,
  `name` varchar(1024) NOT NULL,
  `filter` text,
  `cron` varchar(255) DEFAULT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `create_by` varchar(38) DEFAULT NULL,
  `tenant_id` int(10) NOT NULL,
  `auto` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
);
-- projects_review_entity_info
CREATE TABLE `projects_review_entity_info` (
  `user_id` varchar(40) NOT NULL,
  `entity_review` datetime DEFAULT NULL,
  `entity_uniqID` varchar(255) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`user_id`,`entity_uniqID`),
  KEY `entity_uniqID` (`entity_uniqID`)
);
-- projects_tags
CREATE TABLE `projects_tags` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `tenant_id` int(11) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `create_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
);
-- projects_tasks
CREATE TABLE `projects_tasks` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `responsible_id` char(38) NOT NULL,
  `priority` int(11) NOT NULL,
  `status` int(11) NOT NULL,
  `create_by` char(38) NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `project_id` int(11) NOT NULL,
  `milestone_id` int(11) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `deadline` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `responsible_id` (`responsible_id`),
  KEY `project_id` (`project_id`),
  KEY `milestone_id` (`milestone_id`)
);
-- projects_tasks_trace
CREATE TABLE `projects_tasks_trace` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `task_id` int(11) NOT NULL,
  `action_date` datetime NOT NULL,
  `action_owner_id` char(38) NOT NULL,
  `status` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `task_id` (`task_id`),
  KEY `action_owner_id` (`action_owner_id`)
);
-- projects_template_message
CREATE TABLE `projects_template_message` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `project_id` int(11) NOT NULL,
  `text` mediumtext,
  `create_by` char(38) NOT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_Project` (`project_id`)
);
-- projects_template_milestone
CREATE TABLE `projects_template_milestone` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `project_id` int(11) NOT NULL,
  `duration` int(11) NOT NULL,
  `flags` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_Project` (`project_id`)
);
-- projects_template_project
CREATE TABLE `projects_template_project` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `description` text,
  `responsible` char(38) DEFAULT NULL,
  `tags` varchar(1024) DEFAULT NULL,
  `team` text,
  `create_by` char(38) NOT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
);
-- projects_template_task
CREATE TABLE `projects_template_task` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `project_id` int(11) NOT NULL,
  `description` text,
  `milestone_id` int(11) NOT NULL DEFAULT '0',
  `priority` int(11) NOT NULL DEFAULT '0',
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `responsible` char(38) DEFAULT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_Project` (`project_id`)
);
-- projects_time_tracking
CREATE TABLE `projects_time_tracking` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `note` varchar(255) DEFAULT NULL,
  `date` datetime NOT NULL,
  `hours` float DEFAULT '0',
  `tenant_id` int(11) NOT NULL,
  `relative_task_id` int(11) DEFAULT NULL,
  `person_id` char(38) NOT NULL,
  `project_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `person_id` (`person_id`),
  KEY `project_id` (`project_id`),
  KEY `relative_task_id` (`relative_task_id`)
);

