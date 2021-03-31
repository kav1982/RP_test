using UnityEngine;

public class PerObjectMaterialProperties : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField]
    Color baseColor = Color.white;
    static MaterialPropertyBlock block;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (block == null)
            block = new MaterialPropertyBlock();

        block.SetColor(baseColorId, baseColor);
        //允许您设置或清除per-renderer或per-material参数覆盖。
        //Lets you set or clear per-renderer or per-material parameter overrides.
        GetComponent<Renderer>().SetPropertyBlock(block);
    }
    
}
