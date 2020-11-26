# REQUIREMENTS

0. Read the license
1. .NET 5 installed
2. Access to a SQL Database

# SETUP GUIDE
1. Configure appsettings.production.json
2. Deploy application using IIS (TBA: Docker) (use Release version for deploy, which use apppsettings.production.json)
3. Configure the Organizations using URLs and PAT Tokens

# USE GUIDE
1. Create 1+ organizations
2. If using CodeHub (local) provider - Create a Project and Epic in that Organization
3. If using External Work Providers, configure an Organization with a PAT Token and Organization URL
4. Check WorkProvider pages to see if your data was properly received


# EXTENSIONS: 
- You can add a Work Provider (ex. Jira, GitLab), by implementing functionality available in IWorkProvider, and using DevOps as an example
- Required Work Item properties: Id, WorkProviderType, Title, ChangedDate, WorkItemType
- You can use different SQL Database by changing RegisterDatabaseExtensions. No further changes should be required with SQL like databases, however NoSQL databases haven't been tested
