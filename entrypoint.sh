#!/bin/sh

# Check if there are any migrations
if [ ! -d "./Migrations" ]; then
    echo "No migrations found. Creating InitialCreate migration..."
    dotnet ef migrations add InitialCreate --no-build
else
    echo "Migrations already exist. Skipping migration creation."
fi

# Apply database migrations
echo "Applying database migrations..."

dotnet ef database update --no-build

# Start the application
echo "Starting the application..."
exec dotnet MovieApi.dll