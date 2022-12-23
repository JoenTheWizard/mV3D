#include "GBuffer.h"
/*
Entire thing is mainly used for Deferred Rendering. Generates a Geometry buffer with the transform,
normal, albedo (transform/color + specular) and places it in within a RBO Framebuffer object.
Later this will be passed through a shader program that handles multiple lights for an entire scene.

Also important to note that the attachments should all be glDrawBuffers() with each layer of the
framebuffer object. Something like this:
 GLuint attachments[3] = {GL_COLOR_ATTACHMENT0, GL_COLOR_ATTACHMENT1, GL_COLOR_ATTACHMENT2};
 glDrawBuffers(3, attachments);
*/
GBuffer::GBuffer()
{
	//Generate the GBuffer
	glGenFramebuffers(1, &gBuffer);
	glBindFramebuffer(GL_FRAMEBUFFER, gBuffer);
}
GBuffer::GBuffer(GLsizei WIDTH, GLsizei HEIGHT)
{
	SCR_WIDTH = WIDTH;
	SCR_HEIGHT = HEIGHT;
	//Generate the GBuffer
	glGenFramebuffers(1, &gBuffer);
	glBindFramebuffer(GL_FRAMEBUFFER, gBuffer);
}
GBuffer::~GBuffer(){}

//Generate Pos (diffuse) buffer
void GBuffer::Gen_GPosition()
{
	//Gen and bind transform texture buffer
	glGenTextures(1, &gPosition);
	glBindTexture(GL_TEXTURE_2D, gPosition);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA16F,SCR_WIDTH,SCR_HEIGHT,0,GL_RGBA,GL_FLOAT, 0);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	//Bind texture to framebuffer object (Index 0)
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gPosition, 0);
}

//Generate Normal buffer
void GBuffer::Gen_GNormal()
{
	glGenTextures(1, &gNormal);
	glBindTexture(GL_TEXTURE_2D, gNormal);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA16F, SCR_WIDTH, SCR_HEIGHT, 0, GL_RGBA, GL_FLOAT, 0);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	//Bind texture to framebuffer object (Index 1)
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT1, GL_TEXTURE_2D, gNormal, 0);
}

//Generate Albedo (diffuse + specular) buffer
void GBuffer::Gen_GAlbedo()
{
	glGenTextures(1, &gAlbedo);
	glBindTexture(GL_TEXTURE_2D, gAlbedo);
	//Generate texture with unsigned byte type. 
	//Also using base internal format of GL_RGBA to combine the entire texture values
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, SCR_WIDTH, SCR_HEIGHT, 0, GL_RGBA, GL_UNSIGNED_BYTE, 0);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	//Bind texture to framebuffer object (Index 1)
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT1, GL_TEXTURE_2D, gAlbedo, 0);
}

GLuint GBuffer::Gen_RBO()
{
	//Gen and bind render buffer object
	GLuint rbo;
	glGenRenderbuffers(1,&rbo);
	glBindFramebuffer(GL_RENDERBUFFER, rbo);
	//Specify the render buffer object's storage (width, height, internal format etc)
	glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT, SCR_WIDTH, SCR_HEIGHT);
	//Attach render buffer as frame buffer object
	glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, rbo);
	//Error handle: check if frame buffer is fully initialized
	if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
		std::cout << "There was an error with rendering the RBO Frame buffer object!" << std::endl;
	return rbo;
}