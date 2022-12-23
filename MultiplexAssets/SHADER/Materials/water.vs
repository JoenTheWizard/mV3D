#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormals;
layout (location = 2) in vec2 aTexture;

out vec2 Textures;

out vec3 FragPos;
out vec4 ClipSpace;
out vec3 Normals;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;
void main()
{
	gl_Position = projection * view * model * vec4(aPos,1.0);
	
	Textures = aTexture;
	FragPos = vec3(model * vec4(aPos,1.0));
	Normals = aNormals;
	ClipSpace = gl_Position;
}