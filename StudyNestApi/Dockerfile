# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Set environment variable for Firestore credentials
# (Render will override this via Environment settings)
ENV GOOGLE_APPLICATION_CREDENTIALS="/run/secrets/serviceAccount.json"

# Expose the port your API will run on
EXPOSE 8080

# Start the API
ENTRYPOINT ["dotnet", "StudyNestApi.dll"]
