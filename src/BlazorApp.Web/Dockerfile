FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BlazorApp.Web/BlazorApp.Web.csproj", "src/BlazorApp.Web/"]
COPY ["src/BlazorApp.Application/BlazorApp.Application.csproj", "src/BlazorApp.Application/"]
COPY ["src/BlazorApp.Domain/BlazorApp.Domain.csproj", "src/BlazorApp.Domain/"]
COPY ["src/BlazorApp.Infrastructure/BlazorApp.Infrastructure.csproj", "src/BlazorApp.Infrastructure/"]
RUN dotnet restore "src/BlazorApp.Web/BlazorApp.Web.csproj"
COPY . .
RUN dotnet publish "src/BlazorApp.Web/BlazorApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorApp.Web.dll"]
