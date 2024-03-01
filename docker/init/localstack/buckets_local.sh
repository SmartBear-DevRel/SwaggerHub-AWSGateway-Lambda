#!/bin/bash
set -x
awslocal s3 mb s3://${ARTIFACTS_BUCKET}
set +x
