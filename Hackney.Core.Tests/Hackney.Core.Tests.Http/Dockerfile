FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.Http/Hackney.Core.Http.csproj ./Hackney.Core/Hackney.Core.Http/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Http/Hackney.Core.Tests.Http.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Http/
COPY ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.csproj ./Hackney.Core/Hackney.Core.Testing/

RUN dotnet restore ./Hackney.Core/Hackney.Core.Http/Hackney.Core.Http.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Http/Hackney.Core.Tests.Http.csproj
RUN dotnet restore ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.Http/Hackney.Core.Tests.Http.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Http
