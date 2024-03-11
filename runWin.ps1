

function sam_test {
	dotnet test --arch x64 books-api/test/Bookstore.Test/Bookstore.Tests.csproj
}

function sam_build {
	cd books-api/src/Bookstore; sam build --template ../../template.yaml --no-cached
}
function sam_start {
	cd books-api/src/Bookstore; sam local start-api --warm-containers eager
}

function sam_curl_books_endpoint_local {
	$url = "http://127.0.0.1:3000/books"
	Invoke-RestMethod -Uri $url | ConvertTo-Json
}

function sam_curl_books_id_endpoint_local {
	if ($env:BOOK_ID -eq "") {
		Write-Host "please set BOOK_ID env var"
		exit 1
	}
	$url = "http://127.0.0.1:3000/books/$env:BOOK_ID"
	Invoke-RestMethod -Uri $url | ConvertTo-Json
}

function install_explore_cli {
	dotnet tool install --global Explore.Cli
}
function upload_to_explore {
	if ($env:SESSION_TOKEN -eq "") {
		Write-Host "please set SESSION_TOKEN env var"
		exit 1
	}
	if ($env:XSRF_TOKEN -eq "") {
		Write-Host "please set XSRF_TOKEN env var"
		exit 1
	}
	explore.Cli import-spaces --explore-cookie "SESSION=$env:SESSION_TOKEN;XSRF-TOKEN=$env:XSRF_TOKEN" -fp $env:EXPLORE_SPACES_FILE
}