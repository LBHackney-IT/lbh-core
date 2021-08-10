FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.Middleware/Hackney.Core.Middleware.csproj ./Hackney.Core.Middleware/
COPY ./Hackney.Core.Middleware.Tests/Hackney.Core.Middleware.Tests.csproj ./Hackney.Core.Middleware.Tests/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.Middleware/Hackney.Core.Middleware.csproj
RUN dotnet restore ./Hackney.Core.Middleware.Tests/Hackney.Core.Middleware.Tests.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Middleware.Tests/Hackney.Core.Middleware.Tests.csproj

CMD dotnet test Hackney.Core.Middleware.Tests
