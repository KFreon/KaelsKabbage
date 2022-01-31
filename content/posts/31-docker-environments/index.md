---
title: "Dockerising a full (SQL, aspnetcore, React) stack with a nice F5 experience"
date: 2022-01-27T16:24:16+10:00
draft: true
type: "post"
slug: "dockerising-sql-aspnetcore-react-with-nice-f5"
tags: ["docker"]
---

I've been interested in playing with Docker for a while, and I finally came up with an example I wanted to get running.  
A full stack project with good F5 debugging experience and easy onboarding!  
I succeeded! Kinda...   

<!--more-->  

{{% toc levels="two" %}}  

----------   
I had to do a lot of reading for this (listed at the bottom), and the final code solution is [here](https://github.com/kael-larkin/DockerisationExperiment) on Github (also link at bottom)

# Goals  
My goal here is simple: I want a template project for how to create a dockerised environment.  

I want a new dev coming onto a project to be able to:  

- Get up and running quickly without having to worry about databases (or data), specific versions of things like node or dotnet, etc  
- Have a low friction development experience (i.e. F5 in Visual Studio, simple database and UI execution and update)  

The end solution is a standard Create React App that GET's from an Api which queries an SQL database, all from within Docker.  
We'll be able to visit `https://localhost:3000` and see the standard CRA app, where the "Learn React" link now says "Learn React from {Name from Database entry}" (in this demo, it'll be 'Learn React from me').  

# Setup  
- Windows
- WSL2 with Ubuntu distro installed (with NodeJS installed)  
- Visual Studio 2022 (with Container Tools installed)  
- VSCode (with Remote-WSL and container extensions installed)  
- Docker Desktop with Linux containers  
- Some basic working knowledge of Dockerfiles (there are images and containers, and commands like FROM, and COPY, etc)  


## Setup/startup gotchas to avoid  
I ran into some issues when working through these, so we'll start on the right foot this time.  

### WSL2 default distro  
Somehow, my default distro was set to `docker-desktop` which is not right at all...  
It was causing all sorts of problems, like the `docker` command not working, along with other commands just not behaving like I expected them to.  
Ensure your default WSL2 distro is set to something proper (Ubuntu for me).
You can check with `wsl --list`, and set it with `wsl -s Ubuntu` (for example)  

{{< image path="img/WSL2DefaultDistro" alt="My WSL2 Distros" >}}  

