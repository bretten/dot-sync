#!/bin/sh

perl -0777 -pe "
    s|__CERT_PATH__|$CERT_PATH|g;
    s|__PK_PATH__|$PK_PATH|g;
    s|__TRUST_ARN__|$TRUST_ARN|g;
    s|__PROFILE_ARN__|$PROFILE_ARN|g;
    s|__ROLE_ARN__|$ROLE_ARN|g;
    s|__REGION__|$REGION|g;
    " /home/app/.aws/credentials_template > /home/app/.aws/credentials

dotnet DotSync.Apps.WebApp.dll