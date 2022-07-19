#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Server.BWPS/Server.BWPS.csproj", "Server.BWPS/"]
COPY ["RT.Cryptography/RT.Cryptography.csproj", "RT.Cryptography/"]
COPY ["HighResolutionTimer/HighResolutionTimer.csproj", "HighResolutionTimer/"]
COPY ["Server.Database/Server.Database.csproj", "Server.Database/"]
COPY ["Server.Common/Server.Common.csproj", "Server.Common/"]
COPY ["RT.Common/RT.Common.csproj", "RT.Common/"]
COPY ["Server.Plugins/Server.Plugins.csproj", "Server.Plugins/"]
COPY ["Server.Plugins.Interface/Server.Plugins.Interface.csproj", "Server.Plugins.Interface/"]
COPY ["Server.Pipeline/Server.Pipeline.csproj", "Server.Pipeline/"]
COPY ["RT.Models/RT.Models.csproj", "RT.Models/"]
RUN dotnet restore "Server.BWPS/Server.BWPS.csproj"
COPY . .
WORKDIR "/src/Server.BWPS"
RUN dotnet build "Server.BWPS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.BWPS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.BWPS.dll"]