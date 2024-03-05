#!/bin/bash
set -o pipefail

  AWS_TEMP_CREDS=`awslocal sts assume-role --role-arn ${ARN_ROLE} --role-session-name ${ARN_SESSION_NAME}| jq -c '.Credentials'`
  export AWS_ACCESS_KEY_ID=`echo $AWS_TEMP_CREDS | jq -r '.AccessKeyId'`
  export AWS_SECRET_ACCESS_KEY=`echo $AWS_TEMP_CREDS | jq -r '.SecretAccessKey'`
  export AWS_SESSION_TOKEN=`echo $AWS_TEMP_CREDS | jq -r '.SessionToken'`

echo running command "$@"

"$@"