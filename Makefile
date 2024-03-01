.PHONY: docker 

LOCALSTACK_URL:=localhost:4566

# Allow for LocalStack existing running instance
up:
	if curl -sSf $$LOCALSTACK_URL> /dev/null; then\
		make docker_no_localstack;\
	else\
		make docker;\
	fi
docker:
	docker-compose up -d --build
docker_no_localstack:
	docker-compose config --services | grep -v localstack | xargs docker-compose up -d 

s3_copy:
	awslocal s3 cp --recursive s3://${ARTIFACTS_BUCKET} ./tmp

logs_localstack:
	docker logs -f swaggerhub-awsgateway-lambda-localstack-1

sam_pre_reqs: setup_localstack
sam_test:
	dotnet test --arch x64 books-api/test/Bookstore.Test/Bookstore.Tests.csproj
sam_build:
	samlocal build --template books-api/template.yaml
sam_package:
	samlocal package \
            --s3-bucket $${ARTIFACTS_BUCKET:-swaggerhub-awsgateway-lambda-artifacts} \
            --region $${AWS_DEFAULT_REGION} \
            --output-template-file packaged-lambda.yaml
sam_deploy:
	samlocal deploy --stack-name $${STACK_NAME:-books-api-dev} \
            --template packaged-lambda.yaml \
            --region $${AWS_DEFAULT_REGION} \
            --s3-bucket $${ARTIFACTS_BUCKET:-swaggerhub-awsgateway-lambda-artifacts} \
            --no-fail-on-empty-changeset \
            --role-arn 'arn:aws:iam::000000000000:role/admin'
sam_intgration_test:
	dotnet test books-api/test/Bookstore.IntegrationTest/Bookstore.IntegrationTest.csproj
sam_output_endpoint:
	@awslocal cloudformation describe-stacks --stack-name books-api-dev --query "Stacks[0].Outputs[0].OutputValue" --output text
sam_curl_endpoint:
	curl localhost:4566/restapis/$(shell make sam_output_endpoint | cut -d'/' -f3 | awk -F'.' '{print $$1}')/Prod/_user_request_/books | jq .

setup_localstack:
	docker/init/localstack/buckets_local.sh
	docker/init/localstack/roles_local.sh
