#pragma once
#include <glad/glad.h>
#include "Shader.h"
class FBOLayer
{
public:
	FBOLayer(); ~FBOLayer();
	unsigned int textureBuffer;
	unsigned int initFBO(Shader *shader);
	unsigned int initRBO();
	void begin(unsigned int fbo);
	void BindTexture(unsigned int rbo, int width, int height);
	void initMainFBO(int width, int height);
};

