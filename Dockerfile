FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["microblog-heads.csproj", "./"]
RUN dotnet restore "microblog-heads.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "microblog-heads.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "microblog-heads.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# The app runs as the non-root "app" user, so it can't create folders in /app itself.
# Create the upload folder here and give that user ownership of it.
USER root
RUN mkdir -p /app/Media && chown app:app /app/Media
USER app
ENTRYPOINT ["dotnet", "microblog-heads.dll"]
