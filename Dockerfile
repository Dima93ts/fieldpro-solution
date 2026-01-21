# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tutta la solution
COPY . .

# Restore e publish dell'API, che referenzia gli altri progetti
RUN dotnet restore "./FieldPro.Api/FieldPro.Api.csproj"
RUN dotnet publish "./FieldPro.Api/FieldPro.Api.csproj" -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FieldPro.Api.dll"]
