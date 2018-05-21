using UnityEngine;
using UnityEngine.UI;

public abstract class IndicatorUIBase : MonoBehaviour
{
    public abstract void UpdateValues(float angle, float energyConsumption);
}
