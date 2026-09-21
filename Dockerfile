FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/SportsBooking.Domain/SportsBooking.Domain.csproj src/SportsBooking.Domain/
COPY src/SportsBooking.Application/SportsBooking.Application.csproj src/SportsBooking.Application/
COPY src/SportsBooking.Infrastructure/SportsBooking.Infrastructure.csproj src/SportsBooking.Infrastructure/
COPY src/SportsBooking.API/SportsBooking.API.csproj src/SportsBooking.API/
RUN dotnet restore src/SportsBooking.API/SportsBooking.API.csproj

COPY src/ src/
RUN dotnet publish src/SportsBooking.API/SportsBooking.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SportsBooking.API.dll"]
