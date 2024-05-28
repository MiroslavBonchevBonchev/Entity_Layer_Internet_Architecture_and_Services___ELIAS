#!/bin/bash



docker volume prune
docker volume rm elias_volume___system_database
docker volume rm elias_volume___system_gateway
docker volume rm elias_volume___system_core
docker volume rm elias_volume___service_societal



docker volume create elias_volume___system_database
docker volume create elias_volume___system_gateway
docker volume create elias_volume___system_core
docker volume create elias_volume___service_societal
