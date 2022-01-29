---
title: "31 Docker Environments"
date: 2022-01-27T16:24:16+10:00
draft: true
type: "post"
slug: "31-docker-environments"
tags: ["docker"]
---



<!--more-->  

Readings
STARTED WITH https://mherman.org/blog/dockerizing-a-react-app/
https://docs.docker.com/samples/aspnet-mssql-compose/
https://blog.logrocket.com/docker-sql-server/
https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment?view=sql-server-ver15&pivots=cs1-bash
https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver15&preserve-view=true&pivots=cs1-bash
https://www.pluralsight.com/blog/software-development/how-to-build-custom-containers-docker
https://www.section.io/engineering-education/build-and-dockerize-a-full-stack-react-app-with-nodejs-and-nginx/
https://towardsdatascience.com/deploying-a-react-nodejs-application-with-docker-part-i-of-ii-910bc6edb46e
https://medium.com/geekculture/getting-started-with-docker-in-your-react-js-application-the-basics-6e5300cf749d

looots of stack overflow

- More documentation, some in code comments to port here, more to write
- blog post

and make sure it's doing what I want, how I want.

- make sure default distro is ubuntu
- copy code to wsl
- open in VSCode (remote-wsl)
- "docker compose up"
- ???
- PROPHIT?

- SQL needs Compmgmt --> Sql server configuration manager --> SQL server network configuration --> TCP/IP --> Enable, then make sure that in "IP addresses" make sure the ports are the same as we want (1433 is default)

- If you have SQL server on the host, ENSURE your port bindings for SQL container are NOT 1433, so something like 1644:1433 will map the CONTAINER 1433 --> host 1644
  - Avoids the collision with host


- Pretty cool, React has a 'proxy' setting in `package.json` where it can forward requests to the backend and avoid cors issues, etc.
  - Further, the networking features of Docker means that if you have the backend service called `api` (e.g.) you can set that proxy to `http://api:5555` (my custom port was 5555) and it will "just work"

- While changing the dockerfile that VS uses isn't super useful, you can set the launchSettings to the specific ports you want to be mapped outside the container. i.e. for use in the React project above.

- THe MAIN thing here, is that the VS project setup and the rest of the Docker project stack aren't really shared.
- VS uses it's own dockerfile setup, and I don't see a way to make it use the docker-compose setup.
- It IS possible to run docker-compose and have the whole stack setup, then debug into that running container, but that's not the nicest experience.
- It's further possible to have the container run `dotnet watch` so you can change the code and have it reflected BUT that's messy and not a great experience either (when VS doesn't behave...)

- Having a hard time getting a nice F5 experience...
  - Current method is having the whole stack there, but not running up the API part
  - Spin the others up, but use Visual Studio to run the APi part OUTSIDE the stack
  - VS connects the container to the stack network for comms
  - ISSUE can't get stupid ssl to work, so having to fall back to http
    - For some reason, I can't communicate with the 443 port on the internal container
    - It's exposed and everything, and works for the VS container, but not for the shared one
    - It could be another port conflict situation where 443 is taken by the host and it's looking on the host even though it shouldnt...
    - Current solution: fall back to http for dev
      - NOTE that the container variables for the VS container still says ASPNETCORE_URLS=http:/+:80, but it seems to work pointing at :5555


Something to note, the VS container doesn't stop when you stop debugging in VS. 

[Github](https://github.com/kael-larkin/DockerisationExperiment)