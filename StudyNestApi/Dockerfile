# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ./StudyNestApi.csproj ./StudyNestApi.csproj
RUN dotnet restore ./StudyNestApi.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish ./StudyNestApi.csproj -c Release -o /app/publish

# Use the official ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variable for Firestore credentials
ENV GOOGLE_APPLICATION_CREDENTIALS="/run/secrets/serviceAccount.json"

# Expose the port Render expects
EXPOSE 8080

# Start the API
ENTRYPOINT ["dotnet", "StudyNestApi.dll"]
