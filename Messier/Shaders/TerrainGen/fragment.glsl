#version 410 core

layout(location = 0) out vec4 Color;

in vec2 texCoordPS_in;
in vec3 normalPSi_in;

uniform float timer;

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

    return min(1.0, max(1.0 - abs(dot(d, vec4(52.0))), 0.0));
}

float snoiseFractal(vec3 m) {
    return   0.5333333* snoise(m)
                +0.2666667* snoise(2.0*m)
                +0.1333333* snoise(4.0*m)
                +0.0666667* snoise(8.0*m);
}

float snoise2(vec3 p) {

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

    return min(1.0, max(dot(d, vec4(52.0)), 0.0));
}

float snoiseFractal2(vec3 m) {
    return   0.5333333* snoise2(m)
                +0.2666667* snoise2(2.0*m)
                +0.1333333* snoise2(4.0*m)
                +0.0666667* snoise2(8.0*m);
}

void main()
{
	//Color.r = snoiseFractal2(vec3(texCoordPS_in, timer) * 2.0 + 10.0);// * step(0.85, snoiseFractal(vec3(texCoordPS_in, timer) * 4.0 + 43.0));
	//Color.g = snoise2(vec3(texCoordPS_in, timer) * 16.0 + 800.0) * step(0.85, snoiseFractal(vec3(texCoordPS_in, timer) * 4.0 + 3.0)) + snoise(vec3(texCoordPS_in, timer) * 8.0 + 80.0) * (1.0 - snoiseFractal(vec3(texCoordPS_in, timer) * 8.0 + 8.0)) * step(0.85, snoiseFractal(vec3(texCoordPS_in, timer) * 4.0 + 3.0));
	//Color.b = snoiseFractal2(vec3(texCoordPS_in, timer) * 2.0 + 0.0);

	vec3 normalPS_in = vec3(texCoordPS_in, timer);

	Color.r = snoiseFractal2(normalPS_in * 2.0 + 10.0);// * step(0.85, snoiseFractal(vec3(texCoordPS_in, timer) * 4.0 + 43.0));
	Color.g = snoise(normalPS_in * 4.0 + 3.0);
	Color.g = Color.g * Color.g * Color.g;
	Color.g *= 1 - snoiseFractal(normalPS_in * 4.0);
	
	Color.b = 0;
	
	Color.a = 1;
	
	Color.a = 1;
}