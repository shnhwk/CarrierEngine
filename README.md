# Carrier Engine POC with RabbitMQ, MassTransit and Docker 

## Start up a RabbitMQ image
> docker run -d -p 5672:5672 -p 15672:15672 --hostname my-rabbit --name rabbitmq-local rabbitmq:3-management

RabbitMQ dashboard can be accessed by going to http://localhost:15672/#/ and using guest/guest to login


## Docker

> Open a command window and switch to your solution directory  cd C:\Users\shnhw\source\repos\CarrierEngine  

#### To create and run the producer API

>docker build -f CarrierEngine.Producer\Dockerfile -t carrier-engine-producer 

>docker run -d -p 8080:80 --name carrier-engine-producer carrier-engine-producer

http://localhost:8080/api/trackingrequest

Sample Post
`{
  "carrier": "A carrier",
  "banyanLoadId": 1,
  "bolNumber": "BOL1234",
  "proNumber": "PRO#"
}`

http://localhost:8080/api/ratingrequest

Sample Post
`{
  "carrier": "A carrier",
  "banyanLoadId": 1,
  "quantity": 100,
  "product": "Test Product"
}`



### To build consumers
>docker build -f CarrierEngine.Consumer.TrackingRequests\Dockerfile -t tracking-requests-consumer .

>docker build -f CarrierEngine.Consumer.RatingRequests\Dockerfile -t rating-requests-consumer .


### To run consumers
I use docker desktop to start them, but you can also use

> docker run -i --name tracking-requests-consumer tracking-requests-consumer

> docker run -i --name rating-requests-consumer rating-requests-consumer
