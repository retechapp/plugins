#!/bin/bash

ABSOLUTE_PATH=$(dirname $(dirname $(realpath $0)))

protoc \
  --proto_path=$ABSOLUTE_PATH/../ \
  --csharp_out=Retech/Network \
  --csharp_opt=file_extension=.cs \
  $ABSOLUTE_PATH/../plugin.proto
