-- webstudio_fckuploads
CREATE TABLE "webstudio_fckuploads" (
  "TenantID" INTEGER NOT NULL,
  "StoreDomain" varchar(50) NOT NULL,
  "FolderID" varchar(100) NOT NULL,
  "ItemID" varchar(100) NOT NULL,
  PRIMARY KEY ("TenantID","StoreDomain","FolderID","ItemID")
);


-- webstudio_quicklinks
CREATE TABLE "webstudio_quicklinks" (
  "ID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Name" varchar(255) DEFAULT NULL,
  "URL" text,
  "UserCreatorID" char(38) DEFAULT NULL,
  "Date" datetime DEFAULT NULL,
  "DisplayOnTopPanel" INTEGER DEFAULT NULL,
  "Tenant" INTEGER NOT NULL DEFAULT 0
);


-- webstudio_settings
CREATE TABLE "webstudio_settings" (
  "TenantID" INTEGER NOT NULL,
  "ID" varchar(64) NOT NULL,
  "UserID" varchar(64) NOT NULL,
  "Data" blob,
  PRIMARY KEY ("ID","UserID","TenantID")
);


-- webstudio_useractivity
CREATE TABLE "webstudio_useractivity" (
  "ID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "ProductID" char(38) NOT NULL,
  "ModuleID" char(38) NOT NULL,
  "UserID" char(38) NOT NULL,
  "ContentID" varchar(256) NOT NULL,
  "Title" varchar(4000) NOT NULL,
  "URL" varchar(4000) NOT NULL,
  "BusinessValue" INTEGER NOT NULL DEFAULT 0,
  "ActionType" INTEGER NOT NULL,
  "ActionText" varchar(256) NOT NULL,
  "ActivityDate" datetime NOT NULL,
  "ImageFileName" varchar(1024) DEFAULT NULL,
  "PartID" varchar(38) DEFAULT NULL,
  "ContainerID" varchar(38) DEFAULT NULL,
  "AdditionalData" varchar(256) DEFAULT NULL,
  "TenantID" INTEGER NOT NULL,
  "HtmlPreview" TEXT,
  "SecurityId" varchar(255) DEFAULT NULL
);
CREATE INDEX "webstudio_useractivity_UserID" ON "webstudio_useractivity" ("UserID");
CREATE INDEX "webstudio_useractivity_actiontype" ON "webstudio_useractivity" ("TenantID","ActionType","ProductID");
CREATE INDEX "webstudio_useractivity_ProductID" ON "webstudio_useractivity" ("TenantID","ProductID","ModuleID");
CREATE INDEX "webstudio_useractivity_containerid" ON "webstudio_useractivity" ("ContainerID");
CREATE INDEX "webstudio_useractivity_ActivityDate" ON "webstudio_useractivity" ("ActivityDate");


-- webstudio_widgetcontainer
CREATE TABLE "webstudio_widgetcontainer" (
  "ContainerID" varchar(64) NOT NULL,
  "UserID" varchar(64) NOT NULL,
  "SchemaID" INTEGER NOT NULL,
  "Name" varchar(256) DEFAULT '',
  "SortOrder" INTEGER NOT NULL DEFAULT 0,
  "ID" varchar(64) NOT NULL,
  "TenantID" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("ID")
);


-- webstudio_widgetstate
CREATE TABLE "webstudio_widgetstate" (
  "WidgetID" varchar(64) NOT NULL,
  "IsCollapse" INTEGER NOT NULL DEFAULT 0,
  "ColumnID" INTEGER NOT NULL,
  "SortOrderInColumn" INTEGER NOT NULL DEFAULT 0,
  "WidgetContainerID" varchar(64) NOT NULL,
  PRIMARY KEY ("WidgetContainerID","WidgetID")
);



