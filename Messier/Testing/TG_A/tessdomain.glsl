#version 410 core

layout(quads, ccw) in;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
uniform float Fcoef;

uniform samplerCube heightmap;

in vec3 worldES_in[];
in vec2 texCoordES_in[];
in vec3 normalES_in[];

out vec3 worldPS_in;
out vec2 texCoordPS_in;
out vec3 normalPS_in;

out float flogz;




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

vec4 snoiseCoeff(vec3 p)
{
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

	return d;
}

float snoise(vec3 p) {

	vec4 d = snoiseCoeff(p);

    return min(1.0, max(1.0 - abs(dot(d, vec4(52.0))), 0.0));
}

float snoiseFractal(vec3 m) {
    return   0.5333333* snoise(m)
                +0.2666667* snoise(2.0*m)
                +0.1333333* snoise(4.0*m)
                +0.0666667* snoise(8.0*m);
}

float snoise2(vec3 p) {

	vec4 d = snoiseCoeff(p);

    return min(1.0, max(dot(d, vec4(52.0)), 0.0));
}

float snoiseFractal2(vec3 m) {
    return   0.5333333* snoise2(m)
                +0.2666667* snoise2(2.0*m)
                +0.1333333* snoise2(4.0*m)
                +0.0666667* snoise2(8.0*m);
}



vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2, vec2 v3)
{
	vec2 p0 = mix(v0, v3, gl_TessCoord.y);
	vec2 p1 = mix(v1, v2, gl_TessCoord.y);
	return normalize(mix(p0, p1, gl_TessCoord.x));
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2, vec3 v3)
{
	vec3 p0 = mix(v0, v3, gl_TessCoord.y);
	vec3 p1 = mix(v1, v2, gl_TessCoord.y);
	return normalize(mix(p0, p1, gl_TessCoord.x));
}

void main()
{
	// Interpolate the attributes of the output vertex using the barycentric coordinates
   	texCoordPS_in = interpolate2D(texCoordES_in[0], texCoordES_in[1], texCoordES_in[2], texCoordES_in[3]);
   	normalPS_in = interpolate3D(normalES_in[0], normalES_in[1], normalES_in[2], normalES_in[3]);
   	worldPS_in = interpolate3D(worldES_in[0], worldES_in[1], worldES_in[2], worldES_in[3]);
   	normalPS_in = normalize(worldPS_in);
	
	vec4 hP = texture(heightmap, normalPS_in);	
	hP.r = snoiseFractal2(normalPS_in * 2.0 + 10.0);// * step(0.85, snoiseFractal(vec3(texCoordPS_in, timer) * 4.0 + 43.0));
	hP.g = snoise(normalPS_in * 4.0 + 3.0);
	hP.g = hP.g * hP.g * hP.g;
	hP.g *= 1 - snoiseFractal(normalPS_in * 4.0);
	hP.g += snoiseFractal2(normalPS_in * 5000) * 0.05;

	hP.b = 0;
	
	hP.a = 1;

	gl_Position = (Proj * View * World) * vec4(worldPS_in, 1);
	//gl_Position = (Proj * View * World) * vec4(normalize(worldPS_in) + (hP.g - hP.r) * 0.1f * normalPS_in, 1);
	gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0; 	

	flogz = 1.0 + gl_Position.w;
}