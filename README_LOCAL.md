# Local Execution

## Prerequisities

- [Docker](https://docs.docker.com/get-docker/) (to run LocalStack)
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- [SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html)

Optional

- [awslocal](https://docs.localstack.cloud/user-guide/integrations/aws-cli/)
- [sam-cli-local](https://github.com/localstack/aws-sam-cli-local)

If not using awslocal, you will need to configure an aws localstack profile

```sh
aws configure --profile localstack
```

For the following

- AWS Access Key ID `AKIAIOSFODNN7EXAMPLE`
- AWS Secret Access Key `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY`
- AWS default region `us-east-1`
- Default output forman `None`

## Steps

There is a `Makefile` containing command commands for Unix based systems.
There is a `runWin.ps1` containing commands for Windows based systems.

Windows users must source the powershell script first with

```powershell
 . .\runWin.ps1
```

1. Start up LocalStack via Docker

Linux/MacOS

```sh
make up
```

Windows

```powershell
 docker_up
```

2. Run unit tests

Linux/MacOS

```sh
make sam_test
```

Windows

```powershell
 sam_test
```

3. Build our lambda

Linux/MacOS

```sh
make sam_build
```

Windows

```powershell
 sam_build
```

4. Package our lambda

Linux/MacOS

```sh
make sam_package
```

Windows

```powershell
 sam_package
```

5. Deploy our lambda

Linux/MacOS

```sh
make sam_deploy
```

Windows

```powershell
 sam_deploy
```

5. Get our deployed endpoint

Linux/MacOS

```sh
make sam_output_endpoint
```

Windows

```powershell
 sam_output_endpoint
```


6. Test our lambda via cURL - /books endpoint

Linux/MacOS

```sh
make sam_curl_books_endpoint
```

Windows

```powershell
 sam_curl_books_endpoint
```

7. Test our lambda via cURL - /books/{id} endpoint

Linux/MacOS

```sh
make sam_curl_books_id_endpoint
```

Windows

```powershell
 sam_curl_books_id_endpoint
```

8. Run integration tests

Linux/MacOS

```sh
make sam_integration_test
```

Windows

```powershell
 sam_integration_test
```
