#version 440 core

layout(location = 0) in vec4 position;
layout(location = 1) in float mat;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
uniform float Fcoef;
uniform vec3 Normal;

uniform int range1;
uniform int range2;
uniform int range3;
uniform int range4;
uniform int range5;


out float material;
out vec3 worldCS_in;
out vec3 normal_in;
out float flogz;

void main()
{
	vec4 wPos = World * vec4(position.xyz, 1);

	worldCS_in = wPos.xyz;
	
	if(gl_VertexID < range1)
	{
		normal_in = vec3(0, 0, -1);
	}else if(gl_VertexID < range2)
	{
		normal_in = vec3(0, -1, 0);
	}else if(gl_VertexID < range3)
	{
		normal_in = vec3(-1, 0, 0);
	}else if(gl_VertexID < range4)
	{
		normal_in = vec3(0, 0, 1);
	}else if(gl_VertexID < range5)
	{
		normal_in = vec3(0, 1, 0);
	}else{
		normal_in = vec3(1, 0, 0);
	}

	gl_Position = Proj * View * World * vec4(position.xyz, 1);
	gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0; 	
	gl_PointSize = 10;

	material = mat;
	flogz = 1.0 + gl_Position.w;
}