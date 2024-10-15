FROM mcr.microsoft.com/dotnet/sdk:7.0@sha256:d32bd65cf5843f413e81f5d917057c82da99737cb1637e905a1a4bc2e7ec6c8d AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore
RUN dotnet restore src/Promul.Server~/Promul.Relay.Server.csproj
# Build and publish a release
RUN dotnet publish src/Promul.Server~/Promul.Relay.Server.csproj -c Release -o out /p:UseAppHost=false


# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0@sha256:c7d9ee6cd01afe9aa80642e577c7cec9f5d87f88e5d70bd36fd61072079bc55b
WORKDIR /App

# Exposed Ports
EXPOSE 80
EXPOSE 4098

# Enviroment Variables
ENV JOIN_CODE_LENGTH=6
ENV RELAY_PORT=4098
ENV RELAY_ADDRESS=us628.relays.net.fireworkeyes.com
ENV ENABLE_DESTROY_API=false

COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "Promul.Relay.Server.dll"]