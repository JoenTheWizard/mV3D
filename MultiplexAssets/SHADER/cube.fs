#version 330 core

out vec4 FragColor;
in vec3 Normals;
in vec2 Textures;
in vec3 Position;

in vec3 FragPos;

uniform vec3 viewPos;
uniform sampler2D textureA;
uniform sampler2D textureANormals;

vec3 calcLighting(vec3 color, vec3 lightColor, vec3 normals) {
	float ambientStrength = 0.5;
	vec3 ambient = ambientStrength * lightColor;
	
	//FragPosition
	vec3 LightPos = vec3(0,3.5,0);
	
	//Diffuse
	vec3 norm = normalize(normals);
	vec3 lightDir = normalize(LightPos - FragPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;
	
	//Specular
	float specularStrength = 0.5;
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir,norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	vec3 specular = specularStrength * spec * lightColor;
	
	return (ambient + diffuse + specular) * color;
}
void main()
{
	//Main texture
	vec3 col = texture(textureA,Textures).rgb;
	//Normal map
	vec4 normalMapColor = texture(textureANormals, Textures);
	vec3 brickNormal = vec3(normalMapColor.r * 2. - 1., normalMapColor.b, normalMapColor.g * 2. - 1);
	//Lighting
	vec3 Lighting = calcLighting(col, vec3(1.0), Normals);
	FragColor = vec4(Lighting, 1.0);
}