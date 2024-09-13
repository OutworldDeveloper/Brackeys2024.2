using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ItemModel : MonoBehaviour
{

    public static string GLOW_SHADER_PARAM = "_ShouldGlow";

    public ItemModel EnableCollision(bool enable)
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = enable;
        return this;
    }

    public ItemModel EnableGlow(bool enable)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.material.SetFloat(GLOW_SHADER_PARAM, enable ? 1 : 0);
        return this;
    }

}
