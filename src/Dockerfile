#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
USER app
WORKDIR /app
EXPOSE 8080

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --update npm
ARG TARGETARCH
ARG BUILDPLATFORM
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["App/app.client/nuget.config", "App/app.client/"]
COPY ["App/App.Server/App.Server.csproj", "App/App.Server/"]
COPY ["App/app.client/app.client.esproj", "App/app.client/"]
COPY ["ConsoleJobScheduler.Core/ConsoleJobScheduler.Core.csproj", "ConsoleJobScheduler.Core/"]
COPY ["ConsoleJobScheduler.Messaging/ConsoleJobScheduler.Messaging.csproj", "ConsoleJobScheduler.Messaging/"]
RUN dotnet restore "./App/App.Server/./App.Server.csproj"
COPY . .
WORKDIR "/src/App/App.Server"
RUN dotnet build "./App.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build -a $TARGETARCH

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./App.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -a $TARGETARCH

FROM --platform=$BUILDPLATFORM base AS final
ARG TARGETARCH
ARG BUILDPLATFORM

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.Server.dll"]
