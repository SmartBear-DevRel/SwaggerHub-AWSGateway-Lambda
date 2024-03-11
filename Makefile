
EXPLORE_SPACES_FILE:=ExploreSpaces.json

sam_test:
	dotnet test --arch x64 books-api/test/Bookstore.Test/Bookstore.Tests.csproj
sam_build:
	cd books-api/src/Bookstore && \
	sam build --template ../../template.yaml --no-cached
sam_start:
	cd books-api/src/Bookstore && \
	sam local start-api --warm-containers eager
sam_curl_books_endpoint_local:
	curl http://127.0.0.1:3000/books | jq .
sam_curl_books_id_endpoint_local:
	if [[ $${BOOK_ID} == "" ]]; then \
		echo BOOK_ID must be set;\
		exit 1;\
	fi;
	curl http://127.0.0.1:3000/books/$${BOOK_ID} | jq .

install_explore_cli:
	dotnet tool install --global Explore.Cli

upload_to_explore:
	if [[ "${SESSION_TOKEN}" == "" ]]; then \
		echo "please set SESSION_TOKEN env var";\
		exit 1;\
	fi
	if [[ "${XSRF_TOKEN}" == "" ]]; then\
		echo "please set XSRF_TOKEN env var";\
		exit 1;\
	fi
	explore.cli import-spaces --explore-cookie "SESSION=${SESSION_TOKEN};XSRF-TOKEN=${XSRF_TOKEN}" -fp $(EXPLORE_SPACES_FILE)