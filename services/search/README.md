# Deployment instructions

1. Run azuredeploy.json to deploy Azure Search instance and Search indexer Function App.
1. Once deployed, confirm that the Function App has the proper app settings, including settings for accessing Search.
1. Configure the Function App to deploy from this GitHub repo.
1. Once the Function App is deployed, open the `UpdateProjects` function in the portal and click the Run button to load projects into the search index.
1. Open the Search instance in the portal and confirm that the indexing succeeded.
1. Update the website with the search settings. Use the pre-generated query key instead of the admin key.
1. Set up a webhook in the `dotnet/foundation` GitHub repo to call `UpdateProjects` whenever there is a push to the repo.