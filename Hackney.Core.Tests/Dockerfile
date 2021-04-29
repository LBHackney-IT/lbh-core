FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.sln ./
COPY ./Hackney.Core/Hackney.Core.csproj ./Hackney.Core/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.csproj ./Hackney.Core.Tests/

RUN dotnet restore ./Hackney.Core/Hackney.Core.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.csproj

CMD dotnet test
