FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /
COPY nuget.config ./
COPY ["Maliev.PdfService.Api/Maliev.PdfService.Api.csproj", "Maliev.PdfService.Api/"]
COPY ["Maliev.PdfService.Data/Maliev.PdfService.Data.csproj", "Maliev.PdfService.Data/"]
# Since we are using project references for internal packages during dev implementation, 
# we need to COPY them if we were building from root. 
# But standard pattern assumes they are published as NuGet.
# For now, I'll stick to the standard Dockerfile pattern from Constitution X.

RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_password \
    NUGET_USERNAME=$(cat /run/secrets/nuget_username) \
    NUGET_PASSWORD=$(cat /run/secrets/nuget_password) \
    dotnet restore "Maliev.PdfService.Api/Maliev.PdfService.Api.csproj"
COPY . .
WORKDIR "/Maliev.PdfService.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN chown -R app:app /app
USER app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
HEALTHCHECK CMD curl -f http://localhost:8080/pdf/liveness || exit 1
ENTRYPOINT ["dotnet", "Maliev.PdfService.Api.dll"]