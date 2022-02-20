using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
	bool useDynamicBatching, useGPUInstancing;

	ShadowSettings shadowSettings;

	public CustomRenderPipeline(
		bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
	{
		this.useDynamicBatching = useDynamicBatching;
		this.useGPUInstancing = useGPUInstancing;       
		this.shadowSettings = shadowSettings;
		GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
		GraphicsSettings.lightsUseLinearIntensity = true;
	}

	CameraRenderer renderer = new CameraRenderer();
	//创建一个渲染的实例,使用它在一个循环中渲染所有的相机
	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		foreach (Camera camera in cameras)
		{
			renderer.Render(context, camera, useDynamicBatching, useGPUInstancing,
				shadowSettings);
		}
	}

    

}