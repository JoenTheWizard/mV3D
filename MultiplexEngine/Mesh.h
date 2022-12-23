#pragma once
#include <glad/glad.h>

#include <glm.hpp>
#include <gtc/matrix_transform.hpp>

#include "Shader.h"
#include <string>
#include <vector>
struct Vertex {
   glm::vec3 Position;
   glm::vec3 Normal;
   glm::vec2 TexCoords;
   glm::vec3 Tangent;
   glm::vec3 Bitangent;
};
struct Texture {
   unsigned int id;
   std::string type;
   std::string path;
};
using namespace std;
class Mesh
{
public:
   vector<Vertex> verticies;
   vector<unsigned int> indices;
   vector<Texture> textures;

   Mesh(vector<Vertex> verticies, vector<unsigned int> indices, vector<Texture> textures);
   ~Mesh();
   void Draw(Shader& shader);

private:
   unsigned int VAO, VBO, EBO;
   void setupMesh();
};

