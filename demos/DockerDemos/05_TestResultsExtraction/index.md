# Test Result Extraction

- Build and create a container 
    - Run the script [run.sh](run.sh) which uses the [Dockerfile](Dockerfile)
    - The script will create a Docker image and uses an intermediate layer for running the unit tests.
    - We run multipe targets and then extract the results from the intermediate layer image.
- The test results are now available outside of the container and can be integrated into your Azure DevOps Test Run Results.

