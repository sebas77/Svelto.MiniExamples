# Deterministic2DPhysics
Deterministic 2D Physics for C# Console Apps.

This is an early commit to demo how its possible to use Svelto 3.0 in a non-Unity context.

## Dependencies
* Svelto.Common
* Svelto.ECS
* SDL2-CS.Core
* SDL2.dll

## Setup
Clone the repo, build then run the `MiniExamples.DeterministicPhysicDemo` project to see boxes bounding around the screen

## Controls
This contains a super simple simulation setup, will start up SDL2 and bounce some boxes around.

The following key bindings are set:
* Ecs - Closes the program
* ` - Sets simulation speed to 10
* 1 - Sets simulation speed to 1.0
* 2 - Sets simulation speed to 0.5
* 3 - Sets simulation speed to 0.25
* 4 - Sets simulation speed to 0.125
* 5 - Sets simulation speed to 0.01
* 6 - Sets simulation speed to 0.0
* Home - Zooms out
* End - Zooms in
* Arrow keys - pans around
