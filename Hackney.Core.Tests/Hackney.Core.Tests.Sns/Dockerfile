FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Hackney.Core/Hackney.Core.Sns/Hackney.Core.Sns.csproj ./Hackney.Core/Hackney.Core.Sns/
COPY ./Hackney.Core.Tests/Hackney.Core.Tests.Sns/Hackney.Core.Tests.Sns.csproj ./Hackney.Core.Tests/Hackney.Core.Tests.Sns/
COPY ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/Hackney.Core.Testing.Shared.csproj ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./Hackney.Core/Hackney.Core.Sns/Hackney.Core.Sns.csproj
RUN dotnet restore ./Hackney.Core.Tests/Hackney.Core.Tests.Sns/Hackney.Core.Tests.Sns.csproj
RUN dotnet restore ./Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/Hackney.Core.Testing.Shared.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out Hackney.Core.Tests/Hackney.Core.Tests.Sns/Hackney.Core.Tests.Sns.csproj

CMD dotnet test Hackney.Core.Tests/Hackney.Core.Tests.Sns
