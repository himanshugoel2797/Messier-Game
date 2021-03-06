﻿#version 410 core

layout(location = 0) out vec4 Color;
in float material;
in vec3 worldCS_in;
in vec3 normal_in;
in float flogz;
uniform float Fcoef;
uniform float timer;
uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
uniform vec3 lightDir;
uniform sampler2D tex;
uniform samplerBuffer materialColors;
//  <www.shadertoy.com/view/XsX3zB>
//  by Nikita Miropolskiy

/* discontinuous pseudorandom uniformly distributed in [-0.5, +0.5]^3 */
vec3 random3(vec3 c) {
    float j = 4096.0*sin(dot(c,vec3(17.0, 59.4, 15.0)));
    vec3 r;
    r.z = fract(512.0*j);
    j *= .125;
    r.x = fract(512.0*j);
    j *= .125;
    r.y = fract(512.0*j);
    return r-0.5;
}







const float F3 =  0.3333333;
const float G3 =  0.1666667;
float snoise(vec3 p) {
    vec3 s = floor(p + dot(p, vec3(F3)));
    vec3 x = p - s + dot(s, vec3(G3));
    vec3 e = step(vec3(0.0), x - x.yzx);
    vec3 i1 = e*(1.0 - e.zxy);
    vec3 i2 = 1.0 - e.zxy*(1.0 - e);
    vec3 x1 = x - i1 + G3;
    vec3 x2 = x - i2 + 2.0*G3;
    vec3 x3 = x - 1.0 + 3.0*G3;
    vec4 w, d;
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);
    w = max(0.6 - w, 0.0);
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);
    w *= w;
    w *= w;
    d *= w;
    return dot(d, vec4(52.0));
}







float snoiseFractal(vec3 m) {
    return   0.5333333* snoise(m)
                +0.2666667* snoise(2.0*m)
                +0.1333333* snoise(4.0*m)
                +0.0666667* snoise(8.0*m);
}








void main()
{
    vec4 materialInfo = texelFetch(materialColors, int(material));
    vec3 wNorm = normalize((World * vec4(normal_in, 0)).xyz);
    //if(material == 1)Color.rgb = vec3(1);
	vec3 coords = worldCS_in;
    vec3 blending = abs( wNorm );
    blending = normalize(max(blending, 0.00001));
    // Force weights to sum to 1.0
	float b = (blending.x + blending.y + blending.z);
    blending /= vec3(b, b, b);
    vec4 xaxis = texture2D( tex, coords.yz);
    vec4 yaxis = texture2D( tex, coords.xz);
    vec4 zaxis = texture2D( tex, coords.xy);
    // blend the results of the 3 planar projections.
	vec4 tex = xaxis * blending.x + yaxis * blending.y + zaxis * blending.z;
    
    Color.rgb = tex.rgb * max(dot(lightDir, wNorm), 0);
    Color.a = materialInfo.a;
    gl_FragDepth = log2(flogz) * Fcoef * 0.5;
}





