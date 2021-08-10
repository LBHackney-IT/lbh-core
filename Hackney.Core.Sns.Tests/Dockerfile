FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.Sns/Hackney.Core.Sns.csproj ./Hackney.Core.Sns/
COPY ./Hackney.Core.Sns.Tests/Hackney.Core.Sns.Tests.csproj ./Hackney.Core.Sns.Tests/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.Sns/Hackney.Core.Sns.csproj
RUN dotnet restore ./Hackney.Core.Sns.Tests/Hackney.Core.Sns.Tests.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Sns.Tests/Hackney.Core.Sns.Tests.csproj

CMD dotnet test Hackney.Core.Sns.Tests
