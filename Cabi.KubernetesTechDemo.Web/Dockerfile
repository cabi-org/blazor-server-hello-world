#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Cabi.KubernetesTechDemo.Web/Cabi.KubernetesTechDemo.Web.csproj", "Cabi.KubernetesTechDemo.Web/"]
RUN dotnet restore "Cabi.KubernetesTechDemo.Web/Cabi.KubernetesTechDemo.Web.csproj"
COPY . .
WORKDIR "/src/Cabi.KubernetesTechDemo.Web"
RUN dotnet build "Cabi.KubernetesTechDemo.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cabi.KubernetesTechDemo.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cabi.KubernetesTechDemo.Web.dll"]