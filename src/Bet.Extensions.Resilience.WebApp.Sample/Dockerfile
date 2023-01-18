
FROM kdcllc/dotnet-sdk:7.0 as builder
RUN dotnet publish "./src/Bet.Extensions.Resilience.WebApp.Sample/Bet.Extensions.Resilience.WebApp.Sample.csproj" -c Release  -o /app

# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:7.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=builder /app .
ENTRYPOINT ["dotnet", "Bet.Extensions.Resilience.WebApp.Sample.dll"]
