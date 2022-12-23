#version 330 core
out vec4 FragColor;

in vec2 TexCoords;
in vec3 Normals;
in vec3 FragPos;

uniform sampler2D texture_diffuse1;
uniform vec3 viewPos;

uniform float multiplex_time;

vec4 checkerboard(vec2 tex_coords, float scale) {
  float s = tex_coords[0];
  float t = tex_coords[1];

  float sum = floor(s * scale) + floor(t * scale);
  bool isEven = mod(sum,2.0)==0.0;
  float percent = (isEven) ? 1.0 : 0.0;

  vec3 purp = vec3(1,0,0.5);
  float totPercent = percent;
  return vec4(purp * totPercent,1.);
}

void main(){
    FragColor = checkerboard(TexCoords, 40.);
}