FROM microsoft/dotnet:2.1.403-sdk AS build-env-netcore
WORKDIR /app
ADD . /app
RUN dotnet publish example.serialisation -c Release -o out
