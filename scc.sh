#!/bin/sh
scc . -x "json,LICENSE,md,.gitignore,sln,user,csproj,svg,sh,yml,xml,sql,yaml,ps1,sh,Dockerfile,.dockerignore,js,css,props,txt,editorconfig" --exclude-dir "console-jobs-scheduler-api,bin,obj"
