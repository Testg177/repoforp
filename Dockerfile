FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN mkdir -p /app/archiwum

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BlazorApp.Web/BlazorApp.Web.csproj", "src/BlazorApp.Web/"]
COPY ["src/BlazorApp.Application/BlazorApp.Application.csproj", "src/BlazorApp.Application/"]
COPY ["src/BlazorApp.Domain/BlazorApp.Domain.csproj", "src/BlazorApp.Domain/"]
COPY ["src/BlazorApp.Infrastructure/BlazorApp.Infrastructure.csproj", "src/BlazorApp.Infrastructure/"]

RUN dotnet restore "src/BlazorApp.Web/BlazorApp.Web.csproj"

COPY . .
WORKDIR "/src/src/BlazorApp.Web"
RUN dotnet publish "BlazorApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorApp.Web.dll"]
