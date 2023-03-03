In order to be able to leverage the GitHub action deployment for your API and Lambda serverless function implementing the API, a few IAM roles and AWS resources need to be setup. Following the setup, the respective ARNs can be added to the environmental variables for your forked repository.

Overview of roles / resources to be created
| Name      | Type | Description |
| ----------- | ----------- | ------------- |
| S3 Bucket      | Resource | The created S3 bucket with store the code artifacts during deployment (e.g., the Lambda function code) |
| LambdaExecutionRole | Role | The role that will execute the deployed lambda function |
| CloudFormationRole   | Role | This role is where the permissions for deploying the application-specific resources are defined within AWS IAM |
| PipelineExecutionRole | Role | This role is assumed by the principal who is initiating the deployment process |

The resources and roles below can be created via the AWS admin console or CLI. The instructions describe the steps via the admin console.

### Step 1 - Create S3 Bucket

Create an S3 bucket as follows:
- Navigate to the *S3* service
- Click *Create bucket*
- Name the bucket uniquely and choose the appropriate _AWS Region_
- Keep remaining default settings and click *Create bucket*

_We'll come back and adjust permissions in a later step!_

### Step 2 - Create a LambdaExecutionRole
- Navigate to the *IAM* service and choose *Roles* in the Access Management pane (or via _Top Features_ menu)
- Click *Create role*
- Choose *AWS service* as the _Trusted entity type_ and *Lambda* as the _Use case_ (note we'll be overriding this anyway)
- Click *Next*
- On the _Add permissions_ page, search for `AWSLambdaBasicExecutionRole`, select it and click *Next*
- Enter a role name (e.g. `books-api-dev-bookstorefunction`)
- Click *Create role*

### Step 3 - Create a CloudFormationRole

Create a new IAM role as follows:
- Navigate to the *IAM* service and choose *Roles* in the Access Management pane (or via _Top Features_ menu)
- Click *Create role*
- Choose *AWS service* as the _Trusted entity type_ and *CloudFormation* as the _Use case_
- Click *Next*
- On the _Add permissions_ page click *Next*
- On the _Role details_ page, enter a meaningful name in the Role name input (e.g. `aws-actions-dev-CloudFormationExecutionRole`)
- Click *Create role*
- Open the role details again, and click on *Add permissions* > *Create inline policy*
- Choose the *JSON* tab, and paste the following
    ```
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Action": "*",
                "Resource": "*",
                "Effect": "Allow"
            }
        ]
    }
    ```
- Click *Review policy* and then choose an appropriate policy name (e.g. `GrantCloudFormationFullAccess`)
- Click *Create policy*

### Step 4 - Create a PipelineExecutionRole

Create a new IAM role as follows:
- Navigate to the *IAM* service and choose *Roles* in the Access Management pane (or via _Top Features_ menu)
- Click *Create role*
- Choose *AWS service* as the _Trusted entity type_ and *Lambda* as the _Use case_ (note we'll be overriding this anyway)
- Click *Next*
- On the _Add permissions'  click *Next*
- On the _Role details_ page, enter a meaningful name in the Role name input (e.g. `aws-actions-dev-PipelineExecutionRole`)
- Update the default description to something more meaningful (e.g. `This role is assumed by the principal who is initiating the deployment process`)
- Click *Create role*
- Open the role details again, and click on *Add permissions* > *Create inline policy*
- Choose the *JSON* tab, and paste the following:
    ```
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Action": "iam:PassRole",
                "Resource": "<PUT ARN OF ROLE FROM STEP 3 HERE>",
                "Effect": "Allow"
            },
            {
                "Action": [
                    "cloudformation:CreateChangeSet",
                    "cloudformation:DescribeChangeSet",
                    "cloudformation:ExecuteChangeSet",
                    "cloudformation:DeleteStack",
                    "cloudformation:DescribeStackEvents",
                    "cloudformation:DescribeStacks",
                    "cloudformation:GetTemplate",
                    "cloudformation:GetTemplateSummary",
                    "cloudformation:DescribeStackResource"
                ],
                "Resource": "*",
                "Effect": "Allow"
            },
            {
                "Action": [
                    "s3:DeleteObject",
                    "s3:GetObject*",
                    "s3:PutObject*",
                    "s3:GetBucket*",
                    "s3:List*"
                ],
                "Resource": [
                    "<PUT ARN OF S3 FROM STEP 1 HERE>/*",
                    "<PUT ARN OF S3 FROM STEP 1 HERE>"
                ],
                "Effect": "Allow"
            },
            {
                "Action": [
                    "iam:Get*",
                    "iam:List*",
                    "iam:CreateRole",
                    "iam:DeleteRole",
                    "iam:AttachRolePolicy",
                    "iam:DeleteRolePolicy",
                    "iam:PutRolePolicy",
                    "iam:TagRole",
                    "iam:UntagRole"
                ],
                "Resource": [
                    "<PUT ARN OF LAMBDA ROLE FROM STEP 2 HERE>"
                ],
                "Effect": "Allow"
            }
        ]
    }
    ```
- Replace the `<PUT ARN OF ROLE FROM STEP 3 HERE>` string with the ARN from the CloudFormationRole created in step 3
- Replace the `<PUT ARN OF S3 FROM STEP 1 HERE>` strings with the ARN of the S3 bucket created in step 1
- Replace the `<PUT ARN OF LAMBDA ROLE FROM STEP 2 HERE>` string the ARN from the LambdaExecutionRole created in step 2
- Click *Review policy* and then choose an appropriate policy name (e.g. `PipelineExecutionRolePermissions`)
- Click *Create policy*
- Choose *Trust relationships* and click *Edit trust policy*
- Paste in the following JSON:
    ```
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": {
                    "AWS": "<PUT ARN OF appropriate IAM pipeline user here>"
                },
                "Action": "sts:AssumeRole"
            }
        ]
    }
    ```
- Replace the `<PUT ARN OF appropriate IAM pipeline user here>` string with the user URN bound to the AWS credentials added to the GitHub Repo secrets
- Click *Update policy*

### Step 5 - Allow appropriate access to S3 bucket

Now we're going to allow access to the S3 bucket created in step 1, but limit access to the _CloudFormationRole_ and _PipelineExecutionRole_ created in steps 3 and 4.

- Navigate to the *S3* service, and choose *Buckets* (either from _Amazon S3_ menu pane or from the _Top Features_ shortcut menu)
- Open the S3 bucket created in *Step 1* above
- Choose the *Permissions* tab
- Click *Edit* within the _Block public access (bucket settings)_ and *uncheck* the _Block all public access' checkbox
- Click *Save changes* 
- Enter `confirm` into the prompt dialog and click *Confirm*
- Click *Edit* within the _Bucket policy' section
- Paste the following and then replace the relevant strings:
```
{
    "Version": "2008-10-17",
    "Statement": [
        {
            "Effect": "Deny",
            "Principal": "*",
            "Action": "s3:*",
            "Resource": [
                "<PUT ARN OF S3 FROM STEP 1 HERE>/*",
                "<PUT ARN OF S3 FROM STEP 1 HERE>"
            ],
            "Condition": {
                "Bool": {
                    "aws:SecureTransport": "false"
                }
            }
        },
        {
            "Effect": "Allow",
            "Principal": {
                "AWS": [
                    "<PUT ARN OF ROLE FROM STEP 3 HERE>",
                    "<PUT ARN OF ROLE FROM STEP 4 HERE>"
                ]
            },
            "Action": [
                "s3:GetObject*",
                "s3:PutObject*",
                "s3:GetBucket*",
                "s3:List*"
            ],
            "Resource": [
                "<PUT ARN OF S3 FROM STEP 1 HERE>/*",
                "<PUT ARN OF S3 FROM STEP 1 HERE>"
            ]
        }
    ]
}
```
- Replace the `<PUT ARN OF S3 FROM STEP 1 HERE>` strings with the ARN of the S3 bucket created in step 1 (this is the ARN of the bucket you're editing)
- Replace the `<PUT ARN OF ROLE FROM STEP 3 HERE>` string with the ARN from the CloudFormationRole created in step 3
- Replace the `<PUT ARN OF ROLE FROM STEP 4 HERE>` string the ARN from the PipelineExecutionRole created in step 4
- Click *Save changes*

### Step 6 - Update the Actions, secrets and variable for your forked repository

#### Add the appropriate AWS credentials to repository secrets
- Within your forked repository,navigate to *Settings*
- From the left-hand navigation pane, navigate to _Security > Secrets and variables > Actions_
- Set up two secrets called `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` with the appropriate key credentials from the AWS user

Follow the [principle of least privileges](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html#grant-least-privilege) guide.

#### Replace the appropriate environment variables within the GitHub Action
- Within your forked repository,navigate to *Settings*
- From the left-hand navigation pane, navigate to _Security > Secrets and variables > Actions_
- Choose the *Variables* tab and add the following variables

| Name      | Value | 
| ----------- | ----------- | 
| PIPELINE_EXECUTION_ROLE | Put in the ARN from the PipelineExecutionRole created in step 4 |
| CLOUDFORMATION_EXECUTION_ROLE | Put in ARN from the CloudFormationRole created in step 3 |
| ARTIFACTS_BUCKET | Put in *NAME* of the S3 bucket created in step 1 |
| AWS_REGION | Put in the region you want the resources to be created in |



