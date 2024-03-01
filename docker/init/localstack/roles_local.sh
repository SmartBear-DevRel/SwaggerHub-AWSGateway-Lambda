#!/bin/bash
set -x
awslocal iam create-role --role-name admin --assume-role-policy-document '{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Action":"*","Resource":"*"}]}'
set +x
