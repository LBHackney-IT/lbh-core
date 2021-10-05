FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.Middleware/Hackney.Core.Middleware.csproj ./Hackney.Core/Hackney.Core.Middleware/
COPY ./Hackney.Core/Hackney.Core.Middleware/nuget.config ./Hackney.Core/Hackney.Core.Middleware/nuget.config
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Middleware/
COPY ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/Hackney.Core.Testing.Shared.csproj ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/
COPY ./Hackney.Core/Hackney.Core.Logging/Hackney.Core.Logging.csproj ./Hackney.Core/Hackney.Core.Logging/

RUN dotnet restore ./Hackney.Core/Hackney.Core.Middleware/Hackney.Core.Middleware.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj
RUN dotnet restore ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/Hackney.Core.Testing.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Middleware
