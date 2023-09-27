FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN mkdir /app
ADD delivery-linux-x64.tgz /app/
WORKDIR /app
RUN chmod +x /app/Delivery
ENTRYPOINT ["/app/Delivery"]
