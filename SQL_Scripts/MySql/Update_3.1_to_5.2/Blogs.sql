-- blogs_blogs
ALTER TABLE `blogs_blogs`
	CHANGE COLUMN `Tenant` `Tenant` int(11) NOT NULL AFTER `group_id`;

-- blogs_comments
ALTER TABLE `blogs_comments`
	CHANGE COLUMN `Tenant` `Tenant` int(11) NOT NULL AFTER `inactive`;

-- blogs_tags
ALTER TABLE `blogs_tags` RENAME TO `blogs_tags_temp`;
CREATE TABLE `blogs_tags` (
	`post_id` VARCHAR(38) NOT NULL,
	`name` VARCHAR(255) NOT NULL,
	`Tenant` INT(11) NOT NULL,
	PRIMARY KEY (`Tenant`, `post_id`, `name`),
	INDEX `name` (`name`)
);
INSERT INTO blogs_tags SELECT post_id, name, Tenant FROM blogs_tags_temp GROUP BY 1,2,3;
DROP TABLE blogs_tags_temp;