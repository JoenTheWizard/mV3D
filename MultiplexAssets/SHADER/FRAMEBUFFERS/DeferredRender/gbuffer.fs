#version 330
//Layout each of the GBuffer layers
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

in vec3 FragPos;
in vec2 TexCoords;
in vec3 Normal;

uniform sampler2D texture_diffuse;
uniform sampler2D texture_specular;

void main()
{
		//Position and normal vectors are stored in the gbuffer texture layers
		gPosition = FragPos;
		gNormal = normalize(Normal);
		
		gAlbedoSpec.rgb = texture(texture_diffuse, TexCoords).rgb;
		//This will store the intensity of the specular within the alpha channel
		gAlbedoSpec.a = texture(texture_specular, TexCoords).r;
		
}