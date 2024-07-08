# Use the official .NET Core SDK image to build the app
# This will also cache the dependencies to speed up subsequent builds
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the entire project and restore any dependencies
COPY . ./
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image to run the app
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Copy the build output from the build stage
COPY --from=build-env /app/out .

# Expose the port the app runs on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
