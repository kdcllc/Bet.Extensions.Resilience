
FROM kdcllc/dotnet:3.0-sdk-buster as builder
RUN dotnet publish "./src/Bet.Extensions.Resilience.SampleWebApp/Bet.Extensions.Resilience.SampleWebApp.csproj" -c Release  -o /app

# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview9-buster-slim AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=builder /app .
ENTRYPOINT ["dotnet", "Bet.Extensions.Resilience.SampleWebApp.dll"]
