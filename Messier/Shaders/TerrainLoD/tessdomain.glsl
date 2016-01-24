#version 410 core

layout(triangles, equal_spacing, ccw) in;
uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
uniform float Fcoef;
in vec3 worldES_in[];
in vec3 normalES_in[];
out vec3 worldPS_in;
out vec3 normalPS_in;
out float flogz;

uniform sampler2D normMap;
uniform sampler2D snowMap;

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
   	normalPS_in = interpolate3D(normalES_in[0], normalES_in[1], normalES_in[2]);
    vec3 wPos = interpolate3D(worldES_in[0], worldES_in[1], worldES_in[2]);

	//Calculate the face normal
	vec3 t0 = worldES_in[2] - worldES_in[1];
	vec3 t1 = worldES_in[0] - worldES_in[1];
	vec3 fNorm = normalize(cross(t0, t1));
    vec3 cNorm = normalize((normalES_in[0] + normalES_in[1] + normalES_in[2])/3);

	worldPS_in = wPos;// + normalPS_in * cross(fNorm, normalPS_in) * cross(normalES_in[1], normalES_in[0]) * cross(normalES_in[2], normalES_in[0]);

	//if(any(equal(gl_TessCoord, vec3(0))))worldPS_in = wPos;


    //vec3 cPos = (worldES_in[0] + worldES_in[1] + worldES_in[2])/3;
    //worldPS_in = wPos + (cNorm - normalPS_in) * (0.5 - min(distance(cPos, wPos), 0.5));
    
	gl_Position = (Proj * View * World) * vec4(worldPS_in, 1);
    gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0;
    flogz = 1.0 + gl_Position.w;
}

