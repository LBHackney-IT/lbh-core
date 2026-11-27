FROM mcr.microsoft.com/dotnet/sdk:10.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.DynamoDb/Hackney.Core.Testing.DynamoDb.csproj ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.DynamoDb/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Testing.DynamoDb/Hackney.Core.Tests.Testing.DynamoDb.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Testing.DynamoDb/
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config

# We mount secrets so they can't end up in logs or build layers.
# We chain both restore commands so we only make the token available
# once and don't store it elsewhere.
# see:
#   - https://docs.docker.com/reference/dockerfile/#arg
#   - https://docs.docker.com/compose/how-tos/use-secrets/
RUN --mount=type=secret,id=LBHPACKAGESTOKEN \
  export LBHPACKAGESTOKEN=$(cat /run/secrets/LBHPACKAGESTOKEN) && \
  dotnet restore ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.DynamoDb/Hackney.Core.Testing.DynamoDb.csproj && \
  dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Testing.DynamoDb/Hackney.Core.Tests.Testing.DynamoDb.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.Testing.DynamoDb/Hackney.Core.Tests.Testing.DynamoDb.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Testing.DynamoDb --collect:"XPlat Code Coverage;Format=opencover" --results-directory ./coverage
