using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler , IPointerExitHandler
{
    [NonSerialized]
    public bool down;

    Image image;
    bool enter;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (down) image.color = new Color(Mathf.Lerp(image.color.r, 0.75f, 5 * Time.deltaTime), Mathf.Lerp(image.color.g, 0.75f, 5 * Time.deltaTime), Mathf.Lerp(image.color.b, 0.75f, 5 * Time.deltaTime), 1);
        else
        {
            if (enter) image.color = new Color(Mathf.Lerp(image.color.r, 1, 5 * Time.deltaTime), Mathf.Lerp(image.color.g, 1, 5 * Time.deltaTime), Mathf.Lerp(image.color.b, 1, 5 * Time.deltaTime), 1);
            else image.color = new Color(Mathf.Lerp(image.color.r, 0.85f, 5 * Time.deltaTime), Mathf.Lerp(image.color.g, 0.85f, 5 * Time.deltaTime), Mathf.Lerp(image.color.b, 0.85f, 5 * Time.deltaTime), 1);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        enter = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        enter = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        down = false;
    }
}
