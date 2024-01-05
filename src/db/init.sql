CREATE TABLE IF NOT EXISTS qrtz_job_details
  (
    sched_name TEXT NOT NULL,
	job_name  TEXT NOT NULL,
    job_group TEXT NOT NULL,
    description TEXT NULL,
    job_class_name   TEXT NOT NULL, 
    is_durable BOOL NOT NULL,
    is_nonconcurrent BOOL NOT NULL,
    is_update_data BOOL NOT NULL,
	requests_recovery BOOL NOT NULL,
    job_data BYTEA NULL,
    PRIMARY KEY (sched_name,job_name,job_group)
);

CREATE TABLE IF NOT EXISTS qrtz_triggers
  (
    sched_name TEXT NOT NULL,
	trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    job_name  TEXT NOT NULL, 
    job_group TEXT NOT NULL,
    description TEXT NULL,
    next_fire_time BIGINT NULL,
    prev_fire_time BIGINT NULL,
    priority INTEGER NULL,
    trigger_state TEXT NOT NULL,
    trigger_type TEXT NOT NULL,
    start_time BIGINT NOT NULL,
    end_time BIGINT NULL,
    calendar_name TEXT NULL,
    misfire_instr SMALLINT NULL,
    job_data BYTEA NULL,
    PRIMARY KEY (sched_name,trigger_name,trigger_group),
    FOREIGN KEY (sched_name,job_name,job_group) 
		REFERENCES qrtz_job_details(sched_name,job_name,job_group) 
);

