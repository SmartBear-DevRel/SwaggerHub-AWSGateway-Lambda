#!/bin/bash
set -e

### Step 1 - Create S3 Bucket

awslocal s3 mb s3://${ARTIFACTS_BUCKET}

### Step 2 - Create a LambdaExecutionRole
awslocal iam create-role --role-name books-api-dev-bookstorefunction --assume-role-policy-document '{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "sts:AssumeRole"
            ],
            "Principal": {
                "Service": [
                    "lambda.amazonaws.com"
                ]
            }
        }
    ]
}'

### Step 3 - Create a CloudFormationRole

awslocal iam create-role --role-name aws-actions-dev-CloudFormationExecutionRole --assume-role-policy-document '{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Action":"*","Resource":"*"}]}'

### Step 4 - Create a PipelineExecutionRole

awslocal iam create-role --role-name aws-actions-dev-PipelineExecutionRole --assume-role-policy-document "{
        \"Version\": \"2012-10-17\",
        \"Statement\": [
            {
                \"Action\": \"iam:PassRole\",
                \"Resource\": \"${CLOUDFORMATION_EXECUTION_ROLE_ARN}\",
                \"Effect\": \"Allow\"
            },
            {
                \"Action\": [
                    \"cloudformation:CreateChangeSet\",
                    \"cloudformation:DescribeChangeSet\",
                    \"cloudformation:ExecuteChangeSet\",
                    \"cloudformation:DeleteStack\",
                    \"cloudformation:DescribeStackEvents\",
                    \"cloudformation:DescribeStacks\",
                    \"cloudformation:GetTemplate\",
                    \"cloudformation:GetTemplateSummary\",
                    \"cloudformation:DescribeStackResource\"
                ],
                \"Resource\": \"*\",
                \"Effect\": \"Allow\"
            },
            {
                \"Action\": [
                    \"s3:DeleteObject\",
                    \"s3:GetObject*\",
                    \"s3:PutObject*\",
                    \"s3:GetBucket*\",
                    \"s3:List*\"
                ],
                \"Resource\": [
                    \"${ARTIFACTS_BUCKET_ARN}/*\",
                    \"${ARTIFACTS_BUCKET_ARN}\"
                ],
                \"Effect\": \"Allow\"
            },
            {
                \"Action\": [
                    \"iam:Get*\",
                    \"iam:List*\",
                    \"iam:CreateRole\",
                    \"iam:DeleteRole\",
                    \"iam:AttachRolePolicy\",
                    \"iam:DeleteRolePolicy\",
                    \"iam:PutRolePolicy\",
                    \"iam:TagRole\",
                    \"iam:UntagRole\"
                ],
                \"Resource\": [
                    \"${LAMBDA_EXECUTION_ROLE_ARN}\"
                ],
                \"Effect\": \"Allow\"
            }
        ]
    }"