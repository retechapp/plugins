#!/bin/bash

ABSOLUTE_PATH=$(dirname $(dirname $(realpath $0)))
DEPENDENCIES_PATH="$ABSOLUTE_PATH/.dependencies"

if [ ! -f $DEPENDENCIES_PATH/depot-downloader/DepotDownloader ]; then
  mkdir -p $DEPENDENCIES_PATH/depot-downloader
  curl -L https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_3.4.0/DepotDownloader-macos-arm64.zip \
    -o $DEPENDENCIES_PATH/depot-downloader/bundle.zip
  unzip $DEPENDENCIES_PATH/depot-downloader/bundle.zip -d $DEPENDENCIES_PATH/depot-downloader
  rm -rf $DEPENDENCIES_PATH/depot-downloader/bundle.zip
fi

echo "regex:RustDedicated_Data/Managed/.+\.dll" > $DEPENDENCIES_PATH/filelist.txt

$DEPENDENCIES_PATH/depot-downloader/DepotDownloader anonymous -app 258550 -branch public -os windows -dir $DEPENDENCIES_PATH -filelist $DEPENDENCIES_PATH/filelist.txt
