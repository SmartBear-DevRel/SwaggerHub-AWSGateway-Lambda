# SwaggerHub-AWSGateway-Lambda
A sample solution taking an API definition from SwaggerHub, crafting a Lambda function from the API definition, and then publishing the function behind AWS Gateway.

![SwaggerHub-AWSGateway-AWSLambda](./SwaggerHub-AWSAPIGateway-Lambda.png)

 ## Problem Statement
 
 API design tools and API gateways are two essential components in the development of modern API-based applications. Integrating these tools not only streamlines the API development process but also provides numerous benefits that enhance API functionality, security, and scalability.
 
SwaggerHub is a multi-spec API design and documentation tool that seamlessly integrates with Amazon Web Services. With this integration, you can establish a relationship between your design/development and your gateway/production environment. Having an integration between the design-time and the run-time allows you to push your API documentation directly from SwaggerHub into your gateway. Every time you update your document in SwaggerHub trigger the required processes to automate the journey towards the API management plane. 

Managing your APIs from your gateway becomes a simpler task when you have your API documentation loaded into the tool automatically, there's no need to manually define how your APIs behave when SwaggerHub pushes your API definitions automatically. 
 
By leveraging the seamless integration between SwaggerHub’s API design and documentation capabilities with AWS’s scalable and feature-rich gateway, organizations can confidently deliver innovative, customer-centric services and applications. 

## Solution Overview
ToDo
## Prerequisites

You will need an AWS subscription and a SwaggerHub account in order to be able to work on the API design, AWS Lambda implementation, and automated deployment to AWS API Gateway.

1. Sign up for an [AWS account](https://aws.amazon.com/free/) (if required)
2. Sign up for a [SwaggerHub trial](https://try.smartbear.com/swaggerhub?utm_medium=product&utm_source=GitHub&utm_campaign=devrel-marketplaces-api&utm_content=code-samples) account (if required)
3. Sign up for a [GitHub account](https://github.com/join) (if required)

The sample Lambda function generated within this repo uses `dotnet6`. If you would like to edit the code implementation, then the following are required:
- [VS Code](https://code.visualstudio.com/download) (or similar IDE)
- Install [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Install Amazon Lambda Tools
  - `dotnet tool install -g Amazon.Lambda.Tools`
- Install [SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) (AWS Serverless Application Model Command Line Interface)

## Instructions

The instructions laid out below cover the following steps:
1. Forking the repository for your local needs
2. Importing the _Book API_ into SwaggerHub
3. Setup _Auto Mock_ integration in SwaggerHub
4. Test the _Auto Mock_ integration
5. IAM Role Setup for Deployment towards AWS
6. Run GitHub Action to deploy _AWS API Gateway_ and _AWS Lambda Function_
7. Update the SwaggerHub Books API with the _AWS API Gateway_ endpoint
8. Calling your _AWS API Gateway_ hosted Books API from SwaggerHub

OK - let's get started!

### Fork the repo
- Fork the repo to your local GitHub profile/organization

### Import the _Book API_ into SwaggerHub
- Login into SwaggerHub
- From the **Create New** menu select **Import and Document API**
- Enter the following URL in the **Path or URL** input box
  - `https://raw.githubusercontent.com/SmartBear-DevRel/SwaggerHub-AWSGateway-Lambda/main/API-Definition/openapi.yaml`
- Press the **Import** button
- In the next pop-up window click the **Import Definition** button

### Setup _Auto Mock_ integration in SwaggerHub
- In the SwaggerHub Portal page, click on the `Books API`
- In the left pane, click on the API name `books-api`
- Click on the **Integrations** tab
- Click on **Add New Integrations**
- In the Integrations dropdown, select **API Auto Mocking** and click the **ADD** button
- In the Name text box, enter `Auto Mocking`
- Click the **CREATE AND EXECUTE** button
- Click on the **DONE** button
- Click on the API name `book-api` again to return to the editor view

> Note that a new description and url tags have been added in the servers section of the API 

### Test the _Auto Mock_ integration
- In the right panel, in the **Servers** dropdown, select the **SwaggerHub API Auto Mocking** server URL
- Open any of the Method/Path end-points (e.g. `GET /books`) and click the **Try it out** button
- Provide any `required` parameters
- Click the **Execute** button
- Review the data returned in the Server response box


> In the next steps, we'll deploy a working version of the `books-api` to AWS API Gateway and have a functioning Lambda function as the API implementation. Once deployed, we'll call the implemented API from SwaggerHub!
### IAM Role Setup for Deployment to AWS
- Follow the [IAM and Resource Setup Guide](./IAM_DEPLOYMENT_ROLES.md) to ensure you can run the pipeline

### Run GitHub Action to deploy _AWS API Gateway_ and _AWS Lambda Function_
- In your forked GitHub repository, navigate to the _Actions_ tab
- Click on the `Pipeline` action on the left-hand pane
- Run the `Pipeline` workflow by clicking on the **Run workflow** button
- Once the pipeline has completed, navigate to the bottom of the pipeline summary and locate the **output-endpoint summary** section
- Copy the **AWS_API_Gateway_Endpoint** URL

> We'll need the API endpoint above to call the API, so keep it to hand!
### Update the SwaggerHub Books API with the _AWS API Gateway_ endpoint
- In the SwaggerHub Portal page, click on `Books API` to open the API definition
- Locate the `servers` tag, and replace the `url` for the `AWS API Gateway Endpoint` server tag (currently holding a value of `https://replace-me.com`) with the value of the URL value copied from the pipeline summary above
- Click the **Save** button

### Calling your _AWS API Gateway_ hosted Books API from SwaggerHub
- In the SwaggerHub Portal page, click on `Books API` to open the API definition
- In the _SwaggerUI_ pane (the right-hand pane), choose the _AWS API Gateway Endpoint_ from the **Servers** dropdown
- Expand `GET /books`, click the **Try it out** button
- Optionally, enter an `title` or `author` query parameter
- Click **Execute**

