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
USER root
RUN apt update && \
    apt install -y apt-transport-https ca-certificates curl gnupg && \
    ( curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.31/deb/Release.key | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg ) && \
    chmod 644 /etc/apt/keyrings/kubernetes-apt-keyring.gpg && \
    ( echo 'deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.31/deb/ /' | tee /etc/apt/sources.list.d/kubernetes.list ) && \
    chmod 644 /etc/apt/sources.list.d/kubernetes.list && \
    apt update && \
    apt install -y kubectl git && \
    apt clean
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DamnSmallCi.Server.dll"]
