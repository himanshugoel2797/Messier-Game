#version 410 core

layout(triangles, equal_spacing, ccw) in;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

in vec3 worldES_in[];
in vec2 texCoordES_in[];
in vec3 normalES_in[];

out vec3 worldPS_in;
out vec2 texCoordPS_in;
out vec3 normalPS_in;

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y) * v1 + vec3(gl_TessCoord.z) * v2;
}

void main()
{
	// Interpolate the attributes of the output vertex using the barycentric coordinates
   	texCoordPS_in = interpolate2D(texCoordES_in[0], texCoordES_in[1], texCoordES_in[2]);
   	normalPS_in = interpolate3D(normalES_in[0], normalES_in[1], normalES_in[2]);
   	normalPS_in = normalize(normalPS_in);
   	worldPS_in = interpolate3D(worldES_in[0], worldES_in[1], worldES_in[2]);
	
	gl_Position = (Proj * View * World) * vec4(worldPS_in, 1); 	
}