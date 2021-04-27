using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public partial class CameraRenderer
{
    CullingResults cullingResults;
    ScriptableRenderContext context;
    Camera camera;
 

    //static Material errorMaterial;
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

    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
            );
        buffer.BeginSample(SampleName);      
        ExecuteBuffer();     
    }

    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //The camera's transparency sort mode is used to determine whether to use orthographic
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        //Set the shader passes that this draw call can render.
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        //指出哪些渲染队列是允许的
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);  
        //ref代表引用传递, 再引用之前就已经创建了函数的实例,不会像值传递那样copy实例的副本
        //引用传递的返回值会影响实例本身,因为没有创建副本,所以开销较小.
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.DrawSkybox(camera);
    }

 

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull (float maxShadowDistance)
    {
        //ScriptableCullingParameters: 在筛选结果中控制筛选过程的参数
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
        bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,ShadowSettings shadowSettings)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    CameraRenderer renderer = new CameraRenderer();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing,
                shadowSettings);
        }
    }

    

}

