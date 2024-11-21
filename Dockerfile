# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj file separately first to ensure the build process can find it
COPY MovieApi/*.csproj ./MovieApi/

# Restore dependencies
RUN dotnet restore MovieApi/MovieApi.csproj

# Install dotnet-ef as a global tool
RUN dotnet tool install --global dotnet-ef

# Ensure dotnet-ef is on the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the rest of the application files into the container
COPY . ./

# Publish the application
RUN dotnet publish MovieApi/MovieApi.csproj -c Release -o /app/publish

# Apply database migrations with the correct project path
RUN dotnet ef migrations add InitialCreate2 --no-build --project "/app/MovieApi/MovieApi.csproj"
# RUN dotnet ef database update --no-build --project "/app/MovieApi/MovieApi.csproj"

# Final stage - runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Start the application
ENTRYPOINT ["dotnet", "MovieApi.dll"]

