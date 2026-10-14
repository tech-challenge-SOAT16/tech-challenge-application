# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TechChallenge.Api/TechChallenge.Api.csproj", "src/TechChallenge.Api/"]
COPY ["src/TechChallenge.Application/TechChallenge.Application.csproj", "src/TechChallenge.Application/"]
COPY ["src/TechChallenge.Application.Abstractions/TechChallenge.Application.Abstractions.csproj", "src/TechChallenge.Application.Abstractions/"]
COPY ["src/TechChallenge.Domain/TechChallenge.Domain.csproj", "src/TechChallenge.Domain/"]
COPY ["src/TechChallenge.Infrastructure.Auth/TechChallenge.Infrastructure.Auth.csproj", "src/TechChallenge.Infrastructure.Auth/"]
COPY ["src/TechChallenge.Infrastructure.Database/TechChallenge.Infrastructure.Database.csproj", "src/TechChallenge.Infrastructure.Database/"]
COPY ["src/TechChallenge.Infrastructure.IoC/TechChallenge.Infrastructure.IoC.csproj", "src/TechChallenge.Infrastructure.IoC/"]
COPY ["src/TechChallenge.Infrastructure.Notifications/TechChallenge.Infrastructure.Notifications.csproj", "src/TechChallenge.Infrastructure.Notifications/"]
RUN dotnet restore "./src/TechChallenge.Api/TechChallenge.Api.csproj"
COPY ["src/TechChallenge.Api/", "src/TechChallenge.Api/"]
COPY ["src/TechChallenge.Application/", "src/TechChallenge.Application/"]
COPY ["src/TechChallenge.Application.Abstractions/", "src/TechChallenge.Application.Abstractions/"]
COPY ["src/TechChallenge.Domain/", "src/TechChallenge.Domain/"]
COPY ["src/TechChallenge.Infrastructure.Auth/", "src/TechChallenge.Infrastructure.Auth/"]
COPY ["src/TechChallenge.Infrastructure.Database/", "src/TechChallenge.Infrastructure.Database/"]
COPY ["src/TechChallenge.Infrastructure.IoC/", "src/TechChallenge.Infrastructure.IoC/"]
COPY ["src/TechChallenge.Infrastructure.Notifications/", "src/TechChallenge.Infrastructure.Notifications/"]
WORKDIR "/src/src/TechChallenge.Api"
RUN dotnet build "./TechChallenge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TechChallenge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "TechChallenge.Api.dll"]
