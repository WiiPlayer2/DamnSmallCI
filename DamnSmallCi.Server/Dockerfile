FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DamnSmallCi.Server/DamnSmallCi.Server.csproj", "DamnSmallCi.Server/"]
RUN dotnet restore "DamnSmallCi.Server/DamnSmallCi.Server.csproj"
COPY . .
WORKDIR "/src/DamnSmallCi.Server"
RUN dotnet build "DamnSmallCi.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DamnSmallCi.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DamnSmallCi.Server.dll"]
