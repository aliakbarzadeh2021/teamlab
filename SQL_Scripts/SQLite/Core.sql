-- core_acl
CREATE TABLE "core_acl" (
  "tenant" INTEGER NOT NULL,
  "subject" varchar(38) NOT NULL,
  "action" varchar(38) NOT NULL,
  "object" varchar(255) NOT NULL DEFAULT '',
  "acetype" INTEGER NOT NULL,
  PRIMARY KEY ("tenant","subject","action","object")
);


-- core_group
CREATE TABLE "core_group" (
  "tenant" INTEGER NOT NULL,
  "id" varchar(38) NOT NULL,
  "name" varchar(128) NOT NULL,
  "categoryid" varchar(38) DEFAULT NULL,
  "parentid" varchar(38) DEFAULT NULL,
  "removed" INTEGER NOT NULL DEFAULT 0,
  "last_modified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("id")
);
CREATE INDEX "core_group_parentid" ON "core_group" ("tenant","parentid");
CREATE INDEX "core_group_last_modified" ON "core_group" ("last_modified");


-- core_settings
CREATE TABLE "core_settings" (
  "tenant" INTEGER NOT NULL,
  "id" varchar(128) NOT NULL,
  "value" BLOB NOT NULL,
  PRIMARY KEY ("tenant","id")
);


-- core_subscription
CREATE TABLE "core_subscription" (
  "tenant" INTEGER NOT NULL,
  "source" varchar(38) NOT NULL,
  "action" varchar(128) NOT NULL,
  "recipient" varchar(38) NOT NULL,
  "object" varchar(128) NOT NULL,
  "unsubscribed" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("tenant","source","action","recipient","object")
);


-- core_subscriptionmethod
CREATE TABLE "core_subscriptionmethod" (
  "tenant" INTEGER NOT NULL,
  "source" varchar(38) NOT NULL,
  "action" varchar(128) NOT NULL,
  "recipient" varchar(38) NOT NULL,
  "sender" varchar(1024) NOT NULL,
  PRIMARY KEY ("tenant","source","action","recipient")
);


-- core_user
CREATE TABLE "core_user" (
  "tenant" INTEGER NOT NULL,
  "id" varchar(38) NOT NULL,
  "username" varchar(64) NOT NULL,
  "firstname" varchar(64) NOT NULL,
  "lastname" varchar(64) NOT NULL,
  "sex" INTEGER DEFAULT NULL,
  "bithdate" datetime DEFAULT NULL,
  "status" INTEGER NOT NULL DEFAULT '1',
  "activation_status" INTEGER NOT NULL DEFAULT 0,
  "email" varchar(64) DEFAULT NULL,
  "workfromdate" datetime DEFAULT NULL,
  "terminateddate" datetime DEFAULT NULL,
  "title" varchar(64) DEFAULT NULL,
  "department" varchar(64) DEFAULT NULL,
  "contacts" varchar(1024) DEFAULT NULL,
  "location" varchar(255) DEFAULT NULL,
  "notes" varchar(512) DEFAULT NULL,
  "removed" INTEGER NOT NULL DEFAULT 0,
  "last_modified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("id")
);
CREATE INDEX "core_user_last_modified" ON "core_user" ("last_modified");
CREATE INDEX "core_user_username" ON "core_user" ("tenant","username");
CREATE INDEX "core_user_email" ON "core_user" ("email");


-- core_usergroup
CREATE TABLE "core_usergroup" (
  "tenant" INTEGER NOT NULL,
  "userid" varchar(38) NOT NULL,
  "groupid" varchar(38) NOT NULL,
  "ref_type" INTEGER NOT NULL,
  "removed" INTEGER NOT NULL DEFAULT 0,
  "last_modified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("tenant","userid","groupid","ref_type")
);
CREATE INDEX "core_usergroup_last_modified" ON "core_usergroup" ("last_modified");


