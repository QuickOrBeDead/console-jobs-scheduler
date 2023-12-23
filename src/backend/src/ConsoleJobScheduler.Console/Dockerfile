FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ConsoleJobScheduler.Console/ConsoleJobScheduler.Console.csproj", "ConsoleJobScheduler.Console/"]
RUN dotnet restore "ConsoleJobScheduler.Console/ConsoleJobScheduler.Console.csproj"
COPY . .
WORKDIR "/src/ConsoleJobScheduler.Console"
RUN dotnet build "ConsoleJobScheduler.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ConsoleJobScheduler.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleJobScheduler.Console.dll"]
