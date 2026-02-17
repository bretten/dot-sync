# What is it?
.NET web application that allows users to track files and back them up to managed cloud storages.

## How does it work?
The app is intended to be deployed as a container on a file server.
A path on this host server is defined as the **main storage location**.
Any files within this directory are tracked in the app's database and can be backed up to an indefinite number of cloud storages.
The database also keeps track of which file has been uploaded to which cloud storage to ensure all cloud storages are in sync.

### Example Deployment
 - TrueNAS (both pre or post kubernetes - 24.10)
 - PostgreSQL container
 - DotSync container

## Architecture?
Core libraries follow basic DDD-centric principles. Libraries are then referenced by the ASP.NET Core apps in [Apps](src/Apps).
General libraries are in [Common](src/Common) while the domains are in their respective dirs, (`FileSystem` is in [FileSystem](src/FileSystem)).

---

# Which technologies?
- .NET (for writing the application's layers)
  - EF Core (+ Npgsql for an operational PostgreSQL database)
- ASP.NET Core (user facing app)
  - Blazor
    - Component library: [MudBlazor](https://mudblazor.com/)
- Docker
- AWS
  - S3 (cloud storage option)
  - Secrets Manager (for loading sensitive data when deployed to a remote server)
  - Roles Anywhere (for giving access to a remote server)
  - Cognito (OIDC for authentication and authorization)
- OpenID 

---

# How can I run it?

## Demo
You can run the demo with [docker compose](docker-compose.demo.yaml) and the following users:
- `user1` / `User1user1!` (`Admin` user group - **_can start jobs_**)
- `user2` / `User2user2!` (`Viewer` user group - **_cannot start jobs_**)

### Docker compose
To start it, from the root dir:
```shell
docker compose -f docker-compose.yaml -f docker-compose.demo.yaml up --build -d 
```

To stop it:
```shell
docker compose -f docker-compose.yaml -f docker-compose.demo.yaml down
```

The web application will be available at: http://localhost:7040/