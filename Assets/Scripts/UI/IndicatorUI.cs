using UnityEngine;
using UnityEngine.UI;

public class IndicatorUI : IndicatorUIBase
{
    public Image speedIndicator;
    public RectTransform angleIndicator;
    public RectTransform angleIndicator1;
    public RectTransform backGround;
    public RadialCurve powerIndicator1;
    public Image powerIndicator;
    public float gearRatio = 50;
    public float speedRatio = 0.01f;
    private float previousUpdate;
    private float previousAngle;
    private Color originalPowerIndicatorColor;

    // Use this for initialization
    void Start () {
        this.previousUpdate = Time.time;
        this.originalPowerIndicatorColor = powerIndicator.color;
	}

    public void Update()
    {
        backGround.transform.Rotate(Vector3.forward, -10 * Time.deltaTime);
        powerIndicator1.transform.Rotate(Vector3.forward, 10 * Time.deltaTime);
    }

    public override void UpdateValues(float angle, float energyConsumption)
    {
        this.angleIndicator.transform.localRotation = Quaternion.Euler(0, 0, angle);
        this.angleIndicator1.transform.localRotation = Quaternion.Euler(0, 0, -angle * gearRatio);

        float deltaTime = Time.time - this.previousUpdate;
        var speed = (angle - this.previousAngle) / deltaTime;
        this.speedIndicator.fillAmount = Mathf.Abs(speed) * speedRatio;
        this.previousAngle = angle;
        this.previousUpdate = Time.time;

        this.powerIndicator1.UpdateData(energyConsumption);
        this.powerIndicator.fillAmount = energyConsumption / 500;
        this.powerIndicator.color = Color.Lerp(originalPowerIndicatorColor, Color.red, energyConsumption / 500 * 4 - 2);
    }
}
