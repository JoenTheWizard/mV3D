#pragma once
#include <glad/glad.h>
#include <iostream>
class GBuffer
{
public:
	GBuffer();
	GBuffer(GLsizei WIDTH, GLsizei HEIGHT);
	~GBuffer();
	//Main G-Buffer texture pass
	unsigned int gBuffer;
	//G-Buffer attributes (Transform, normal, albedo etc)
	GLuint gPosition;
	GLuint gNormal;
	GLuint gAlbedo;
	//Seperate methods to generate each buffer pass
	void Gen_GPosition();
	void Gen_GNormal();
	void Gen_GAlbedo();
	//Renderbuffer Object
	GLuint Gen_RBO();
private:
	GLsizei SCR_WIDTH = 800;
	GLsizei SCR_HEIGHT = 600;
};

