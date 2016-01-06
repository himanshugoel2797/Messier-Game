#version 410 core

layout(location = 0) in vec4 position;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
uniform float Fcoef;
uniform vec3 Normal;

flat out int material;
out vec3 worldCS_in;

out float flogz;

void main()
{
	vec4 wPos = World * vec4(position.xyz, 1);

	worldCS_in = wPos.xyz;
	gl_Position = Proj * View * World * vec4(position.xyz, 1);
	gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0; 	

	gl_PointSize = 10;

	material = int(position.w);
	flogz = 1.0 + gl_Position.w;
}