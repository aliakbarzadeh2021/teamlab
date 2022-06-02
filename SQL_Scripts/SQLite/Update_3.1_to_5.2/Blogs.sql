-- blogs_blogs
CREATE TABLE "blogs_blogs_temp" (
	"id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"name" varchar(255) DEFAULT NULL,
	"user_id" char(38) NOT NULL,
	"group_id" char(38) NOT NULL,
	"Tenant" INTEGER NOT NULL
);
INSERT INTO blogs_blogs_temp SELECT id, name, user_id, group_id, Tenant FROM blogs_blogs;
DROP TABLE blogs_blogs;
ALTER TABLE blogs_blogs_temp RENAME TO blogs_blogs;
CREATE INDEX "blogs_blogs_ixBlogs_Tenant" ON "blogs_blogs" ("Tenant", "id");


-- blogs_comments
CREATE TABLE "blogs_comments_temp" (
	"id" char(38) NOT NULL,
	"post_id" char(38) NOT NULL,
	"content" text,
	"created_by" char(38) NOT NULL,
	"created_when" datetime NOT NULL,
	"parent_id" char(38) DEFAULT NULL,
	"inactive" INTEGER DEFAULT NULL,
	"Tenant" INTEGER NOT NULL,
	PRIMARY KEY ("Tenant","id")
);
INSERT INTO blogs_comments_temp SELECT id, post_id, content, created_by, created_when, parent_id, inactive, Tenant FROM blogs_comments;
DROP TABLE blogs_comments;
ALTER TABLE blogs_comments_temp RENAME TO blogs_comments;
CREATE INDEX "blogs_comments_ixComments_Created" ON "blogs_comments" ("Tenant", "created_when");
CREATE INDEX "blogs_comments_ixComments_PostId" ON "blogs_comments" ("Tenant", "post_id");


-- blogs_tags
DROP INDEX "blogs_tags_ixTags_PostId";
DROP INDEX "blogs_tags_ixTags_Name";
CREATE TABLE "blogs_tags_temp" (
	"post_id" varchar(38) NOT NULL,
	"name" varchar(255) NOT NULL,
	"Tenant" INTEGER NOT NULL DEFAULT 0,
	PRIMARY KEY ("Tenant","post_id","name")
);
INSERT INTO blogs_tags_temp SELECT post_id, name, Tenant FROM blogs_tags GROUP BY 1,2,3;
DROP TABLE blogs_tags;
ALTER TABLE blogs_tags_temp RENAME TO blogs_tags;
CREATE INDEX "blogs_tags_name" ON "blogs_tags" ("name");



