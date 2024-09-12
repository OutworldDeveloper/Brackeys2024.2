using UnityEngine;

public class Lamp : MonoBehaviour
{

    [SerializeField] private Light _lightSource;

    public void TurnOn()
    {
        _lightSource.enabled = true;
    }

    public void TurnOff()
    {
        _lightSource.enabled = false;
    }

}
