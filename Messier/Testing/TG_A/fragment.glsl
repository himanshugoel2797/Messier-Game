#version 410 core

layout(location = 0) out vec4 Color;

in vec2 texCoordPS_in;

uniform sampler2D img;

void main()
{
	Color = texture2D(img, texCoordPS_in);
}