drop table core_acl;
alter table core_group rename to core_group_tmp;
alter table core_settings rename to core_settings_tmp;
alter table core_subscription rename to core_subscription_tmp;
alter table core_subscriptionmethod rename to core_subscriptionmethod_tmp;
alter table core_user rename to core_user_tmp;
alter table core_usergroup rename to core_usergroup_tmp;
alter table core_userphoto rename to core_userphoto_tmp;
alter table core_usersecurity rename to core_usersecurity_tmp;
drop table tenants_forbiden;
drop table tenants_owner;
drop table tenants_interim;
drop table tenants_quota;
alter table tenants_quotarow rename to tenants_quotarow_tmp;
drop table tenants_template_acl;
drop table tenants_template_subscription;
drop table tenants_template_subscriptionmethod;
alter table tenants_tenants rename to tenants_tenants_tmp;

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

INSERT INTO tenants_quota VALUES (-1, 26214400, 10737418240, 0, 1); 

-- templates
insert  into tenants_template_subscription(source, action, recipient) values ('6a598c74-91ae-437d-a5f4-ad339bd11bb2','new post','abef62db-11a8-4673-9d32-ef1d8af19dc0');
insert  into tenants_template_subscriptionmethod(source, action, recipient,sender) values('6a598c74-91ae-437d-a5f4-ad339bd11bb2','new post','abef62db-11a8-4673-9d32-ef1d8af19dc0','messanger.sender');
insert  into tenants_template_subscription(source, action, recipient) values ('6504977c-75af-4691-9099-084d3ddeea04','new feed','abef62db-11a8-4673-9d32-ef1d8af19dc0');
insert  into tenants_template_subscriptionmethod(source, action, recipient,sender) values('6504977c-75af-4691-9099-084d3ddeea04','new feed','abef62db-11a8-4673-9d32-ef1d8af19dc0','messanger.sender');
insert  into tenants_template_subscription(source, action, recipient) values('asc.web.studio','send_whats_new','abef62db-11a8-4673-9d32-ef1d8af19dc0');
insert  into tenants_template_subscriptionmethod(source, action, recipient,sender) values('asc.web.studio','send_whats_new','abef62db-11a8-4673-9d32-ef1d8af19dc0','email.sender');
insert  into tenants_template_subscription(source, action, recipient) values('asc.web.studio','admin_notify','cd84e66b-b803-40fc-99f9-b2969a54a1de');
insert  into tenants_template_subscriptionmethod(source, action, recipient,sender) values('asc.web.studio','admin_notify','cd84e66b-b803-40fc-99f9-b2969a54a1de','email.sender');

-- default permissions
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, '5d5b7260-f7f7-49f1-a1c9-95fbb6a12604', 'ef5e6790-f346-4b6e-b662-722bc28cb0db', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, '5d5b7260-f7f7-49f1-a1c9-95fbb6a12604', 'f11e8f3f-46e6-4e55-90e3-09c22ec565bd', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '088d5940-a80f-4403-9741-d610718ce95c', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '08d66144-e1c9-4065-9aa1-aa4bba0a7bc8', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '08d75c97-cf3f-494b-90d1-751c941fe2dd', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '0d1f72a8-63da-47ea-ae42-0900e4ac72a9', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '13e30b51-5b4d-40a5-8575-cb561899eeb1', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '19f658ae-722b-4cd8-8236-3ad150801d96', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '2c6552b3-b2e0-4a00-b8fd-13c161e337b1', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '388c29d3-c662-4a61-bf47-fc2f7094224a', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '40bf31f4-3132-4e76-8d5c-9828a89501a3', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '49ae8915-2b30-4348-ab74-b152279364fb', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '63e9f35f-6bb5-4fb1-afaa-e4c2f4dec9bd', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '722003c2-2a4c-47c5-aee4-98142f391329', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '9018c001-24c2-44bf-a1db-d1121a570e74', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '948ad738-434b-4a88-8e38-7569d332910a', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '9d75a568-52aa-49d8-ad43-473756cd8903', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'a362fe79-684e-4d43-a599-65bc1f4e167f', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'c426c349-9ad4-47cd-9b8f-99fc30675951', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'd11ebcb9-0e6e-45e6-a6d0-99c41d687598', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'd1f3b53d-d9e2-4259-80e7-d24380978395', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'd49f4e30-da10-4b39-bc6d-b41ef6e039d3', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'd852b66f-6719-45e1-8657-18f0bb791690', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'e0759a42-47f0-4763-a26a-d5aa665bec35', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'e37239bd-c5b5-4f1e-a9f8-3ceeac209615', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'fbc37705-a04c-40ad-a68c-ce2f0423f397', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'fcac42b8-9386-48eb-a938-d19b3c576912', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '13e30b51-5b4d-40a5-8575-cb561899eeb1', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '49ae8915-2b30-4348-ab74-b152279364fb', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '63e9f35f-6bb5-4fb1-afaa-e4c2f4dec9bd', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '9018c001-24c2-44bf-a1db-d1121a570e74', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'd1f3b53d-d9e2-4259-80e7-d24380978395', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'e0759a42-47f0-4763-a26a-d5aa665bec35', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'e37239bd-c5b5-4f1e-a9f8-3ceeac209615', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'f11e88d7-f185-4372-927c-d88008d2c483', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'f11e8f3f-46e6-4e55-90e3-09c22ec565bd', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '00e7dfc5-ac49-4fd3-a1d6-98d84e877ac4', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '14be970f-7af5-4590-8e81-ea32b5f7866d', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '18ecc94d-6afa-4994-8406-aee9dff12ce2', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '298530eb-435e-4dc6-a776-9abcd95c70e9', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '430eaf70-1886-483c-a746-1a18e3e6bb63', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '557d6503-633b-4490-a14c-6473147ce2b3', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '724cbb75-d1c9-451e-bae0-4de0db96b1f7', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '7cb5c0d1-d254-433f-abe3-ff23373ec631', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '91b29dcd-9430-4403-b17a-27d09189be88', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'a18480a4-6d18-4c71-84fa-789888791f45', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'b630d29b-1844-4bda-bbbe-cf5542df3559', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'c62a9e8d-b24c-4513-90aa-7ff0f8ba38eb', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'd7cdb020-288b-41e5-a857-597347618533', '', 0);
INSERT INTO core_acl (tenant, subject, action, object, acetype) VALUES (-1, '712d9ec3-5d2b-4b13-824f-71f00191dcca', 'e0759a42-47f0-4763-a26a-d5aa665bec35', '', 0);
insert into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '0000ffff-ae36-4d2e-818d-726cb650aeb7', 'asc.web.studio.core.tcpipfiltersecurityobject|0000:0000:0000:0000:0000:0000:0.0.0.0', 0);

