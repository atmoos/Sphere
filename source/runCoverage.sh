#!/bin/sh

dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:'./**/coverage.*.xml' -targetdir:"coveragereport" -reporttypes:Html >/dev/null 2>&1

URL="file:coveragereport/index.html"
[[ -x $BROWSER ]] && exec "$BROWSER" "$URL"
path=$(which xdg-open || which gnome-open) && exec "$path" "$URL" >/dev/null 2>&1
echo "Can't find browser"
