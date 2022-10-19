FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telemetry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

WORKDIR /app

# Copy csproj and nuget config and restore as distinct layers
# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.Enums/Hackney.Core.Enums.csproj ./Hackney.Core/Hackney.Core.Enums/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Enums/Hackney.Core.Tests.Enums.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Enums/
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./Hackney.Core/Hackney.Core.Enums/Hackney.Core.Enums.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Enums/Hackney.Core.Tests.Enums.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.Enums/Hackney.Core.Tests.Enums.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Enums