-- restricted tenant names
INSERT INTO tenants_forbiden (address) VALUES ('about');
INSERT INTO tenants_forbiden (address) VALUES ('api');
INSERT INTO tenants_forbiden (address) VALUES ('asset');
INSERT INTO tenants_forbiden (address) VALUES ('audio');
INSERT INTO tenants_forbiden (address) VALUES ('aws');
INSERT INTO tenants_forbiden (address) VALUES ('blogs');
INSERT INTO tenants_forbiden (address) VALUES ('business');
INSERT INTO tenants_forbiden (address) VALUES ('buzz');
INSERT INTO tenants_forbiden (address) VALUES ('calendar');
INSERT INTO tenants_forbiden (address) VALUES ('client');
INSERT INTO tenants_forbiden (address) VALUES ('clients');
INSERT INTO tenants_forbiden (address) VALUES ('community');
INSERT INTO tenants_forbiden (address) VALUES ('data');
INSERT INTO tenants_forbiden (address) VALUES ('db');
INSERT INTO tenants_forbiden (address) VALUES ('dev');
INSERT INTO tenants_forbiden (address) VALUES ('developer');
INSERT INTO tenants_forbiden (address) VALUES ('developers');
INSERT INTO tenants_forbiden (address) VALUES ('doc');
INSERT INTO tenants_forbiden (address) VALUES ('docs');
INSERT INTO tenants_forbiden (address) VALUES ('download');
INSERT INTO tenants_forbiden (address) VALUES ('downloads');
INSERT INTO tenants_forbiden (address) VALUES ('e-mail');
INSERT INTO tenants_forbiden (address) VALUES ('feed');
INSERT INTO tenants_forbiden (address) VALUES ('feeds');
INSERT INTO tenants_forbiden (address) VALUES ('file');
INSERT INTO tenants_forbiden (address) VALUES ('files');
INSERT INTO tenants_forbiden (address) VALUES ('flash');
INSERT INTO tenants_forbiden (address) VALUES ('forum');
INSERT INTO tenants_forbiden (address) VALUES ('forumsforumblog');
INSERT INTO tenants_forbiden (address) VALUES ('help');
INSERT INTO tenants_forbiden (address) VALUES ('jabber');
INSERT INTO tenants_forbiden (address) VALUES ('localhost');
INSERT INTO tenants_forbiden (address) VALUES ('mail');
INSERT INTO tenants_forbiden (address) VALUES ('management');
INSERT INTO tenants_forbiden (address) VALUES ('manual');
INSERT INTO tenants_forbiden (address) VALUES ('media');
INSERT INTO tenants_forbiden (address) VALUES ('movie');
INSERT INTO tenants_forbiden (address) VALUES ('music');
INSERT INTO tenants_forbiden (address) VALUES ('my');
INSERT INTO tenants_forbiden (address) VALUES ('nct');
INSERT INTO tenants_forbiden (address) VALUES ('net');
INSERT INTO tenants_forbiden (address) VALUES ('network');
INSERT INTO tenants_forbiden (address) VALUES ('new');
INSERT INTO tenants_forbiden (address) VALUES ('news');
INSERT INTO tenants_forbiden (address) VALUES ('office');
INSERT INTO tenants_forbiden (address) VALUES ('online-help');
INSERT INTO tenants_forbiden (address) VALUES ('onlinehelp');
INSERT INTO tenants_forbiden (address) VALUES ('organizer');
INSERT INTO tenants_forbiden (address) VALUES ('plan');
INSERT INTO tenants_forbiden (address) VALUES ('plans');
INSERT INTO tenants_forbiden (address) VALUES ('press');
INSERT INTO tenants_forbiden (address) VALUES ('project');
INSERT INTO tenants_forbiden (address) VALUES ('projects');
INSERT INTO tenants_forbiden (address) VALUES ('radio');
INSERT INTO tenants_forbiden (address) VALUES ('reg');
INSERT INTO tenants_forbiden (address) VALUES ('registration');
INSERT INTO tenants_forbiden (address) VALUES ('rss');
INSERT INTO tenants_forbiden (address) VALUES ('security');
INSERT INTO tenants_forbiden (address) VALUES ('share');
INSERT INTO tenants_forbiden (address) VALUES ('source');
INSERT INTO tenants_forbiden (address) VALUES ('stat');
INSERT INTO tenants_forbiden (address) VALUES ('static');
INSERT INTO tenants_forbiden (address) VALUES ('stream');
INSERT INTO tenants_forbiden (address) VALUES ('support');
INSERT INTO tenants_forbiden (address) VALUES ('talk');
INSERT INTO tenants_forbiden (address) VALUES ('task');
INSERT INTO tenants_forbiden (address) VALUES ('tasks');
INSERT INTO tenants_forbiden (address) VALUES ('teamlab');
INSERT INTO tenants_forbiden (address) VALUES ('time');
INSERT INTO tenants_forbiden (address) VALUES ('tools');
INSERT INTO tenants_forbiden (address) VALUES ('user-manual');
INSERT INTO tenants_forbiden (address) VALUES ('usermanual');
INSERT INTO tenants_forbiden (address) VALUES ('video');
INSERT INTO tenants_forbiden (address) VALUES ('wave');
INSERT INTO tenants_forbiden (address) VALUES ('wiki');
INSERT INTO tenants_forbiden (address) VALUES ('wikis');

