#!/bin/bash
# This script allows you to backup a single volume from a container
# Data in given volume is saved in the current directory in a tar archive.
#
# Example
# ./volume-backup.sh [container: mysqldb] [path in container: /var/lib/mysql] [archive filename: archive-04-2024]
# ./volume_backup_container.sh elias_driver___system_database /var/lib/mysql archive-21-04-2024

CONTAINER_NAME=$1
VOLUME_PATH=$2
ARCHIVE_FILENAME=$3

usage() {
  echo "Usage: $0 [container name] [volume path]"
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

sudo docker run --rm --volumes-from $CONTAINER_NAME -v $(pwd):/backup ubuntu tar -cPzvf /backup/$ARCHIVE_FILENAME.tar.gz $VOLUME_PATH
