# Svelto.Common
Shared code between the Svelto repositories

For Unity Users: to solve the unsafe dependency you need to add the following scopedRegistries in manifest.json:
```
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.openupm",
        "com.sebaslab.svelto.common",
        "com.sebaslab.svelto.ecs",
        "com.sebaslab.svelto.unsafe",
        "org.nuget.system.buffers",
        "org.nuget.system.memory",
        "org.nuget.system.numerics.vectors",
        "org.nuget.system.runtime.compilerservices.unsafe"
      ]
    }
  ]
```
