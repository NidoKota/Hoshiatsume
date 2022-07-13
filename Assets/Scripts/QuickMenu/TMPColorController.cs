using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;
using TMPro;

/// <summary>
/// 5行の色を制御する
/// </summary>
public class TMPLineColorController
{
    MatchCollection lineMatch;
    TMP_Text tmp;
    Color[,] lineVertexColors = new Color[6, 2];

    public TMPLineColorController(TMP_Text tmp)
    {
        this.tmp = tmp;
        UpdateText(lineMatch);
    }

    /// <summary>
    /// 行ごとに色を指定する(indexが2なら一番上の行、-2は一番下、-3はその他の行)
    /// </summary>
    public void SetLineVertexColor(int index, Color up, Color down, float lerp)
    {
        lineVertexColors[index + 3, 0] = Color.Lerp(lineVertexColors[index + 3, 0], down, lerp);
        lineVertexColors[index + 3, 1] = Color.Lerp(lineVertexColors[index + 3, 1], up, lerp);
    }

    /// <summary>
    /// 行の色を取得する(indexが2なら一番上の行、-2は一番下、-3はその他の行)
    /// </summary>
    public (Color up, Color down) GetLineVertexColor(int index)
    {
        return (lineVertexColors[index + 2, 0], lineVertexColors[index + 2, 1]);
    }

    /// <summary>
    /// TMPの文字の色を適応する
    /// </summary>
    public void ApplyTMPColor(int selectLine)
    {
        for (int line = 0; line < lineMatch.Count; line++)
        {
            //変更する文字列が始まる位置と終わる位置を取得
            int start = lineMatch[line].Groups["v"].Index;
            int end = start + lineMatch[line].Groups["v"].Value.Count() - 1;

            if (Mathf.Abs(line - selectLine) >= 3) SetTextColor(start, end, lineVertexColors[0, 1], lineVertexColors[0, 0], lineVertexColors[0, 0], lineVertexColors[0, 1]);
            else SetTextColor(start, end, lineVertexColors[line - selectLine + 3, 1], lineVertexColors[line - selectLine + 3, 0], lineVertexColors[line - selectLine + 3, 0], lineVertexColors[line - selectLine + 3, 1]);
        }
    }

    public void UpdateText(MatchCollection lineMatch)
    {
        this.lineMatch = lineMatch;
        //menuTextのtextInfoを強制的に更新させる
        tmp.ForceMeshUpdate();
    }
    
    //指定した文字の色を変更する
    void SetTextColor(int start, int end, Color leftDown, Color leftUp, Color rightUp, Color rightDown)
    {
        //1文字ごとに処理を行う
        for (int index = start; index <= end; index++)
        {
            Color32[] vertexColors = tmp.textInfo.meshInfo[tmp.textInfo.characterInfo[index].materialReferenceIndex].colors32;

            int vertexIndex = tmp.textInfo.characterInfo[index].vertexIndex;

            //一応表示されているか確認する
            if (tmp.textInfo.characterInfo[index].isVisible)
            {
                vertexColors[vertexIndex + 0] = new Color(leftDown.r, leftDown.g, leftDown.b, leftDown.a);
                vertexColors[vertexIndex + 1] = new Color(leftUp.r, leftUp.g, leftUp.b, leftUp.a);
                vertexColors[vertexIndex + 2] = new Color(rightUp.r, rightUp.g, rightUp.b, rightUp.a);
                vertexColors[vertexIndex + 3] = new Color(rightDown.r, rightDown.g, rightDown.b, rightDown.a);
            }
            //文字の色を更新させる
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
