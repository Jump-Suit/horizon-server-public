#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Server.SVO/Server.SVO.csproj", "Server.SVO/"]
COPY ["Server.Pipeline/Server.Pipeline.csproj", "Server.Pipeline/"]
COPY ["RT.Cryptography/RT.Cryptography.csproj", "RT.Cryptography/"]
COPY ["RT.Models/RT.Models.csproj", "RT.Models/"]
COPY ["Server.Common/Server.Common.csproj", "Server.Common/"]
COPY ["RT.Common/RT.Common.csproj", "RT.Common/"]
RUN dotnet restore "Server.SVO/Server.SVO.csproj"
COPY . .
WORKDIR "/src/Server.SVO"
RUN dotnet build "Server.SVO.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.SVO.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.SVO.dll"]