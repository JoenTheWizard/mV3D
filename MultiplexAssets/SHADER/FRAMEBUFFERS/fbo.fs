#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D envFbo;
uniform float time;
//uniform float gamma;

//const float offset = 1.0/300.0;
void main()
{
	//vec3 fg = pow(texture(screenTexture, TexCoords).rgb, vec3(1.0/gamma));
	FragColor = texture(envFbo, TexCoords); //vec4(fg,1.0);
}