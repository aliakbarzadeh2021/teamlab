ALTER TABLE `forum_post`  ADD INDEX `LastModified` (`TenantID` , `LastModified`);

ALTER TABLE `forum_question`  ADD INDEX `topic_id` (`TenantID`, `topic_id` );

ALTER TABLE `forum_topic`  ADD INDEX `LastModified` (`TenantID` , `LastModified`);