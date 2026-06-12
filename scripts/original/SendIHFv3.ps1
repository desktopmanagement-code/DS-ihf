# Configuration
# 1.  Get required variables from .\config\settings.json:
$variables = Get-Content .\config\settings.json -Raw | ConvertFrom-Json
$CC_EMAIL = $variables.CC_EMAIL
$CC_NAME = $variables.CC_NAME

$SIGNER_EMAIL = $DSEMAIL
$SIGNER_NAME = $DSASP

function Select-FolderDialog {
	param(
        [string]$RootFolder = "MyComputer"
        )
        
	[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") | Out-Null
	$objForm = New-Object System.Windows.Forms.FolderBrowserDialog
	
    $objForm.RootFolder = $RootFolder
	
	$Show = $objForm.ShowDialog()
	If ($Show -eq "OK") {
         Return $objForm.SelectedPath
	   }
	Else {
         $a = ""
		 Return $a
	   }
    }


function Add-OemContent {
	param(
		$destination,
		$content
	)
	Add-Content -Path $destination -Value $content -Encoding oem -NoNewline
}
function Add-OemContentUTF {
     param(
        $destination,
        $content
                )
				Add-Content -Path $destination -Value $content -Encoding UTF8 -NoNewline
}

function sendDS {
	param(
	$filename,
	$DSEMAIL,
	$DSASP,
	$DSSubject
)


# Step 2. Obtain your OAuth access token
$accessToken = Get-Content ".\config\ds_access_tokenJWT.txt"

# Obtain your accountId from demo.docusign.net -- the account id is shown in
# the drop down on the upper right corner of the screen by your picture or
# the default picture.
$accountId = Get-Content ".\config\API_ACCOUNT_ID"

# ***DS.snippet.0.start

# Step 3. Construct the request body
#  document 1 (html) has tag **signature_1**
#  document 2 (docx) has tag /sn1/
#  document 3 (pdf) has tag /sn1/
#
#  The envelope has two recipients.
#  recipient 1 - signer
#  recipient 2 - cc
#  The envelope will be sent first to the signer.
#  After it is signed, a copy is sent to the cc person.

$apiUri = "https://eu.docusign.net/restapi"

# temp files
$requestData = New-TemporaryFile
$response = New-TemporaryFile

$doc1 = Get-Item $filename


#Write-Output "Sending the envelope request to DocuSign..."
#Write-Output "Results:"

$json = @{
	emailSubject = "Bitte unterzeichnen Sie dieses Dokument ($DSVA)";
	documents    = @(@{
			name          = $DSSubject;
			fileExtension = "docx";
			documentId    = "1";
		}); 
	recipients   = @{
		signers      = @(@{
				email        = $DSEMAIL;
				name         = $DSASP;
				recipientId  = "1";
				routingOrder = "1";
				tabs         = @{
					signHereTabs = @(@{
							anchorString  = "**signature_1**";
							anchorYOffset = "-10";
							anchorUnits   = "pixels";
							anchorXOffset = "20";
						}; @{
							anchorString  = "/sn1/";
							anchorYOffset = "-10";
							anchorUnits   = "pixels";
							anchorXOffset = "20";
						})
					DateSignedTabs = @(@{
							anchorString  = "/d1/";
							font="Arial"
							fontsize="Size11"
							
						})
					TextTabs = @(@{
							value="               "
							tooltip="hier bitte den Ort eintragen"
							font="Arial"
							fontsize="Size11"
							width="40"
							anchorString  = "/L1/";
							anchorYOffset = "-3";
							anchorUnits   = "pixels";
							
						})
				}
			});
		
	};
	status       = "sent"
} | ConvertTo-Json -Depth 32 -Compress;


$CRLF = "`r`n"
$boundary = "multipartboundary_multipartboundary"
Add-OemContent $requestData "--$boundary"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData "Content-Type: application/json"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData "Content-Disposition: form-data"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData "${CRLF}"
#Add-OemContent $requestData $json
Add-OemContentUTF $requestData $json
Add-OemContent $requestData "${CRLF}"

# Next add the documents. Each document has its own mime type,
# filename, and documentId. The filename and documentId must match
# the document's info in the JSON.

Add-OemContent $requestData "--$boundary"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData "Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData "Content-Disposition: file; filename=`"Vertrag `"$DSSubject;documentid=1"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData "${CRLF}"
Add-OemContent $requestData (Get-Content $doc1 -Encoding oem -Raw)
Add-OemContent $requestData "${CRLF}"

# Add closing boundary
Add-OemContent $requestData "--$boundary--"
Add-OemContent $requestData "${CRLF}"

# Send request
try {
	# Step 4. Call the eSignature REST API
	Invoke-RestMethod `
		-Uri "${apiUri}/v2.1/accounts/${accountId}/envelopes" `
		-Method 'POST' `
		-Headers @{
		'Authorization' = "Bearer $accessToken";
		'Content-Type'  = "multipart/form-data; boundary=${boundary}";
	} `
		-InFile (Resolve-Path $requestData).Path `
		-OutFile $response

	"Response: $(Get-Content -Raw $response)"|add-content '.\DSMailing.log'
}
catch {
	Write-Error $_
	$_|add-content '.\DSMailing.log'
}
# ***DS.snippet.0.end

#Get-Content $response

# cleanup
Remove-Item $requestData
Remove-Item $response

Write-Output "Done."
}
function selFolder
{
$dialog = New-Object System.Windows.Forms.FolderBrowserDialog
if ($dialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
    $directoryName = $dialog.SelectedPath
    Write-Host "Directory selected is $directoryName"
}
}


#main
$docxPath=Select-FolderDialog
$files = @(Get-ChildItem $docxPath"\*.docx")

$word = New-Object -ComObject Word.application


		foreach ($file in $files) {

            copy-item $file -Destination $env:temp
            
			$document = $word.Documents.Open($env:temp+"\"+$file.name)
			#$counter=0
			$zeilen= $document.Content.text.split("`n`r")
				foreach ($zeile in $zeilen) {
					$gefunden=$zeile -match '(?<=DSEMAIL: )\S*.*'
						if ($gefunden) {
							$DSEMAIL=$matches.0
						   }
					$gefunden=$zeile -match '(?<=DSASP: )\S*.*'
						if ($gefunden) {
							$DSASP=$matches.0
						   }
					$gefunden=$zeile -match '(?<=DSVA: )\S*.*'
						if ($gefunden) {
							$DSVA=$matches.0
						   }
				
				}
						 #  $DSASP
						  # $DSVA
						  # $DSEMAIL
						  
						sendDS $Document.fullname $DSEMAIL $DSASP $DSVA	
	
			$document.close()
		}
		$word.Quit() 
