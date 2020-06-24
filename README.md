

## must have
RUN dotnet tool install dotnet-counters --global
RUN dotnet tool install dotnet-trace --global
RUN dotnet tool install dotnet-dump --global
RUN dotnet tool install dotnet-gcdump --global

docker exec -it $(docker ps -f name=monitor --format="{{.ID}}") dotnet counters monitor --process-id 1 System.Runtime Microsoft.AspNetCore.Hosting

```
Could not execute because the specified command or file was not found.
Possible reasons for this include:
  * You misspelled a built-in dotnet command.
  * You intended to execute a .NET Core program, but dotnet-xyz does not exist.
  * You intended to run a global tool, but a dotnet-prefixed executable with this name could not be found on the PATH.
```

https://docs.microsoft.com/en-us/dotnet/core/tools/troubleshoot-usage-issues

```
COPY --from=build /root/.dotnet/tools/ /app/tools
ENV PATH="${PATH}:/app/tools"
```

sudo apt install apache2-utils

```bash
ab -k -n 1000000 -c 100 -t 600 http://localhost:8086/
```

Sometimes get this:

```
apr_socket_recv: Connection reset by peer (104)
```

which means that the other end (webserver) suddenly disconnected in the middle of the session. have a look at the apache or nginx error logs to see if there is anything suspicious there.

Either increase the maxThread or request queue length

!! need to look for kestrel config !!
It's pointing to a docker issue, more so than a kestrel


```bash
watch -n 1 "ss -s"
```

To get pid:
```
docker ps -a | grep monitor

caad7cd4ba72        monitor                "dotnet monitor.dll"     18 minutes ago      Exited (0) 3 minutes ago
          monitor_monitor_1
```

```
docker exec -it $(docker ps -f name=monitor --format="{{.ID}}") dotnet gcdump collect -process-id 1

Writing gcdump to '/app/20200624_115944_1.gcdump'...
        Finished writing 313784 bytes.
```

```
docker cp caa:/app/20200624_114456_1.gcdump .
```

To download PerfView
```
https://github.com/Microsoft/perfview/releases
```

```xml
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
...
    <ThreadPoolMinThreads>1</ThreadPoolMinThreads>
    <ThreadPoolMaxThreads>10</ThreadPoolMaxThreads>
  </PropertyGroup>

```


