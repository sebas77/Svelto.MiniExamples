# Svelto.ECS.MiniExamples

New Mini Examples for Svelto.ECS and Svelto.Tasks.

* Svelto.ECS: https://github.com/sebas77/Svelto.ECS
* Svelto.Tasks: https://github.com/sebas77/Svelto.Tasks (Svelto.Tasks 1.5 doesn't work with the latest Svelto.Common and Svelto.Tasks 2.0 is not production ready)

These examples use the last beta version of unity available at the moment of the update.

Warning: these examples may use using unofficial versions of Svelto that you won't find on the main repository until officially released.

## Example 1: Doofuses Must Eat (Pure ECS)

![Image](https://github.com/sebas77/Svelto.MiniExamples/blob/master/Example1-DOTS-DoofusesMustEat/2020-12-22%2016-05-22.gif)

Object-less pure ECS example that shows the basics of Svelto.ECS and Svelto.Tasks.

Article:

http://www.sebaslab.com/svelto-mini-examples-doofuses-must-eat/

Goals of this example are to: 
* intoduce to the concept of entity descriptors, entity structs and groups. 
* Show the integration with UnityECS, shows the use of Svelto.Tasks 2.0 (alpha state) 
* show the integration with Unity Jobs and Burst (using Svelto.ECS 3.0)
* test Full jobified/burstified code with Svelto ECS 3.0
* test integration with IL2CPP
  
## Example 2: The classic Survival demo (Hybrid ECS)

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/gif_animation_002.gif)

Basic integration with Unity GameObjects and Monobehaviours.

Goal of this example: 

* show the integration with OOP platforms (Unity in this case) throught the use of Entity View Components and Implementors.
* *Test WebGL support*

I used the Survival Shooter Unity Demo to show how an ECS framework could work inside Unity. I am not sure about the license of this demo, so use it only for learning purposes.
Most of the source code has been rewritten to work with Svelto.ECS framework. The Survival Demo is tested with the latest version of Unity, so I cannot guarantee that it always works, but it should work with all the versions from 5.3 and above.

* Note: This demo shows just one way to abstract OOP code, it is not _THE WAY_. Other strategies are in fact listed in the Example 6. 
* Note: This demo code is old and it's still using ExclusiveGroups instead of GroupCompounds. You should use GroupCompunds.
* Note: The purposes of this demo is NOT to show how to write fast code. In fact most of the solutions you will find in this demo are not optimal at all. Svelto ECS is used only to wrap high level code as all the low level functionalities are executed through standard gameobjects.

## Example 3: GUI and Service Layer (Hybrid ECS for GUI)

![Image](https://i2.wp.com/www.sebaslab.com/wp-content/uploads/2019/07/image-2.png?w=701)

Integration with Unity UI and Svelto entities 3.0

Goal of this example

* Show how EntityStreams work to publish data changes. 
* Show how to enable databinding with ExclusiveGroups. 
* Show how to setup a data oriented GUI with nested prefabs. 
* Show a basic usage of the Svelto.Services (https://github.com/sebas77/Svelto.Services)

Main article: http://www.sebaslab.com/svelto-miniexamples-gui-and-services-layer/

## Example 4: Pure .net + SDL example

Goal of this example

* Show how to use Svelto outside unity in an advanced scenario
* Show how to use efficently group compounds

![Image](https://github.com/sebas77/Svelto.MiniExamples/blob/master/Example4-NET-SDL/2020-12-23%2011-54-54.gif)

## Example 5: Vanilla

Basic Platform Agnostic Svelto.ECS 3.0 example

* Goal: Shows the very foundation of a simple entity and engine logic, without any Unity or other platform dependency (pure .net). This example won't cover all the aspects of SECS, but only the basics.

## Example 6: Abstract Object Oriented Code

* Goal: this example shows the two main strategies to abstract OOP code. Mixing the two strategies will result in the least boiler plate and fastest code.
* Fist integration wraps gameobjects through the use of EntityViewComponents like seen in MiniExamples 2 Survival
* Second integration shows a more efficent approach, where pure ECS is used as much as possible and engines objects are synched only as late as possible.
