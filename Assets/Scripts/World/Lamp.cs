using UnityEngine;

public class Lamp : MonoBehaviour
{

    [SerializeField] private Light _lightSource;
    [SerializeField] private Material _disabledMaterial;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private bool _invert;
    [SerializeField] private bool _ignore;

    public bool Ignore => _ignore;
    public bool IsInverted => _invert;

    private Material _defaultMaterial;

    private void Awake()
    {
        _defaultMaterial = _renderer.sharedMaterial;
    }

    public void TurnOn()
    {
        _lightSource.enabled = true;
        _renderer.sharedMaterial = _defaultMaterial;
    }

    public void TurnOff()
    {
        _lightSource.enabled = false;
        _renderer.sharedMaterial = _disabledMaterial;
    }

}
