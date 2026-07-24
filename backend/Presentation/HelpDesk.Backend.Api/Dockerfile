FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Core/HelpDesk.Backend.Domain/HelpDesk.Backend.Domain.csproj \
    Core/HelpDesk.Backend.Domain/
COPY Core/HelpDesk.Backend.Application/HelpDesk.Backend.Application.csproj \
    Core/HelpDesk.Backend.Application/
COPY Infrastructure/HelpDesk.Backend.Infrastructure/HelpDesk.Backend.Infrastructure.csproj \
    Infrastructure/HelpDesk.Backend.Infrastructure/
COPY Presentation/HelpDesk.Backend.Api/HelpDesk.Backend.Api.csproj \
    Presentation/HelpDesk.Backend.Api/

RUN dotnet restore \
    Presentation/HelpDesk.Backend.Api/HelpDesk.Backend.Api.csproj

COPY . .

RUN dotnet publish \
    Presentation/HelpDesk.Backend.Api/HelpDesk.Backend.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "HelpDesk.Backend.Api.dll"]
