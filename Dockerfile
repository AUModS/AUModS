FROM --platform=amd64 mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /source

ENV AmongUs=/source/src/AmongUs
ENV AmongUsLatest=/source/src/AmongUs

COPY SupplementalAUMod/SupplementalAUMod.csproj ./src/SupplementalAUMod.csproj

RUN dotnet restore -r "linux-x64" ./src/SupplementalAUMod.csproj

COPY SupplementalAUMod/. ./src/
# Require the AmongUs files
COPY AmongUs/. ./src/AmongUs/

CMD dotnet publish -c release -o /app -r "linux-x64" -p:VersionSuffix="docker" --no-restore ./src/SupplementalAUMod.csproj
