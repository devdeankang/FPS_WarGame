using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OnHide
{
    OnlyText,
    OnlyBar,
    HideTexture,
    None
}

public class DisplayAboveObject : MonoBehaviour
{
    public GUISkin guiSKin;
    public OnHide m_OnHideShow;
    public float m_health = 100;
    public Texture2D Hide_Texture;
    public Texture2D Bar_Texture;
    public Transform positionDisplay;
    public string playerName;
    public Color Name_Color;
    public Color Bar_Color;
    public Color HideTexureColor;
    public float Hide_Distance = 10;
    [HideInInspector]
    public float m_Distance;
    public bool Hide_In_Distance = true;
    public bool Show_Bar = true;
    public bool Show_Text = true;
    public bool OutLine = true;
    public float HideTextureSize = 25;
    public int OutLineSize = 2;
    public Vector2 Bar_Size = Vector2.zero;
    public Vector2 BacgroundBarSize = Vector2.zero;
    GUIStyle hpStyle = new GUIStyle();
    Transform m_transform;
    [HideInInspector]
    public float Max_Health;

    void Awake()
    {

        if (!positionDisplay)
        {
            positionDisplay = transform;
        }
        if (guiSKin != null)
        {
            hpStyle.font = guiSKin.font;
            hpStyle.fontSize = guiSKin.GetStyle("Label").fontSize;
        }

        hpStyle.alignment = TextAnchor.MiddleCenter;
        Max_Health = m_health;
    }

    void OnEnable()
    {
        this.m_transform = this.transform;
    }

    void FixedUpdate()
    {
        if (playerName == string.Empty || playerName == null)
        {
            playerName = gameObject.name;
        }
        hpStyle.normal.textColor = Name_Color;
    }

    void OnGUI()
    {
        GUI.skin = guiSKin;
        GUI.depth = 2;
        float offset = new float();

        if (Camera.main)
        {
            this.m_Distance = Vector3.Distance(this.m_transform.position, Camera.main.transform.position);
            //Calculations to make name label always above player
            Vector3 screenPos = Camera.main.WorldToScreenPoint(positionDisplay.position);
            if ((float)(screenPos.z * 3) < 50)
            {
                offset = screenPos.z * 3;
            }
            else
            {
                offset = 50;
            }

            if (screenPos.z > 0)
            {
                if (m_Distance <= Hide_Distance)
                {
                    if (Show_Text)
                    {
                        if (OutLine)
                        {
                            Rect rec = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 5 - offset, 200, 30);
                            DrawOutline(rec, playerName, guiSKin.GetStyle("label"), Color.black, Name_Color, OutLineSize);
                        }
                        else
                        {
                            GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - 5 - offset, 200, 30), playerName, hpStyle);
                        }
                    }
                    //Bar
                    if (Show_Bar)
                    {
                        GUI.Box(new Rect(screenPos.x - Max_Health / 2, Screen.height - screenPos.y + 25 - offset, Max_Health + BacgroundBarSize.x, 5 + BacgroundBarSize.y), "");
                        //Player hp
                        GUI.color = Bar_Color;
                        GUI.DrawTexture(new Rect(screenPos.x - m_health / 2, Screen.height - screenPos.y + 25 - offset, m_health + Bar_Size.x, 5 + Bar_Size.y), Bar_Texture, ScaleMode.StretchToFill);
                        GUI.color = Color.white;
                    }
                }
                else if (Hide_In_Distance)
                {
                    if (m_OnHideShow == OnHide.HideTexture)
                    {
                        GUI.color = HideTexureColor;
                        GUI.DrawTexture(new Rect(screenPos.x - 13, ((Screen.height - screenPos.y)) - (offset / 2), HideTextureSize, HideTextureSize), Hide_Texture, ScaleMode.StretchToFill);
                        GUI.color = Color.white;
                    }
                    else if (m_OnHideShow == OnHide.OnlyBar)
                    {
                        if (Show_Bar)
                        {
                            GUI.Box(new Rect(screenPos.x - Max_Health / 2, Screen.height - screenPos.y + 25 - offset, Max_Health + BacgroundBarSize.x, 5 + BacgroundBarSize.y), "");
                            //Player hp
                            GUI.color = Bar_Color;
                            GUI.DrawTexture(new Rect(screenPos.x - m_health / 2, Screen.height - screenPos.y + 25 - offset, m_health + Bar_Size.x, 5 + Bar_Size.y), Bar_Texture, ScaleMode.StretchToFill);
                            GUI.color = Color.white;
                        }
                    }
                    else if (m_OnHideShow == OnHide.OnlyText)
                    {
                        if (Show_Text)
                        {
                            if (OutLine)
                            {
                                Rect rec = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 5 - offset, 200, 30);
                                DrawOutline(rec, playerName, guiSKin.GetStyle("label"), Color.black, Name_Color, OutLineSize);
                            }
                            else
                            {
                                GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - 5 - offset, 200, 30), playerName, hpStyle);
                            }
                        }
                    }
                }
                else
                {
                    if (Show_Text)
                    {
                        if (OutLine)
                        {
                            Rect rec = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 5 - offset, 200, 30);
                            DrawOutline(rec, playerName, guiSKin.GetStyle("label"), Color.black, Name_Color, OutLineSize);
                        }
                        else
                        {
                            GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - 5 - offset, 200, 30), playerName, hpStyle);
                        }
                    }
                    if (Show_Bar)
                    {
                        GUI.Box(new Rect(screenPos.x - Max_Health / 2, Screen.height - screenPos.y + 25 - offset, Max_Health + BacgroundBarSize.x, 5 + BacgroundBarSize.y), "");
                        //Player hp
                        GUI.color = Bar_Color;
                        GUI.DrawTexture(new Rect(screenPos.x - m_health / 2, Screen.height - screenPos.y + 25 - offset, m_health + Bar_Size.x, 5 + Bar_Size.y), Bar_Texture, ScaleMode.StretchToFill);
                        GUI.color = Color.white;
                    }
                }

            }
        }
    }

    public void DrawOutline(Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
    {
        float num = size * 0.5f;
        GUIStyle style2 = new GUIStyle(style);
        Color color = GUI.color;
        outColor.a = inColor.a;
        style.normal.textColor = outColor;
        style.alignment = TextAnchor.MiddleCenter;
        GUI.color = outColor;
        rect.x -= num;
        GUI.Label(rect, text, style);
        rect.x += size;
        GUI.Label(rect, text, style);
        rect.x -= num;
        rect.y -= num;
        GUI.Label(rect, text, style);
        rect.y += size;
        GUI.Label(rect, text, style);
        rect.y -= num;
        style.normal.textColor = inColor;
        GUI.color = color;
        GUI.Label(rect, text, style);
        style = style2;
    }
}
