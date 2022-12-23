#version 330 core

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;
uniform sampler2D lightSource;

void main()
{
	vec4 outTx = texture(lightSource, fs_in.TexCoords);
	if (outTx.r > 0.6)
		discard;
	gl_FragColor = outTx;
}