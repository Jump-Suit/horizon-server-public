#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server.NAT/Server.NAT.csproj", "Server.NAT/"]
COPY ["Server.Pipeline/Server.Pipeline.csproj", "Server.Pipeline/"]
COPY ["RT.Common/RT.Common.csproj", "RT.Common/"]
COPY ["RT.Cryptography/RT.Cryptography.csproj", "RT.Cryptography/"]
COPY ["RT.Models/RT.Models.csproj", "RT.Models/"]
COPY ["Server.Common/Server.Common.csproj", "Server.Common/"]
RUN dotnet restore "./Server.NAT/./Server.NAT.csproj"
COPY . .
WORKDIR "/src/Server.NAT"
RUN dotnet build "./Server.NAT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Server.NAT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.NAT.dll"]