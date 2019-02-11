---
title: "Using Service Connections in Azure DevOps"
date: 2019-02-07T13:45:59+10:00
draft: false
type: "post"
---

I was recently setting up a build pipeline in Azure DevOps and realised I didn't have access to the Azure resources to be able to pick the app service as a deployment target.  
The easy way would be to get my account access to the Azure resources, but:  

1. I'd be the only one with the ability to change those deployment options, or at least new devs would to get access as well before they could.  
2. The onsite guy for that kind of access was away.  

I had access to an account that had Azure resource access, so there had to be a way to use it in the pipeline somehow.  
Turns out there is, and it's via the [Azure Service Connections](https://docs.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops). 
Most of this is based off [this tutorial](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal) from Microsoft.  

They use applications and service principals and the like to connect to Azure Resources (or a [multitide](https://docs.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops#common-service-connection-types) of other services like Github), and provide them for Pipeline access.  

So to illustrate the situation:  
{{% image path="/img/ServiceConnections/Situation" alt="Situation is two accounts with different access" %}}  

Starting with the **Azure Portal account**, let's create an app registration to represent our connection to Azure AD.  
{{% image path="/img/ServiceConnections/CreatingAppRegistration" alt="Creating an App Registration in AD" %}} 
{{% image path="/img/ServiceConnections/CreatingApp" alt="Signon url is not important here" %}} 

Then we need to assign that registration to the subscription containing the resources we want via Access Control.
{{% image path="/img/ServiceConnections/GettingSubscription" alt="Assigning app registration to subscription" %}} 
{{% image path="/img/ServiceConnections/SubscriptionRoleAssignment" alt="Creating the role assignment" %}} 

That's the basic plumbing. There's security and stuff that can be added at those various steps (e.g. who's allowed to access the app registration, etc), but that's all I needed.   

Now we need to get some bits that Azure DevOps needs to make the link.  
{{% image path="/img/ServiceConnections/GettingAppRegistrationId" alt="Get the service connection ID (App registration ID)" %}} 
{{% image path="/img/ServiceConnections/DirectoryGuid" alt="AAD Tenant ID" %}} 

Now using the **Azure DevOps account** in Azure DevOps:
{{% image path="/img/ServiceConnections/NewServiceConnection" alt="Creating a new service connection in Azure DevOps" %}}  

NOTE the slight catch here is that we need to go to the advanced view in order to specify the app registration we created.
{{% image path="/img/ServiceConnections/AdvancedNewServiceConnection" alt="Additional steps when creating the service connection" %}} 


And that's done! Now your pipelines can access the Azure resources.  