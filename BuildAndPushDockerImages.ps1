param (
    [string]$version = "0.1.0",
    [string]$registry = "yourregistry.azurecr.io"
)

# Function to build and push Docker images
function BuildAndPush-DockerImage {
    param (
        [string]$dockerfileDir,
        [string]$imageName
    )

    Write-Output "Building Docker image $imageName ..."
    docker build -t $imageName -f "$($dockerfileDir)/Dockerfile" .

    if ($?) {
        Write-Output "Pushing Docker image $imageName ..."
        docker push $imageName

        if ($?) {
            Write-Output "Successfully pushed $imageName"
        } else {
            Write-Error "Failed to push $imageName"
        }
    } else {
        Write-Error "Failed to build $imageName"
    }
}

# Get all directories containing a Dockerfile
$dockerfileDirs = Get-ChildItem -Recurse -Filter Dockerfile | Select-Object -ExpandProperty DirectoryName

foreach ($dir in $dockerfileDirs) {
    $projectName = (Split-Path -Leaf $dir).ToLower()
    $imageName = "$($registry)/fastfood-$($projectName):$($version)"
    BuildAndPush-DockerImage -dockerfileDir $dir -imageName $imageName
}