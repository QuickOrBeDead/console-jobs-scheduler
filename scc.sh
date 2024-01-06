#!/bin/sh
scc $(dirname "$0") -x "json,LICENSE,md,.gitignore,sln,csproj,svg,sh,yml,xml,sql,yaml,ps1,Dockerfile,.dockerignore" --exclude-dir "console-jobs-scheduler-api"
