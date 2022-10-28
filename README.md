# Svelto.ECS.MiniExamples

New Mini Examples for Svelto.ECS.

* Svelto.ECS: https://github.com/sebas77/Svelto.ECS

These examples use the last beta version of unity available at the moment of the update.

Warning: these examples may use using unofficial versions of Svelto that you won't find on the main repository until officially released.

## Example 1: Doofuses Must Eat (Pure ECS/GameObject versions/Stride version)

![Image](https://github.com/sebas77/Svelto.MiniExamples/blob/master/Example1-DoofusesMustEat/2020-12-22%2016-05-22.gif)

Object-less pure ECS example that shows the basics of Svelto.ECS and Svelto.Tasks.

Article:

http://www.sebaslab.com/svelto-mini-examples-doofuses-must-eat/

Goals of the **Pure ECS** example are to: 
* intoduce to the concept of entity descriptors, entity structs and groups. 
* Show the integration with UnityECS, shows the use of Svelto.Tasks 2.0 (alpha state) 
* show the integration with Unity Jobs and Burst (using Svelto.ECS 3.0)
* test Full jobified/burstified code with Svelto ECS 3.0
* test integration with IL2CPP

Goals of the **GameObjects** example are to: 
* show how the OOP abtraction layer works (https://www.sebaslab.com/oop-abstraction-layer-in-a-ecs-centric-application/)
* show how the resource managers work to interface objects and entities 
* show that even with OOP interfacing, ECS helps to achieve high performance

Goals of the **Stride example** are to: 
* show a complex integration with an engine different than Unity (Stride Engine)
  
## Example 2: The classic Survival demo (Hybrid ECS)

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/gif_animation_002.gif)

**Warning**: I am in the process of completely rewriting this example. I cannot reccomend the use of imlpementors and the subs/publisher, so I am rewriting the demo to not use either of those. This will take some time. Check the Doofuses GameObject examples for better ways to integrate Svelto.ECS and GameObjects

Basic integration with Unity GameObjects and Monobehaviours.

Goal of this example: 

* show the integration with OOP platforms (Unity in this case) throught the use of Entity View Components and Implementors.
* *Test WebGL support*

I used the Survival Shooter Unity Demo to show how an ECS framework could work inside Unity. I am not sure about the license of this demo, so use it only for learning purposes.
Most of the source code has been rewritten to work with Svelto.ECS framework. The Survival Demo is tested with the latest version of Unity, so I cannot guarantee that it always works, but it should work with all the versions from 5.3 and above.

* Note: This demo shows just one way to abstract OOP code, it is not _THE WAY_. Other strategies are in fact listed in the Example 6. 
* Note: The purposes of this demo is NOT to show how to write fast code. In fact most of the solutions you will find in this demo are not optimal at all. Svelto ECS is used only to wrap high level code as all the low level functionalities are executed through standard gameobjects.

## ~~Example 3: GUI and Service Layer (Hybrid ECS for GUI)~~

~~Integration with Unity UI and Svelto entities 3.0~~

~~Goal of this example~~

* ~~Show how EntityStreams work to publish data changes.~~ 
* ~~Show how to enable databinding with ExclusiveGroups.~~ 
* ~~Show how to setup a data oriented GUI with nested prefabs.~~ 
* ~~Show a basic usage of the Svelto.Services (https://github.com/sebas77/Svelto.Services)~~

~~Main article: http://www.sebaslab.com/svelto-miniexamples-gui-and-services-layer/~~

UI and ECS is an on going problem to solve. I have some better solution than what proposed in this example now.

## Example 4: Pure .net + SDL example

Goal of this example

* Show how to use Svelto outside unity in an advanced scenario
* Show how to use efficently group compounds

![Image](https://github.com/sebas77/Svelto.MiniExamples/blob/master/Example4-NET-SDL/2020-12-23%2011-54-54.gif)

## Example 5: Hello World (previously called Vanilla)

Basic Platform Agnostic Svelto.ECS 3.0 example

* Goal: Shows the very foundation of a simple entity and engine logic, without any Unity or other platform dependency (pure .net). This example won't cover all the aspects of SECS, but only the basics.

## Example 6: Abstract Object Oriented Code

* Goal: this example shows the two main strategies to abstract OOP code. Mixing the two strategies will result in the least boiler plate and fastest code.
* Fist integration wraps gameobjects through the use of EntityViewComponents like seen in MiniExamples 2 Survival. **Attention** EntityViewComponents and Implementors are NOT reccomended to use over the second approach.
* Second integration shows a more efficent approach, where pure ECS is used as much as possible and engines objects are synched only as late as possible.

## Example 7: Awkward foundation for a possible defense game built with the magnificent engine that Stride is

* Goal: showing Svelto working with Stride Engine (svelto can work with any engine that supports c# natively)
* Goal: showing how to use EntityReferences to transform hierarchies
* Foundation of my new article

![image](https://user-images.githubusercontent.com/945379/134925979-145e5b0e-fd5d-4562-abc3-07bafca2fbe6.png)

