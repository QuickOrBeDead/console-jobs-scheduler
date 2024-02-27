#!/bin/bash

declare -A contexts
contexts["SettingsDbContext"]="Domain/Settings/Infra/Migrations"
contexts["IdentityManagementDbContext"]="Domain/Identity/Infra/Migrations"
contexts["HistoryDbContext"]="Domain/History/Infra/Migrations"
contexts["RunnerDbContext"]="Domain/Runner/Infra/Migrations"

if [ -z "$1" ]
then
  echo "migration name parameter is required"
  exit 1
fi

if [ -z "$2" ]
then
  echo "db context name parameter is required"
  exit 1
fi

out="${contexts[$2]}"
if [ -z "$out" ]
then
  echo "output-dir not found for $2"
  exit 1
fi

dotnet ef migrations add "$1" --context "$2" --output-dir "$out"