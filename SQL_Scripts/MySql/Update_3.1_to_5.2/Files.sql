-- files_bunch_objects
ALTER TABLE `files_bunch_objects`
	ADD INDEX `left_node` (`left_node`);

-- files_file
ALTER TABLE `files_file`
	ADD COLUMN `converted_type` varchar(10) DEFAULT NULL,
	ADD INDEX `modified_on` (`modified_on`);

-- files_security
ALTER TABLE `files_security`
	CHANGE COLUMN `tenant_id` `tenant_id` int(10) NOT NULL,
	DROP PRIMARY KEY,
	ADD PRIMARY KEY (`tenant_id`,`entry_id`,`entry_type`,`subject`);

