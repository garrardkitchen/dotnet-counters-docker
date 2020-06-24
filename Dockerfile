#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["monitor.csproj", "./"]
RUN dotnet restore "monitor.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "monitor.csproj" -c Release -o /app/build
EXPOSE 80
EXPOSE 443

## must have
RUN dotnet tool install dotnet-counters --global
RUN dotnet tool install dotnet-trace --global
RUN dotnet tool install dotnet-gcdump --global

FROM build AS publish
RUN dotnet publish "monitor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

## must do
COPY --from=build /root/.dotnet/tools/ /app/tools
ENV PATH="${PATH}:/app/tools"

ENTRYPOINT ["dotnet", "monitor.dll"]
