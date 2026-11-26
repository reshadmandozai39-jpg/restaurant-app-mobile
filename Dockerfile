ARG DOTNET_VERSION=10.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY RestaurantBackend/RestaurantBackend.csproj RestaurantBackend/
COPY Services/RestaurantCatalog.cs Services/
RUN dotnet restore RestaurantBackend/RestaurantBackend.csproj
COPY RestaurantBackend/Program.cs RestaurantBackend/
RUN dotnet publish RestaurantBackend/RestaurantBackend.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "RestaurantBackend.dll"]
