#pragma once
#ifndef SHADER_H
#define SHADER_H

#include <glad/glad.h>
#include <string>
#include <fstream>
#include <sstream>
#include <iostream>
#include <glm.hpp>
class Shader
{
public:
   unsigned int ID;
   const char* fragmentString;
   Shader(const char* vertexPath, const char* fragmentPath, const char* geometryShader = nullptr);
   void use();
   void CompileFragmentFromString(const char* fragmentPath, const char* fragmentCode);
   //GLSL types
   void setBool(const std::string& name, bool value) const;
   void setInt(const std::string& name, int value) const;
   void setFloat(const std::string& name, float value) const;
   void setVec3(const std::string& name, const glm::vec3& value) const;
   void setFloat3(const std::string& name, float value0, float value1, float value2) const;
   void setMat4(const std::string& name, const glm::mat4& mat) const;
private:
	GLuint compileVertex;
};
#endif