CREATE TABLE IF NOT EXISTS qrtz_simple_triggers
  (
    sched_name TEXT NOT NULL,
	trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    repeat_count BIGINT NOT NULL,
    repeat_interval BIGINT NOT NULL,
    times_triggered BIGINT NOT NULL,
    PRIMARY KEY (sched_name,trigger_name,trigger_group),
    FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS QRTZ_SIMPROP_TRIGGERS 
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL ,
    trigger_group TEXT NOT NULL ,
    str_prop_1 TEXT NULL,
    str_prop_2 TEXT NULL,
    str_prop_3 TEXT NULL,
    int_prop_1 INTEGER NULL,
    int_prop_2 INTEGER NULL,
    long_prop_1 BIGINT NULL,
    long_prop_2 BIGINT NULL,
    dec_prop_1 NUMERIC NULL,
    dec_prop_2 NUMERIC NULL,
    bool_prop_1 BOOL NULL,
    bool_prop_2 BOOL NULL,
	time_zone_id TEXT NULL,
	PRIMARY KEY (sched_name,trigger_name,trigger_group),
    FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS qrtz_cron_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    cron_expression TEXT NOT NULL,
    time_zone_id TEXT,
    PRIMARY KEY (sched_name,trigger_name,trigger_group),
    FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS qrtz_blob_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    blob_data BYTEA NULL,
    PRIMARY KEY (sched_name,trigger_name,trigger_group),
    FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS qrtz_calendars
  (
    sched_name TEXT NOT NULL,
    calendar_name  TEXT NOT NULL, 
    calendar BYTEA NOT NULL,
    PRIMARY KEY (sched_name,calendar_name)
);

CREATE TABLE IF NOT EXISTS qrtz_paused_trigger_grps
  (
    sched_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL, 
    PRIMARY KEY (sched_name,trigger_group)
);

CREATE TABLE IF NOT EXISTS qrtz_fired_triggers 
  (
    sched_name TEXT NOT NULL,
    entry_id TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    instance_name TEXT NOT NULL,
    fired_time BIGINT NOT NULL,
	sched_time BIGINT NOT NULL,
    priority INTEGER NOT NULL,
    state TEXT NOT NULL,
    job_name TEXT NULL,
    job_group TEXT NULL,
    is_nonconcurrent BOOL NOT NULL,
    requests_recovery BOOL NULL,
    PRIMARY KEY (sched_name,entry_id)
);

CREATE TABLE IF NOT EXISTS qrtz_scheduler_state 
  (
    sched_name TEXT NOT NULL,
    instance_name TEXT NOT NULL,
    last_checkin_time BIGINT NOT NULL,
    checkin_interval BIGINT NOT NULL,
    PRIMARY KEY (sched_name,instance_name)
);

CREATE TABLE IF NOT EXISTS qrtz_locks
  (
    sched_name TEXT NOT NULL,
    lock_name  TEXT NOT NULL, 
    PRIMARY KEY (sched_name,lock_name)
);

create index IF NOT EXISTS idx_qrtz_j_req_recovery on qrtz_job_details(requests_recovery);
create index IF NOT EXISTS idx_qrtz_t_next_fire_time on qrtz_triggers(next_fire_time);
create index IF NOT EXISTS idx_qrtz_t_state on qrtz_triggers(trigger_state);
create index IF NOT EXISTS idx_qrtz_t_nft_st on qrtz_triggers(next_fire_time,trigger_state);
create index IF NOT EXISTS idx_qrtz_ft_trig_name on qrtz_fired_triggers(trigger_name);
create index IF NOT EXISTS idx_qrtz_ft_trig_group on qrtz_fired_triggers(trigger_group);
create index IF NOT EXISTS idx_qrtz_ft_trig_nm_gp on qrtz_fired_triggers(sched_name,trigger_name,trigger_group);
create index IF NOT EXISTS idx_qrtz_ft_trig_inst_name on qrtz_fired_triggers(instance_name);
create index IF NOT EXISTS idx_qrtz_ft_job_name on qrtz_fired_triggers(job_name);
create index IF NOT EXISTS idx_qrtz_ft_job_group on qrtz_fired_triggers(job_group);
create index IF NOT EXISTS idx_qrtz_ft_job_req_recovery on qrtz_fired_triggers(requests_recovery);

-- Table: public.qrtz_job_history

CREATE TABLE IF NOT EXISTS public.qrtz_job_history
(
    id character varying(30) COLLATE pg_catalog."default" NOT NULL,
    sched_name text COLLATE pg_catalog."default" NOT NULL,
    instance_name text COLLATE pg_catalog."default" NOT NULL,
    job_name text COLLATE pg_catalog."default" NOT NULL,
    job_group text COLLATE pg_catalog."default" NOT NULL,
	package_name text COLLATE pg_catalog."default",
    trigger_name text COLLATE pg_catalog."default" NOT NULL,
    trigger_group text COLLATE pg_catalog."default" NOT NULL,
    fired_time bigint NOT NULL,
    sched_time bigint NOT NULL,
    last_signal_time bigint NOT NULL,
    run_time bigint,
    has_error boolean NOT NULL,
    error_message text COLLATE pg_catalog."default",
	error_details text COLLATE pg_catalog."default",
    vetoed boolean NOT NULL,
    completed boolean NOT NULL,
    CONSTRAINT qrtz_job_history_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.qrtz_job_history
    OWNER to quartz;

GRANT ALL ON TABLE public.qrtz_job_history TO quartz;


-- Table: public.qrtz_job_run_log

CREATE TABLE IF NOT EXISTS public.qrtz_job_run_log
(
    id bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    job_run_id character varying(30) COLLATE pg_catalog."default" NOT NULL,
    content text COLLATE pg_catalog."default",
    is_error boolean NOT NULL,
    create_time bigint NOT NULL,
    CONSTRAINT qrtz_job_run_log_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.qrtz_job_run_log
    OWNER to quartz;

GRANT ALL ON TABLE public.qrtz_job_run_log TO quartz;
-- Index: IX_qrtz_job_run_log_job_run_id

CREATE INDEX IF NOT EXISTS "IX_qrtz_job_run_log_job_run_id"
    ON public.qrtz_job_run_log USING btree
    (job_run_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;



-- Table: public.qrtz_job_run_email

CREATE TABLE IF NOT EXISTS public.qrtz_job_run_email
(
    id uuid NOT NULL,
    job_run_id character varying(30) COLLATE pg_catalog."default" NOT NULL,
    subject character varying(256) COLLATE pg_catalog."default" NOT NULL,
    body text COLLATE pg_catalog."default" NOT NULL,
    message_to text COLLATE pg_catalog."default" NOT NULL,
    message_cc text COLLATE pg_catalog."default" NOT NULL,
    message_bcc text COLLATE pg_catalog."default" NOT NULL,
    is_sent boolean NOT NULL,
    create_time bigint NOT NULL,
    CONSTRAINT qrtz_job_run_email_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.qrtz_job_run_email
    OWNER to quartz;

GRANT ALL ON TABLE public.qrtz_job_run_email TO quartz;
-- Index: IX_qrtz_job_run_email_job_run_id

CREATE INDEX IF NOT EXISTS "IX_qrtz_job_run_email_job_run_id"
    ON public.qrtz_job_run_email USING btree
    (job_run_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;



-- Table: public.qrtz_job_run_attachment

CREATE TABLE IF NOT EXISTS public.qrtz_job_run_attachment
(
    id bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    job_run_id character varying(30) COLLATE pg_catalog."default" NOT NULL,
    email_id uuid NULL,
    name character varying(256) COLLATE pg_catalog."default" NOT NULL,
    content_type character varying(128) COLLATE pg_catalog."default" NOT NULL,
    content bytea NOT NULL,
    create_time bigint NOT NULL,
    CONSTRAINT qrtz_job_run_attachment_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.qrtz_job_run_attachment
    OWNER to quartz;

GRANT ALL ON TABLE public.qrtz_job_run_attachment TO quartz;
-- Index: IX_qrtz_job_run_attachment_job_run_id

CREATE INDEX IF NOT EXISTS "IX_qrtz_job_run_attachment_job_run_id"
    ON public.qrtz_job_run_attachment USING btree
    (job_run_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

-- Table: public.qrtz_settings

CREATE TABLE IF NOT EXISTS public.qrtz_settings
(
   "CategoryId" integer NOT NULL,
    "Name" character varying(255) COLLATE pg_catalog."default" NOT NULL,
    "Value" text COLLATE pg_catalog."default",
    CONSTRAINT qrtz_settings_pkey PRIMARY KEY ("CategoryId", "Name")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.qrtz_settings
    OWNER to quartz;

GRANT ALL ON TABLE public.qrtz_settings TO quartz;



-- Table: public.qrtz_packages
CREATE TABLE IF NOT EXISTS public.qrtz_packages
(
    name character varying(256) COLLATE pg_catalog."default" NOT NULL,
    content bytea NOT NULL,
    file_name character varying(1024) COLLATE pg_catalog."default" NOT NULL,
    arguments character varying(1024) COLLATE pg_catalog."default" NOT NULL,
    author character varying(50) COLLATE pg_catalog."default" NOT NULL,
    description character varying(1024) COLLATE pg_catalog."default" NOT NULL,
    version character varying(50) COLLATE pg_catalog."default" NOT NULL,
	create_time bigint NOT NULL,
    CONSTRAINT qrtz_packages_pkey PRIMARY KEY (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.qrtz_packages
    OWNER to quartz;

GRANT ALL ON TABLE public.qrtz_packages TO quartz;
