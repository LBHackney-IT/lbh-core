FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.Http/Hackney.Core.Http.csproj ./Hackney.Core.Http/
COPY ./Hackney.Core.Http.Tests/Hackney.Core.Http.Tests.csproj ./Hackney.Core.Http.Tests/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.Http/Hackney.Core.Http.csproj
RUN dotnet restore ./Hackney.Core.Http.Tests/Hackney.Core.Http.Tests.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Http.Tests/Hackney.Core.Http.Tests.csproj

CMD dotnet test Hackney.Core.Http.Tests
