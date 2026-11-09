# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY microblog-heads.csproj .
RUN dotnet restore

COPY . .
# Don't use --no-restore: the restore above ran before the .razor files were copied,
# so it skipped the package that provides _framework/blazor.web.js. Restoring again
# here (cached, so fast) picks it up; without it no page in the container is interactive.
RUN dotnet publish -c Release -o /app/publish

# ---------- Stage 2: run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "microblog-heads.dll"]
