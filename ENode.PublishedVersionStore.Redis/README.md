# ENode.PublishedVersionStore.Redis
Implement ENode.PublishedVersionStore by redis.

## Install

```
dotnet add package ENode.PublishedVersionStore.Redis
```

## Usage

```csharp
using ENode.PublishedVersionStore.Redis;

var string redisConfig = "127.0.0.1:6379,syncTimeout=3000,defaultDatabase=0,name=demo,allowAdmin=false";
// Use before build ioc container.
enodeConfigure.UseRedisPublishedVersionStore();
// Initialize
enodeConfigure.InitializeRedisPublishedVersionStore(redisConfig, "demo");

```

## Clean Test Data
```sh
# clean demo's published version data.
 redis-cli -h 192.168.117.198 -p 6379 -n 0 keys "demo:pv:*" | xargs redis-cli -h 192.168.117.198 -p 6379 -n 0 del
```

## Publish History

### 1.0.0

- init
