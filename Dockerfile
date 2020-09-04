FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# copy the solution and project; restore everything to grab the dependencies
COPY ./dndApi.sln ./
COPY ./dndHitpointApi/dndHitpointApi.csproj ./dndHitpointApi/
RUN dotnet restore

# copy code and compile the release build
COPY . ./
RUN dotnet publish -c release -o out

# copy the build objects
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

# the release server's on 80; so map whatever you like to 80 and go
EXPOSE 80
ENTRYPOINT ["dotnet", "dndHitpointApi.dll"]