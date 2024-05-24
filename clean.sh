#!/bin/bash

# Clean up the local docker

docker stop $(docker ps -a -q)
docker rm -v $(docker ps --filter status=exited -q)
docker rmi $(docker images -q)
docker system prune
docker volume prune
docker image ls
docker volume ls
