# Scan for vulnerabilities demo

We need to scan the composition of our docker image and validate all the dependencies agains a vulnerability database. Possible scanners are:

- Snyk
- Trivy
- Grype
- Clair
- ...

For this demo, we will use Trivy which can be installed locally or be used as a container image.

- Build and create a container 
    - Run the script [run.sh](run.sh) which uses the [Dockerfile](Dockerfile)
    - The script will create a Docker image and then create an Sbom and validates the components.

Alternatively, you can run it with Docker Scout

- Run the script [run-dockerscout.sh](run-dockerscout.sh) which scans the image produced in the prior step for CVEs.