### Issues with NodeJS on Windows host interfering with WSL  
The Windows path is added to WSL so you can execute Windows programs, etc  
It's super handy, but in this case, it's in the way :(  
In WSL, create [wsl.conf](https://docs.microsoft.com/en-us/windows/wsl/wsl-config#wslconf):
- `sudo touch /etc/wsl.conf`  
- Add:
``` 
[interop]
appendWindowsPath=false
```
- `wsl --shutdown` and wait a minute or so for it to restart  






OH DEAR, this breaks access to docker too :( 
So I can either have Node or Docker...










### File watching issues in WSL  
File watching across the Windows/WSL file system boundary is currently [not working correctly](https://github.com/microsoft/WSL/issues/4739).  
The scenario was that I had my code at `C:/Source` and everything was working, but no file changes were being detected, regardless of file watcher settings (CHOKIDAR, etc)  

The workaround is to place the code in the WSL2 file system and just run it from there.  
I did this by opening a WLS2 Ubuntu shell (or just opening a terminal and typing `wsl`), navigating to `~`, typing `explorer.exe .` and using Windows Explorer to drop my code in there.  

Due to the above issues, let's just start this whole thing in WSL2 ~ in a folder of your choice.  

# Let's begin! 
## Project Structure  
```
The folder of your choice
├── UI  
├── API  
├── SQL  
├ docker-compose.yml 
``` 

> ENSURE YOU'RE IN THE WSL FILE SYSTEM FROM NOW ON!!  
> I found this easiest with VSCode --> Search for "Remote-WSL: New Window"  

## Setting up the database (Sql)    
Let's start with the bottom layer of the app.  

Inside the SQL folder, create `DockerFile_SQL` with the following contents:  

**DockerFile_SQL**
``` docker  
FROM mcr.microsoft.com/mssql/server:2019-latest

WORKDIR /app

COPY . .

# Set scripts to be executable. Note that I had to change user for permissions.
USER root
RUN chmod +x ./initialise-sql.sh
RUN chmod +x ./entrypoint.sh
USER mssql

# NOT default port, so it doesn't interfere with host install
# This is not required if you don't need to access from host OR don't have sql server installed on your host
EXPOSE 1633
CMD /bin/bash ./entrypoint.sh
```  

I want the dev experience to be as frictionless as I can, so I want the database already setup with data, which means I need some kind of setup script.  
SQL Server doesn't allow this implicitly, so we're going to have a custom entrypoint called `entrypoint.sh`.  
This script starts sqlserver, but also starts another script which initialises our database.  

**entrypoint.sh**
``` bash  
# Run Microsoft SQl Server and initialization script (at the same time)
/app/initialise-sql.sh & /opt/mssql/bin/sqlservr
```  

**initialise-sql.sh**
``` bash  
# Sql Server container can take some time to start up. 
# This may need to be tweaked per machine, e.g. I can set this to 10s and it's fine
sleep 30s

# Note the SA password here as well.
# Ideally, I'd have an environment variable to handle this (maybe we can use the SA_PASSWORD one?)
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P y0l0Swaggins -d master -i /app/InitialSQL.sql
```  

We wait for SQL server to start, then runs a sql script against the DB using the username and password setup by the dockerfile.  
> The docs recommend this kind of script also update the `sa` password since it's exposed as an environment variable in Docker (and thus anyone who looks at the container can see it)  

{{< image path="img/SQLSAPasswordInDocker" alt="SA password exposed as an environment variable in Docker Desktop" >}}

**InitialSQL.sql**  
``` sql
-- Basic DB setup script for testing

CREATE DATABASE Swag
GO

USE Swag
CREATE TABLE Yolo (Id INT, DisplayName NVARCHAR(50), SwagAmount INT)
GO
INSERT INTO Yolo VALUES (1, 'me', 9001)
GO
```

Mature naming, I know. I was having a bit of a day when I started this project!   

Once all that is done, we have a database container that, when run, will contain an initialised database with some dummy data.  

## Setting up aspnetcore API
I used Visual Studio to create a new NET 6 project with Docker Support.  

- Aspnetcore web app  
- NET 6  
- Tick "Enable Docker" and "Linux"  

> I'm 100% sure you can do the same with the `dotnet new` command, but I didn't investigate.  

This leaves you with a folder and .sln inside the Api folder.  
This is important as Visual Studio uses [Fast Mode](https://aka.ms/containerfastmode) to build when debugging. There's examples of turning it off and making it run the Docker build properly, but I did not investigate as for this example (and many of my actual projects) we don't need to do anything tricky in the dockerfile.  

**Current structure**  
```
The folder of your choice
├── UI  
├── API  
├───── YourProjectFolder  
├───── .dockerignore  
├───── YourProject.sln  
├── SQL  
├───── DockerFile_SQL  
├───── entrypoint.sh  
├───── initialise-sql.sh 
├───── InitialSQL.sql 
├ docker-compose.yml 
```

### Why do it this way?  
I'll discuss it [later]({{< relref "#why-have-the-vs-f5-container-why-so-complicated" >}}), but I do have a reason!   

{{< split >}}
{{% splitLeft title="VS Dockerfile" %}}
``` docker 
# Can't really add/change anything here, it won't matter (Fast mode ignores everything outside base)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 442
EXPOSE 5555
```
{{% /splitLeft %}}  
{{% splitRight title="DockerFile_API" %}}  
``` docker 
# This is the Dockerfile we use during the main stack build.
# NOT used for F5 in Visual Studio, that's in ./DockerTest

# This is essentially what I got from doing Right Click --> Add --> Docker support on the project
# I've just fixed the paths and exposed port 5555
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 442
EXPOSE 5555

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./DockerTest/DockerTest.csproj", "/src"]
RUN dotnet restore "/src/DockerTest.csproj"
COPY ./DockerTest .
RUN dotnet build "/src/DockerTest.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "/src/DockerTest.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerTest.dll"]
```
{{% /splitRight %}}
{{< /split >}}  

I'll refer to running the VS Dockerfile as the "F5 container" or "VS F5 container" throughout this article.  
I just mean being in VS and hitting F5 and getting it's magic debugging container.  

I added EFCore (don't judge me) and created a simple endpoint that returns the first row from the database container.  

{{< split >}}
{{% splitLeft title="Program.cs" %}}
``` cs
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<YoloDbContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("SQL")));
var app = builder.Build();

// Weird. The type "HttpContext" is required to be able to return "item"
// If the type is omitted (even though it's the same type?), you can't return "item", only Task
// Even if you go "Task.FromResult(item)" nothing is sent back in the response.
// So explicit type it is.
app.MapGet("/api/items", (HttpContext context) =>
{
    var dbContext = context.RequestServices.GetRequiredService<YoloDbContext>();
    var item = dbContext.Yolo.First();
    return item;
});

app.Run();

```
{{% /splitLeft %}}
{{% splitRight title="YoloDbContext.cs" %}}
``` cs
using Microsoft.EntityFrameworkCore;

public class YoloDbContext : DbContext
{
    public class Item
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int SwagAmount { get; set; }
    }

    public YoloDbContext(DbContextOptions<YoloDbContext> opts) : base(opts) { }

    public DbSet<Item> Yolo { get; set; }
}
```
{{% /splitRight %}}
{{< /split >}}  

### Caveats!  
With this split setup, the F5 version builds on the host and is copied into the container due to Fast Mode.  
It can be turned off, but for me, this is fine, however it is quite a caveat...  
BUT that's why I also wanted the full stack to be buildable and runnable and debuggable entirely inside Docker.  
Were I to have a project like this where the build couldn't be run on the host, I could run up the stack and attach the debugger OR disable Fast Mode and use the Dockerfile properly.  

### Achieving a nice debugging in VS  
I very much want to have an F5 experience with this API project.  
I want to be able to open the solution, hit F5, and just have it work with everything else I have setup.  

In order to get this, the api needs same URL in the F5 AND the `DockerFile_API` setup.  
I'm in the habit of using non-standard ports right now, so let's expose port 5555 in both Dockerfiles, then in the VS Launch Settings, set Kestral to run on port 5555.  
{{< image path="img/VSLaunchSettings" alt="Visual Studio custom launch configurations" >}}

In those launch settings, you can also see the container name is set to `api` and the network to my mature naming standards.  
Docker containers are isolated by default, so my F5 container can't talk to any of my other containers unless we hook it up.  

When we setup [Docker Compose later]({{< relref "#docker-compose" >}}), the other containers will run in a private bridge network, and this configuration adds the F5 container to that network.  
Further, by setting the name of the container to `api`, the other containers can address it by `http://api:5555`.  
In Docker Compose, we'll set the API project container name to `api` as well, meaning we won't need to edit the UI project files in order to change which container it uses (F5 or compose stack)  
Pretty cool!  

This means that we can set the connection string in AppSettings.json to: `"SQL": "Server=sql;Database=Swag;User Id=sa;Password=y0l0Swaggins;"`, as the Sql container will be named `sql` (we'll set that up later using Docker Compose)  
I also set `httpPort` to 5555 in `launchsettings.json` so I can access it externally. This likely isn't required.  

## Setting up the UI  
> I use NodeJS installed in WSL here, but you could set this up without it, fully inside Docker, but I wanted the Dev experience to be able to use npm commands later on.  

I'm going to use [Create-React-App](https://reactjs.org/docs/create-a-new-react-app.html) to spin up a quick React project to play with.   
In the UI folder `npx create-react-app your-app-name`  

Once it's all setup, let's create `DockerFile_UI`:  
```docker
FROM node:14-alpine AS development 

WORKDIR /app

# This is separate so layers are cached nicely
COPY ./package.json .
COPY ./package-lock.json .

RUN npm ci
CMD ["npm", "start"]
```

For this basic example, let's go to `App.js` and call our Api endpoint and do something with the data from the database.  
``` jsx
import logo from './logo.svg';
import './App.css';
import { useEffect, useState } from 'react';

function App() {
  // Basic setup to fetch data from the API and save it in state for use in the UI.
  const [text, setText] = useState('');
  useEffect(() => fetch('/api/items')
    .then(response => response.json())
    .then(data => setText(data.displayName)));

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.js</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React from {text}
        </a>
      </header>
    </div>
  );
}

export default App;
```

If you were to run that, it won't work 😊    
We need to proxy the request to our Api, ideally avoiding issues like Cors. Fortunately, Create React App supports exactly this.  
In `package.json`, add `"proxy": "http://api:5555"`, as we set our api container name to `api` and our Kestral port to 5555.  

# Docker Compose  
All those Dockerfiles are great, and you could tweak them to run together as they are, but let's get them all chummy and more easily controlled.  
Create `docker-compose.yml` in the root (as per the [project structure]({{< relref "#project-structure" >}})):   

**docker-compose.yml**  
``` yml 
version: '3.9'

services:

  # UI Container spec. note that 'ui' is the name of the container internally (as well as 'container_name', but I like 'ui')
  ui:
    container_name: yolo-swaggins-ui
    image: yolo-swaggins-ui
    environment:
      - CHOKIDAR_USEPOLLING=true  # This allows CRA to hot reload over the file system barrier
      - BROWSER=none
      - NODE_ENV=development
    build: 
      context: ./UI
      dockerfile: DockerFile_UI
      target: development
    volumes: 
      - ./UI:/app  # Mount these as volumes so we can watch them. 
      - /app/node_modules  # I think this was so it didn't overwrite or copy or something...
    ports: 
      - 3000:3000

  # Database Container spec.
  sql:
    container_name: yolo-swaggins-sql
    image: yolo-swaggins-sql
    environment:
      ACCEPT_EULA: 'Y'
      SA_PASSWORD: 'y0l0Swaggins'
    build:
      context: ./DockerDB
      dockerfile: DockerFile_SQL
    ports:
      - 1633:1433 # Map 1433 from inside the container to 1633 host to avoid port conflict with local install

  # API container spec.
  api:
    container_name: yolo-swaggings-api
    image: yolo-swaggins-api
    build:
      context: ./Api
      dockerfile: DockerFile_API
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5555
```  

[Docker Compose](https://docs.docker.com/compose/) is about container orchestration, and lets us connect and control the separate containers more easily.  
In this case, the main things we get for free are a bridge network connecting all three containers, and the ability to build and run them all in one command.  
The network this creates was mentioned in the API section, and is required to get the F5 container in VS connecting to the SQL and UI containers.  

> This network is named whatever the stack name is (usually the root folder name) + "_default" by default.  
> You can view docker networks using `docker network list`  

# Running it all up  
Now, in the root, you can run `docker compose up` and the whole lot will spin up and "just work".  
You should be able to visit `http://localhost:3000` and see the page with "Learn React from me" showing!  
{{< image path="img/FinalResult" alt="It's done!" >}}

We've got it all running! If you open Docker Desktop, you can see the containers all listed there.  
{{< image path="img/YoloSwagginsInDockerDesktop" alt="Final setup in Docker Desktop" >}}


It works, but what is it like to work with?  

## The Debugging Experience. Did I succeed?    
Let's take a look at the experience working with each of the three pieces.  

### Database  
In the `docker-compose.yml`, I mapped the database container port 1433 --> 1633 on the host, so we can connect to it as we would any other database.  
{{< image path="img/ADSLogin" alt="Logging into the database container SQL database with Azure Data Studio" >}}

### API  
I wanted to be able to F5 the solution, and I can!  
All the normal workflows are fine here EXCEPT tests...  
I go over that a bit [later](what-about-running-tests-and-other-useful-things)
Otherwise...Good!  

### UI  
I wanted to be able to open the solution in VSCode and do `npm start`.  
I didn't quite get that...  
We need to open VSCode at the root, so we have `docker-compose.yml` available, and then we can run a bunch of docker comamnds.  
I also wanted devs to be able to do `npm run docker` or something, but we'd need to `cd ..` out to the parent to call `docker compose up ui`.  
The more I think about it, the less sense it makes.

### Overall  
I added a few scripts to `package.json` such as `"docker": "docker compose up ui"` so I can emulate my current way of working.  

The idea would be to open the .sln in VS, and the root directory in VSCode.  
 and in VSCode, do `npm run start-sql` (runs SQL in background) then `npm run docker` which will spin up the UI and listen to the output.  

You can edit files in the UI as normal, and the UI will hot reload as expected.  
In VS, you can stop debugging, edit files, hit F5 and those changes will be reflected.  

Winna winna chicken curry with vegetables and rice dinner!!  


# Revisiting some issues and decisions  
Let's go back and take a look at some decisions and choices.  

## Why have the VS F5 container? Why so complicated!  
I had a bunch of options here, I could have completely ignored the Docker Support option during creation, and just built the dockerfile and used it as it is.  
The thing is, I REALLY wanted to be able to work the way I normally work, which is with VS and VSCode side by side, VS running the API, VSCode running everything else.  

If I use the `DockerFile_API`, I'd have to attach the VS debugger all the time.  
I think that can be setup to auto-reattach, but I wanted to just be able to F5 the solution.  

Another option is to use `dotnet watch` in the API container and get hot reload ala the UI project, but I know there's some issues with `dotnet watch` and I still want that F5 in Visual Studio to work.  

The last option I explored was adding Container Orchestration to the project, and having VS run the Docker Compose.  
The main issue here is that for some reason the combination of VS, Docker Compose, and WSL was not a happy one. VS seems to pass the wrong path to Docker Compose OR Compose doesn't understand the path in the way it was given by VS.  
Even when I File --> new project with Container Orchestration and F5, it fails to read the path to the docker compose file.  
I suspect this is a bug in one of the three applications involved, and I'm not sure which!  
My investigation stopped there, so it may well be possible to get this running.  

In the end, I chose to have the VS container running OUTSIDE the docker compose stack, but with the same internal name and connected to the stack network.  
The proxy address in the UI is then the same for running VS F5 AND for running the stack wholesale.  

## NodeJS installed on WSL  
This is more personal taste flavoured by inexperience with Docker and WSL.  
Currently, I work a lot with npm, so running projects by doing `npm run ...` is natural, and I know where to look for the commands I have available in the project (package.json)  

If I did the whole thing in Docker, I'd have to remember all the docker commands to make things do what I wanted, and so would any new dev coming onto the project.  
Now, that may end up being superior as we'd get more control, more understanding, less complexity, and we could remove node, leaving Docker and WSL the only requirements, but for now it's easier to get running this way.  


# Other general stumbling blocks  
I wanted to be able to access the DB from the host so I can add data or just look around, but I wasn't able to.
The reasons were:  

- SQL was installed on the host, and thus port conflicts
  - Note in the SQL containers and compose script --> 1633:1433, which maps the default port sql server runs on from inside the container to 1633 on the host to avoid the conflict
- Sql Server host needs some network config to connect to "network" instances
  - Computer Management --> Sql server configuration manager --> SQL server network configuration --> TCP/IP --> Enable, then make sure that in "IP addresses" make sure the ports are the same as we want (1433 is default)
  > Note I'm no longer convinced this is necessary, but it might help if you're having connection troubles

## Other notes:  

- I was unable to get SSL working for the API, not sure exactly why. Could have been certificates, port conflicts, who knows.  
- Even after I told Kestral to run on port 5555, the container variables are still `+:80` but it all connects fine...I'm not sure what's going on there.  
- The VS F5 container doesn't stop running when you press the "Stop" in VS.
- When npm packages are added or changed, the whole container will need rebuilding. Docker Compose SHOULD know about this change ([diffs](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/#leverage-build-cache) package.json as text) 

## What about running tests and other useful things?  
Currently VS2022 does not support running tests in Docker.  
You can build a docker setup to run the tests, and output a trx file that the VS Test explorer can read, but I decided that was too complicated to be worth it.  
A much more sensible way would be to have a Docker container setup which ran `dotnet test` live and ran the tests on change.  

# Readings
Primary resource: https://mherman.org/blog/dockerizing-a-react-app/  
https://docs.docker.com/samples/aspnet-mssql-compose/  
https://blog.logrocket.com/docker-sql-server/  
https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment?view=sql-server-ver15&pivots=cs1-bash  
https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver15&preserve-view=true&pivots=cs1-bash  
https://www.pluralsight.com/blog/software-development/how-to-build-custom-containers-docker  
https://www.section.io/engineering-education/build-and-dockerize-a-full-stack-react-app-with-nodejs-and-nginx/  
https://towardsdatascience.com/deploying-a-react-nodejs-application-with-docker-part-i-of-ii-910bc6edb46e  
https://medium.com/geekculture/getting-started-with-docker-in-your-react-js-application-the-basics-6e5300cf749d  
Looots of stack overflow  

# Github Code
[Github](https://github.com/kael-larkin/DockerisationExperiment)  