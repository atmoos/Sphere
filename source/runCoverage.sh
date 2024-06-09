#!/bin/sh

dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:'./**/coverage.*.xml' -targetdir:"coveragereport" -reporttypes:Html

URL="file:coveragereport/index.html"
[[ -x $BROWSER ]] && exec "$BROWSER" "$URL"
path=$(which xdg-open || which gnome-open) && exec "$path" "$URL"
echo "Can't find browser"
