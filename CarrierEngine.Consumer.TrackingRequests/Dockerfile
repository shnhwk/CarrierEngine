FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CarrierEngine.Consumer.TrackingRequests/CarrierEngine.Consumer.TrackingRequests.csproj", "CarrierEngine.Consumer.TrackingRequests/"]
RUN dotnet restore "CarrierEngine.Consumer.TrackingRequests/CarrierEngine.Consumer.TrackingRequests.csproj"
COPY . .
WORKDIR "/src/CarrierEngine.Consumer.TrackingRequests"
RUN dotnet build "CarrierEngine.Consumer.TrackingRequests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarrierEngine.Consumer.TrackingRequests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarrierEngine.Consumer.TrackingRequests.dll"]