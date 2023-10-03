using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PieChart : VisualElement
{
    float m_Radius = 100.0f;
    public float m_Leader1 = 16.6666666f;
    public float m_Leader2 = 16.6666666f;
    public float m_Leader3 = 16.6666666f;
    public float m_Leader4 = 16.6666666f;
    public float m_Leader5 = 16.6666666f;
    public float m_Leader6 = 16.6666666f;

    public float radius{
        get => m_Radius;
        set
        {
            m_Radius = value;
        }
    }

    public float diameter => m_Radius * 2.0f;


    public PieChart() {
        generateVisualContent += DrawCanvas;
    }

    void DrawCanvas(MeshGenerationContext ctx) {
        var painter = ctx.painter2D;
        painter.strokeColor = Color.white;
        painter.fillColor = Color.white;
        
        var percentages = new float[] {
            m_Leader1, m_Leader2, m_Leader3, m_Leader4, m_Leader5, m_Leader6
        };
        var colors = new Color32[] {
            new Color32(182, 235, 122, 255),
            new Color32(251, 120, 19, 255),
            new Color32(182, 235, 122, 255),
            new Color32(251, 120, 19, 255),
            new Color32(182, 235, 122, 255),
            new Color32(251, 120, 19, 255)
        };
        float angle = 0.0f;
        float anglePct = 0.0f;
        int k = 0;
        foreach (var pct in percentages)
        {
            anglePct += 360.0f * (pct / 100);

            painter.fillColor = colors[k++];
            painter.BeginPath();
            painter.MoveTo(new Vector2(m_Radius, m_Radius));
            painter.Arc(new Vector2(m_Radius, m_Radius), m_Radius, angle, anglePct);
            painter.Fill();

            angle = anglePct;
        }
    }

}
