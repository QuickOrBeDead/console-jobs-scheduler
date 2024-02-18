#!/bin/sh
scc . -x "json,LICENSE,md,.gitignore,sln,csproj,svg,sh,yml,xml,sql,yaml,ps1,Dockerfile,.dockerignore" --exclude-dir "console-jobs-scheduler-api"
