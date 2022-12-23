#version 330 core
out vec4 FragColor;

in vec2 Textures;
in vec3 FragPos;
in vec4 ClipSpace;
in vec3 Normals;

uniform vec3 viewPosition;
uniform sampler2D water;
uniform sampler2D dudv;
uniform samplerCube cubemap;
uniform float velocity;

vec4 WaterShader()
{
	vec2 ndc = (ClipSpace.xy/ClipSpace.w)/2.0+0.5;
	vec2 reflectTextCoords = vec2(ndc.x, -ndc.y);
	
	vec2 distortion1 = texture(dudv, vec2(Textures.x + velocity, Textures.y)).rg*0.1;
	distortion1 = Textures + vec2(distortion1.x,distortion1.y+velocity);
	vec2 totalDistortion = (texture(dudv,distortion1).rg*2.0-1.0)*0.5;
	
	reflectTextCoords += totalDistortion;
	return texture(water,reflectTextCoords);
}

void main()
{
	vec3 I = normalize(FragPos - viewPosition);
	vec3 R = reflect(I, normalize(Normals));
	
	vec2 distortion1 = (texture(dudv, vec2(Textures.x + velocity, Textures.y)).rg * 2.0 - 1.0) * 0.02;
	vec2 distortion2 = (texture(dudv, vec2(-Textures.x, Textures.y + velocity)).rg * 2.0 - 1.0) * 0.02;
	vec2 totalDistortion = distortion1 + distortion2;
	
	R.xy += totalDistortion;
	
	//Fresnel
	vec3 fresNorm = normalize(viewPosition - FragPos);
	float fresnelAngle = dot(fresNorm, vec3(0,1,0));
	
	//Output water 
	//Lineraly interpolates the cubemap texture with the water texture via the angle of camera position from the normal
	vec3 WaterResult = vec3(mix(texture(cubemap,vec3(R.xy,-R.z)),WaterShader(),clamp(fresnelAngle,0,1)));
	
	FragColor = vec4(WaterResult,1.);
}