#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPosition;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_normal1;
uniform sampler2D texture_specular1;
uniform vec3 viewPosition;
uniform float multiplex_time;

vec3 CalcPhongLighting(vec3 normal, vec3 fragPos, vec3 viewPos) {
	vec3 ambL = vec3(.5);
	vec3 ambient = ambL * texture(texture_diffuse1,TexCoords).rgb;

	vec3 diffL = vec3(.4);
	vec3 norm = normalize(normal);
	vec3 lightPos = vec3(0,1,8);
	vec3 lightDir = normalize(lightPos - fragPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diffL* diff * texture(texture_diffuse1,TexCoords).rgb;

	float specularStrength = 0.5;
	vec3 viewDir = normalize(viewPos - fragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 64);
	vec3 specular = vec3(specularStrength) * spec * texture(texture_specular1,TexCoords).rgb;

	return ambient + diffuse + specular;
}

void main()
{    
	vec3 res = CalcPhongLighting(texture(texture_normal1,TexCoords).rgb,FragPosition,viewPosition);
    FragColor = vec4(res,1);
}