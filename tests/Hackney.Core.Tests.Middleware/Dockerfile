FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src/Hackney.Core.Middleware/Hackney.Core.Middleware.csproj ./src/Hackney.Core.Middleware/
COPY ./src/Hackney.Core.Middleware/nuget.config ./src/Hackney.Core.Middleware/nuget.config
COPY ./tests/Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj ./tests/Hackney.Core.Tests.Middleware/
COPY ./tests/Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./tests/Hackney.Core.Tests.Shared/

RUN dotnet restore ./src/Hackney.Core.Middleware/Hackney.Core.Middleware.csproj
RUN dotnet restore ./tests/Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj
RUN dotnet restore ./tests/Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out tests/Hackney.Core.Tests.Middleware/Hackney.Core.Tests.Middleware.csproj

CMD dotnet test tests/Hackney.Core.Tests.Middleware
