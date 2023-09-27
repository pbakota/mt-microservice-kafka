FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN mkdir /app
ADD api-gw-linux-x64.tgz /app/
WORKDIR /app
RUN chmod +x /app/ApiGW
ENTRYPOINT ["/app/ApiGW"]
