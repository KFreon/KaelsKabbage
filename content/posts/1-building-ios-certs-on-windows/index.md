---
title: "Building iOS store certificates on Windows"
date: 2018-05-31T07:11:59+07:00
draft: false
type: "post"
slug: "building-ios-certs-on-windows"
tags: ["ios"]
---

**UPDATE Oct 2019:** 2FA causes significant problems  

How many times have you been working on a cross platform app and been ready to submit to the app stores, but then the Apple store needs a .csr that you should "use a Mac" to generate?  

When I google this, I got lots of complicated methods using IIS to request certificates through a quite frankly terrible UI/UX, and most of the guides glossed over how to actually get that .p12 at the end you needed.
  
<!--more-->  

> This is slightly unrelated to certificates, but the `codeSignIdentity` MUST be `iPhone Developer` regardless of production/development.  

I’m sure some of those guides work, but I was sure it had to be easier than that. I found [this post](http://www.iandevlin.com/blog/2012/11/phonegap/building-an-ios-signing-key-for-phonegap-in-windows/) which does it all in four commands…A colleague pointed out that it’s from 2012 and is still relevant, but it works.  

I’ll pull it out here in case the links break but this is all Ian Devlin's work. I’m just replicating it.  

1. Install [OpenSSL](https://www.openssl.org/) (Directly didn’t work for me, but it came installed with [Cmder](http://cmder.net/)) 
> Now I'd recommend using wsl.   

2. Generate a key  

    ``` cmd
        openssl genrsa -des3 -out ios.key 2048 // must be 2048
    ```

3. Use key to generate Certificate Signing Request (CSR)  

    ``` cmd
        openssl req -new -key ios.key -out <csrName>.csr -subj '/emailAddress=MY-EMAIL-ADDRESS, CN=COMPANY-NAME, C=COUNTRY-CODE'

        // Or if you have a config file (which replaces the passed in config above)
        openssl req -new -key ios.key -out <csrName>.csr -config config.txt
    ```

4. Upload the .CSR to the portal which then gives you a .CER in return
5. Convert .CER to a .P12 (Required to sign apps, or at least Cordova apps)

    ``` cmd  
        openssl x509 -in ios_<development/distribution>.cer -inform DER -out <pemName>.pem -outform PEM
    ```

    ``` cmd  
        // May need `-legacy` now as the cipher has changed.  e.g. `...-export -legacy -inkey...`
        openssl pkcs12 -export -inkey ios.key -in <pemName>.pem -out <p12Name>.p12

        // Or if you get a .crt that needs to be a .pfx
        openssl pkcs12 -export -inkey ios.key -in <crtName>.crt -out <pfxName>.pfx
    ```
> If you get "MAC verification failed, password incorrect?" augment the pkcs12 line above `...-export -legacy -inkey...` as the Azure agents haven't updated to openssl v3 yet apparently.
> [For reference](https://stackoverflow.com/questions/70431528/mac-verification-failed-during-pkcs12-import-wrong-password-azure-devops), it's an algorithm change between versions of openssl.  

That’s it! No more hunting for someone around the office with a Mac.  

**Update Aug 2020**: If you do want to run things up on a Mac at some point, you'll need those `.CER` and `.P12` files to install into the keychain.  The `CER` is the certificate, and the `P12` is the private key.  
Should just be able to double click them (`CER` first) and follow any prompts to get them into the keychain.  

**UPDATE Oct 2019**  
I had an issue where the only Apple ID in use by the client (and our CI pipeline) was upgraded with 2FA.  
This caused the old 'deploy to AppStores' stuff to break, and apparently there are only two fixes at this time (without xcode):  

> According to the official documentation on the Microsoft Azure Devops 'Deploy to Appstore' task, 2FA shouldn't be enabled on the CI Apple ID, and there should be one specifically for CI. This wasn't an option for me at the time.  
  
  
- Deploy from AppCenter (Needs App Specific passwords, which it'll tell you about and is fairly well documented)  
  - NOTE that this automatically deploys and you aren't able to change any settings outside of what AppCenter provides (name, icons, screenshots, etc)  
- Get a Fastlane session token for use with the Azure Devops task  
  - Get a mac 😢  
  - Install Ruby and Fastlane  
  - Be logged in with the relevant Apple Id  
  - `fastlane spaceauth -u <appleId user name>`
  - Copy that crazy cookie off the Mac  
  - You're already likely using a service connection to deploy to the App Store via that MS task (if not, HOW!!), so edit that and add the Fastlane cookie as the Fastlane Session  