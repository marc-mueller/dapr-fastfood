# Docker Basics Demo

## Run the compile application inside a container

- Build and create a container 
  - Run the script [01_CompileAndCopyToContainer/run.sh](01_CompileAndCopyToContainer/run.sh) which uses the [01_CompileAndCopyToContainer/Dockerfile](01_CompileAndCopyToContainer/Dockerfile)
- Run the container
  ```
  docker run -it --rm -p 8080:8080 --name democompileandcopy financeservice-demo-compileandcopy
  ```
- Verify that the container is running
  - Open the URL [http://localhost:8080/healthz](http://localhost:8080/healthz)
  - Open the URL [http://localhost:8080/api/revenuereport/byyear](http://localhost:8080/healthz)

## Compile and run the application inside the container

- Build and create a container 
  - Run the script [02_BuildAndRunInContainer/run.sh](02_BuildAndRunInContainer/run.sh) which uses the [02_BuildAndRunInContainer/Dockerfile](02_BuildAndRunInContainer/Dockerfile)
- Run the container
  ```
  docker run -it --rm -p 8080:8080 --name demobuildandrun financeservice-demo-buildandrun
  ```
- Verify that the container is running
  - Open the URL [http://localhost:8080/healthz](http://localhost:8080/healthz)
  - Open the URL [http://localhost:8080/api/revenuereport/byyear](http://localhost:8080/healthz)

## Use a multi stage Dockerfile

- Build and create a container 
  - Run the script [03_Multistage/run.sh](03_Multistage/run.sh) which uses the [03_Multistage/Dockerfile](03_Multistage/Dockerfile)
- Run the container
  ```
  docker run -it --rm -p 8080:8080 --name demomultistage financeservice-demo-multistage
  ```
- Verify that the container is running
  - Open the URL [http://localhost:8080/healthz](http://localhost:8080/healthz)
  - Open the URL [http://localhost:8080/api/revenuereport/byyear](http://localhost:8080/healthz)

## Use docker layer image caching for faster builds

- Build and create a container 
  - Run the script [04_Caching/run.sh](04_Caching/run.sh) which uses the [04_Caching/Dockerfile](04_Caching/Dockerfile)
  - Run the builds multiple times with various changes and see when the image layer cache hits.
- Run the container
  ```
  docker run -it --rm -p 8080:8080 --name democaching financeservice-demo-caching
  ```
- Verify that the container is running
  - Open the URL [http://localhost:8080/healthz](http://localhost:8080/healthz)
  - Open the URL [http://localhost:8080/api/revenuereport/byyear](http://localhost:8080/healthz)