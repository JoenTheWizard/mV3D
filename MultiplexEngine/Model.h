#pragma once
#include <string>
#include "Shader.h"
#include <vector>
#include "Mesh.h"
//Assimp
#include <assimp/Importer.hpp>
#include <assimp/scene.h>
#include <assimp/postprocess.h>
#include <stb_image.h>
using namespace std;
class Model
{
public:
   vector<Mesh> meshes;
   vector<Texture> textures_loaded; //List of textures
   string directory;
   string fragmentShaderDirectory; //What fragment shader is the model using
   bool gammaCorrection;
   Model(const char* path, bool gamma = false) : gammaCorrection(gamma)
   {
      loadModel(path);
   }
   void Draw(Shader& shader);
private:
   void processNode(aiNode* node, const aiScene* scene);
   Mesh processMesh(aiMesh* mesh, const aiScene* scene);
   vector<Texture> loadMaterialTextures(aiMaterial* mat, aiTextureType type, string typeName);

   unsigned int TextureFromFile(const char* path, const string& directory);

   void loadModel(string path);
};

