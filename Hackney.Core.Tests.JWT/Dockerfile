FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.JWT/Hackney.Core.JWT.csproj ./Hackney.Core.JWT/
COPY ./Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj ./Hackney.Core.Tests.JWT/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.JWT/Hackney.Core.JWT.csproj
RUN dotnet restore ./Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests.JWT/Hackney.Core.Tests.JWT.csproj

CMD dotnet test Hackney.Core.Tests.JWT
