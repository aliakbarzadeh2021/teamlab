CREATE INDEX bookmarking_bookmark_Tenant ON bookmarking_bookmark ("Tenant");

CREATE TABLE "bookmarking_bookmarktag_temp" (
  "BookmarkID" INTEGER NOT NULL,
  "TagID" INTEGER NOT NULL,
  "Tenant" INTEGER NOT NULL,
  PRIMARY KEY ("BookmarkID", "TagID")
);
INSERT INTO bookmarking_bookmarktag_temp SELECT BookmarkID, TagID, Tenant FROM bookmarking_bookmarktag group by 1,2;
DROP TABLE bookmarking_bookmarktag;
ALTER TABLE bookmarking_bookmarktag_temp RENAME TO bookmarking_bookmarktag;
CREATE INDEX bookmarking_bookmarktag_Tenant ON bookmarking_bookmarktag ("Tenant");

CREATE INDEX bookmarking_tag_Name ON bookmarking_tag ("Tenant", "Name");

CREATE INDEX "bookmarking_userbookmark_LastModified" ON "bookmarking_userbookmark" ("Tenant", "LastModified");

CREATE TABLE "bookmarking_userbookmarktag_temp" (
  "UserBookmarkID" INTEGER NOT NULL,
  "TagID" INTEGER NOT NULL,
  "Tenant" INTEGER NOT NULL,
  PRIMARY KEY ("TagID", "UserBookmarkID")  
);
INSERT INTO bookmarking_userbookmarktag_temp SELECT UserBookmarkID, TagID, Tenant FROM bookmarking_userbookmarktag group by 1,2;
DROP TABLE bookmarking_userbookmarktag;
ALTER TABLE bookmarking_userbookmarktag_temp RENAME TO bookmarking_userbookmarktag;
CREATE INDEX "bookmarking_userbookmarktag_Tenant" ON "bookmarking_userbookmarktag" ("Tenant");