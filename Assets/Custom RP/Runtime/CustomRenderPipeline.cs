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
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
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


    public void Render (ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
            return;

        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
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

    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        //指出哪些渲染队列是允许的
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);       
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

    bool Cull ()
    {      
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}

public class CustomRenderPipeline : RenderPipeline
{
    public CustomRenderPipeline()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }

    CameraRenderer renderer = new CameraRenderer();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }

}

