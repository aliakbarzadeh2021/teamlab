-- projects_comments
CREATE TABLE "projects_comments" (
  "id" char(38) NOT NULL DEFAULT '',
  "content" text,
  "inactive" INTEGER NOT NULL DEFAULT 0,
  "create_by" char(38) NOT NULL,
  "create_on" datetime NOT NULL,
  "parent_id" char(38) DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL,
  "target_uniq_id" varchar(50) NOT NULL,
  PRIMARY KEY ("id")
);
CREATE INDEX "projects_comments_target_uniq_id" ON "projects_comments" ("tenant_id","target_uniq_id");


-- projects_events
CREATE TABLE "projects_events" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "create_by" char(38) NOT NULL,
  "create_on" datetime NOT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL,
  "from_date" datetime NOT NULL,
  "to_date" datetime NOT NULL,
  "project_id" INTEGER NOT NULL,
  "tenant_id" INTEGER NOT NULL
);


-- projects_files
CREATE TABLE "projects_files" (
  "version" INTEGER NOT NULL,
  "id" INTEGER NOT NULL,
  "title" varchar(255) DEFAULT NULL,
  "create_by" char(38) NOT NULL,
  "content_type" varchar(255) DEFAULT NULL,
  "content_length" INTEGER DEFAULT NULL,
  "create_on" datetime NOT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL,
  "project_id" INTEGER NOT NULL,
  "target_type" varchar(255) DEFAULT NULL,
  "target_id" INTEGER DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL,
  "category_id" INTEGER DEFAULT NULL,
  PRIMARY KEY ("version","id")
);
CREATE INDEX "projects_files_last_modified_by" ON "projects_files" ("last_modified_by");
CREATE INDEX "projects_files_create_by" ON "projects_files" ("create_by");
CREATE INDEX "projects_files_target_id" ON "projects_files" ("target_id");
CREATE INDEX "projects_files_project_id" ON "projects_files" ("project_id");


-- projects_files_category
CREATE TABLE "projects_files_category" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "create_on" datetime DEFAULT NULL,
  "create_by" varchar(40) DEFAULT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" varchar(40) DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL,
  "project_id" INTEGER DEFAULT NULL
);


-- projects_following_project_participant
CREATE TABLE "projects_following_project_participant" (
  "project_id" INTEGER NOT NULL,
  "participant_id" char(38) NOT NULL,
  PRIMARY KEY ("participant_id","project_id")
);
CREATE INDEX "projects_following_project_participant_project_id" ON "projects_following_project_participant" ("project_id");


-- projects_issues
CREATE TABLE "projects_issues" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "issue_id" varchar(64) NOT NULL,
  "project_id" INTEGER NOT NULL,
  "title" varchar(255) NOT NULL,
  "description" text,
  "create_on" datetime NOT NULL,
  "create_by" varchar(38) NOT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" varchar(40) DEFAULT NULL,
  "detected_in_version" varchar(64) NOT NULL,
  "corrected_in_version" varchar(64) DEFAULT NULL,
  "priority" INTEGER NOT NULL,
  "assigned_on" varchar(38) NOT NULL,
  "status" INTEGER NOT NULL,
  "tenant_id" INTEGER NOT NULL
);


-- projects_messages
CREATE TABLE "projects_messages" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "create_by" char(38) NOT NULL,
  "create_on" datetime NOT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL,
  "content" TEXT,
  "project_id" INTEGER NOT NULL,
  "tenant_id" INTEGER NOT NULL
);
CREATE INDEX "projects_messages_tenant_id" ON "projects_messages" ("tenant_id");
CREATE INDEX "projects_messages_project_id" ON "projects_messages" ("project_id");


-- projects_milestones
CREATE TABLE "projects_milestones" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "deadline" datetime NOT NULL,
  "status" INTEGER NOT NULL,
  "create_by" char(38) DEFAULT NULL,
  "create_on" datetime DEFAULT NULL,
  "is_notify" INTEGER NOT NULL DEFAULT 0,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL,
  "project_id" INTEGER NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  "is_key" INTEGER DEFAULT 0
);
CREATE INDEX "projects_milestones_tenant_id" ON "projects_milestones" ("tenant_id");
CREATE INDEX "projects_milestones_project_id" ON "projects_milestones" ("project_id");


