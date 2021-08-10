FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.DynamoDb/Hackney.Core.DynamoDb.csproj ./Hackney.Core.DynamoDb/
COPY ./Hackney.Core.DynamoDb.Tests/Hackney.Core.DynamoDb.Tests.csproj ./Hackney.Core.DynamoDb.Tests/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.DynamoDb/Hackney.Core.DynamoDb.csproj
RUN dotnet restore ./Hackney.Core.DynamoDb.Tests/Hackney.Core.DynamoDb.Tests.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.DynamoDb.Tests/Hackney.Core.DynamoDb.Tests.csproj

CMD dotnet test Hackney.Core.DynamoDb.Tests
