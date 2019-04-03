# Svelto.ECS.MiniExamples

New Mini Examples for Svelto.ECS and Svelto.Tasks

Note: these examples are using unofficial versions of Svelto that you won't find on the main repository until officially released.

## Example 1:

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/Example1-Doofuses.gif)

Object-less pure ECS example that shows the basics of Svelto.ECS and Svelto.Tasks.

* Goal of this example: show the simplest use of Svelto 2.8 (beta state), intoduce to the concept of entity descriptors, entity structs and groups. 
* Secondary goal: show integration with UnityECS, show the use of Svelto.Tasks 2.0 (alpha state), show integration with Burst

* **Iteration 1: Example1-DoofusesMustEat (done):**
  * Integration with UnityECS for rendering
  * Move the camera with arrows and mouse, left button to drop food
* **Iteration 2: Example1B-DoofusesMustEatBurst (done):**
  * Integration with Burst for better performance
  * Move the camera with arrows and mouse, left button to drop 100 random food only once
* **Iteration 3 (to do):**
  * Enable Svelto.Tasks multithreading for better performance
* **Iteration 4 (to do):**
  * Something I will say later
  
## Example 2: Work In Progress

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/gif_animation_002.gif)

Integration with Unity GameObjects and Monobehaviours. Uses Svelto ECS 2.0 (beta) and Svelto Tasks 1.5

* Goal of this example: show the integration with OOP platforms (Unity in this case) throught the use of Entity View Structs and implementors.

Main Article: http://www.sebaslab.com/learning-svelto-ecs-by-example-the-survival-example/

I used the Survival Shooter Unity Demo to show how an ECS framework could work inside Unity. I am not sure about the license of this demo, so use it only for learning purposes.

Most of the source code has been rewritten to work with Svelto.ECS framework. The Survival Demo is tested with the latest version of Unity, so I cannot guarantee that it always works, but it should work with all the versions from 5.3 and above.

## Example 3: To do
