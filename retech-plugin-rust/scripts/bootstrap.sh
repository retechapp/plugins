#!/bin/bash

DEPENDENCIES_PATH="$(dirname $(dirname $(realpath $0)))/.dependencies"

if [ ! -f $DEPENDENCIES_PATH/.DepotDownloader-bin/DepotDownloader ]; then
  rm -rf $DEPENDENCIES_PATH
  mkdir -p $DEPENDENCIES_PATH/.DepotDownloader-bin
  curl -L https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_3.4.0/DepotDownloader-macos-arm64.zip \
    -o $DEPENDENCIES_PATH/.DepotDownloader-bin/bundle.zip
  unzip $DEPENDENCIES_PATH/.DepotDownloader-bin/bundle.zip -d $DEPENDENCIES_PATH/.DepotDownloader-bin
  rm -rf $DEPENDENCIES_PATH/.DepotDownloader-bin/bundle.zip
fi

echo "regex:RustDedicated_Data/Managed/.+\.dll" > $DEPENDENCIES_PATH/filelist.txt

$DEPENDENCIES_PATH/.DepotDownloader-bin/DepotDownloader anonymous -app 258550 -branch public -os windows -dir $DEPENDENCIES_PATH -filelist $DEPENDENCIES_PATH/filelist.txt
