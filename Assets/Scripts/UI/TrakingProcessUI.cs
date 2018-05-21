using UnityEngine;
using UnityEngine.UI;

public class TrakingProcessUI : MonoBehaviour {

    [SerializeField]
    private Image selfimage;
    [SerializeField]
    private TextMesh selftextmesh;
    [SerializeField]
    private TextMesh processTitle;

    private float titletimer = 0;

    public AnimationCurve curve;

    //private float lerpsmoothing = 0.025f;

    private bool issetprocess = false;
    private float middle = 0;
    // Use this for initialization
    void Start ()
    {
        if (null == selfimage)
        {
            selfimage = GetComponent<Image>();
        }
        if (null == selftextmesh)
        {
            selftextmesh = GetComponentInChildren<TextMesh>();
        }
        Process = 0;
    }

    private float process;

    public float Process
    {
        get
        {
            return process;
        }

        set
        {
            process = value;
            middle = 0;
            issetprocess = true;
        }
    }


    private void Update()
    {
        if (issetprocess)
        {
            if (Mathf.Abs(Process - selfimage.fillAmount) < 0.01f)
            {
                selfimage.fillAmount = Process;
                issetprocess = false;
            }
            else
            {
                middle += (Time.deltaTime / 4);
                selfimage.fillAmount = Mathf.Lerp(selfimage.fillAmount, Process, middle);
            }

            if (selftextmesh != null)
            {
                if (selfimage.fillAmount < 0.01f)
                {
                    selftextmesh.text = "0.0";
                }
                else {
                    selftextmesh.text = ((int)(selfimage.fillAmount * 100)).ToString("f1") + "%";
                }

                float scaler = curve.Evaluate(middle * 10) * 50 + 100;
                selftextmesh.fontSize = (int)scaler;
            }
        }


        if (processTitle)
        {
            titletimer += Time.deltaTime;
            if (titletimer > 1)
            {
                titletimer = 0;
                Sound.Instance.PlayerOneShout("buttonUi02", transform.position);
            }

            int timerlimit = (int)Time.time % 3;
            string add = "";
            switch (timerlimit)
            {
                case 0:
                    add = ".";
                    break;

                case 1:
                    add = "..";
                    break;

                case 2:
                    add = "...";
                    break;

                default:
                    break;
            }
            processTitle.text = "机械手检测中" + add;
        }

    }

}
