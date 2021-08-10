FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.HealthCheck/Hackney.Core.HealthCheck.csproj ./Hackney.Core.HealthCheck/
COPY ./Hackney.Core.Tests.HealthCheck/Hackney.Core.Tests.HealthCheck.csproj ./Hackney.Core.Tests.HealthCheck/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.HealthCheck/Hackney.Core.HealthCheck.csproj
RUN dotnet restore ./Hackney.Core.Tests.HealthCheck/Hackney.Core.Tests.HealthCheck.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests.HealthCheck/Hackney.Core.Tests.HealthCheck.csproj

CMD dotnet test Hackney.Core.Tests.HealthCheck
