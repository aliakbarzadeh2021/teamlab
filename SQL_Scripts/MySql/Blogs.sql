-- blogs_blogs
CREATE TABLE `blogs_blogs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `user_id` char(38) NOT NULL,
  `group_id` char(38) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `ixBlogs_Tenant` (`Tenant`,`id`)
);
-- blogs_comments
CREATE TABLE `blogs_comments` (
  `id` char(38) NOT NULL,
  `post_id` char(38) NOT NULL,
  `content` text,
  `created_by` char(38) NOT NULL,
  `created_when` datetime NOT NULL,
  `parent_id` char(38) DEFAULT NULL,
  `inactive` int(11) DEFAULT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`id`),
  KEY `ixComments_Created` (`Tenant`,`created_when`),
  KEY `ixComments_PostId` (`Tenant`,`post_id`)
);
-- blogs_posts
CREATE TABLE `blogs_posts` (
  `id` char(38) NOT NULL,
  `title` varchar(255) NOT NULL,
  `content` mediumtext NOT NULL,
  `created_by` char(38) NOT NULL,
  `created_when` datetime NOT NULL,
  `blog_id` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  `LastCommentId` char(38) DEFAULT NULL,
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Tenant`,`id`),
  KEY `ixPosts_CreatedBy` (`Tenant`,`created_by`),
  KEY `ixPosts_CreatedWhen` (`Tenant`,`created_when`),
  KEY `ixPosts_LastCommentId` (`Tenant`,`LastCommentId`)
);
-- blogs_reviewposts
CREATE TABLE `blogs_reviewposts` (
  `post_id` char(38) NOT NULL,
  `reviewed_by` char(38) NOT NULL,
  `timestamp` datetime NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Tenant`,`post_id`,`reviewed_by`)
);
-- blogs_tags
CREATE TABLE `blogs_tags` (
  `post_id` varchar(38) NOT NULL,
  `name` varchar(255) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`post_id`,`name`),
  KEY `name` (`name`)
);

