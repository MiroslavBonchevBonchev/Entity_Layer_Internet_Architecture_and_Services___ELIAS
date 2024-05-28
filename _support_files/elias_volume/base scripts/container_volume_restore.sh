#!/bin/bash
# This script allows you to restore a single volume from a container
# Data in restored in volume with same backupped path
#
# Example
# ./volume-restore.sh [container (mysqldb)] [path in container (/var/lib/mysql)] [src-archive (archive-04-2024.tar.gz)]
# ./volume-restore.sh elias_driver___system_database /var/lib/mysql archive-21-04-2024

CONTAINER_NAME=$1
VOLUME_PATH=$2
ARCHIVE_FILENAME=$3

usage() {
  echo "Usage: $0 [container name] [volume] [archive filename]"
  exit 1
}

if [ -z $CONTAINER_NAME ]
then
  echo "Error: missing container name parameter."
  usage
fi

if [ -z $VOLUME_PATH ]
then
  echo "Error: missing volume path parameter."
  usage
fi

if [ -z $ARCHIVE_FILENAME ]
then
  echo "Error: missing archive filename."
  usage
fi

sudo docker run --rm --volumes-from $CONTAINER_NAME -v $(pwd):/backup ubuntu tar xvf /backup/$ARCHIVE_FILENAME $VOLUME_PATH
