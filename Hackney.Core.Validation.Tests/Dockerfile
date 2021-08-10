FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.Validation/Hackney.Core.Validation.csproj ./Hackney.Core.Validation/
COPY ./Hackney.Core.Validation.Tests/Hackney.Core.Validation.Tests.csproj ./Hackney.Core.Validation.Tests/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.Validation/Hackney.Core.Validation.csproj
RUN dotnet restore ./Hackney.Core.Validation.Tests/Hackney.Core.Validation.Tests.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Validation.Tests/Hackney.Core.Validation.Tests.csproj

CMD dotnet test Hackney.Core.Validation.Tests
