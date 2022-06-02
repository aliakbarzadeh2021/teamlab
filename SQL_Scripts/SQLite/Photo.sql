-- photo_album
CREATE TABLE "photo_album" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Caption" varchar(255) DEFAULT NULL,
  "Event" INTEGER NOT NULL,
  "User" varchar(38) DEFAULT NULL,
  "FaceImage" INTEGER NOT NULL DEFAULT 0,
  "Timestamp" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "ImagesCount" INTEGER NOT NULL DEFAULT 0,
  "ViewsCount" INTEGER NOT NULL DEFAULT 0,
  "CommentsCount" INTEGER NOT NULL DEFAULT 0,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX "photo_album_Photo_Album_Index1" ON "photo_album" ("Event");


-- photo_comment
CREATE TABLE "photo_comment" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Text" text NOT NULL,
  "User" varchar(38) NOT NULL,
  "Timestamp" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "Image" INTEGER NOT NULL,
  "Parent" INTEGER NOT NULL DEFAULT 0,
  "Inactive" INTEGER NOT NULL DEFAULT 0,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX "photo_comment_Photo_Comment_Index1" ON "photo_comment" ("Image");


-- photo_event
CREATE TABLE "photo_event" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Name" varchar(255) DEFAULT NULL,
  "Description" varchar(255) DEFAULT NULL,
  "User" varchar(38) DEFAULT NULL,
  "Timestamp" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);


-- photo_image
CREATE TABLE "photo_image" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Album" INTEGER NOT NULL,
  "Name" varchar(255) DEFAULT NULL,
  "Description" varchar(255) DEFAULT NULL,
  "Location" varchar(1024) DEFAULT NULL,
  "Timestamp" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "User" varchar(38) DEFAULT NULL,
  "ThumbnailWidth" INTEGER NOT NULL DEFAULT 0,
  "ThumbnailHeight" INTEGER NOT NULL DEFAULT 0,
  "PreviewWidth" INTEGER NOT NULL DEFAULT 0,
  "PreviewHeight" INTEGER NOT NULL DEFAULT 0,
  "CommentsCount" INTEGER NOT NULL DEFAULT 0,
  "ViewsCount" INTEGER NOT NULL DEFAULT 0,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX "photo_image_Photo_Image_Index1" ON "photo_image" ("Album");


-- photo_imageview
CREATE TABLE "photo_imageview" (
  "Image" INTEGER NOT NULL,
  "User" varchar(38) NOT NULL,
  "Timestamp" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "Tenant" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("Image","User","Tenant")
);



