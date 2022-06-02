ALTER TABLE `events_feed`  ADD INDEX `LastModified` (`Tenant` , `LastModified`);

ALTER TABLE `events_pollvariant`  ADD INDEX `Poll` (`Poll`);