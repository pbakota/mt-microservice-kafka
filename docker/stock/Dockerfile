FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN mkdir /app
ADD stock-linux-x64.tgz /app/
WORKDIR /app
RUN chmod +x /app/Stock
ENTRYPOINT ["/app/Stock"]