-- projects_project_change_request
CREATE TABLE "projects_project_change_request" (
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


-- projects_project_participant
CREATE TABLE "projects_project_participant" (
  "project_id" INTEGER NOT NULL,
  "participant_id" char(38) NOT NULL,
  "security" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("project_id","participant_id")
);
CREATE INDEX "projects_project_participant_participant_id" ON "projects_project_participant" ("participant_id");


-- projects_project_tag
CREATE TABLE "projects_project_tag" (
  "tag_id" INTEGER NOT NULL,
  "project_id" INTEGER NOT NULL,
  PRIMARY KEY ("project_id","tag_id")
);


-- projects_project_tag_change_request
CREATE TABLE "projects_project_tag_change_request" (
  "tag_id" INTEGER NOT NULL,
  "project_id" INTEGER NOT NULL,
  PRIMARY KEY ("project_id","tag_id")
);


-- projects_projects
CREATE TABLE "projects_projects" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "status" INTEGER NOT NULL,
  "title" varchar(255) DEFAULT NULL,
  "description" text,
  "responsible_id" char(38) NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  "private" INTEGER NOT NULL DEFAULT 0,
  "create_on" datetime DEFAULT NULL,
  "create_by" char(38) DEFAULT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL
);
CREATE INDEX "projects_projects_responsible_id" ON "projects_projects" ("responsible_id");
CREATE INDEX "projects_projects_tenant_id" ON "projects_projects" ("tenant_id");


-- projects_report_template
CREATE TABLE "projects_report_template" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "type" INTEGER NOT NULL,
  "name" varchar(1024) NOT NULL,
  "filter" text,
  "cron" varchar(255) DEFAULT NULL,
  "create_on" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "create_by" varchar(38) DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL,
  "auto" INTEGER NOT NULL DEFAULT 0
);


-- projects_review_entity_info
CREATE TABLE "projects_review_entity_info" (
  "user_id" varchar(40) NOT NULL,
  "entity_review" datetime DEFAULT NULL,
  "entity_uniqID" varchar(255) NOT NULL,
  "tenant_id" INTEGER NOT NULL,
  PRIMARY KEY ("user_id","entity_uniqID")
);
CREATE INDEX "projects_review_entity_info_entity_uniqID" ON "projects_review_entity_info" ("entity_uniqID");


-- projects_tags
CREATE TABLE "projects_tags" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "tenant_id" INTEGER DEFAULT NULL,
  "create_on" datetime DEFAULT NULL,
  "create_by" char(38) DEFAULT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL
);
CREATE INDEX "projects_tags_tenant_id" ON "projects_tags" ("tenant_id");


-- projects_tasks
CREATE TABLE "projects_tasks" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "description" text,
  "responsible_id" char(38) NOT NULL,
  "priority" INTEGER NOT NULL,
  "status" INTEGER NOT NULL,
  "create_by" char(38) NOT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL,
  "create_on" datetime DEFAULT NULL,
  "project_id" INTEGER NOT NULL,
  "milestone_id" INTEGER DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL,
  "sort_order" INTEGER NOT NULL DEFAULT 0,
  "deadline" datetime DEFAULT NULL
);
CREATE INDEX "projects_tasks_tenant_id" ON "projects_tasks" ("tenant_id");
CREATE INDEX "projects_tasks_responsible_id" ON "projects_tasks" ("responsible_id");
CREATE INDEX "projects_tasks_project_id" ON "projects_tasks" ("project_id");
CREATE INDEX "projects_tasks_milestone_id" ON "projects_tasks" ("milestone_id");


-- projects_tasks_trace
CREATE TABLE "projects_tasks_trace" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "task_id" INTEGER NOT NULL,
  "action_date" datetime NOT NULL,
  "action_owner_id" char(38) NOT NULL,
  "status" INTEGER NOT NULL,
  "tenant_id" INTEGER NOT NULL
);
CREATE INDEX "projects_tasks_trace_task_id" ON "projects_tasks_trace" ("task_id");
CREATE INDEX "projects_tasks_trace_action_owner_id" ON "projects_tasks_trace" ("action_owner_id");


-- projects_template_message
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


-- projects_template_milestone
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


-- projects_template_project
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


-- projects_template_task
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


-- projects_time_tracking
CREATE TABLE "projects_time_tracking" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "note" varchar(255) DEFAULT NULL,
  "date" datetime NOT NULL,
  "hours" float DEFAULT 0,
  "tenant_id" INTEGER NOT NULL,
  "relative_task_id" INTEGER DEFAULT NULL,
  "person_id" char(38) NOT NULL,
  "project_id" INTEGER NOT NULL
);
CREATE INDEX "projects_time_tracking_person_id" ON "projects_time_tracking" ("person_id");
CREATE INDEX "projects_time_tracking_project_id" ON "projects_time_tracking" ("project_id");
CREATE INDEX "projects_time_tracking_relative_task_id" ON "projects_time_tracking" ("relative_task_id");



