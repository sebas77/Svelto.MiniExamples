# Svelto.ECS.Debugger
## Alarm!
WIP. This project have bugs, Bugs and BUGS.
### Install

- Copy Svelto.ECS.Debugger to your project folder.
- Make sure that Svelto.ECS.Debugger have references to Svelto.ECS and Svelto.Common
- Attach Debugger with `_enginesRoot.AttachDebugger();` in your MainCompositionRoot.cs
- Detach Debugger in Destroy() event with `_enginesRoot.DetachDebugger();`
- (Optional) Rename `new EnginesRoot(_scheduler)` to `new EnginesRootNamed(_scheduler, "ThisAwesomeRoot")`
- (Optional) Rename `new ExclusiveGroup()` to `new ExclusiveGroupNamed("ThisAwesomeGroup")`

### Using
Just open debugger in top menu Window/Analysis/Svelto.ECS Debugger

### Known issues
- In Entity List you can see magic zero-id element. Ignore it.
