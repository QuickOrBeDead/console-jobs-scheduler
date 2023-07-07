-- Table: public.qrtz_job_history

DROP TABLE IF EXISTS public.qrtz_job_history;

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