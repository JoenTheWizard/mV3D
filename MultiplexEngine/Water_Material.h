#pragma once
#include <vector>
#include <Shader.h>
#include <Texture2D.h>
#include <glm.hpp>
#include <gtc/matrix_transform.hpp>
#include <GLFW/glfw3.h>
class Water_Material
{
public:
	Water_Material(); ~Water_Material();
	void DrawWaterPlane(Shader* waterShader, Texture2D waterTexture, Texture2D dudvMap, 
		glm::vec3 CamPos, unsigned int cubeMapTextures, glm::mat4 model, glm::mat4 projection, 
		glm::mat4 view);
	void setWaterPlaneSize(float size);
	unsigned int sizeVAO; //for reusibility of the water vao
	std::vector<glm::vec3> positions;
};

