-- forum_answer
CREATE TABLE "forum_answer" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "question_id" INTEGER NOT NULL,
  "create_date" datetime DEFAULT NULL,
  "user_id" char(38) NOT NULL,
  "TenantID" INTEGER NOT NULL DEFAULT 0
);


-- forum_answer_variant
CREATE TABLE "forum_answer_variant" (
  "answer_id" INTEGER NOT NULL,
  "variant_id" INTEGER NOT NULL,
  PRIMARY KEY ("answer_id","variant_id")
);


-- forum_attachment
CREATE TABLE "forum_attachment" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" varchar(500) NOT NULL,
  "post_id" INTEGER NOT NULL,
  "size" INTEGER NOT NULL DEFAULT 0,
  "download_count" INTEGER NOT NULL DEFAULT 0,
  "content_type" INTEGER NOT NULL DEFAULT 0,
  "mime_content_type" varchar(100) DEFAULT NULL,
  "create_date" datetime DEFAULT NULL,
  "path" varchar(1000) NOT NULL,
  "TenantID" INTEGER NOT NULL DEFAULT 0
);


-- forum_category
CREATE TABLE "forum_category" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(500) NOT NULL,
  "description" varchar(500) NOT NULL DEFAULT '',
  "sort_order" INTEGER NOT NULL DEFAULT 0,
  "create_date" datetime NOT NULL,
  "poster_id" char(38) NOT NULL,
  "TenantID" INTEGER NOT NULL DEFAULT 0
);


-- forum_lastvisit
CREATE TABLE "forum_lastvisit" (
  "tenantid" INTEGER NOT NULL,
  "user_id" char(38) NOT NULL,
  "thread_id" INTEGER NOT NULL,
  "last_visit" datetime NOT NULL,
  PRIMARY KEY ("user_id","thread_id")
);


-- forum_post
CREATE TABLE "forum_post" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "topic_id" INTEGER NOT NULL,
  "poster_id" char(38) NOT NULL,
  "create_date" datetime NOT NULL,
  "subject" varchar(500) NOT NULL DEFAULT '',
  "text" TEXT NOT NULL,
  "edit_date" datetime DEFAULT NULL,
  "edit_count" INTEGER NOT NULL DEFAULT 0,
  "is_approved" INTEGER NOT NULL DEFAULT 0,
  "parent_post_id" INTEGER NOT NULL DEFAULT 0,
  "formatter" INTEGER NOT NULL DEFAULT 0,
  "editor_id" char(38) DEFAULT NULL,
  "TenantID" INTEGER NOT NULL DEFAULT 0,
  "LastModified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX "forum_post_LastModified" ON "forum_post" ("TenantID","LastModified");


-- forum_question
CREATE TABLE "forum_question" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "topic_id" INTEGER NOT NULL,
  "type" INTEGER NOT NULL DEFAULT 0,
  "name" varchar(500) NOT NULL,
  "create_date" datetime NOT NULL,
  "TenantID" INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX "forum_question_topic_id" ON "forum_question" ("TenantID","topic_id");


-- forum_tag
CREATE TABLE "forum_tag" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" varchar(200) NOT NULL,
  "is_approved" INTEGER NOT NULL DEFAULT 0,
  "TenantID" INTEGER NOT NULL DEFAULT 0
);


-- forum_thread
CREATE TABLE "forum_thread" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "title" varchar(500) NOT NULL,
  "description" varchar(500) NOT NULL DEFAULT '',
  "sort_order" INTEGER NOT NULL DEFAULT 0,
  "category_id" INTEGER NOT NULL,
  "topic_count" INTEGER NOT NULL DEFAULT 0,
  "post_count" INTEGER NOT NULL DEFAULT 0,
  "is_approved" INTEGER NOT NULL DEFAULT 0,
  "TenantID" INTEGER NOT NULL DEFAULT 0,
  "recent_post_id" INTEGER NOT NULL DEFAULT 0,
  "recent_topic_id" INTEGER NOT NULL DEFAULT 0,
  "recent_post_date" datetime DEFAULT NULL,
  "recent_poster_id" char(38) DEFAULT NULL
);


-- forum_topic
CREATE TABLE "forum_topic" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "thread_id" INTEGER NOT NULL,
  "title" varchar(500) NOT NULL,
  "type" INTEGER NOT NULL DEFAULT 0,
  "create_date" datetime NOT NULL,
  "view_count" INTEGER NOT NULL DEFAULT 0,
  "post_count" INTEGER NOT NULL DEFAULT 0,
  "recent_post_id" INTEGER NOT NULL DEFAULT 0,
  "is_approved" INTEGER NOT NULL DEFAULT 0,
  "poster_id" char(38) DEFAULT NULL,
  "sticky" INTEGER NOT NULL DEFAULT 0,
  "closed" INTEGER DEFAULT 0,
  "question_id" varchar(45) DEFAULT 0,
  "TenantID" INTEGER NOT NULL DEFAULT 0,
  "LastModified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX "forum_topic_LastModified" ON "forum_topic" ("TenantID","LastModified");


-- forum_topic_tag
CREATE TABLE "forum_topic_tag" (
  "topic_id" INTEGER NOT NULL,
  "tag_id" INTEGER NOT NULL,
  PRIMARY KEY ("topic_id","tag_id")
);


-- forum_topicwatch
CREATE TABLE "forum_topicwatch" (
  "TenantID" INTEGER NOT NULL,
  "UserID" char(38) NOT NULL,
  "TopicID" INTEGER NOT NULL,
  "ThreadID" INTEGER NOT NULL,
  PRIMARY KEY ("TenantID","UserID","TopicID")
);


-- forum_variant
CREATE TABLE "forum_variant" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" varchar(200) NOT NULL,
  "question_id" INTEGER NOT NULL,
  "sort_order" INTEGER NOT NULL DEFAULT 0
);



