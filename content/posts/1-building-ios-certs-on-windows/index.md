---
title: "Building iOS store certificates on Windows"
date: 2018-05-31T07:11:59+07:00
draft: false
type: "post"
slug: "building-ios-certs-on-windows"
tags: ["ios"]
---

UPDATE Oct 2019: 2FA breaks things :(  

How many times have you been working on a cross platform app and been ready to submit to the app stores, but then the Apple store needs a .csr that you should “use a Mac” to generate?  

When I google this, I got lots of complicated methods using IIS to request certificates through a quite frankly terrible UI/UX, and most of the guides glossed over how to actually get that .p12 at the end you needed.
  
<!--more-->  

UPDATE: 2FA breaks the automated links with the AppStore, making things quite painful indeed.  
While this is all still correct, you may still need a Mac to help if you have 2FA on the Apple Id.  
Details at the end.  

I’m sure some of those guides work, but I was sure it had to be easier than that. I found [this post](http://www.iandevlin.com/blog/2012/11/phonegap/building-an-ios-signing-key-for-phonegap-in-windows/) which does it all in four commands…A colleague pointed out that it’s from 2012 and is still relevant, but it works.  

I’ll pull it out here in case the links break but this is all Ian Devlin’s work. I’m just replicating it.  

1. Install [OpenSSL](https://www.openssl.org/) (Directly didn’t work for me, but it came installed with [Cmder](http://cmder.net/))  

2. Generate a key  

    ``` cmd
        openssl genrsa -des3 -out ios.key 2048 // must be 2048
    ```

3. Use key to generate Certificate Signing Request (CSR)  

    ``` cmd
        openssl req -new -key ios.key -out <csrName>.csr -subj '/emailAddress=MY-EMAIL-ADDRESS, CN=COMPANY-NAME, C=COUNTRY-CODE'
    ```

4. Upload the .CSR to the portal which then gives you a .CER in return
5. Convert .CER to a .P12 (Required to sign apps, or at least Cordova apps)

    ``` cmd 
        openssl x509 -in ios_<development/distribution>.cer -inform DER -out <pemName>.pem -outform PEM
    ```

    ``` cmd 
        openssl pkcs12 -export -inkey ios.key -in <pemName>.pem -out <p12Name>.p12
    ```

That’s it! No more hunting for someone around the office with a Mac.  

**UPDATE Oct 2019**  
I had an issue where the only Apple ID in use by the client (and our CI pipeline) was upgraded with 2FA.  
This caused the old 'deploy to AppStores' stuff to break, and apparently there are only two fixes at this time (without xcode):  

> According to the official documentation on the Microsoft Azure Devops 'Deploy to Appstore' task, 2FA shouldn't be enabled on the CI Apple ID, and there should be one specifically for CI. This wasn't an option for me at the time.  
  
  
- Deploy from AppCenter (Needs App Specific passwords, which it'll tell you about and is fairly well documented)  
  - NOTE that this automatically deploys and you aren't able to change any settings outside of what AppCenter provides (name, icons, screenshots, etc)  
- Get a Fastlane session token for use with the Azure Devops task  
  - Get a mac :(  
  - Install Ruby and Fastlane  
  - Be logged in with the relevant Apple Id  
  - `fastlane spaceauth -u <appleId user name>`
  - Copy that crazy cookie off the Mac  
  - You're already likely using a service connection to deploy to the App Store via that MS task (if not, HOW!!), so edit that and add the Fastlane cookie as the Fastlane Session  