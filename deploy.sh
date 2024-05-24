#!/bin/bash



docker volume rm elias_volume_system_database
docker volume rm elias_volume_system_gateway
docker volume rm elias_volume_service_query
docker volume rm elias_volume_service_societal



docker volume create elias_volume_system_database
docker volume create elias_volume_system_gateway
docker volume create elias_volume_service_query
docker volume create elias_volume_service_societal



docker compose -f ./_deployment/restore-volume-system_database.yml run --rm go
