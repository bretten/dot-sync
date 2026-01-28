###
### Base ###
###
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Copy the test files for the app
RUN mkdir -p /home/app/dot_sync
COPY ["src/Apps/DotSync.Apps.WebApp.Demo/docker/files_seed/", "/home/app/dot_sync"]
RUN chown $APP_UID:$APP_UID /home/app/dot_sync \
    && chown $APP_UID:$APP_UID /home/app/dot_sync/ -R

# App
USER $APP_UID
WORKDIR /app
EXPOSE 8080

###
### .NET Build stage ###
###
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_ID
ENV BUILD_ID=${BUILD_ID}
WORKDIR /src

# Build properties
COPY Directory.Build.props .

# Restore dependencies for build
COPY ["src/Apps/DotSync.Apps.WebApp.Demo/DotSync.Apps.WebApp.Demo.csproj", "."]
RUN dotnet restore "DotSync.Apps.WebApp.Demo.csproj"

# Source code for building
COPY src .

# Build
WORKDIR "/src/Apps/DotSync.Apps.WebApp.Demo"
RUN dotnet build "DotSync.Apps.WebApp.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/build

#.NET Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DotSync.Apps.WebApp.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

###
### Final stage ###
###
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotSync.Apps.WebApp.Demo.dll"]
