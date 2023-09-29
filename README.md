## Preface
This project is a small demo that demonstrates the microservice architecture with C#, Kafka MassTransit, and EntityFramework. The microservices communicate with events pushed to Kafka in a publish/subscribe manner. Each microservice is a separate entity, they do not have any direct connection between them, the microservices manage their own separate databases as well(SQLite for this demonstration). Also, the demo implements a very basic API gateway using Ocelot, to demonstrate how a unified API platform can be presented to the end users and which can be used for API authentication and rate limiting (not included in this demonstration).

## The architecture

![Alt text](https://github.com/pbakota/mt-microservice-kafka/blob/main/figures/figure-1.svg)

The architecture is very simple and straightforward. The event bus is implemented with the producer/consumer. Each microservice is subscribing to certain topics and also can/will produce events for other topics. The API gateway is used to have a unified API surface and to hide/protect the microservices from the end users.

## Implementation
The project is divided into small independent projects that implement microservices
* _Orders_
    Implements microservice that accepts and manages orders
* _Payments_
    Implements microservice that accepts and manages payments
* _Stock_
    Implements microservice that accepts and manages stock/inventory
* _Delivery_
    Implements microservice that accepts and manages delivery

The project also implements the roll-back mechanism. That means if there is any problem in workflow all previous operations can/will be reversed (marked as failed)

The project is developed on Linux operating system and it uses **GNU Makefile**

To initialize the DB execute
* _Orders_ `$ make update-db-orders`
* _Payments_ `$ make update-db-payments`
* _Stock_ `$ make update-db-stock`
* _Delivery_ `$ make update-db-delivery`

To build the whole project execute
`$ make`

To run microservices independently execute
* _Orders_ `$ make run-orders`
* _Payments_ `$ make run-payments`
* _Stock_ `$ make run-stock`
* _Delivery_ `$ make run-delivery`

To run the API gateway execute
`$ make run-api-gw`

The starting point is `POST http://127.0.0.1:5000/orders` (see the "Create order" part), from where you can place the order and the procedure will begin.

You can test the reverting by not specifying the "Address", the delivery part will report an error and the whole transaction will be reverted.

You can check the latest delivery by using `GET http://127.0.0.1:5003/deliveries?top=1` (see "Get last 5 delivery" part)

### Implemented REST endpoints on gateway

##### Create order
```
POST http://127.0.0.1:5050/gateway/orders
Content-Type: application/json

{
    "Item": "Product 1",
    "Quantity": 1,
    "Amount": 3,
    "PaymentMethod": "CreditCard",
    "Address": "address 1"
}
```

##### Get last 5 order
```
GET http://127.0.0.1:5050/gateway/orders?top=5
```

##### Get last 5 payment
```
GET http://127.0.0.1:5050/gateway/payments?top=5
```

##### Add stock item
```
POST http://127.0.0.1:5050/gateway/items
Content-Type: application/json

{
    "Item": "Product 1",
    "Quantity": 1
}
```

##### Get stock item
```
GET http://127.0.0.1:5050/gateway/items/Product%201
```

##### Get top 5 stock items
```
GET http://127.0.0.1:5050/gateway/items?top=5
```

##### Get last 5 delivery
```
GET http://127.0.0.1:5050/gateway/deliveries?top=5
```


#### Deployment with Docker

The directory "docker" contains all the files needed for docker deployment. But before you can do this for each service must prepare the TAR.GZ archive.

To create a TAR.GZ archive for all services from the "source" directory
`$ make package-all`

The TAR.GZ files can be found in the "releases" folder. Please note that the packages contain linux-x64 executables, which means the container where they will be deployed must be a Linux container.

To run all microservices with API-gw in one stack execute from the "docker" directory
`make start-stack`

To stop the stack execution from the "docker" directory
`make stop-stack`

After successfully deploying the system the API gateway is available on  **_http://\<servername or ip\>:5160_**

You can test by adding a new stock item in the db with:
```
POST http://<servername or ip>:5160/gateway/items
Content-Type: application/json

{
    "Item": "Product 1",
    "Quantity": 1
}
```

**IMPORTANT: The stack does not contain the Kafka service, which must be already available. The address of the bootstrap server must be specified in the "BOOTSTRAP_SERVERS" environment variable**
e.g: `export BOOTSTRAP_SERVERS=localhost:9092`


## Footnotes

A similar project is available for Java Spring Boot 3 https://github.com/pbakota/java-microservice-kafka
