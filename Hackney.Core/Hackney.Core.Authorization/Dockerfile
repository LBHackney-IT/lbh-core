FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.Authorization/Hackney.Core.Authorization.csproj ./Hackney.Core/Hackney.Core.Authorization/
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./Hackney.Core/Hackney.Core.Authorization/Hackney.Core.Authorization.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core/Hackney.Core.Authorization/Hackney.Core.Authorization.csproj
