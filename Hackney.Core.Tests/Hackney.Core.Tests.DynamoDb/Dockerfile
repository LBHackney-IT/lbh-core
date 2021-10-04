FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.DynamoDb/Hackney.Core.DynamoDb.csproj ./Hackney.Core/Hackney.Core.DynamoDb/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb/Hackney.Core.Tests.DynamoDb.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb/
COPY ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.csproj ./Hackney.Core/Hackney.Core.Testing/

RUN dotnet restore ./Hackney.Core/Hackney.Core.DynamoDb/Hackney.Core.DynamoDb.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb/Hackney.Core.Tests.DynamoDb.csproj
RUN dotnet restore ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb/Hackney.Core.Tests.DynamoDb.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.DynamoDb
