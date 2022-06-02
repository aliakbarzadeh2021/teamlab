-- files_bunch_objects
CREATE TABLE "files_bunch_objects" (
  "left_node" varchar(255) NOT NULL,
  "right_node" varchar(255) NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  PRIMARY KEY ("right_node","tenant_id")
);
CREATE INDEX "files_bunch_objects_left_node" ON "files_bunch_objects" ("left_node");


-- files_file
CREATE TABLE "files_file" (
  "id" INTEGER NOT NULL,
  "version" INTEGER NOT NULL,
  "current_version" INTEGER NOT NULL DEFAULT 0,
  "folder_id" INTEGER NOT NULL DEFAULT 0,
  "title" varchar(400) NOT NULL,
  "content_type" varchar(255) DEFAULT NULL,
  "content_length" INTEGER NOT NULL DEFAULT 0,
  "file_status" INTEGER NOT NULL DEFAULT 0,
  "category" INTEGER NOT NULL DEFAULT 0,
  "create_by" char(38) NOT NULL,
  "create_on" datetime NOT NULL,
  "modified_by" char(38) NOT NULL,
  "modified_on" datetime NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  "converted_type" varchar(10) DEFAULT NULL,
  PRIMARY KEY ("tenant_id","id","version")
);
CREATE INDEX "files_file_folder_id" ON "files_file" ("folder_id","title");
CREATE INDEX "files_file_modified_on" ON "files_file" ("modified_on");


-- files_folder
CREATE TABLE "files_folder" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "parent_id" INTEGER NOT NULL DEFAULT 0,
  "title" varchar(400) NOT NULL,
  "folder_type" INTEGER NOT NULL DEFAULT 0,
  "create_by" char(38) NOT NULL,
  "create_on" datetime NOT NULL,
  "modified_by" char(38) NOT NULL,
  "modified_on" datetime NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  "foldersCount" INTEGER NOT NULL DEFAULT 0,
  "filesCount" INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX "files_folder_parent_id" ON "files_folder" ("parent_id","title");


-- files_folder_tree
CREATE TABLE "files_folder_tree" (
  "folder_id" INTEGER NOT NULL,
  "parent_id" INTEGER NOT NULL,
  "level" INTEGER NOT NULL,
  PRIMARY KEY ("parent_id","folder_id")
);
CREATE INDEX "files_folder_tree_folder_id" ON "files_folder_tree" ("folder_id");


-- files_security
CREATE TABLE "files_security" (
  "tenant_id" INTEGER NOT NULL,
  "entry_id" INTEGER NOT NULL,
  "entry_type" INTEGER NOT NULL,
  "subject" char(38) NOT NULL,
  "owner" char(38) NOT NULL,
  "security" INTEGER NOT NULL,
  PRIMARY KEY ("tenant_id","entry_id","entry_type","subject")
);


-- files_tag
CREATE TABLE "files_tag" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" varchar(255) NOT NULL,
  "owner" varchar(38) NOT NULL,
  "flag" INTEGER NOT NULL DEFAULT 0,
  "tenant_id" INTEGER NOT NULL
);
CREATE INDEX "files_tag_name" ON "files_tag" ("owner","name","flag");


-- files_tag_link
CREATE TABLE "files_tag_link" (
  "tag_id" INTEGER NOT NULL,
  "entry_id" INTEGER NOT NULL,
  "entry_type" INTEGER NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  PRIMARY KEY ("tenant_id","tag_id","entry_id","entry_type")
);



