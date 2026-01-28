FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Apps/DotSync.Apps.WebApp.Demo/DotSync.Apps.WebApp.Demo.csproj", "src/Apps/DotSync.Apps.WebApp.Demo/"]
RUN dotnet restore "src/Apps/DotSync.Apps.WebApp.Demo/DotSync.Apps.WebApp.Demo.csproj"
COPY . .
WORKDIR "/src/src/Apps/DotSync.Apps.WebApp.Demo"
RUN dotnet build "./DotSync.Apps.WebApp.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DotSync.Apps.WebApp.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotSync.Apps.WebApp.Demo.dll"]
