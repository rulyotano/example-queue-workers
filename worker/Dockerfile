FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.17 as base
WORKDIR /src
COPY /worker.csproj .
RUN dotnet restore

FROM base as code
COPY . .

FROM code as build
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine as prod
COPY --from=build /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "worker.dll"]
