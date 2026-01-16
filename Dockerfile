ARG APP_PORT=7182
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/activityhistory_api/activityhistory_api.csproj
RUN dotnet publish src/activityhistory_api/activityhistory_api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG APP_PORT=7182
ENV ASPNETCORE_URLS=http://*:${APP_PORT}
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE ${APP_PORT}
ENTRYPOINT ["dotnet", "activityhistory_api.dll"]