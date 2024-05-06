#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Query_Service/Query_Service/Query_Service.csproj", "Query_Service/Query_Service/"]
COPY ["Query_Service/Query_Service.Client/Query_Service.Client.csproj", "Query_Service/Query_Service.Client/"]
RUN dotnet restore "./Query_Service/Query_Service/Query_Service.csproj"
COPY . .
WORKDIR "/src/Query_Service/Query_Service"
RUN dotnet build "./Query_Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Query_Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Query_Service.dll"]