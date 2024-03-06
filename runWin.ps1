$LOCALSTACK_URL = "localhost:4566"

# Allow for LocalStack existing running instance
function up {
	if (Test-Connection -ComputerName $LOCALSTACK_URL -Quiet) {
		docker_no_localstack
	} else {
		docker
	}
}

function docker {
	docker-compose up -d --build
}

function down {
	docker-compose down
}

function docker_no_localstack {
	docker-compose config --services | Select-String -Pattern "localstack" -NotMatch | ForEach-Object { docker-compose up -d $_ }
}

function env_vars {
	Write-Host $env:AWS_ACCESS_KEY_ID
	Write-Host $env:AWS_SECRET_ACCESS_KEY
	Write-Host $env:AWS_SESSION_TOKEN
}

function s3_copy {
	awslocal s3 cp --recursive s3://${env:ARTIFACTS_BUCKET} ./tmp
}

function s3_create {
	./docker/init/localstack/buckets_local.sh
}

function iam_create {
	./docker/init/localstack/roles_local.sh
}

function logs_localstack {
	docker logs -f swaggerhub-awsgateway-lambda-localstack-1
}

function sam_pre_reqs {
	setup_localstack
}

function sam_test {
	dotnet test --arch x64 books-api/test/Bookstore.Test/Bookstore.Tests.csproj
}

function sam_build {
	samlocal build --template books-api/template.yaml
}

function sam_package {
	samlocal package `
		--s3-bucket ${env:ARTIFACTS_BUCKET:-swaggerhub-awsgateway-lambda-artifacts} `
		--region ${env:AWS_DEFAULT_REGION} `
		--output-template-file packaged-lambda.yaml
}

function sam_deploy {
	samlocal deploy `
		--stack-name ${env:STACK_NAME:-books-api-dev} `
		--template packaged-lambda.yaml `
		--capabilities CAPABILITY_IAM `
		--region ${env:AWS_DEFAULT_REGION} `
		--s3-bucket ${env:ARTIFACTS_BUCKET:-swaggerhub-awsgateway-lambda-artifacts} `
		--no-fail-on-empty-changeset `
		--role-arn ${env:CLOUDFORMATION_EXECUTION_ROLE_ARN:-arn:aws:iam::000000000000:role/aws-actions-dev-CloudFormationExecutionRole}
}

function sam_integration_test {
	dotnet test --arch x64 books-api/test/Bookstore.IntegrationTest/Bookstore.IntegrationTest.csproj
}

function sam_output_endpoint {
	awslocal cloudformation describe-stacks --stack-name books-api-dev --query "Stacks[0].Outputs[0].OutputValue" --output text
}

function sam_curl_books_endpoint {
	$apiId = (sam_output_endpoint | Split-Path -Leaf).Split('.')[0]
	$url = "localhost:4566/restapis/$apiId/Prod/_user_request_/books"
	Invoke-RestMethod -Uri $url | ConvertTo-Json
}

function sam_curl_books_id_endpoint {
	$apiId = (sam_output_endpoint | Split-Path -Leaf).Split('.')[0]
	$url = "localhost:4566/restapis/$apiId/Prod/_user_request_/books/be05885a-41fc-4820-83fb-5db17015ed4a"
	Invoke-RestMethod -Uri $url | ConvertTo-Json
}

function setup_localstack {
	& ./docker/init/localstack/buckets_local.sh
	& ./docker/init/localstack/roles_local.sh
}
