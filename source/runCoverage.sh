#!/bin/sh

rm -r ./**/TestResults
rm -r ./coveragereport

dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:'./**/coverage.*.xml' -targetdir:"coveragereport" -reporttypes:Html -verbosity:Error

URL="file:coveragereport/index.html"
[[ -x $BROWSER ]] && exec "$BROWSER" "$URL"
path=$(which xdg-open || which gnome-open) && exec "$path" "$URL" &
exit 0
