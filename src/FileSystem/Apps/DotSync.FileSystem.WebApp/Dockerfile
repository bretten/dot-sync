FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FileSystem/Apps/DotSync.FileSystem.WebApp/DotSync.FileSystem.WebApp.csproj", "src/FileSystem/Apps/DotSync.FileSystem.WebApp/"]
RUN dotnet restore "src/FileSystem/Apps/DotSync.FileSystem.WebApp/DotSync.FileSystem.WebApp.csproj"
COPY . .
WORKDIR "/src/src/FileSystem/Apps/DotSync.FileSystem.WebApp"
RUN dotnet build "DotSync.FileSystem.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DotSync.FileSystem.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotSync.FileSystem.WebApp.dll"]
