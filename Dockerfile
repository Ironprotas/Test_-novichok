# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TestNewPeople/TestNewPeople.csproj TestNewPeople/
RUN dotnet restore TestNewPeople/TestNewPeople.csproj

COPY TestNewPeople/ TestNewPeople/
RUN dotnet publish TestNewPeople/TestNewPeople.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DATA_DIR=/app/data

# В образе уже есть non-root пользователь app ($APP_UID)
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "TestNewPeople.dll"]
