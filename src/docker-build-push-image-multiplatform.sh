#!/bin/sh
docker buildx build --platform linux/amd64,linux/arm64,linux/arm/v7,linux/arm/v8 --push . -t boraakgn/console-jobs-scheduler
