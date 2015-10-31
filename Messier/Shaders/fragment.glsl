#version 410 core

layout(location = 0) out vec4 Color;

in vec2 texCoordPS_in;

uniform float timer;
uniform sampler2D img;
uniform vec3 viewPos;

float chiGGX(float v)
{
    return v > 0 ? 1 : 0;
}

float GGX_Distribution(vec3 n, vec3 h, float alpha)
{
    float NoH = dot(n,h);
    float alpha2 = alpha * alpha;
    float NoH2 = NoH * NoH;
    float den = NoH2 * alpha2 + (1 - NoH2);
    return (chiGGX(NoH) * alpha2) / ( 3.14159f * den * den );
}

float GGX_PartialGeometryTerm(vec3 v, vec3 n, vec3 h, float alpha)
{
    float VoH2 = clamp(dot(v,h), 0.0, 1.0);
    float chi = chiGGX( VoH2 / clamp(dot(v,n), 0.0, 1.0) );
    VoH2 = VoH2 * VoH2;
    float tan2 = ( 1 - VoH2 ) / VoH2;
    return (chi * 2) / ( 1 + sqrt( 1 + alpha * alpha * tan2 ) );
}

vec3 Fresnel_Schlick(float cosT, vec3 F0)
{
  return F0 + (1-F0) * pow( 1 - cosT, 5);
}

vec3 cook_torrance(vec3 lightPos, vec3 normal, vec3 viewPos, float ior, float roughness)
{
		float NoV = clamp(dot(normal, viewPos), 0.0, 1.0);
		vec3 sampleVector = lightPos;
		vec3 F0 = vec3(ior);//clamp(vec3( ((1 - ior)/(1 + ior)) * ((1 - ior)/(1 + ior)) ), 0, 1);

		// Calculate the half vector
        vec3 halfVector = normalize(sampleVector + viewPos);
        float cosT = clamp(dot(sampleVector, normal), 0.0, 1.0 );
        float sinT = sqrt( 1 - (cosT * cosT));

        // Calculate fresnel
        vec3 fresnel = Fresnel_Schlick( clamp(dot( halfVector, viewPos ), 0.0, 1.0), F0 );
        // Geometry term
        float geometry = GGX_PartialGeometryTerm(viewPos, normal, halfVector, roughness) * GGX_PartialGeometryTerm(sampleVector, normal, halfVector, roughness);
        // Calculate the Cook-Torrance denominator
        float denominator = clamp( 4 * (NoV * clamp(dot(halfVector, normal), 0.0, 1.0) + 0.05), 0.0, 1.0 );
        vec3 kS = fresnel;

		return (geometry * fresnel * sinT)/denominator;
}


void main()
{
	vec3 lightPos = viewPos;
	//vec3 lightPos = vec3(1.0f, 1.0f, 1.0f);
	vec3 normal = vec3(0, 0.0f, -1.0f);
	float ior = 0.1f;
	float roughness = 1.0f;

	Color = texture2D(img, texCoordPS_in) * vec4(cook_torrance(lightPos, normal, viewPos, ior, roughness), 1);
}