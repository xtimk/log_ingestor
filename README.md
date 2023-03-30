# Log Ingestor
A log manager written in c# with a microservices architecture. All microservices are containerized using docker.

I created this project mainly to practise with microservices/distributed architectures and containers.

## Build & Test Status
|Microservice|Status
|-|-|
|BaseEnricher|[![base-enricher](https://github.com/xtimk/log_ingestor/actions/workflows/baseenricher.yml/badge.svg?branch=main)](https://github.com/xtimk/log_ingestor/actions/workflows/baseenricher.yml)

*Microservices are build and tested on Ubuntu, Windows, MacOS*

## Getting Started
Make sure you have installed and configured docker in your environment. After that, you can run the below commands from the /src/LogIngestor/ directory to start all services.
```bash
docker-compose build
docker-compose up
```
