#!/bin/bash

# Clean up the local docker

docker stop $(docker ps -a -q)
docker rm -v $(docker ps --filter status=exited -q)
docker rmi $(docker images -q)
docker system prune
docker volume prune

delete_volumes=$1

if [ "volumes" == "$delete_volumes" ] ;
then
   docker volume rm $(docker volume ls -q --filter dangling=true)
fi

docker image ls
docker volume ls
