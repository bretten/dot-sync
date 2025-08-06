FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID:$APP_UID
WORKDIR /app
EXPOSE 8080

# .NET Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_ID
ENV BUILD_ID=${BUILD_ID}
WORKDIR /src

# Build properties
COPY Directory.Build.props .

# Restore dependencies for build
COPY ["src/FileSystem/Apps/DotSync.FileSystem.WebApp/DotSync.FileSystem.WebApp.csproj", "."]
RUN dotnet restore "DotSync.FileSystem.WebApp.csproj"

# Source code for building
COPY src .

# Build
WORKDIR "/src/FileSystem/Apps/DotSync.FileSystem.WebApp"
RUN dotnet build "DotSync.FileSystem.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

#.NET Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DotSync.FileSystem.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotSync.FileSystem.WebApp.dll"]
