-- core_acl
CREATE TABLE `core_acl` (
  `tenant` int(11) NOT NULL,
  `subject` varchar(38) NOT NULL,
  `action` varchar(38) NOT NULL,
  `object` varchar(255) NOT NULL DEFAULT '',
  `acetype` int(11) NOT NULL,
  PRIMARY KEY (`tenant`,`subject`,`action`,`object`)
);
-- core_group
CREATE TABLE `core_group` (
  `tenant` int(11) NOT NULL,
  `id` varchar(38) NOT NULL,
  `name` varchar(128) NOT NULL,
  `categoryid` varchar(38) DEFAULT NULL,
  `parentid` varchar(38) DEFAULT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `parentid` (`tenant`,`parentid`),
  KEY `last_modified` (`last_modified`)
);
-- core_settings
CREATE TABLE `core_settings` (
  `tenant` int(11) NOT NULL,
  `id` varchar(128) NOT NULL,
  `value` mediumblob NOT NULL,
  PRIMARY KEY (`tenant`,`id`)
);
-- core_subscription
CREATE TABLE `core_subscription` (
  `tenant` int(11) NOT NULL,
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `object` varchar(128) NOT NULL,
  `unsubscribed` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`,`source`,`action`,`recipient`,`object`)
);
-- core_subscriptionmethod
CREATE TABLE `core_subscriptionmethod` (
  `tenant` int(11) NOT NULL,
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `sender` varchar(1024) NOT NULL,
  PRIMARY KEY (`tenant`,`source`,`action`,`recipient`)
);
-- core_user
CREATE TABLE `core_user` (
  `tenant` int(11) NOT NULL,
  `id` varchar(38) NOT NULL,
  `username` varchar(64) NOT NULL,
  `firstname` varchar(64) NOT NULL,
  `lastname` varchar(64) NOT NULL,
  `sex` int(11) DEFAULT NULL,
  `bithdate` datetime DEFAULT NULL,
  `status` int(11) NOT NULL DEFAULT '1',
  `activation_status` int(11) NOT NULL DEFAULT '0',
  `email` varchar(64) DEFAULT NULL,
  `workfromdate` datetime DEFAULT NULL,
  `terminateddate` datetime DEFAULT NULL,
  `title` varchar(64) DEFAULT NULL,
  `department` varchar(64) DEFAULT NULL,
  `contacts` varchar(1024) DEFAULT NULL,
  `location` varchar(255) DEFAULT NULL,
  `notes` varchar(512) DEFAULT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `last_modified` (`last_modified`),
  KEY `username` (`tenant`,`username`),
  KEY `email` (`email`)
);
-- core_usergroup
CREATE TABLE `core_usergroup` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `groupid` varchar(38) NOT NULL,
  `ref_type` int(11) NOT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`userid`,`groupid`,`ref_type`),
  KEY `last_modified` (`last_modified`)
);
-- core_userphoto
CREATE TABLE `core_userphoto` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `photo` mediumblob,
  PRIMARY KEY (`userid`)
);
-- core_usersecurity
CREATE TABLE `core_usersecurity` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `pwdhash` varchar(512) DEFAULT NULL,
  `pwdhashsha512` varchar(512) DEFAULT NULL,
  PRIMARY KEY (`userid`),
  KEY `pwdhash` (`pwdhash`(255))
);
-- tenants_forbiden
CREATE TABLE `tenants_forbiden` (
  `address` varchar(50) NOT NULL,
  PRIMARY KEY (`address`)
);
-- tenants_quota
CREATE TABLE `tenants_quota` (
  `tenant` int(10) NOT NULL,
  `max_file_size` bigint(20) NOT NULL DEFAULT '0',
  `max_total_size` bigint(20) NOT NULL DEFAULT '0',
  `https_enable` int(11) NOT NULL DEFAULT '0',
  `security_enable` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`)
);
-- tenants_quotarow
CREATE TABLE `tenants_quotarow` (
  `tenant` int(11) NOT NULL,
  `path` varchar(255) NOT NULL,
  `counter` bigint(20) NOT NULL DEFAULT '0',
  `tag` varchar(1024) DEFAULT NULL,
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`path`),
  KEY `last_modified` (`last_modified`)
);
-- tenants_tariff
CREATE TABLE `tenants_tariff` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `tariff` int(11) NOT NULL,
  `stamp` datetime NOT NULL,
  `comment` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant` (`tenant`)
);
-- tenants_template_subscription
CREATE TABLE `tenants_template_subscription` (
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `object` varchar(128) NOT NULL DEFAULT '',
  `unsubscribed` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`source`,`action`,`recipient`,`object`)
);
-- tenants_template_subscriptionmethod
CREATE TABLE `tenants_template_subscriptionmethod` (
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `sender` varchar(1024) NOT NULL,
  PRIMARY KEY (`source`,`action`,`recipient`)
);
-- tenants_tenants
CREATE TABLE `tenants_tenants` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `alias` varchar(50) NOT NULL,
  `mappeddomain` varchar(50) DEFAULT NULL,
  `language` char(7) NOT NULL DEFAULT 'en-US',
  `timezone` varchar(50) DEFAULT NULL,
  `trusteddomains` varchar(1024) DEFAULT NULL,
  `trusteddomainsenabled` int(10) NOT NULL DEFAULT '1',
  `status` int(11) NOT NULL DEFAULT '0',
  `statuschanged` datetime DEFAULT NULL,
  `creationdatetime` datetime NOT NULL,
  `owner_id` varchar(38) DEFAULT NULL,
  `public` int(10) NOT NULL DEFAULT '0',
  `publicvisibleproducts` varchar(1024) DEFAULT NULL,
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `alias` (`alias`),
  KEY `last_modified` (`last_modified`),
  KEY `mappeddomain` (`mappeddomain`)
);

