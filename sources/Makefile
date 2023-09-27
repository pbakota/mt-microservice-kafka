# vim: ts=8 sts=8 sw=8 noet ai
all:


run-orders:
	dotnet watch run -f "net6.0" -c Developer --project Orders/Orders.csproj

update-db-orders:
	dotnet ef database update --project Orders/Orders.csproj

run-payments:
	dotnet watch run -f "net6.0" -c Developer --project Payments/Payments.csproj

update-db-payments:
	dotnet ef database update --project Payments/Payments.csproj

run-stock:
	dotnet watch run -f "net6.0" -c Developer --project Stock/Stock.csproj

update-db-stock:
	dotnet ef database update --project Stock/Stock.csproj

run-delivery:
	dotnet watch run -f "net6.0" -c Developer --project Delivery/Delivery.csproj

update-db-delivery:
	dotnet ef database update --project Delivery/Delivery.csproj

run-api-gw:
	dotnet watch run --project ApiGW/ApiGW.csproj

