# syntax=docker/dockerfile:1.7

ARG dependency_restore_stage=restore-private

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-base
ARG BUILD_CONFIGURATION=Release
ARG SHARED_LIBRARY_VERSION=1.0.94-alpha
ENV GITHUB_ACTIONS=true
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["NuGet.PRValidation.Config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Maliev.PdfService.Api/Maliev.PdfService.Api.csproj", "Maliev.PdfService.Api/"]
COPY ["Maliev.PdfService.Application/Maliev.PdfService.Application.csproj", "Maliev.PdfService.Application/"]
COPY ["Maliev.PdfService.Domain/Maliev.PdfService.Domain.csproj", "Maliev.PdfService.Domain/"]
COPY ["Maliev.PdfService.Infrastructure/Maliev.PdfService.Infrastructure.csproj", "Maliev.PdfService.Infrastructure/"]
COPY [".ci-packages/", ".ci-packages/"]

FROM build-base AS restore-private
RUN --mount=type=secret,id=nuget_username,required=true \
    --mount=type=secret,id=nuget_password,required=true \
    NUGET_USERNAME="$(cat /run/secrets/nuget_username)" \
    NUGET_PASSWORD="$(cat /run/secrets/nuget_password)" \
    dotnet restore "./Maliev.PdfService.Api/Maliev.PdfService.Api.csproj" \
      -p:SharedLibraryVersion=$SHARED_LIBRARY_VERSION

FROM build-base AS restore-local
RUN dotnet restore "./Maliev.PdfService.Api/Maliev.PdfService.Api.csproj" \
      --configfile "NuGet.PRValidation.Config" \
      -p:SharedLibraryVersion=$SHARED_LIBRARY_VERSION

FROM ${dependency_restore_stage} AS build
ARG BUILD_CONFIGURATION=Release
ARG SHARED_LIBRARY_VERSION=1.0.94-alpha
COPY . .
WORKDIR "/src/Maliev.PdfService.Api"
RUN dotnet publish "./Maliev.PdfService.Api.csproj" \
      -c $BUILD_CONFIGURATION \
      --no-restore \
      -o /app/publish \
      /p:UseAppHost=false \
      -p:SharedLibraryVersion=$SHARED_LIBRARY_VERSION

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .

USER app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:8080/pdf/liveness || exit 1

ENTRYPOINT ["dotnet", "Maliev.PdfService.Api.dll"]
