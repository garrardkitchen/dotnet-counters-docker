

## must haves:

```
RUN dotnet tool install dotnet-counters --global
RUN dotnet tool install dotnet-trace --global
RUN dotnet tool install dotnet-dump --global
RUN dotnet tool install dotnet-gcdump --global
```

```bash
$ docker exec -it $(docker ps -f name=monitor --format="{{.ID}}") dotnet counters monitor --process-id 1 System.Runtime Microsoft.AspNetCore.Hosting
```

Will result in this if you have not updated the PATH with the location of the above tools:

```
Could not execute because the specified command or file was not found.
Possible reasons for this include:
  * You misspelled a built-in dotnet command.
  * You intended to execute a .NET Core program, but dotnet-xyz does not exist.
  * You intended to run a global tool, but a dotnet-prefixed executable with this name could not be found on the PATH.
```

Ref: [troubleshoot](https://docs.microsoft.com/en-us/dotnet/core/tools/troubleshoot-usage-issues)

Add this to your Dockerfile:

```dockerfile
COPY --from=build /root/.dotnet/tools/ /app/tools
ENV PATH="${PATH}:/app/tools"
```

To load test the sample application, install the ab tool:

```script
$ sudo apt install apache2-utils
```

To run a load test, start docker-compose in one terminal, and run the `ab` command in another:

```script
docker-compose build
docker-compose up

ab -k -n 1000000 -c 100 -t 600 http://localhost:8086/
```

You may sometimes get this:

```
apr_socket_recv: Connection reset by peer (104)
```

The means that the other end (webserver) suddenly disconnected in the middle of the session. have a look at the apache or nginx error logs to see if there is anything suspicious there.

Solutions to this is (wip):

- Either increase the maxThread or request queue length
- Need to look for kestrel config
- It's pointing to a docker issue, more so than a kestrel

To look at the sockets statistics, run the ss command:

```script
watch -n 1 "ss -s"
```

Ref: (sockets)[https://www.cyberciti.biz/tips/linux-investigate-sockets-network-connections.html]

To get pid, type:
```script
$ docker ps -a | grep monitor

caad7cd4ba72        monitor                "dotnet monitor.dll"     18 minutes ago      Exited (0) 3 minutes ago     monitor_monitor_1
```

To grab the gc dump of this container instance, type:
```script
$ docker exec -it $(docker ps -f name=monitor --format="{{.ID}}") dotnet gcdump collect -process-id 1

Writing gcdump to '/app/20200624_115944_1.gcdump'...
        Finished writing 313784 bytes.
```

To copy from the docker instance to the host, type:

```script
$ docker cp caa:/app/20200624_114456_1.gcdump .
```

You'll need a utility to get the most out of this file.  I recommend PerfView.  You can download it from `https://github.com/Microsoft/perfview/releases`.


To test with different ThreadPool limits:

To set the min/max ThreadPool Threads, open your c# project file and the `ThreadPool*Theads` elements to the PropertyGroup as shown here:
```xml
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
...
    <ThreadPoolMinThreads>1</ThreadPoolMinThreads>
    <ThreadPoolMaxThreads>10</ThreadPoolMaxThreads>
  </PropertyGroup>

```


