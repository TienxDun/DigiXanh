FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DigiXanh.API.csproj ./
RUN dotnet restore DigiXanh.API.csproj

COPY . .
RUN dotnet publish DigiXanh.API.csproj \
	-c Release \
	-o /app/publish \
	/p:UseAppHost=false \
	/p:DebugType=None \
	/p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

COPY --from=build /app/publish .

EXPOSE 10000
USER app
ENTRYPOINT ["sh", "-c", "dotnet DigiXanh.API.dll --urls=http://0.0.0.0:${PORT:-10000}"]