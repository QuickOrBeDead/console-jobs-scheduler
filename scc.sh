#!/bin/sh
scc $(dirname "$0") -x "json,LICENSE,md,.gitignore,sln,csproj,svg,sh" --exclude-dir "console-jobs-scheduler-api"
