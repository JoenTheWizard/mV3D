#include "FBOLayer.h"
FBOLayer::FBOLayer() {}
FBOLayer::~FBOLayer() {}
/// <summary>
/// This is to initialize the FBO object as a uint. Set sampler2D uniform as 'envFbo'
/// </summary>
/// <param name="shader"></param>
/// <returns></returns>
unsigned int FBOLayer::initFBO(Shader* shader) {
	shader->use();
	shader->setInt("envFbo",0);
	//fbo variable
	unsigned int fbo;
	glGenFramebuffers(1, &fbo);
	glBindFramebuffer(GL_FRAMEBUFFER, fbo);
	//Bind the texture variable through
	glGenTextures(1, &textureBuffer);
	glBindTexture(GL_TEXTURE_2D, textureBuffer);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_SRGB, 800, 600, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL); //'SRGB' is used for Gamma Correction
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, textureBuffer, 0);
	return fbo;
}

unsigned int FBOLayer::initRBO() {
	unsigned int rbo;
	glGenRenderbuffers(1, &rbo);
	glBindRenderbuffer(GL_RENDERBUFFER, rbo);
	glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, 800, 600); // use a single renderbuffer object for both a depth AND stencil buffer.
	glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rbo); // now actually attach it

	if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
		std::cout << "ERROR::FRAMEBUFFER:: Framebuffer is not complete!" << std::endl;
	//Go back to standard framebuffer
	glBindFramebuffer(GL_FRAMEBUFFER, 0);
	return rbo;
}

void FBOLayer::begin(unsigned int fbo) {
	glBindFramebuffer(GL_FRAMEBUFFER, fbo);
}

void FBOLayer::BindTexture(unsigned int rbo, int width, int height) {
	glBindTexture(GL_TEXTURE_2D, textureBuffer);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
	glBindRenderbuffer(GL_RENDERBUFFER, rbo);
	glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, width, height);
}

void FBOLayer::initMainFBO(int width, int height) {
	glViewport(0, 0, width, height);
	glBindFramebuffer(GL_FRAMEBUFFER, 0);
	glDisable(GL_DEPTH_TEST);
	glClearColor(0.0f, .0f, .0f, 1.0f);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
}