###
### Configure AWS Stage ###
###
FROM alpine:3 AS aws-config

# Need cURL to get the AWS signing helper
RUN apk add --no-cache curl

# Copy AWS credentials template
COPY ["src/Apps/DotSync.Apps.WebApp/docker/aws_credentials_template.txt", "credentials_template"]
COPY ["src/Apps/DotSync.Apps.WebApp/docker/aws_replace_placeholders.sh", "aws_replace_placeholders.sh"]

# Get the AWS credential helper tool
RUN curl https://rolesanywhere.amazonaws.com/releases/1.7.1/X86_64/Linux/Amzn2023/aws_signing_helper --output aws_signing_helper

###
### Base ###
###
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Copy AWS files
COPY --from=aws-config credentials_template /home/app/.aws/credentials_template
COPY --from=aws-config aws_signing_helper /home/app
COPY --from=aws-config aws_replace_placeholders.sh /home/app
RUN mkdir -p /home/app/.aws \
    && chown $APP_UID:$APP_UID /home/app/.aws \
    && chown $APP_UID:$APP_UID /home/app/.aws/credentials_template \
    && chown $APP_UID:$APP_UID /home/app/aws_signing_helper  \
    && chmod +x /home/app/aws_signing_helper  \
    && chown $APP_UID:$APP_UID /home/app/aws_replace_placeholders.sh \
    && chmod +x /home/app/aws_replace_placeholders.sh

# App
USER $APP_UID:$APP_UID
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
COPY ["src/Apps/DotSync.Apps.WebApp/DotSync.Apps.WebApp.csproj", "."]
RUN dotnet restore "DotSync.Apps.WebApp.csproj"

# Source code for building
COPY src .

# Build
WORKDIR "/src/Apps/DotSync.Apps.WebApp"
RUN dotnet build "DotSync.Apps.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

#.NET Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DotSync.Apps.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

###
### Final stage ###
###
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/home/app/aws_replace_placeholders.sh"]


#docker run -dt `
#    -e CERT_PATH="/mnt/some.crt" `
#    -e PK_PATH="/mnt/some.key" `
#    -e TRUST_ARN="trust_arn" `
#    -e PROFILE_ARN="profile_arn" `
#    -e ROLE_ARN="role_arn" `
#    -e REGION="us-east-1" `
#    -e Aws__Region="us-east-1" `
#    -e Aws__S3__StorageClass="GLACIER_IR" `
#    -e Aws__SecretsManager__SecretName="secret-name" `
#    -e Aws__SecretsManager__Region="us-east-1" `
#    -e URLS="http://*:7219" `
#    -p 7219:7219 `
#    -v "C:\cert\path:/mnt/certs/" `
#    -v "C:\storage\path:/mnt/storage/" `
#    --name dot-sync dot-sync