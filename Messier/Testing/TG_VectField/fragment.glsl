#version 410 core

layout(location = 0) out vec4 Color;

in vec2 texCoordPS_in;
in vec3 normalPS_in;

in float flogz;
uniform float Fcoef;
uniform sampler2D heightmap;
uniform float texScale;
uniform vec2 texOffset;

void main()
{
	
	Color.rgb = texture2D(heightmap, (texCoordPS_in +  + texOffset) * texScale).rgb;
	Color.a = 1;
	gl_FragDepth = log2(flogz) * Fcoef * 0.5;
}