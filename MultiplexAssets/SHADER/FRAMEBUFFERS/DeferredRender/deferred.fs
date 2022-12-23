#version 330 
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

void main()
{
	//Temporarily using this to just debug the deferred render (such as the normal texture layer)
	FragColor = texture(gNormal, TexCoords);
}