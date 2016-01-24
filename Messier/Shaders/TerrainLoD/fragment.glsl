#version 410 core

layout(location = 0) out vec4 Color;
in vec3 worldPS_in;
in vec3 normalPS_in;
in float flogz;
uniform float Fcoef;
uniform vec3 eyePos;
uniform float timer;
uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
uniform vec3 lightDir;
uniform sampler2D tex;
uniform sampler2D normMap;
uniform sampler2D snowMap;

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




vec3 getBlendFactor(vec3 wNorm)
{
    vec3 blending = abs( wNorm );
    blending = normalize(max(blending, 0.00001));
    // Force weights to sum to 1.0
	float b = (blending.x + blending.y + blending.z);
    blending /= vec3(b, b, b);
    return blending;
}






vec4 calcBlend(sampler2D tex, vec3 blending, vec3 coords)
{
    vec4 xaxis = texture2D( tex, coords.yz);
    vec4 yaxis = texture2D( tex, coords.xz);
    vec4 zaxis = texture2D( tex, coords.xy);
    // blend the results of the 3 planar projections.
	vec4 t = xaxis * blending.x + yaxis * blending.y + zaxis * blending.z;
    return t;
}





mat3 cotangent_frame(vec3 N, vec3 p, vec2 uv)
{
    // get edge vectors of the pixel triangle
    vec3 dp1 = dFdx( p );
    vec3 dp2 = dFdy( p );
    vec2 duv1 = dFdx( uv );
    vec2 duv2 = dFdy( uv );
    // solve the linear system
    vec3 dp2perp = cross( dp2, N );
    vec3 dp1perp = cross( N, dp1 );
    vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;
    // construct a scale-invariant frame 
    float invmax = inversesqrt( max( dot(T,T), dot(B,B) ) );
    return mat3( T * invmax, B * invmax, N );
}




 
vec3 perturb_normal(vec3 map, vec3 N, vec3 V, vec2 texcoord )
{
    // assume N, the interpolated vertex normal and 
    // V, the view vector (vertex to eye)
   map = map * 255./127. - 128./127.;
    mat3 TBN = cotangent_frame(N, -V, texcoord);
    return normalize(TBN * map);
}





void main()
{
    vec3 wNorm = normalize((World * vec4(normalPS_in, 0)).xyz);
    vec3 bFac = getBlendFactor(wNorm);
    wNorm = perturb_normal(calcBlend(normMap, bFac, worldPS_in/4).rgb, normalPS_in, worldPS_in, worldPS_in.xy);
    vec3 col;
    if(worldPS_in.y - (dot(vec3(0, 1, 0), wNorm)) * 5 > 10 - snoiseFractal(worldPS_in / 16 + 8) * 10){
    //if(worldPS_in.y > 5 * 20 - snoiseFractal(worldPS_in / 16 + 8) * 10 && dot(normalPS_in, vec3(0, 1, 0)) > 0.9){
	    col = vec3(0.8);
		wNorm = perturb_normal(calcBlend(snowMap, bFac, worldPS_in/4).rgb, normalPS_in, worldPS_in, worldPS_in.xy);
    }
	else col = calcBlend(tex, bFac, worldPS_in/4).rgb;
    wNorm = normalize((World * vec4(wNorm, 0)).xyz);
    vec3 v2edir = normalize(eyePos - worldPS_in);
    vec3 lightReflect = normalize(reflect(-lightDir, wNorm));
    Color.rgb = col * min(dot(lightDir, wNorm) + dot(lightReflect, v2edir) + 0.2, 1);
    //Color.rgb = normalPS_in;
    Color.a = 1;
    gl_FragDepth = log2(flogz) * Fcoef * 0.5;
}


















