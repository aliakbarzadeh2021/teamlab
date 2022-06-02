update projects_files set category_id = 0 where category_id is null;
update projects_tasks set milestone_id = 0 where milestone_id is null;
update projects_time_tracking set relative_task_id = 0 where relative_task_id is null;

ALTER TABLE projects_tasks ADD COLUMN sort_order int not null default 0;

ALTER TABLE projects_comments ADD COLUMN target_uniq_id VARCHAR(50);
update projects_comments set target_uniq_id = REPLACE(target_type,'ASC.Projects.Core.Domain.','') || '_' || target_id;
ALTER TABLE projects_comments rename to projects_comments_tmp;
CREATE TABLE "projects_comments" (
  "id" char(38) NOT NULL,
  "content" text,
  "inactive" tinyint(1) NOT NULL DEFAULT 0,
  "create_by" char(38) NOT NULL,
  "create_on" datetime NOT NULL,
  "parent_id" char(38) DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL,
  "target_uniq_id" varchar(50) NOT NULL,
  PRIMARY KEY ("id")
);
insert into "projects_comments" select id, content, inactive, create_by, create_on, parent_id, tenant_id, target_uniq_id from projects_comments_tmp;
drop table projects_comments_tmp;

DROP TABLE projects_participants;

alter table projects_review_entity_info add column tenant_id integer not null default 0;

CREATE TABLE "projects_report_template" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "type" INTEGER NOT NULL,
  "name" varchar(1024) NOT NULL,
  "filter" text,
  "cron" varchar(255) DEFAULT NULL,
  "create_on" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "create_by" varchar(38) DEFAULT NULL,
  "tenant_id" int(10) NOT NULL,
  "auto" int(10) NOT NULL DEFAULT 0
);

drop table "projects_files_category";
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

drop table "projects_tags";
CREATE TABLE "projects_tags" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(255) DEFAULT NULL,
  "tenant_id" INTEGER DEFAULT NULL,
  "create_on" datetime DEFAULT NULL,
  "create_by" char(38) DEFAULT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" char(38) DEFAULT NULL
);

CREATE TABLE "projects_issues" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "issue_id" varchar(64) NOT NULL,
  "project_id" INTEGER NOT NULL,
  "title" varchar(255) NOT NULL,
  "create_by" varchar(38) NOT NULL,
  "last_modified_on" datetime DEFAULT NULL,
  "last_modified_by" varchar(40) DEFAULT NULL,
  "create_on" datetime NOT NULL,
  "detected_in_version" varchar(64) NOT NULL,
  "priority" INTEGER NOT NULL,
  "description" text,
  "assigned_on" varchar(38) NOT NULL,
  "status" INTEGER NOT NULL,
  "corrected_in_version" varchar(64) DEFAULT NULL,
  "tenant_id" INTEGER NOT NULL
);
CREATE UNIQUE INDEX "projects_issues_id" ON "projects_issues" ("id");
CREATE INDEX "projects_projects_ix_tenant" ON "projects_projects" ("tenant_id");
CREATE INDEX "projects_comments_parent_id" ON "projects_comments" ("parent_id");
CREATE INDEX "projects_comments_create_by" ON "projects_comments" ("create_by");
CREATE INDEX "projects_comments_ix_target_uniq_id" ON "projects_comments" ("target_uniq_id");
