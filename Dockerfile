FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["first_net8.0.csproj", "."]
RUN dotnet restore "first_net8.0.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "first_net8.0.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "first_net8.0.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "first_net8.0.dll"]