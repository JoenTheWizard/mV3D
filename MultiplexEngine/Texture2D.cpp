#include "Texture2D.h"
Texture2D::Texture2D(const char* fileName, bool mipmapNearest, bool isRGBA)
{
   glGenTextures(1, &texture2D);
   glBindTexture(GL_TEXTURE_2D, texture2D);
   // set the texture wrapping parameters
   glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (isRGBA) ? GL_CLAMP_TO_EDGE : GL_REPEAT);	// set texture wrapping to GL_REPEAT (default wrapping method) or GL_CLAMP_TO_EDGE for more transparent textures
   glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (isRGBA) ? GL_CLAMP_TO_EDGE : GL_REPEAT);
   // set texture filtering parameters
   glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (mipmapNearest) ? GL_LINEAR_MIPMAP_NEAREST : GL_LINEAR);
   glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
   // load image, create texture and generate mipmaps
   stbi_set_flip_vertically_on_load(true); // tell stb_image.h to flip loaded texture's on the y-axis.
   // The FileSystem::getPath(...) is part of the GitHub repository so we can find files on any IDE/platform; replace it with your own image path.
   unsigned char* data = stbi_load(fileName, &width, &height, &nrChannels, 0);
   if (data)
   {
      glTexImage2D(GL_TEXTURE_2D, 0, (isRGBA) ? GL_RGBA : GL_RGB, width, height, 0, (isRGBA) ? GL_RGBA : GL_RGB, GL_UNSIGNED_BYTE, data);
      glGenerateMipmap(GL_TEXTURE_2D);
   }
   else
      std::cout << "Failed to load texture" << std::endl;
   stbi_image_free(data);
}

Texture2D::~Texture2D(){

}
void Texture2D::Use()
{
   glBindTexture(GL_TEXTURE_2D, texture2D);
}