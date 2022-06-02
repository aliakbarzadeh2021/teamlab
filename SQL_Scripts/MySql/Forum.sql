-- forum_answer
CREATE TABLE `forum_answer` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `question_id` int(11) NOT NULL,
  `create_date` datetime DEFAULT NULL,
  `user_id` char(38) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
);
-- forum_answer_variant
CREATE TABLE `forum_answer_variant` (
  `answer_id` int(11) NOT NULL,
  `variant_id` int(11) NOT NULL,
  PRIMARY KEY (`answer_id`,`variant_id`)
);
-- forum_attachment
CREATE TABLE `forum_attachment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(500) NOT NULL,
  `post_id` int(11) NOT NULL,
  `size` int(11) NOT NULL DEFAULT '0',
  `download_count` int(11) NOT NULL DEFAULT '0',
  `content_type` int(11) NOT NULL DEFAULT '0',
  `mime_content_type` varchar(100) DEFAULT NULL,
  `create_date` datetime DEFAULT NULL,
  `path` varchar(1000) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
);
-- forum_category
CREATE TABLE `forum_category` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(500) NOT NULL,
  `description` varchar(500) NOT NULL DEFAULT '',
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `create_date` datetime NOT NULL,
  `poster_id` char(38) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
);
-- forum_lastvisit
CREATE TABLE `forum_lastvisit` (
  `tenantid` int(11) NOT NULL,
  `user_id` char(38) NOT NULL,
  `thread_id` int(11) NOT NULL,
  `last_visit` datetime NOT NULL,
  PRIMARY KEY (`user_id`,`thread_id`)
);
-- forum_post
CREATE TABLE `forum_post` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `topic_id` int(11) NOT NULL,
  `poster_id` char(38) NOT NULL,
  `create_date` datetime NOT NULL,
  `subject` varchar(500) NOT NULL DEFAULT '',
  `text` mediumtext NOT NULL,
  `edit_date` datetime DEFAULT NULL,
  `edit_count` int(11) NOT NULL DEFAULT '0',
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `parent_post_id` int(11) NOT NULL DEFAULT '0',
  `formatter` int(11) NOT NULL DEFAULT '0',
  `editor_id` char(38) DEFAULT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `LastModified` (`TenantID`,`LastModified`)
);
-- forum_question
CREATE TABLE `forum_question` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `topic_id` int(11) NOT NULL,
  `type` int(11) NOT NULL DEFAULT '0',
  `name` varchar(500) NOT NULL,
  `create_date` datetime NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `topic_id` (`TenantID`,`topic_id`)
);
-- forum_tag
CREATE TABLE `forum_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
);
-- forum_thread
CREATE TABLE `forum_thread` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(500) NOT NULL,
  `description` varchar(500) NOT NULL DEFAULT '',
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `category_id` int(11) NOT NULL,
  `topic_count` int(11) NOT NULL DEFAULT '0',
  `post_count` int(11) NOT NULL DEFAULT '0',
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  `recent_post_id` int(11) NOT NULL DEFAULT '0',
  `recent_topic_id` int(11) NOT NULL DEFAULT '0',
  `recent_post_date` datetime DEFAULT NULL,
  `recent_poster_id` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`)
);
-- forum_topic
CREATE TABLE `forum_topic` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `thread_id` int(11) NOT NULL,
  `title` varchar(500) NOT NULL,
  `type` int(11) NOT NULL DEFAULT '0',
  `create_date` datetime NOT NULL,
  `view_count` int(11) NOT NULL DEFAULT '0',
  `post_count` int(11) NOT NULL DEFAULT '0',
  `recent_post_id` int(11) NOT NULL DEFAULT '0',
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `poster_id` char(38) DEFAULT NULL,
  `sticky` int(11) NOT NULL DEFAULT '0',
  `closed` int(11) DEFAULT '0',
  `question_id` varchar(45) DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `LastModified` (`TenantID`,`LastModified`)
);
-- forum_topic_tag
CREATE TABLE `forum_topic_tag` (
  `topic_id` int(11) NOT NULL,
  `tag_id` int(11) NOT NULL,
  PRIMARY KEY (`topic_id`,`tag_id`)
);
-- forum_topicwatch
CREATE TABLE `forum_topicwatch` (
  `TenantID` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `TopicID` int(11) NOT NULL,
  `ThreadID` int(11) NOT NULL,
  PRIMARY KEY (`TenantID`,`UserID`,`TopicID`)
);
-- forum_variant
CREATE TABLE `forum_variant` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `question_id` int(11) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
);

