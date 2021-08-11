FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.JWT/Hackney.Core.JWT.csproj ./Hackney.Core/Hackney.Core.JWT/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.JWT/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core/Hackney.Core.JWT/Hackney.Core.JWT.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out tests/Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj

CMD dotnet test tests/Hackney.Core.Tests.JWT
