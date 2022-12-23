#pragma once
#include <stb_image.h>
#include <glad/glad.h>
#include <iostream>
class Texture2D
{
public:
   //Store width, height and the image channels
   int width, height, nrChannels;
   Texture2D(const char* fileName, bool mipmapNearest, bool isRGBA);
   ~Texture2D();
   void Use();
   //The uint to store the texture identifier (very useful)
   unsigned int texture2D;
};

