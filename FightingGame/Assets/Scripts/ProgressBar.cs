using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Transform cameraTarget;
    public Transform leftLimit;
    public Transform rightLimit;
    public List<Image> segments;

    public Color p1Color = Color.yellow;
    public Color p2Color = Color.blue;
    public Color neutralColor = Color.white;

    private float totalDistance;

    void Start()
    {
        if (leftLimit != null && rightLimit != null)
        {
            totalDistance = Vector3.Distance(new Vector3(leftLimit.position.x, 0, 0), new Vector3(rightLimit.position.x, 0, 0));
        }
    }

    void Update()
    {
        if (cameraTarget == null || segments.Count == 0) return;

        float currentX = cameraTarget.position.x;
        float relativeX = currentX - leftLimit.position.x;
        float progressPercent = Mathf.Clamp01(relativeX / totalDistance);
        int activeIndex = Mathf.FloorToInt(progressPercent * segments.Count);
        activeIndex = Mathf.Clamp(activeIndex, 0, segments.Count - 1);

        for (int i = 0; i < segments.Count; i++)
        {
            if (i < activeIndex)
            {
                segments[i].color = p1Color;
            }
            else if (i > activeIndex)
            {
                segments[i].color = p2Color;
            }
            else
            {
                segments[i].color = neutralColor;
            }
        }
    }
}
