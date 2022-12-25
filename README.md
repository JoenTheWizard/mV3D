# mV3D

This is a 3 seperate programs merged into one (mv3DViewer, ShadeD and Multiplex engine) which combines and merges them all into one program I call mV3D.
It's a project that mainly deals with 3D modelling, graphics, shaders etc.

This project is seperate to a similar project called `mVCMD`

## mV3DViewer
mV3DViewer is a program created in C# WPF (and utilizes HelixToolKit library) that loads basic wavefront 3D models and is able to create very simple geometric models. It has simple cloth physics as well, cutting objects (based on normal vectors), 3D mathematical expression modelling tool and
an unfinished scripting engine called mV3DScript.

## ShadeD
ShadeD is also made with C# WPF and runs GLSL shader code under WebGL interface (similar to that of popular Shadertoy site). It relies on a vertex and fragment shader and is configurable through a pseudo-markup language where you can load youtube videos in the
background and load textures from external sources. It is also able to set the shaders as wallpapers using Win32 API (although not recommended as it can be heavy on the GPU)

## Multiplex Engine
Multiplex Engine is created in C++ with Dear ImGUI and GLFW. You can also import basic wavefront 3D models, and also edit the shaders of the models with a real-time GLSL editor. It also provides joystick compatibility and a Lua scripting engine

## Preview

![mV3D](imgs/tetsd.jpg)