set @@session.sql_mode = concat(@@session.sql_mode, ',NO_AUTO_VALUE_ON_ZERO');

insert into core_group select tenant, id, name, categoryid, parentid, 0, utc_timestamp() from core_group_tmp;
insert into core_settings select -1, id, value from core_settings_tmp where id = 'SmtpSettings';
insert into core_subscription select tenant, source, action, recipient, coalesce(object, ''), unsubscribed from core_subscription_tmp group by 1,2,3,4,5;
insert into core_subscriptionmethod select tenant, source, action, recipient, sender from core_subscriptionmethod_tmp;
insert into core_user select tenant, id, username, firstname, lastname, sex, bithdate, status, 1, email, workfromdate, terminateddate, title, department, contacts, location, notes, 0, utc_timestamp() from core_user_tmp;
insert into core_usergroup select tenant, userid, groupid, 0, 0, utc_timestamp() from core_usergroup_tmp;
insert into core_usergroup select tenant, userid, groupid, 1, 0, utc_timestamp() from core_groupmanager;
insert into core_userphoto select tenant, userid, photo from core_userphoto_tmp;
insert into core_usersecurity select (select u.tenant from core_user u where u.id = userid), userid, pwdhash, pwdhashsha512 from core_usersecurity_tmp where userid in (select id from core_user_tmp);
insert into tenants_quotarow select tenant, path, counter, tag, utc_timestamp() from tenants_quotarow_tmp;
insert into tenants_tenants select id, name, alias, mappeddomain, language, timezone, trusteddomains, trusteddomainsenabled, status, statuschanged, creationdatetime, null, 0, null, utc_timestamp() from tenants_tenants_tmp;
update tenants_tenants set statuschanged = null where statuschanged = '0001-01-01 00:00:00';

drop table core_group_tmp;
drop table core_settings_tmp;
drop table core_subscription_tmp;
drop table core_subscriptionmethod_tmp;
drop table core_user_tmp;
drop table core_usergroup_tmp;
drop table core_groupmanager;
drop table core_userphoto_tmp;
drop table core_usersecurity_tmp;
drop table tenants_quotarow_tmp;
drop table tenants_tenants_tmp;