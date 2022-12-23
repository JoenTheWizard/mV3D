#include "Water_Material.h"

Water_Material::Water_Material() {
	setWaterPlaneSize(25.0f);
}

Water_Material::~Water_Material() {}
void Water_Material::setWaterPlaneSize(float size) {
	unsigned int planeVAO;
	float planeVertices[] = {
		// positions            // normals         // texcoords
		 size, -0.5f,  size,  0.0f, 1.0f, 0.0f,  size,  0.0f,
		-size, -0.5f,  size,  0.0f, 1.0f, 0.0f,   0.0f,  0.0f,
		-size, -0.5f, -size,  0.0f, 1.0f, 0.0f,   0.0f, size,

		 size, -0.5f,  size,  0.0f, 1.0f, 0.0f,  size,  0.0f,
		-size, -0.5f, -size,  0.0f, 1.0f, 0.0f,   0.0f, size,
		 size, -0.5f, -size,  0.0f, 1.0f, 0.0f,  size, size
	};
	// plane VAO
	unsigned int planeVBO;
	glGenVertexArrays(1, &planeVAO);
	glGenBuffers(1, &planeVBO);
	glBindVertexArray(planeVAO);
	glBindBuffer(GL_ARRAY_BUFFER, planeVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(planeVertices), planeVertices, GL_STATIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(1);
	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(3 * sizeof(float)));
	glEnableVertexAttribArray(2);
	glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(6 * sizeof(float)));
	glBindVertexArray(0);
	sizeVAO = planeVAO;
}

void Water_Material::DrawWaterPlane(Shader* waterShader,
	Texture2D waterTexture, Texture2D dudvMap, glm::vec3 CamPos,
	unsigned int cubeMapTextures, glm::mat4 model, glm::mat4 projection, glm::mat4 view) {
	glActiveTexture(GL_TEXTURE0);
	waterTexture.Use();
	glActiveTexture(GL_TEXTURE1);
	dudvMap.Use();
	glActiveTexture(GL_TEXTURE2);
	glBindTexture(GL_TEXTURE_CUBE_MAP, cubeMapTextures);

	waterShader->use();
	waterShader->setMat4("projection", projection);
	waterShader->setMat4("view", view);
	//Texture Binding
	waterShader->setInt("water", 0);
	waterShader->setInt("dudv", 1);
	waterShader->setInt("cubemap", 2);
	//Input vars
	waterShader->setFloat("velocity", 0.03 * glfwGetTime());
	waterShader->setVec3("viewPosition", CamPos);

	glBindVertexArray(sizeVAO);
	for (int i = 0; i < positions.size(); i++) {
		model = glm::mat4(1.0);
		model = glm::translate(model, positions.at(i));
		waterShader->setMat4("model", model);
		glDrawArrays(GL_TRIANGLES, 0, 6);
	}
}