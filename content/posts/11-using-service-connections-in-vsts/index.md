---
title: "Using Service Connections in Azure DevOps"
date: 2019-02-07T13:45:59+10:00
draft: false
type: "post"
slug: "using-service-connections-in-vsts"
tags: ["azure"]
---

I was recently setting up a build pipeline in Azure DevOps and realised I didn't have access to the Azure resources to be able to pick the app service as a deployment target.  
The easy way would be to get my account access to the Azure resources, but:  

1. I'd be the only one with the ability to change those deployment options, or at least new devs would to get access as well before they could.  
2. The onsite guy for that kind of access was away.  

<!--more-->  


I had access to an account that had Azure resource access, so there had to be a way to use it in the pipeline somehow.  
Turns out there is, and it's via the [Azure Service Connections](https://docs.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops). 
Most of this is based off [this tutorial](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal) from Microsoft.  

They use applications and service principals and the like to connect to Azure Resources (or a [multitide](https://docs.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops#common-service-connection-types) of other services like Github), and provide them for Pipeline access.  

So to illustrate the situation:  
![Situation is two accounts with different access](img/Situation.png)  

Starting with the **Azure Portal account**, let's create an app registration to represent our connection to Azure AD.  
![Creating an App Registration in AD](img/CreatingAppRegistration.png) 
![Signon url is not important here](img/CreatingApp.png) 

Then we need to assign that registration to the subscription containing the resources we want via Access Control.
![Assigning app registration to subscription](img/GettingSubscription.png) 
![Creating the role assignment](img/SubscriptionRoleAssignment.png) 

That's the basic plumbing. There's security and stuff that can be added at those various steps (e.g. who's allowed to access the app registration, etc), but that's all I needed.   

Now we need to get some bits that Azure DevOps needs to make the link.  
![Get the service connection ID (App registration ID)](img/GettingAppRegistrationId.png) 
![AAD Tenant ID](img/DirectoryGuid.png) 

Now using the **Azure DevOps account** in Azure DevOps:
![Creating a new service connection in Azure DevOps](img/NewServiceConnection.png)  

NOTE the slight catch here is that we need to go to the advanced view in order to specify the app registration we created.
![Additional steps when creating the service connection](img/AdvancedNewServiceConnection.png) 


And that's done! Now your pipelines can access the Azure resources.  