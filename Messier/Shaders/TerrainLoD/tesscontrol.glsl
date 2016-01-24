#version 410 core

layout (vertices = 3) out;
uniform vec3 eyePos;
uniform mat4 World;
in vec3 worldCS_in[];
in vec3 normalCS_in[];
out vec3 worldES_in[];
out vec3 normalES_in[];


void main()
{
    worldES_in[gl_InvocationID] = worldCS_in[gl_InvocationID];
    normalES_in[gl_InvocationID] = normalCS_in[gl_InvocationID];
    // Calculate the distance from the camera to the three control points
    
	/*
	float EyeToVertexDistance0 = distance(eyePos, worldCS_in[0]);
    float EyeToVertexDistance1 = distance(eyePos, worldCS_in[1]);
    float EyeToVertexDistance2 = distance(eyePos, worldCS_in[2]);
    // Calculate the tessellation levels
    gl_TessLevelOuter[0] = GetTessLevel(EyeToVertexDistance1, EyeToVertexDistance2);
    gl_TessLevelOuter[1] = GetTessLevel(EyeToVertexDistance2, EyeToVertexDistance0);
    gl_TessLevelOuter[2] = GetTessLevel(EyeToVertexDistance0, EyeToVertexDistance1);
    */

	vec3 avg = (worldCS_in[0] + worldCS_in[1] + worldCS_in[2]) / 3;
	avg = (World * vec4(avg, 1)).xyz;
    float dist = distance(eyePos, avg);
	if(dist < 20)gl_TessLevelOuter[0] = 20;
    else if(dist < 5 * 20)gl_TessLevelOuter[0] = 10;
    else gl_TessLevelOuter[0] = 1/dist;

    gl_TessLevelOuter[1] = gl_TessLevelOuter[0];
    gl_TessLevelOuter[2] = gl_TessLevelOuter[1];
    gl_TessLevelInner[0] = gl_TessLevelOuter[2];
}






