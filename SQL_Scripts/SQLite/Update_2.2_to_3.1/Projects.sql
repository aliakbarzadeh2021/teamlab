DROP INDEX projects_comments_create_by;
DROP INDEX projects_comments_parent_id;
DROP INDEX projects_comments_ix_target_uniq_id;  
CREATE INDEX projects_comments_target_uniq_id ON projects_comments ("tenant_id", "target_uniq_id");

DROP INDEX projects_following_project_participant_participant_id;

DROP INDEX projects_messages_create_by;
DROP INDEX projects_messages_last_modified_by;
CREATE INDEX projects_messages_tenant_id ON projects_messages ("tenant_id");

DROP INDEX projects_milestones_create_by;
DROP INDEX projects_milestones_last_modified_by;
CREATE INDEX projects_milestones_tenant_id ON projects_milestones ("tenant_id");

ALTER TABLE projects_projects  ADD COLUMN private INTEGER NOT NULL DEFAULT 0;
DROP INDEX projects_projects_ix_tenant;
CREATE INDEX projects_projects_tenant_id ON projects_projects ("tenant_id");

CREATE TABLE "projects_project_change_request_temp" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "project_status" INTEGER DEFAULT NULL,
  "is_edit_request" INTEGER DEFAULT NULL,
  "project_id" INTEGER DEFAULT NULL,
  "template_id" INTEGER NOT NULL DEFAULT 0,
  "title" varchar(255) DEFAULT NULL,
  "description" text,
  "private" INTEGER NOT NULL DEFAULT 0,
  "responsible_id" char(38) NOT NULL,
  "create_by" char(38) NOT NULL,
  "create_on" datetime DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL
);
INSERT INTO projects_project_change_request_temp (id, project_status, is_edit_request, project_id, title, description, responsible_id, create_by, create_on, tenant_id) 
			SELECT id, project_status, is_edit_request, project_id, title, description, responsible_id, create_by, create_on, tenant_id FROM projects_project_change_request;
DROP INDEX projects_project_change_request_responsible_id;
DROP INDEX projects_project_change_request_create_by;
DROP TABLE projects_project_change_request;
ALTER TABLE projects_project_change_request_temp RENAME TO projects_project_change_request;

DROP INDEX projects_project_participant_project_id;
ALTER TABLE projects_project_participant  ADD COLUMN security INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "projects_project_tag_temp" (
  "tag_id" INTEGER NOT NULL,
  "project_id" INTEGER NOT NULL,
  PRIMARY KEY ("project_id","tag_id")
);
INSERT INTO projects_project_tag_temp SELECT tag_id,project_id FROM projects_project_tag group by 1,2;
DROP TABLE projects_project_tag;
ALTER TABLE projects_project_tag_temp RENAME TO projects_project_tag;

CREATE TABLE "projects_project_tag_change_request_temp" (
  "tag_id" INTEGER NOT NULL,
  "project_id" INTEGER NOT NULL,
  PRIMARY KEY ("project_id","tag_id")
);
INSERT INTO projects_project_tag_change_request_temp SELECT tag_id,project_id FROM projects_project_tag_change_request group by 1,2;
DROP TABLE projects_project_tag_change_request;
ALTER TABLE projects_project_tag_change_request_temp RENAME TO projects_project_tag_change_request;

DROP INDEX projects_review_entity_info_user_id;
CREATE INDEX projects_review_entity_info_entity_uniqID ON projects_review_entity_info ("entity_uniqID");

CREATE INDEX projects_tags_tenant_id ON projects_tags ("tenant_id");

DROP INDEX projects_tasks_create_by;
DROP INDEX projects_tasks_last_modified_by;
CREATE INDEX projects_tasks_tenant_id ON projects_tasks ("tenant_id");
ALTER TABLE projects_tasks  ADD COLUMN deadline DATETIME NULL DEFAULT NULL;

CREATE TABLE "projects_template_message" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) NOT NULL,
  "project_id" INTEGER NOT NULL,
  "text" TEXT,
  "create_by" char(38) NOT NULL,
  "create_on" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "tenant_id" INTEGER NOT NULL
);
CREATE INDEX "projects_template_message_FK_Project" ON "projects_template_message" ("project_id");

CREATE TABLE "projects_template_milestone" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) NOT NULL,
  "project_id" INTEGER NOT NULL,
  "duration" INTEGER NOT NULL,
  "flags" INTEGER NOT NULL DEFAULT 0,
  "create_by" char(38) NOT NULL,
  "create_on" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "tenant_id" INTEGER NOT NULL
);
CREATE INDEX "projects_template_milestone_FK_Project" ON "projects_template_milestone" ("project_id");

CREATE TABLE "projects_template_project" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) NOT NULL,
  "description" text,
  "responsible" char(38) DEFAULT NULL,
  "tags" varchar(1024) DEFAULT NULL,
  "team" text,
  "create_by" char(38) NOT NULL,
  "create_on" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "tenant_id" INTEGER NOT NULL
);

CREATE TABLE "projects_template_task" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) NOT NULL,
  "project_id" INTEGER NOT NULL,
  "description" text,
  "milestone_id" INTEGER NOT NULL DEFAULT 0,
  "priority" INTEGER NOT NULL DEFAULT 0,
  "sort_order" INTEGER NOT NULL DEFAULT 0,
  "responsible" char(38) DEFAULT NULL,
  "create_by" char(38) NOT NULL,
  "create_on" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "tenant_id" INTEGER NOT NULL
);
CREATE INDEX "projects_template_task_FK_Project" ON "projects_template_task" ("project_id");

CREATE TABLE "projects_todo" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "list_id" INTEGER NOT NULL,
  "text" TEXT,
  "from_time" datetime NOT NULL,
  "to_time" datetime NOT NULL,
  "executed" INTEGER NOT NULL DEFAULT 0,
  "sortorder" INTEGER NOT NULL DEFAULT 0,
  "balloon_x" INTEGER NOT NULL DEFAULT 0,
  "balloon_y" INTEGER NOT NULL DEFAULT 0,
  "balloon_color" varchar(10) DEFAULT NULL,
  "balloon_border_color" varchar(10) DEFAULT NULL,
  "modified_on" datetime NOT NULL,
  "tenant_id" INTEGER NOT NULL
);

CREATE TABLE "projects_todolist" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "create_by" varchar(38) NOT NULL,
  "modified_on" datetime NOT NULL,
  "tenant_id" INTEGER NOT NULL
);