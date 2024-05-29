#!/bin/bash

# Clean up the local docker instance.
delete_volumes=$1

if [[ "with_volumes" != "$delete_volumes" ]] && [[ "without_volumes" != "$delete_volumes" ]] ;
then
   echo ""
   echo "Error when calling the cleanup script. The volume processing is not specified."
   echo "./clean.sh with_volumes             - to cleanup the volumes."
   echo "./clean.sh without_volumes          - to keep the volumes"
   echo ""

   exit 1;
fi



# Clean up the images.
docker stop $(docker ps -a -q)
docker rm -v $(docker ps --filter status=exited -q)
docker rmi $(docker images -q)
yes | docker system prune
yes | docker volume prune



if [ "with_volumes" == "$delete_volumes" ] ;
then
   # Clean up the volumes.
   docker volume rm $(docker volume ls -q --filter dangling=true)
fi



docker image ls
docker system df -v
