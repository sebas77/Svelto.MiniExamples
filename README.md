# Svelto.ECS.MiniExamples

New Mini Examples for Svelto.ECS and Svelto.Tasks.

These examples use the last beta version of unity available at the moment of the update.

Warning: these examples may use using unofficial versions of Svelto that you won't find on the main repository until officially released.

## Example 1: Doofuses Must Eat (Pure ECS)

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/Example1-Doofuses.gif)

Object-less pure ECS example that shows the basics of Svelto.ECS and Svelto.Tasks.

Article:

http://www.sebaslab.com/svelto-mini-examples-doofuses-must-eat/

Goal of this example: 
* show the simplest use of Svelto ECS (mixed versions) 
* intoduce to the concept of entity descriptors, entity structs and groups. 
* Show the integration with UnityECS, show the use of Svelto.Tasks 2.0 (alpha state) 
* show integration with Unity Jobs and Burst (using Svelto.ECS 3.0 (currently alpha state)
 
 ### **Iteration 1 (old)**
   * Integration with UnityECS for rendering
   * Move the camera with arrows and mouse, left button to drop food
 ###  **Iteration 2 (old)**
   * Integration with Burst for better performance
   * Move the camera with arrows and mouse, left button to drop 100 random food only once
 ###  **Iteration 3 (MUST BE USED THIS AS REFERENCE)**
   * Full jobified/burstified code with Svelto ECS 3.0
   * test integration with IL2CPP
  
## Example 2: The classic Survival demo (Hybrid ECS)

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/gif_animation_002.gif)

Basic integration with Unity GameObjects and Monobehaviours. Uses Svelto ECS 3.0 and Svelto Tasks 1.5

Goal of this example: 

* show the integration with OOP platforms (Unity in this case) throught the use of Entity View Components and Implementors.
* *Test WebGL support*

Main Article: http://www.sebaslab.com/learning-svelto-ecs-by-example-the-survival-example/

I used the Survival Shooter Unity Demo to show how an ECS framework could work inside Unity. I am not sure about the license of this demo, so use it only for learning purposes.

Most of the source code has been rewritten to work with Svelto.ECS framework. The Survival Demo is tested with the latest version of Unity, so I cannot guarantee that it always works, but it should work with all the versions from 5.3 and above.

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

![Image](https://twitter.com/i/status/1341714736023564288)

## Example 5: Vanilla

Basic Platform Agnostic Svelto.ECS 3.0 example

* Goal: Shows the very foundation of a simple entity and engine logic, without any Unity or other platform dependency (pure .net). This example won't cover all the aspects of SECS, but only the basics.
