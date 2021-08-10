FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.Logging/Hackney.Core.Logging.csproj ./Hackney.Core.Logging/
COPY ./Hackney.Core.Tests.Logging/Hackney.Core.Tests.Logging.csproj ./Hackney.Core.Tests.Logging/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.Logging/Hackney.Core.Logging.csproj
RUN dotnet restore ./Hackney.Core.Tests.Logging/Hackney.Core.Tests.Logging.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests.Logging/Hackney.Core.Tests.Logging.csproj

CMD dotnet test Hackney.Core.Tests.Logging
