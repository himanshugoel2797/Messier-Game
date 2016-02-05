#version 410 core

layout(location = 0) out vec4 Color;

in vec2 UV;

in float flogz;
uniform float Fcoef;

uniform float timer;
uniform sampler2D img;
uniform vec3 eyePos;

void main()
{
  Color = texture2D(img, UV);
  Color.a = 1;
}