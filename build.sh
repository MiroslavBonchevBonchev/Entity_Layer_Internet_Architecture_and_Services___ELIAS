#!/bin/bash



REGISTRY=miroslavbonchevbonchev



VERSION=$1
version_template="^((([0-9])+.([0-9])+)+)((.\S)*)$"
version_error_msg="Version is required but is not provided. Build command is as follows: ./build.sh <version_major.version_minor[.optional sub-identifier]>"



build_project()
{
   PROJECT_NAME=$(echo "$1" | tr '[:upper:]' '[:lower:]')
   PROJECT_VERSION=$2
   PROJECT_PATH=$3



   if ! [[ $VERSION =~ $version_template ]] ;
   then
      echo $version_error_msg
      exit 1
   fi



   echo "Building docker image for: "$PROJECT_NAME" version: "$PROJECT_VERSION
   echo "====================================================================="



   echo 1. docker build -t $PROJECT_NAME:$PROJECT_VERSION -f $PROJECT_PATH/Dockerfile .
   docker build -t $PROJECT_NAME:$PROJECT_VERSION -f $PROJECT_PATH/Dockerfile .
   if [ 0 -ne $? ];
   then
      echo "Project: " PROJECT_NAME " Step 1. Exit error code: "$?
      exit 1
   fi
   echo "OK\n\n"



   echo 2. docker tag $PROJECT_NAME:$PROJECT_VERSION $REGISTRY/$PROJECT_NAME:$PROJECT_VERSION
   docker tag $PROJECT_NAME:$PROJECT_VERSION $REGISTRY/$PROJECT_NAME:$PROJECT_VERSION
   if [ 0 -ne $? ];
   then
      echo "Project: " PROJECT_NAME " Step 2. Exit error code: "$?
      exit 1
   fi
   echo "OK\n\n"



   echo 3. docker tag $PROJECT_NAME:$PROJECT_VERSION $REGISTRY/$PROJECT_NAME:latest
   docker tag $PROJECT_NAME:$PROJECT_VERSION $REGISTRY/$PROJECT_NAME:latest
   if [ 0 -ne $? ];
   then
      echo "Project: " PROJECT_NAME " Step 3. Exit error code: "$?
      exit 1
   fi
   echo "OK\n\n"



   echo 4. docker push $REGISTRY/$PROJECT_NAME:$PROJECT_VERSION
   docker push $REGISTRY/$PROJECT_NAME:$PROJECT_VERSION
   if [ 0 -ne $? ];
   then
      echo "Project: " PROJECT_NAME " Step 4. Exit error code: "$?
      exit 1
   fi
   echo "OK\n\n"



   echo 5. docker push $REGISTRY/$PROJECT_NAME:latest
   docker push $REGISTRY/$PROJECT_NAME:latest
   if [ 0 -ne $? ];
   then
      echo "Project: " PROJECT_NAME " Step 5. Exit error code: "$?
      exit 1
   fi
   echo "OK\n\n"
}



if ! [[ $VERSION =~ $version_template ]] ;
then
   echo $version_error_msg
   exit 1
fi



build_project "ELIAS_Gateway" $VERSION "_system/ELIAS_Gateway"
build_project "ELIAS_Query_Service" $VERSION "Query_Service/Query_Service"
build_project "ELIAS_Societal_Service" $VERSION "Societal_Service/Societal_Service"
