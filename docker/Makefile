# vim: ts=4 sw=4 sts=4 noet ai
all:

build:
	make build-orders && make build-payments && make build-stock && make build-delivery && make build-gw

build-orders:
	docker build . --tag=microservice-orders:dev

build-payments:
	docker build . --tag=microservice-payments:dev

build-stock:
	docker build . --tag=microservice-stock:dev

build-delivery:
	docker build . --tag=microservice-delivery:dev

build-gw:
	docker build . --tag=microservice-gw:dev

start-all:
	docker compose up

stop-all:
	docker compose down


