#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

#Fetch docker build image dotnet 7.0 SDK from Microsoft
#Copy local solution directories and files and publish Domain.Silo project to directory /app/publish 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish "OrleansShopDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#Set entry point to app
ENTRYPOINT ["dotnet", "OrleansShopDemo.dll"]