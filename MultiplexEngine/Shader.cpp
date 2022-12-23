#include "Shader.h"
using namespace std;
Shader::Shader(const char* vertexPath, const char* fragmentPath, const char* geometryPath)
{
   std::string vertexCode;
   std::string fragmentCode;
   std::string geometryCode;
   std::ifstream vShaderFile;
   std::ifstream fShaderFile;
   std::ifstream gShaderFile;

   vShaderFile.exceptions(std::ifstream::failbit | std::ifstream::badbit);
   fShaderFile.exceptions(std::ifstream::failbit | std::ifstream::badbit);
   gShaderFile.exceptions(std::ifstream::failbit | std::ifstream::badbit);

   try {
      //Open files
      vShaderFile.open(vertexPath);
      fShaderFile.open(fragmentPath);
      std::stringstream vShaderStream, fShaderStream;

      //Read file's buffer contents into streams
      vShaderStream << vShaderFile.rdbuf();
      fShaderStream << fShaderFile.rdbuf();

      //Close file handlers
      vShaderFile.close();
      fShaderFile.close();

      //Convert stream into string
      vertexCode = vShaderStream.str();
      fragmentCode = fShaderStream.str();
      //if the geometry shader is given then load it
      if (geometryPath != nullptr)
      {
         gShaderFile.open(geometryPath);
         std::stringstream gShaderStream;
         gShaderStream << gShaderFile.rdbuf();
         gShaderFile.close();
         geometryCode = gShaderStream.str();
      }
   }
   catch (std::ifstream::failure e) {
      std::cout << "There was an error with compiling the shader program..." << std::endl;
   }
   const char* vShaderCode = vertexCode.c_str();
   const char* fShaderCode = fragmentCode.c_str();

   unsigned int vertex, fragment;
   int success;
   char infoLog[512];

   //Compiling vertex shader
   vertex = glCreateShader(GL_VERTEX_SHADER);
   glShaderSource(vertex, 1, &vShaderCode, NULL);
   glCompileShader(vertex);

   compileVertex = vertex;

   //Compile the fragment shader
   fragment = glCreateShader(GL_FRAGMENT_SHADER);
   glShaderSource(fragment, 1, &fShaderCode, NULL);
   glCompileShader(fragment);

   unsigned int geometry;
   if (geometryPath != nullptr)
   {
      const char* gShaderCode = geometryCode.c_str();
      geometry = glCreateShader(GL_GEOMETRY_SHADER);
      glShaderSource(geometry, 1, &gShaderCode, NULL);
      glCompileShader(geometry);
   }

   //Assign the shader program with vertex and fragment code
   ID = glCreateProgram();
   glAttachShader(ID, vertex);
   glAttachShader(ID, fragment);
   if (geometryPath != nullptr)
      glAttachShader(ID, geometry);
   glLinkProgram(ID);

   //Print any linking errors from the Shader Program
   glGetProgramiv(ID, GL_LINK_STATUS, &success);
   if (!success)
   {
      glGetProgramInfoLog(ID, sizeof(infoLog), NULL, infoLog);
      std::cout << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << infoLog << std::endl;
   }
   glDeleteShader(vertex);
   glDeleteShader(fragment);
   if (geometryPath != nullptr)
      glDeleteShader(geometry);

   //Set the fragment path variable
   fragmentString = fragmentPath;
}

void Shader::use() {
   glUseProgram(ID);
}

void Shader::setBool(const string& name, bool value) const {
   glUniform1i(glGetUniformLocation(ID, name.c_str()), (int)value);
}

void Shader::setInt(const string& name, int value) const {
   glUniform1i(glGetUniformLocation(ID, name.c_str()), value);
}

void Shader::setFloat(const string& name, float value) const {
   glUniform1f(glGetUniformLocation(ID, name.c_str()), value);
}

void Shader::setVec3(const std::string& name, const glm::vec3& value) const {
   //glUniform3fv(glGetUniformLocation(ID, name.c_str()), 1, &value[0]);
   glUniform3f(glGetUniformLocation(ID, name.c_str()), value.x, value.y, value.z);
}

void Shader::setFloat3(const std::string& name, float value1, float value2, float value3) const
{
   glUniform3f(glGetUniformLocation(ID, name.c_str()), value1, value2, value3);
}

void Shader::setMat4(const std::string& name, const glm::mat4& mat) const
{
   glUniformMatrix4fv(glGetUniformLocation(ID, name.c_str()), 1, GL_FALSE, &mat[0][0]);
}

//This will compile the fragment shader code from given input string. It will save the file and then compile new shader
void Shader::CompileFragmentFromString(const char* fragmentPath, const char* fragmentCode)
{
    //Write the new code to the file
    ofstream fl;
    fl.open(fragmentPath);
    fl << fragmentCode;
    fl.close();

    //Compile the new fragment shader program
    GLuint fragment;
    fragment = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fragment, 1, &fragmentCode, NULL);
    glCompileShader(fragment);

    //Make new shader program
    ID = glCreateProgram();
    glAttachShader(ID, compileVertex);
    glAttachShader(ID, fragment);
    glLinkProgram(ID);

    //Print any linking errors from the Shader Program
    int success;
    char infoLog[512];
    glGetProgramiv(ID, GL_LINK_STATUS, &success);
    if (!success)
    {
        glGetProgramInfoLog(ID, sizeof(infoLog), NULL, infoLog);
        std::cout << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << infoLog << std::endl;
    }
    glDeleteShader(fragment);
}