#!/bin/bash

# Define variables
IMAGE_NAME="financeservice-demo-testresultextraction"
# Generate a GUID string for BUILDID (requires uuidgen to be installed)
buildid=$(uuidgen)

# Build the test stage Docker image with the BUILDID argument
docker build -f Dockerfile --build-arg BUILDID="$buildid" --target test ../../
build_status=$?

if [ $build_status -eq 0 ]; then
    echo "Test stage was successfully run in $IMAGE_NAME with BUILDID: $buildid"

    # Ensure the test results directory exists
    testResultsDir="./TestResults/$buildid"
    if [ ! -d "$testResultsDir" ]; then
        mkdir -p "$testResultsDir"
    fi

    # Get the image ID that has the label "testresults=$buildid"
    image_id=$(docker images --filter "label=testresults=$buildid" -q | head -n 1)

    # Create an intermediate container from that image
    container_name="testcontainer-$buildid"
    docker create --name "$container_name" "$image_id"

    # Copy the test results from the container to the host
    docker cp "$container_name":/testresults "$testResultsDir"

    # Remove the intermediate container
    docker rm "$container_name"

    # Process all *.trx files in the test results directory
    for trxFile in "$testResultsDir"/testresults/*.trx; do
        echo "Reading test results from $trxFile ..."
        
        # Parse the XML file using xmlstarlet (ensure xmlstarlet is installed)
        total=$(xmlstarlet sel -N vt="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" -t -v "//vt:ResultSummary/vt:Counters/@total" "$trxFile")
        passed=$(xmlstarlet sel -N vt="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" -t -v "//vt:ResultSummary/vt:Counters/@passed" "$trxFile")
        failed=$(xmlstarlet sel -N vt="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" -t -v "//vt:ResultSummary/vt:Counters/@failed" "$trxFile")

        echo "Test Results for $(basename "$trxFile"): Total: $total, Passed: $passed, Failed: $failed"

        # If any tests failed, exit with an error
        if [ "$failed" -gt 0 ]; then
            echo "Error: There are $failed failing tests in $(basename "$trxFile")" >&2
            exit 1
        fi
    done
else
    echo "Error: Failed to run test stage in $IMAGE_NAME" >&2
    exit 1
fi

# Build the final Docker image
docker build -t "$IMAGE_NAME" -f Dockerfile --target final ../../
