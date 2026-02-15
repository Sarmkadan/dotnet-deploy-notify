FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app
COPY dotnet-deploy-notify.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/out .
HEALTHCHECK --interval=10m --timeout=3s --retries=3 CMD curl --fail http://localhost:80/health || exit 1
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser:appgroup
EXPOSE 80