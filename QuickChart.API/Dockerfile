# STAGE 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
ARG RUNTIME=linux-musl-x64
WORKDIR /src

COPY QuickChart.API/*.csproj ./QuickChart.API/
RUN dotnet restore QuickChart.API/QuickChart.API.csproj

COPY QuickChart.API/. ./QuickChart.API/
WORKDIR /src/QuickChart.API
RUN dotnet publish \
    -c $BUILD_CONFIGURATION \
    -r $RUNTIME \
    --self-contained false \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

# STAGE 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=UTC \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QuickChart.API.dll"]
