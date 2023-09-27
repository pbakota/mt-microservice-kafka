FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN mkdir /app
ADD payments-linux-x64.tgz /app/
WORKDIR /app
RUN chmod +x /app/Payments
ENTRYPOINT ["/app/Payments"]
