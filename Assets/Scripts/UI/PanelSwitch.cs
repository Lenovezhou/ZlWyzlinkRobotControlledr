using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSwitch : MonoBehaviour {
    public RectTransform Panel1;
    public RectTransform Panel2;
    public float distance;

    private int currentPanel = 1;

    public void SwitchBetweenTwoPanels()
    {
        StartCoroutine(SwitchPanels());
    }

    private IEnumerator SwitchPanels()
    {
        
        if (currentPanel == 0)
        {
            currentPanel = 1;
            StartCoroutine(MovePanel(Panel1, -distance));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(MovePanel(Panel2, -distance));
        }
        else
        {
            currentPanel = 0;
            StartCoroutine(MovePanel(Panel2, distance));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(MovePanel(Panel1, distance));
        }
    }

    private IEnumerator MovePanel(RectTransform panel, float distance)
    {
        var targetPosition = panel.anchoredPosition + Vector2.right * distance;
        for (float i = 0; i < 1; i += 0.02f)
        {
            panel.localPosition = Vector2.Lerp(panel.anchoredPosition, targetPosition, i);
            yield return -1;
        }
    }
}
