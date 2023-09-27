## Preface
This project is a small demo that demonstrates microservice architecture with C#, Kafka and MassTransit and EntityFramework. The microservices are communicate with events pushed to kafka in publish/subscribe manner. Each microservice is separate entity, they do not have any direct connection between them, the microservices manages their own separate databases as well(sqlite for this demonstration). Also the demo implements a very basic API gateway using Ocelot, to demonstrate how a unified api platform can be presented to the end users and which can be used for api authentication and rate limiting (not included in this demonstration).

## The architecture

![Alt text](http://127.0.0.1:8000/figures/figure-1.svg "a title")

The architecture is very simple and straightforward. The even bus is implemented with producer/consumer. Each microservice is subscribing to certain topics and also can/will produce events for other topics. The API gateway is used to have a unified API surface and to hide/protect the microservices from the end users.

## Implementation
The project is divided into small independent projects that implements microservices
* Orders
    Implements microservice that accepts and manages orders
* Payments
    Implemnts microservice that accepts and manages payments
* Stock
    Implemnts microservice that accepts and manages stock/inventory
* Delivery
    Implemnts microservice that accepts and manages delivery

The project also implements the roll-back mechanism. That means if there is any problem in workflow all previous operation can/will be reversed (marked as failed)

The project is developed on Linux operating system and it uses **GNU Makefile**

To initialize the DB execute
* Orders `$ make update-db-orders`
* Payments `$ make update-db-payments`
* Stock `$ make update-db-stock`
* Delivery `$ make update-db-delivery`

To build the whole project execute
`$ make`

To run microservices idependently execute
* Orders `$ make run-orders`
* Payments `$ make run-payments`
* Stock `$ make run-stock`
* Delivery `$ make run-delivery`

To run the API gateway execute
`$ make run-api-gw`

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

The directory "docker" contains all the files needed for docker deployment. But before you can do for each service must prepare TAR.GZ archive.

To create TAR.GZ archive for all services from the "source" directory
`$ make package-all`

The TAR.GZ files can be found in "releases" folder. Please note that the packages contains linux-x64 executables, that means the container where they will be deployed must be a linux container.

To run all microservices with api-gw in one stack execute from "docker" directory
`make start-stack`

To stop the stack execute from "docker" directory
`make stop-stack`

After sucessfully deployed system the api gateway is available on  **_http://\<servername or ip\>:5160_**

You can test by adding a new stock item in the db with:
```
POST http://<servername or ip>:5160/gateway/items
Content-Type: application/json

{
    "Item": "Product 1",
    "Quantity": 1
}
```
