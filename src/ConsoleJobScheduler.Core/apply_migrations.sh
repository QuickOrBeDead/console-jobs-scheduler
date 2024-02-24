#!/bin/bash

if [ -z "$1" ]
then
  echo "db context name parameter is required"
  exit 1
fi

dotnet ef database update --context "$1" -- --connection "User ID=quartz;Password=quartz;Host=localhost;Port=5432;Database=quartz_test;"