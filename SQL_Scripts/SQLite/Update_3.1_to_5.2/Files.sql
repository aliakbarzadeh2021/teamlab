-- files_bunch_objects
CREATE INDEX "files_bunch_objects_left_node" ON "files_bunch_objects" ("left_node");


-- files_file
ALTER TABLE files_file ADD COLUMN "converted_type" varchar(10) DEFAULT NULL;
CREATE INDEX "files_file_modified_on" ON "files_file" ("modified_on");


-- -- files_security
CREATE TABLE "files_security_temp" (
  "tenant_id" INTEGER NOT NULL,
  "entry_id" INTEGER NOT NULL,
  "entry_type" INTEGER NOT NULL,
  "subject" char(38) NOT NULL,
  "owner" char(38) NOT NULL,
  "security" INTEGER NOT NULL,
  PRIMARY KEY ("tenant_id","entry_id","entry_type","subject")
);
INSERT INTO files_security_temp SELECT tenant_id, entry_id, entry_type, subject, owner, security FROM files_security;
DROP TABLE files_security;
ALTER TABLE files_security_temp RENAME TO files_security;
