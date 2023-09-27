# vim: ts=4 sw=4 sts=4 noet ai

all: build

build:
	make build-orders && make build-payments && make build-stock && make build-delivery && make build-gw

build-orders:
	cd orders && docker build --progress=plain --no-cache . --tag=microservice-orders:dev

build-payments:
	cd payments && docker build --progress=plain --no-cache . --tag=microservice-payments:dev

build-stock:
	cd stock && docker build --progress=plain --no-cache . --tag=microservice-stock:dev

build-delivery:
	cd delivery && docker build --progress=plain --no-cache . --tag=microservice-delivery:dev

build-gw:
	cd gw && docker build --progress=plain --no-cache . --tag=microservice-gw:dev

run-orders:
	docker run --rm --name orders-service -v ./data:/app/db -e ${BOOTSTRAP_SERVERS} -e ASPNETCORE_URLS=http://*:5000 -p 5100:5000 microservice-orders:dev

run-payments:
	docker run --rm --name payments-service -v ./data:/app/db -e ${BOOTSTRAP_SERVERS} -e ASPNETCORE_URLS=http://*:5000 -p 5101:5000 microservice-payments:dev

run-stock:
	docker run --rm --name stock-service -v ./data:/app/db -e ${BOOTSTRAP_SERVERS} -e ASPNETCORE_URLS=http://*:5000 -p 5102:5000 microservice-stock:dev

run-delivery:
	docker run --rm --name delivery-service -v ./data:/app/db -e ${BOOTSTRAP_SERVERS} -e ASPNETCORE_URLS=http://*:5000 -p 5103:5000 microservice-delivery:dev

run-api-gw:
	docker run --rm --name api-gw-service -v ./config/ocelot.json:/app/ocelot.json:r -p 5160:5050 microservice-api-gw:dev

start-stack:
	docker compose -f stack.yaml up --remove-orphans -d

stop-stack:
	docker compose -f stack.yaml down


