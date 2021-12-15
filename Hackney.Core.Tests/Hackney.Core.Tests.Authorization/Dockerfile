FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core/Hackney.Core.Authorization.csproj ./Hackney.Core/Hackney.Core.Authorization/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Authorization/Hackney.Core.Tests.Authorization.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Authorization/
COPY ./Hackney.Core/Hackney.Core.JWT/Hackney.Core.JWT.csproj ./Hackney.Core/Hackney.Core.JWT/
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./Hackney.Core/Hackney.Core.JWT/Hackney.Core.JWT.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Authorization/Hackney.Core.Tests.Authorization.csproj
RUN dotnet restore ./Hackney.Core/Hackney.Core.Authorization/Hackney.Core.Authorization.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb/Hackney.Core.Tests.DynamoDb.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb