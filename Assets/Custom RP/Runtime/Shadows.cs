using UnityEngine;
using UnityEngine.Rendering;

public class Shadows 
{
    const string bufferName = "Shadows";

    const int maxShadowedDirectionalLightCount = 1;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] ShadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    


    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    CullingResults cullingResults;

    ShadowSettings settings;

    int ShadowedDirectionalLightCount;

    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults,
        ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        ShadowedDirectionalLightCount = 0;
    }
    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if(ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f && 
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            ShadowedDirectionalLights[ShadowedDirectionalLightCount++] =
                new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex
                };
        }
    }

    public void Render()
    {
        if(ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
    }

    static int dirshadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    void RenderDirectionalShadows() 
    {
        int atlasSize = (int)settings.directional.atlasSize;
        buffer.GetTemporaryRT(dirshadowAtlasId, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
    }


    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    public void Cleanup()
    {
        //Add a "release a temporary render texture" command.
        buffer.ReleaseTemporaryRT(dirshadowAtlasId);
        ExecuteBuffer();
    }

}
