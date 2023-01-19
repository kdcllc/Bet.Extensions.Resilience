FROM mcr.microsoft.com/dotnet/core/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Bet.Extensions.Resilience.Worker.Sample/Bet.Extensions.Resilience.Worker.Sample.csproj", "src/Bet.Extensions.Resilience.Worker.Sample/"]
RUN dotnet restore "src/Bet.Extensions.Resilience.Worker.Sample/Bet.Extensions.Resilience.Worker.Sample.csproj"
COPY . .
WORKDIR "/src/src/Bet.Extensions.Resilience.Worker.Sample"
RUN dotnet build "Bet.Extensions.Resilience.Worker.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bet.Extensions.Resilience.Worker.Sample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bet.Extensions.Resilience.Worker.Sample.dll"]
