FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core.Middleware/Hackney.Core.Middleware.csproj ./Hackney.Core.Middleware/
COPY ./Hackney.Core.Middleware/nuget.config ./Hackney.Core.Middleware/nuget.config
COPY ./Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj ./Hackney.Core.Tests.Middleware/
COPY ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./Hackney.Core.Tests.Shared/

RUN dotnet restore ./Hackney.Core.Middleware/Hackney.Core.Middleware.csproj
RUN dotnet restore ./Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj
RUN dotnet restore ./Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj

CMD dotnet test Hackney.Core.Tests.Middleware
