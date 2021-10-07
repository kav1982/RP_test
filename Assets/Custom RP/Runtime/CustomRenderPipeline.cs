using UnityEngine;
using UnityEngine.Rendering;


public partial class CameraRenderer
{
    CullingResults cullingResults;
    ScriptableRenderContext context;
    Camera camera;
 

    //static Material errorMaterial;
    //命令缓冲区的名字 Render Camera
    const string bufferName = "Render Camera";
    static ShaderTagId 
        unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        litShaderTagId = new ShaderTagId("CustomLit");
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    //static ShaderTagId[] legacyShaderTagIds =
    //{
    //    new ShaderTagId("Always"),
    //    new ShaderTagId("ForwardBase"),
    //    new ShaderTagId("PrepassBase"),
    //    new ShaderTagId("Vertex"),
    //    new ShaderTagId("VertexLMRGBM"),
    //    new ShaderTagId("VertexLM")      
    //};

    Lighting lighting = new Lighting();
    //用于渲染单个摄像机的新类,建立相机的渲染流水线
    public void Render (ScriptableRenderContext context, Camera camera, 
        bool useDynamicBatching, bool useGPUInstancing,ShadowSettings shadowSettings
        )
    {
        this.context = context;
        this.camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance))
            return;

        //Setup();
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        lighting.Setup(context, cullingResults, shadowSettings);
        buffer.EndSample(SampleName);
        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        lighting.Cleanup();
        Submit();
    }

    //对不支持的shader指定默认的渲染材质
    //void DrawUnsupportedShaders()
    //{
    //    if (errorMaterial == null)
    //        errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
    //    var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
    //    {
    //        overrideMaterial = errorMaterial
    //    };
    //    //遍历legacyShaderTagIds中所有的shader类型
    //    for (int i = 1; i<legacyShaderTagIds.Length; i++)
    //    {
    //        drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
    //    }
    //    var filteringSettings = FilteringSettings.defaultValue;
    //    context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    //}

    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //The camera's transparency sort mode is used to determine whether to use orthographic
        //sortingSettings 用于确定基于正交还是透视图的应用排序
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = PerObjectData.Lightmaps | PerObjectData.LightProbe |
            PerObjectData.LightProbeProxyVolume
        };
        
        drawingSettings.SetShaderPassName(1, litShaderTagId);      
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);        
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.DrawSkybox(camera);
    }

    void Setup()
    {
        //SetupCameraProperties(camera)摄像机的位置和方向以及透视等信息的传递
        //Setup camera specific global shader variables.
        context.SetupCameraProperties(camera);
        //上下文会延迟实际的渲染,直到我们提交它为止
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,    //深度
            flags == CameraClearFlags.Color,    //颜色
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear   //用于清除的颜色
            );
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }



    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        //执行和清除总是一起完成的
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull (float maxShadowDistance)
    {
        //ScriptableCullingParameters: 在筛选结果中控制筛选过程的参数
        //读取摄像机的Cull设置以便于显示内容,它返回是否成功获取该参数
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}

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

