# vim: ts=8 sts=8 sw=8 noet ai
all:
	dotnet build

run-orders:
	Kafka__BootstrapServers=${BOOTSTRAP_SERVERS} dotnet watch run -f "net6.0" -c Developer --project Orders/Orders.csproj

update-db-orders:
	dotnet ef database update --project Orders/Orders.csproj

run-payments:
	Kafka__BootstrapServers=${BOOTSTRAP_SERVERS} dotnet watch run -f "net6.0" -c Developer --project Payments/Payments.csproj

update-db-payments:
	dotnet ef database update --project Payments/Payments.csproj

run-stock:
	Kafka__BootstrapServers=${BOOTSTRAP_SERVERS} dotnet watch run -f "net6.0" -c Developer --project Stock/Stock.csproj

update-db-stock:
	dotnet ef database update --project Stock/Stock.csproj

run-delivery:
	Kafka__BootstrapServers=${BOOTSTRAP_SERVERS} dotnet watch run -f "net6.0" -c Developer --project Delivery/Delivery.csproj

update-db-delivery:
	dotnet ef database update --project Delivery/Delivery.csproj

run-api-gw:
	dotnet watch run --project ApiGW/ApiGW.csproj

package-orders:
	rm -rf ../releases/linux-x64 && dotnet publish -f "net6.0" -c Release Orders/Orders.csproj -r linux-x64 --output ../releases/linux-x64 --no-self-contained && \
	cd ../releases/linux-x64 && tar -zcf ../orders-linux-x64.tgz * && cd -

package-payments:
	rm -rf ../releases/linux-x64 && dotnet publish -f "net6.0" -c Release Payments/Payments.csproj -r linux-x64 --output ../releases/linux-x64 --no-self-contained && \
	cd ../releases/linux-x64 && tar -zcf ../payments-linux-x64.tgz * && cd -

package-stock:
	rm -rf ../releases/linux-x64 && dotnet publish -f "net6.0" -c Release Stock/Stock.csproj -r linux-x64 --output ../releases/linux-x64 --no-self-contained && \
	cd ../releases/linux-x64 && tar -zcf ../stock-linux-x64.tgz * && cd -

package-delivery:
	rm -rf ../releases/linux-x64 && dotnet publish -f "net6.0" -c Release Delivery/Delivery.csproj -r linux-x64 --output ../releases/linux-x64 --no-self-contained && \
	cd ../releases/linux-x64 && tar -zcf ../delivery-linux-x64.tgz * && cd -

package-api-gw:
	rm -rf ../releases/linux-x64 && dotnet publish -f "net6.0" -c Release ApiGW/ApiGW.csproj -r linux-x64 --output ../releases/linux-x64 --no-self-contained && \
	cd ../releases/linux-x64 && tar -zcf ../api-gw-linux-x64.tgz * && cd -

package-all:
	make package-orders && make package-payments && make package-stock && make package-delivery && make package-api-gw


