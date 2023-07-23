Compatible only with Unity 2021.3 and above

* copy the UnityDLL~/AkroGame.ECS.Analyzer.dll and meta file (important too) and copy them in your project asset folder (compiled from https://github.com/akrogame/svelto-ecs-inspector)
* then install the Unity package https://github.com/James-Frowen/SimpleWebTransport in your Unity project using Svelto. Follow the readme instructions to install the unity package
* then install the other package from the github source: git@github.com:sebas77/svelto-ecs-inspector-unity.git
* then in your main context add the line
```
SveltoInspector.Attach(_enginesRoot);
```
Run the game.

For the client:

you can either use the URL: https://akrogame.github.io/svelto-ecs-inspector/
or best running the node js application found at https://github.com/sebas77/svelto-ecs-inspector/tree/main/inspector

FAQ:

why are engines shown multiple times? like

![image](https://user-images.githubusercontent.com/945379/208312024-8a996eae-cfa7-4f2e-83f8-b0f4c41750b7.png)

each copy is a separate query found in the engine
