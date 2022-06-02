-- events_comment
CREATE TABLE "events_comment" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Feed" INTEGER NOT NULL,
  "Comment" text NOT NULL,
  "Parent" INTEGER NOT NULL DEFAULT 0,
  "Date" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "Creator" varchar(38) DEFAULT NULL,
  "Inactive" INTEGER NOT NULL DEFAULT 0,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);


-- events_feed
CREATE TABLE "events_feed" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "FeedType" INTEGER NOT NULL DEFAULT '1',
  "Caption" text NOT NULL,
  "Text" text,
  "Date" datetime NOT NULL,
  "Creator" varchar(38) DEFAULT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  "LastModified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX "events_feed_LastModified" ON "events_feed" ("Tenant","LastModified");


-- events_poll
CREATE TABLE "events_poll" (
  "Id" INTEGER NOT NULL,
  "PollType" INTEGER NOT NULL DEFAULT 0,
  "StartDate" datetime NOT NULL,
  "EndDate" datetime NOT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("Id")
);


-- events_pollanswer
CREATE TABLE "events_pollanswer" (
  "Variant" INTEGER NOT NULL,
  "User" varchar(64) NOT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("Variant","User")
);


-- events_pollvariant
CREATE TABLE "events_pollvariant" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Poll" INTEGER NOT NULL,
  "Name" varchar(1024) NOT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX "events_pollvariant_Poll" ON "events_pollvariant" ("Poll");


-- events_reader
CREATE TABLE "events_reader" (
  "Feed" INTEGER NOT NULL,
  "Reader" varchar(38) NOT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("Feed","Reader")
);



