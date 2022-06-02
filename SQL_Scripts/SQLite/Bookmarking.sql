-- bookmarking_bookmark
CREATE TABLE "bookmarking_bookmark" (
  "ID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "URL" text,
  "Date" datetime DEFAULT NULL,
  "Name" varchar(255) DEFAULT NULL,
  "Description" text,
  "UserCreatorID" char(38) DEFAULT NULL,
  "Tenant" INTEGER NOT NULL
);
CREATE INDEX "bookmarking_bookmark_Tenant" ON "bookmarking_bookmark" ("Tenant");


-- bookmarking_bookmarktag
CREATE TABLE "bookmarking_bookmarktag" (
  "BookmarkID" INTEGER NOT NULL,
  "TagID" INTEGER NOT NULL,
  "Tenant" INTEGER NOT NULL,
  PRIMARY KEY ("BookmarkID","TagID")
);
CREATE INDEX "bookmarking_bookmarktag_Tenant" ON "bookmarking_bookmarktag" ("Tenant");


-- bookmarking_comment
CREATE TABLE "bookmarking_comment" (
  "ID" char(38) NOT NULL,
  "UserID" char(38) DEFAULT NULL,
  "Content" text,
  "Datetime" datetime DEFAULT NULL,
  "Parent" char(38) DEFAULT NULL,
  "BookmarkID" INTEGER DEFAULT NULL,
  "Inactive" INTEGER DEFAULT NULL,
  "Tenant" INTEGER NOT NULL,
  PRIMARY KEY ("ID")
);
CREATE INDEX "bookmarking_comment_IndexCommentBookmarkID" ON "bookmarking_comment" ("BookmarkID");


-- bookmarking_tag
CREATE TABLE "bookmarking_tag" (
  "TagID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Name" varchar(255) NOT NULL,
  "Tenant" INTEGER NOT NULL
);
CREATE INDEX "bookmarking_tag_Name" ON "bookmarking_tag" ("Tenant","Name");


-- bookmarking_userbookmark
CREATE TABLE "bookmarking_userbookmark" (
  "UserBookmarkID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "UserID" char(38) DEFAULT NULL,
  "DateAdded" datetime DEFAULT NULL,
  "Name" varchar(255) DEFAULT NULL,
  "Description" text,
  "BookmarkID" INTEGER NOT NULL,
  "Raiting" INTEGER NOT NULL DEFAULT 0,
  "Tenant" INTEGER NOT NULL,
  "LastModified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX "bookmarking_userbookmark_LastModified" ON "bookmarking_userbookmark" ("Tenant","LastModified");
CREATE INDEX "bookmarking_userbookmark_BookmarkID" ON "bookmarking_userbookmark" ("BookmarkID");


-- bookmarking_userbookmarktag
CREATE TABLE "bookmarking_userbookmarktag" (
  "UserBookmarkID" INTEGER NOT NULL,
  "TagID" INTEGER NOT NULL,
  "Tenant" INTEGER NOT NULL,
  PRIMARY KEY ("UserBookmarkID","TagID")
);
CREATE INDEX "bookmarking_userbookmarktag_Tenant" ON "bookmarking_userbookmarktag" ("Tenant");



