## Example 2: The classic Survival demo (Hybrid ECS)

![Image](https://github.com/sebas77/GithubWikiImages/blob/master/gif_animation_002.gif?raw=true)

### Basic integration with Unity GameObjects and Monobehaviours.

## Goal of this example:

* show the integration with OOP platforms (Unity in this case) throught the use of OOP abstraction layers.
* *Test WebGL support*

I used the Survival Shooter Unity Demo to show how an ECS framework could work inside Unity. I am not sure about the license of this demo, so use it only for learning purposes.
Most of the source code has been rewritten to work with Svelto.ECS framework. The Survival Demo is tested with the latest version of Unity, so I cannot guarantee that it always works.

* Note: The purposes of this demo is NOT to show how to write fast code. In fact most of the solutions you will find in this demo may not be optimal. Svelto ECS is used only to wrap high level code as all the low level functionalities are executed through standard gameobjects.