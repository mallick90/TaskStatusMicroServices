# Use a base image that supports .NET
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Install .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TaskStatusMicroServices.csproj", "./"]
RUN dotnet restore "TaskStatusMicroServices.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TaskStatusMicroServices.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "TaskStatusMicroServices.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskStatusMicroServices.dll"]

