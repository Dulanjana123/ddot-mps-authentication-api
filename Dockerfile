#Build Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

ARG ENV
ENV ENV=${ENV}

WORKDIR /app
EXPOSE 3300
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DDOT.MPS.Auth.Api/DDOT.MPS.Auth.Api.csproj","DDOT.MPS.Auth.Api/"]
RUN dotnet restore "DDOT.MPS.Auth.Api/DDOT.MPS.Auth.Api.csproj"
COPY . .
WORKDIR "/src/DDOT.MPS.Auth.Api"
# Build the application
RUN dotnet build "DDOT.MPS.Auth.Api.csproj" -c development -o /app/build


FROM build AS publish 
RUN dotnet publish "DDOT.MPS.Auth.Api.csproj" -c development -o /app/publish /p:UseAppHost=false
#Serve Stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./

# Set the environment variable 
ENV ASPNETCORE_ENVIRONMENT=${ENV}

ENTRYPOINT ["dotnet","DDOT.MPS.Auth.Api.dll"]