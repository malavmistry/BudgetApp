FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BudgetApp/BudgetApp.csproj BudgetApp/
RUN dotnet restore BudgetApp/BudgetApp.csproj

COPY . .
RUN dotnet publish BudgetApp/BudgetApp.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BudgetApp.dll"]