-- core_userphoto
CREATE TABLE "core_userphoto" (
  "tenant" INTEGER NOT NULL,
  "userid" varchar(38) NOT NULL,
  "photo" BLOB,
  PRIMARY KEY ("userid")
);


-- core_usersecurity
CREATE TABLE "core_usersecurity" (
  "tenant" INTEGER NOT NULL,
  "userid" varchar(38) NOT NULL,
  "pwdhash" varchar(512) DEFAULT NULL,
  "pwdhashsha512" varchar(512) DEFAULT NULL,
  PRIMARY KEY ("userid")
);
CREATE INDEX "core_usersecurity_pwdhash" ON "core_usersecurity" ("pwdhash");


-- tenants_forbiden
CREATE TABLE "tenants_forbiden" (
  "address" varchar(50) NOT NULL,
  PRIMARY KEY ("address")
);


-- tenants_quota
CREATE TABLE "tenants_quota" (
  "tenant" INTEGER NOT NULL,
  "max_file_size" INTEGER NOT NULL DEFAULT 0,
  "max_total_size" INTEGER NOT NULL DEFAULT 0,
  "https_enable" INTEGER NOT NULL DEFAULT 0,
  "security_enable" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("tenant")
);


-- tenants_quotarow
CREATE TABLE "tenants_quotarow" (
  "tenant" INTEGER NOT NULL,
  "path" varchar(255) NOT NULL,
  "counter" INTEGER NOT NULL DEFAULT 0,
  "tag" varchar(1024) DEFAULT NULL,
  "last_modified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("tenant","path")
);
CREATE INDEX "tenants_quotarow_last_modified" ON "tenants_quotarow" ("last_modified");


-- tenants_tariff
CREATE TABLE "tenants_tariff" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "tenant" INTEGER NOT NULL,
  "tariff" INTEGER NOT NULL,
  "stamp" datetime NOT NULL,
  "comment" varchar(255) DEFAULT NULL
);
CREATE INDEX "tenants_tariff_tenant" ON "tenants_tariff" ("tenant");


-- tenants_template_subscription
CREATE TABLE "tenants_template_subscription" (
  "source" varchar(38) NOT NULL,
  "action" varchar(128) NOT NULL,
  "recipient" varchar(38) NOT NULL,
  "object" varchar(128) NOT NULL DEFAULT '',
  "unsubscribed" INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY ("source","action","recipient","object")
);


-- tenants_template_subscriptionmethod
CREATE TABLE "tenants_template_subscriptionmethod" (
  "source" varchar(38) NOT NULL,
  "action" varchar(128) NOT NULL,
  "recipient" varchar(38) NOT NULL,
  "sender" varchar(1024) NOT NULL,
  PRIMARY KEY ("source","action","recipient")
);


-- tenants_tenants
CREATE TABLE "tenants_tenants" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" varchar(255) NOT NULL,
  "alias" varchar(50) NOT NULL,
  "mappeddomain" varchar(50) DEFAULT NULL,
  "language" char(7) NOT NULL DEFAULT 'en-US',
  "timezone" varchar(50) DEFAULT NULL,
  "trusteddomains" varchar(1024) DEFAULT NULL,
  "trusteddomainsenabled" INTEGER NOT NULL DEFAULT '1',
  "status" INTEGER NOT NULL DEFAULT 0,
  "statuschanged" datetime DEFAULT NULL,
  "creationdatetime" datetime NOT NULL,
  "owner_id" varchar(38) DEFAULT NULL,
  "public" INTEGER NOT NULL DEFAULT 0,
  "publicvisibleproducts" varchar(1024) DEFAULT NULL,
  "last_modified" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX "tenants_tenants_alias" ON "tenants_tenants" ("alias");
CREATE INDEX "tenants_tenants_last_modified" ON "tenants_tenants" ("last_modified");
CREATE INDEX "tenants_tenants_mappeddomain" ON "tenants_tenants" ("mappeddomain");



