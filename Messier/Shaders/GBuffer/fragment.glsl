#version 430 core

in vec2 UV;
in float depth;
in vec3 worldXY;
smooth in vec3 normPos;

layout(location = 0) out vec4 RGBA0;
layout(location = 3) out vec4 Depth0;
layout(location = 1) out vec4 Normal0;
layout(location = 2) out vec4 Material0;

uniform sampler2D DiffuseTex;
uniform sampler2D NormalTex;
uniform vec3 DiffuseColor;
uniform vec3 AmbientColor;
uniform float SpecExp;
uniform vec3 SpecColor;

void main()
{
  RGBA0 = texture(DiffuseTex, UV) * vec4(DiffuseColor, 1) + vec4(AmbientColor, 1) * 0.2;
  //RGBA0 = vec4(1);
  RGBA0.a = 1;
  
  Depth0.xyz = normalize(worldXY);
  Depth0.w = 1;

  Normal0.xyz = normalize(normPos);
  Normal0.w = 1;

  Material0 = vec4(1);
  Material0.xyz = SpecColor.xyz;
  Material0.w = SpecExp;
}
