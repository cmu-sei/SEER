# multi-stage target: dev
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS dev

ENV ASPNETCORE_URLS=http://*:5000 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

COPY . /app
WORKDIR /app

RUN dotnet publish -c Release -o /app/dist

CMD ["dotnet", "run"]

# multi-stage target: prod
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS prod

ARG commit
ENV COMMIT=$commit

COPY --from=dev /app/dist /app
COPY App_Data /app/App_Data
COPY ./license.txt /app/license.txt

WORKDIR /app

ENV SEER_VERSION=3.3.0.0
ENV ASPNETCORE_URLS=http://*:5000

EXPOSE 5000

CMD ["dotnet", "Seer.dll"]
