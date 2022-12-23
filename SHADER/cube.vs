#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNorms;
layout (location = 2) in vec2 aText;

out vec3 Normals;
out vec2 Textures;
out vec3 Position;
out vec3 FragPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() {
	gl_Position = projection * view* model * vec4(aPos, 1.0);
	Normals = aNorms;
	Textures = aText;
	Position = aPos;
	FragPos = vec3(model * vec4(aPos,1.0));
}