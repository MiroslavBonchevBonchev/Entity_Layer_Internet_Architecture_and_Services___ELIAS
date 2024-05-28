#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["_system/ELIAS_Core/ELIAS_Core/ELIAS_Core.csproj", "_system/ELIAS_Core/ELIAS_Core/"]
COPY ["_system/ELIAS_Core/ELIAS_Core.Client/ELIAS_Core.Client.csproj", "_system/ELIAS_Core/ELIAS_Core.Client/"]
RUN dotnet restore "./_system/ELIAS_Core/ELIAS_Core/ELIAS_Core.csproj"
COPY . .
WORKDIR "/src/_system/ELIAS_Core/ELIAS_Core"
RUN dotnet build "./ELIAS_Core.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ELIAS_Core.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/app/https_certificates
WORKDIR /home/app/data_protection_keys
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ELIAS_Core.dll"]