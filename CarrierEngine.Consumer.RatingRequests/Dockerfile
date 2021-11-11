FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CarrierEngine.Consumer.RatingRequests/CarrierEngine.Consumer.RatingRequests.csproj", "CarrierEngine.Consumer.RatingRequests/"]
RUN dotnet restore "CarrierEngine.Consumer.RatingRequests/CarrierEngine.Consumer.RatingRequests.csproj"
COPY . .
WORKDIR "/src/CarrierEngine.Consumer.RatingRequests"
RUN dotnet build "CarrierEngine.Consumer.RatingRequests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarrierEngine.Consumer.RatingRequests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarrierEngine.Consumer.RatingRequests.dll"]