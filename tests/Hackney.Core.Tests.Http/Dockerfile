FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src/Hackney.Core.Http/Hackney.Core.Http.csproj ./src/Hackney.Core.Http/
COPY ./tests/Hackney.Core.Tests.Http/Hackney.Core.Tests.Http.csproj ./tests/Hackney.Core.Tests.Http/
COPY ./tests/Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj ./tests/Hackney.Core.Tests.Shared/

RUN dotnet restore ./src/Hackney.Core.Http/Hackney.Core.Http.csproj
RUN dotnet restore ./tests/Hackney.Core.Tests.Http/Hackney.Core.Tests.Http.csproj
RUN dotnet restore ./tests/Hackney.Core.Tests.Shared/Hackney.Core.Tests.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out tests/Hackney.Core.Tests.Http/Hackney.Core.Tests.Http.csproj

CMD dotnet test tests/Hackney.Core.Tests.Http
