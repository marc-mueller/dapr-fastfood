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
    $buildid = [guid]::NewGuid().ToString()
    docker build -f "$($dockerfileDir)/Dockerfile" --build-arg BUILDID=$($buildid) --target test .
    if ($?) {
        Write-Output "Test stage was successfully run in $imageName ..."
       
        # Ensure the test results directory exists
        $testResultsDir = "./TestResults/$imageName/$buildid/"
        if (-not (Test-Path -Path $testResultsDir)) {
            New-Item -ItemType Directory -Path $testResultsDir | Out-Null
        }
        
        $id=docker images --filter "label=testresults=$($buildid)" -q | Select-Object -First 1
        docker create --name testcontainer-$buildid $id
        docker cp testcontainer-$($buildid):/testresults ./TestResults/$imageName/$buildid/
        docker rm testcontainer-$($buildid)
        
        
        # Get all *.trx files from the test results directory
        $trxFiles = Get-ChildItem -Path "./TestResults/$imageName/$buildid/testresults/" -Filter *.trx

        foreach ($trxFile in $trxFiles) {
            Write-Output "Reading test results from $($trxFile.FullName) ..."
            
            # Load the trx file and parse it as XML
            [xml]$trxContent = Get-Content $trxFile.FullName

            # Extract the statistics from the file
            $total = $trxContent.TestRun.ResultSummary.Counters.total
            $passed = $trxContent.TestRun.ResultSummary.Counters.passed
            $failed = $trxContent.TestRun.ResultSummary.Counters.failed

            Write-Output "Test Results for $($trxFile.Name): Total: $total, Passed: $passed, Failed: $failed"

            # Write an error if any tests failed
            if ($failed -gt 0) {
                Write-Error "There are $failed failing tests in $($trxFile.Name)"
            }
        }
        
    } else {
        Write-Error "Failed to run test stage in $imageName"
    }
    
    docker build -t $imageName -f "$($dockerfileDir)/Dockerfile" --target final .

    if ($?) {
        Write-Output "Pushing Docker image $imageName ..."
        #docker push $imageName

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