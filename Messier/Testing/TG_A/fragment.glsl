#version 410 core

layout(location = 0) out vec4 Color;

in vec2 texCoordPS_in;
in vec3 normalPS_in;

in float flogz;
uniform float Fcoef;
uniform samplerCube heightmap;

void main()
{
	Color = textureCube(heightmap, normalPS_in);
	Color.a = 1;
	gl_FragDepth = log2(flogz) * Fcoef * 0.5;
}