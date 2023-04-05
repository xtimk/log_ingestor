# Log Ingestor
A log manager written in .NET 6 using a microservices architecture. All services are containerized using docker.

I created this project mainly to practise with microservices/distributed architectures and containers.

For microservice comunication it's used the pub/sub pattern, with a message broker/queue. The message broker implementation is abstracted: for now the supported message brokers are `RabbitMQ` and `Kafka`, but this list can be relatively easily extended.

All microservices expose the endpoint /metrics that can be polled by prometheus. In particular two metrics are exposed

 - `logingestor_main_service_in`: tracks the number of events entering the microservice
 - `logingestor_main_service_in`: tracks the number of events exiting the microservice

These metrics can be used with grafana in combination with `rate` to get the events per second entering and exiting all microservices.

## Build & Test Status
|Microservice|Status
|-|-|
|Message Enricher|[![base-enricher](https://github.com/xtimk/log_ingestor/actions/workflows/baseenricher.yml/badge.svg?branch=main&event=push)](https://github.com/xtimk/log_ingestor/actions/workflows/baseenricher.yml)
|Filesystem Writer|[![fs-writer](https://github.com/xtimk/log_ingestor/actions/workflows/fs-writer.yml/badge.svg?branch=main&event=push)](https://github.com/xtimk/log_ingestor/actions/workflows/fs-writer.yml)
|Agent|[![agent](https://github.com/xtimk/log_ingestor/actions/workflows/agent.yml/badge.svg?branch=main&event=push)](https://github.com/xtimk/log_ingestor/actions/workflows/agent.yml)

*Microservices are build and tested using .NET Core 6.0.401 on Ubuntu, Windows, MacOS*

## Performance notes
By testing the system in my local machine I discovered that using `Kafka` drammatically increases performance

The configuration I used for testing is the following (all microservices runs in the same host)

- 1 instance of `Agent`
 - 1 instance of `BaseEnricher`
 - 1 instance of `FSWriter`
 - 1 instance of `RabbitMQ` (no clustering)
 - 1 instance of `Kafka` (no clustering) + `ZooKeeper` + `Kafka-ui` (`provectuslab`) for monitoring
 
 In the following table I reported the performance i measured.
 
|MessageBroker|Performance|
|-|-|
|Kafka|`20K ev/s`|
|RabbitMQ|`1.8K ev/s`|

With `Kafka` even better performances (`50K+ ev/s`) can be achieved, but in that case the `FSWriter` microservice can't write those events to disk in real-time. Probably this is just because of my pc disk limited performances.

## Getting Started
Make sure you have installed and configured docker in your environment. After that, you can run the below commands from the `/src/LogIngestor/` directory to start all services.
```bash
docker-compose build
docker-compose up
```
