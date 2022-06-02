-- blogs_blogs
CREATE TABLE "blogs_blogs" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" varchar(255) DEFAULT NULL,
  "user_id" char(38) NOT NULL,
  "group_id" char(38) NOT NULL,
  "Tenant" INTEGER NOT NULL
);
CREATE INDEX "blogs_blogs_ixBlogs_Tenant" ON "blogs_blogs" ("Tenant","id");


-- blogs_comments
CREATE TABLE "blogs_comments" (
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
CREATE INDEX "blogs_comments_ixComments_Created" ON "blogs_comments" ("Tenant","created_when");
CREATE INDEX "blogs_comments_ixComments_PostId" ON "blogs_comments" ("Tenant","post_id");


-- blogs_posts
CREATE TABLE "blogs_posts" (
  "id" char(38) NOT NULL,
  "title" varchar(255) NOT NULL,
  "content" TEXT NOT NULL,
  "created_by" char(38) NOT NULL,
  "created_when" datetime NOT NULL,
  "blog_id" INTEGER NOT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  "LastCommentId" char(38) DEFAULT NULL,
  "LastModified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("Tenant","id")
);
CREATE INDEX "blogs_posts_ixPosts_CreatedBy" ON "blogs_posts" ("Tenant","created_by");
CREATE INDEX "blogs_posts_ixPosts_CreatedWhen" ON "blogs_posts" ("Tenant","created_when");
CREATE INDEX "blogs_posts_ixPosts_LastCommentId" ON "blogs_posts" ("Tenant","LastCommentId");


-- blogs_reviewposts
CREATE TABLE "blogs_reviewposts" (
  "post_id" char(38) NOT NULL,
  "reviewed_by" char(38) NOT NULL,
  "timestamp" datetime NOT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("Tenant","post_id","reviewed_by")
);


-- blogs_tags
CREATE TABLE "blogs_tags" (
  "post_id" varchar(38) NOT NULL,
  "name" varchar(255) NOT NULL,
  "Tenant" INTEGER NOT NULL,
  PRIMARY KEY ("Tenant","post_id","name")
);
CREATE INDEX "blogs_tags_name" ON "blogs_tags" ("name");



