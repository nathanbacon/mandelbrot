RESOURCE_GROUP_NAME=tfstateresources
STORAGE_ACCOUNT_NAME=ngtfstate21823
CONTAINER_NAME=tfstate
KEY_VAULT_NAME=nathangbuildvault21823
az group create --name $RESOURCE_GROUP_NAME --location westus
az group create --name $RESOURCE_GROUP_NAME --location westus
az storage account create --resource-group $RESOURCE_GROUP_NAME --name $STORAGE_ACCOUNT_NAME --sku Standard_LRS --encryption-services blob
az storage container create --name $CONTAINER_NAME --account-name $STORAGE_ACCOUNT_NAME
ACCOUNT_KEY=$(az storage account keys list --resource-group $RESOURCE_GROUP_NAME --account-name $STORAGE_ACCOUNT_NAME --query '[0].value' -o tsv)
az keyvault create --name $KEY_VAULT_NAME --resource-group $RESOURCE_GROUP_NAME
az keyvault secret set --vault-name $KEY_VAULT_NAME --name tf-backend-key --value $ACCOUNT_KEY

SERVICE_PRINCIPAL_NAME=nathanbacongithubsp
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
OUTPUTJSON=$(az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --role contributor --scopes /subscriptions/$SUBSCRIPTION_ID --sdk-auth)
# az role assignment create --assignee $SERVICE_PRINCIPAL_NAME --role Reader --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/mandelbrotresources
echo $OUTPUTJSON
CLIENT_ID=$(echo "$OUTPUTJSON" | jq -r '.clientId')
CLIENT_SECRET=$(echo "$OUTPUTJSON" | jq -r '.clientSecret')

az keyvault secret set --vault-name $KEY_VAULT_NAME --name github-sp-clientid --value $OUTPUTJSON > /dev/null
az keyvault secret set --vault-name $KEY_VAULT_NAME --name github-sp-clientid --value $CLIENT_ID > /dev/null
az keyvault secret set --vault-name $KEY_VAULT_NAME --name github-sp-clientsecret --value '$CLIENT_SECRET' > /dev/null
