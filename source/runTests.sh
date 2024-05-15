#!/bin/sh

runTest() {
    echo -n " $1> $2: "
    dotnet test --no-build | grep -e "Starting"
}

echo "Building ..."
dotnet build 1>/dev/null

echo "Running tests $1 times:"

for run in $(seq $(($1 - 1))); do
    runTest "├─" $run
done

runTest "└─" $1
