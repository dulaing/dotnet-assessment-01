# build stage: the sdk image has the compiler, and gets thrown away at the end
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# project files first, so restore stays cached until a dependency actually changes
COPY src/Library.Domain/Library.Domain.csproj src/Library.Domain/
COPY src/Library.Application/Library.Application.csproj src/Library.Application/
COPY src/Library.Infrastructure/Library.Infrastructure.csproj src/Library.Infrastructure/
COPY src/Library.ServiceDefaults/Library.ServiceDefaults.csproj src/Library.ServiceDefaults/
COPY src/Library.Api/Library.Api.csproj src/Library.Api/
RUN dotnet restore src/Library.Api/Library.Api.csproj

# now the source, which changes on nearly every build
COPY src/ src/
RUN dotnet publish src/Library.Api/Library.Api.csproj -c Release -o /app/publish --no-restore

# runtime stage: no compiler, no source code, just the compiled output
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# the base image already defines this non-root user
USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Library.Api.dll"]