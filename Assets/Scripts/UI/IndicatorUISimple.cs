using UnityEngine;
using UnityEngine.UI;

public class IndicatorUISimple : IndicatorUIBase
{
    public Image speedIndicatorPositive;
    public Image speedIndicatorNegative;
    public Image accelerationIndicatorPositive;
    public Image accelerationIndicatorNegative;
    public Image powerIndicator;
    public RectTransform angleIndicator;
    public RectTransform innerAngleIndicator;
    public float gearRatio = 10;
    public float speedRatio = 0.01f;
    public float accelerationRatio = 0.01f;
    public float powerRatio = 0.002f;
    private float previousUpdate;
    private float previousAngle;
    private float previousAngularVelocity;
    private Color originalPowerIndicatorColor;
    private Color originalAccelerationIndicatorColor;


    // Use this for initialization
    void Start () {
        this.previousUpdate = Time.time;
        this.originalPowerIndicatorColor = powerIndicator.color;
        this.originalAccelerationIndicatorColor = accelerationIndicatorPositive.color;
	}

    public override void UpdateValues(float angle, float energyConsumption)
    {
        this.angleIndicator.transform.localRotation = Quaternion.Euler(0, 0, angle);
        this.innerAngleIndicator.transform.localRotation = Quaternion.Euler(0, 0, -angle * gearRatio);

        float deltaTime = Time.time - this.previousUpdate;
        this.previousUpdate = Time.time;

        var ancularVelocity = (angle - this.previousAngle) / deltaTime;
        this.speedIndicatorPositive.fillAmount = ancularVelocity * speedRatio;
        this.speedIndicatorNegative.fillAmount = -ancularVelocity * speedRatio;
        this.previousAngle = angle;

        var acceleration = (ancularVelocity - this.previousAngularVelocity) / deltaTime;
        this.accelerationIndicatorPositive.fillAmount = acceleration * accelerationRatio;
        this.accelerationIndicatorNegative.fillAmount = -acceleration * accelerationRatio;
        this.accelerationIndicatorPositive.color = Color.Lerp(originalAccelerationIndicatorColor, Color.red, acceleration * accelerationRatio * 4 - 2);
        this.accelerationIndicatorNegative.color = Color.Lerp(originalAccelerationIndicatorColor, Color.red, -acceleration * accelerationRatio * 4 - 2);
        this.previousAngularVelocity = ancularVelocity;

        this.powerIndicator.fillAmount = energyConsumption * powerRatio;
        this.powerIndicator.color = Color.Lerp(originalPowerIndicatorColor, Color.red, energyConsumption * powerRatio * 4 - 2);
    }
}
