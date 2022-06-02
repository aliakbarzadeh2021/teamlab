CREATE INDEX forum_post_LastModified ON forum_post ("TenantID", "LastModified");

CREATE INDEX forum_question_topic_id ON forum_question ("TenantID", "topic_id");

CREATE INDEX forum_topic_LastModified ON forum_topic ("TenantID", "LastModified");