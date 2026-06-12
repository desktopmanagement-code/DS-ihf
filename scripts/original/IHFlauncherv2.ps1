$ErrorActionPreference = "Stop" # force stop on failure

$configFile = ".\config\settings.json"

if ((Test-Path $configFile) -eq $False) {
    Write-Output "Error: "
    Write-Output "Es fehlt die Datei '$configFile'."
   
}

# Get required environment variables from .\config\settings.json file
$config = Get-Content $configFile -Raw | ConvertFrom-Json

function checkCC {

    # Fill in Quickstart Carbon Copy config values
    if ($config.CC_EMAIL -eq "{CC_EMAIL}" ) {
        Write-Output "It looks like this is your first time running the launcher from Quickstart. "
        $config.CC_EMAIL = Read-Host "Enter a CC email address to receive copies of envelopes"
        $config.CC_NAME = Read-Host "Enter a name for your CC recipient"
        Write-Output ""
        write-output $config | ConvertTo-Json | Set-Content $configFile
    }

}

function  checkOrgId {

    if ($config.ORGANIZATION_ID -eq "{ORGANIZATION_ID}" ) {
        Write-Output "No Organization Id in the config file. Looking for one via the API"
        # Get required environment variables from .\config\settings.json file
        $accessToken = Get-Content .\config\ds_access_tokenJWT.txt

        $base_path = "https://api.docusign.net/management"

        $response = New-TemporaryFile
        Invoke-RestMethod `
            -Uri "$base_path/v2/organizations" `
            -Method 'GET' `
            -Headers @{
            'Authorization' = "Bearer $accessToken";
            'Content-Type'  = "application/json";
        } `
            -OutFile $response

        $organizationId = $(Get-Content $response | ConvertFrom-Json).organizations[0].id

        $config.ORGANIZATION_ID = $organizationId
        write-output $config | ConvertTo-Json | Set-Content $configFile
        Write-Output "Organization id has been written to config file..."
        Remove-Item $response

    }
}

function startSignature {
powershell.exe -Command .\SendIHFv3.ps1 
#|out-null
}
function startLauncher {
  

        $listApiView = $null;
	     powershell.exe -Command .\OAuth\jwt.ps1 -clientId $($config.INTEGRATION_KEY_AUTH_CODE) -apiVersion $("eSignature") |out-null

 if ((Test-Path "./config/ds_access_tokenJWT.txt") -eq $true) {
                    # powershell.exe -Command .\eg001EmbeddedSigning.ps1
                    # This is to prevent getting stuck on the
                    # first example after trying it the first time
                    $firstPassComplete = "true"
                    startSignature
                }
                else {
                    Write-Error "Failed to retrieve OAuth Access token, check your settings.json and that port 8080 is not in use"  -ErrorAction Stop
                }
#|out-null
}
     

#Write-Output "Welcome to the DocuSign PowerShell Launcher"
startLauncher
