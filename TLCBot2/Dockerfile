FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TLCBot2/TLCBot2.csproj", "TLCBot2/"]
RUN dotnet restore "TLCBot2/TLCBot2.csproj"
COPY . .
WORKDIR "/src/TLCBot2"
RUN dotnet build "TLCBot2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TLCBot2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TLCBot2.dll"]