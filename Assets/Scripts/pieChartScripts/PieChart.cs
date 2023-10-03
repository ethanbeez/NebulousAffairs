using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PieChart : VisualElement
{
    float m_Radius = 100.0f;
    public float leader1 = 0f;
    public float leader2 = 0f;
    public float leader3 = 0f;
    public float leader4 = 0f;
    public float leader5 = 0f;
    public float leader6 = 0f;

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

    //has issues with having only 1 leader inside
    void DrawCanvas(MeshGenerationContext ctx) {
        var painter = ctx.painter2D;
        painter.strokeColor = Color.white;
        painter.fillColor = Color.white;
        
        var percentages = new float[] {
            leader1, leader2, leader3, leader4, leader5, leader6
        };
        var colors = new Color32[] {
            new Color32(93, 97, 196, 255),
            new Color32(115, 202, 194, 255),
            new Color32(247, 252, 246, 255),
            new Color32(236, 117, 113, 255),
            new Color32(248, 229, 126, 255),
            new Color32(0, 0, 0, 255)
        };
        float angle = 0.0f;
        float anglePct = 0.0f;
        int k = 0;
        foreach (var pct in percentages)
        {
            anglePct += pct * 360;

            painter.fillColor = colors[k++];
            painter.BeginPath();
            painter.MoveTo(new Vector2(m_Radius, m_Radius));
            painter.Arc(new Vector2(m_Radius, m_Radius), m_Radius, angle, anglePct);
            painter.Fill();

            angle = anglePct;
        }
    }

}
