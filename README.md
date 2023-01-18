# [FileTtl](https://filettl.store)
An anonymous and ephemeral files storage. Inspired by [ttl.sh](https://ttl.sh)

[![Docker Image CI](https://github.com/rwecho/FileTtl/actions/workflows/build-docker-image.yml/badge.svg)](https://github.com/rwecho/FileTtl/actions/workflows/build-docker-image.yml)

## About
Passing files between the mathines.

In some jobs, it's necessary to copy files to remote computers. In these cases, sometimes it's done through SSH. As I usually use the system's default terminal, file operations need to be done through SCP, which can be cumbersome despite being just a few commands. Additionally, some remote tools like xshell, todesk, etc. may not work on some computers for copying or transferring files. Moreover, for some programmers, there's always one or two VPSs that are idle. Instead of letting them go to waste, it would be better to make use of them and let others take advantage of them for a short time. If it provides convenience for you, that is the meaning of this project.

filettl.store solves this by making an ephemeral and anonymous files storage.

## Usage
* On the linux we can use `curl` command:

``` bash
# upload 1.txt to filettl.store
curl https://filettl.store/api/files -F file=@1.txt


# download 1.txt
curl https://filettl.store/api/files?hash=xxxx --output 1.txt

```

* On the windows we can use web browser of [powershell](https://filettl.store/scripts/filettl.ps1):

## Self-Hosting
If you had a VPS, you can deploy with the docker image.

``` bash
docker run -d --name filettl -p 80:80 rwecho/filettl-app:latest
```

``` bash
# docker-compose.yml
version: "3.0"
services:
    filettl-app:
        image: "rwecho/filettl-app"
        container_name: "filettl"
        hostname: "filettl"
        restart: "always"
        ports:
            - 80:80
        environment:
            - Cleanup__ValidHours=24;
        volumes:
            - app-data:/app/App_Data
volumes:
    app-data:

```
