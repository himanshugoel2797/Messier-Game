#version 410 core

layout (vertices = 3) out;

uniform vec3 eyePos;

in vec3 worldCS_in[];
in vec2 texCoordCS_in[];
in vec3 normalCS_in[];

out vec3 worldES_in[];
out vec2 texCoordES_in[];
out vec3 normalES_in[];

float GetTessLevel(float Distance0, float Distance1)
{
    float AvgDistance = (Distance0 + Distance1) / 2.0;

    if (AvgDistance <= 2.0) {
        return 10.0;
    }
    else if (AvgDistance <= 5.0) {
        return 7.0;
    }
    else {
        return 3.0;
    }
}

void main()
{
	texCoordES_in[gl_InvocationID] = texCoordCS_in[gl_InvocationID];
	worldES_in[gl_InvocationID] = worldCS_in[gl_InvocationID];
	normalES_in[gl_InvocationID] = normalCS_in[gl_InvocationID];

	// Calculate the distance from the camera to the three control points
    float EyeToVertexDistance0 = distance(eyePos, worldES_in[0]);
    float EyeToVertexDistance1 = distance(eyePos, worldES_in[1]);
    float EyeToVertexDistance2 = distance(eyePos, worldES_in[2]);

    // Calculate the tessellation levels
    gl_TessLevelOuter[0] = GetTessLevel(EyeToVertexDistance1, EyeToVertexDistance2);
    gl_TessLevelOuter[1] = GetTessLevel(EyeToVertexDistance2, EyeToVertexDistance0);
    gl_TessLevelOuter[2] = GetTessLevel(EyeToVertexDistance0, EyeToVertexDistance1);
    gl_TessLevelInner[0] = gl_TessLevelOuter[2];
}