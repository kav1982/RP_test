#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

//光线的强度
float3 IncomingLight (Surface surface, Light light)
{
	return 
		saturate(dot(surface.normal, light.direction) * light.attenuation) * light.color;
}

float3 DirectBRDF (Surface surface, BRDF brdf, Light light)
{
	return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

float3 GetLighting (Surface surface, BRDF brdf, Light light)
{
	//return IncomingLight(surface,light) * brdf.diffuse;
	return IncomingLight(surface,light) * DirectBRDF(surface, brdf, light);
}

//在GetLighting中获取阴影数据并传递
float3 GetLighting (Surface surfaceWS, BRDF brdf, GI gi)
{
	ShadowData shadowData = GetShadowData(surfaceWS);
	shadowData.shadowMask = gi.shadowMask;
	//return gi.shadowMask.shadows.rgb;
	//return GetLighting(surface, GetDirectionalLight());
	float3 color = gi.diffuse * brdf.diffuse;
	for(int i=0; i < GetDirectionalLightCount(); i++)
	{
		Light light = GetDirectionalLight(i, surfaceWS, shadowData);
		color += GetLighting(surfaceWS, brdf, light);
	}
	return color;
}

#endif




