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

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimationPlayableController
{
    public AnimationMixerPlayable AnimationMixerPlayable { get; private set; } //Animationの再生に使用するAnimationMixerPlayable
    public AnimationPlayableOutput AnimationOutput { get; private set; }       //Animationの再生に使用するAnimationPlayableOutput
    public PlayableGraph AnimationGraph { get; private set; }                  //Animationの再生に使用するPlayableGraph

    public AnimationPlayableController(Animator animator, params AnimationClip[] clips)
    {
        new AnimationPlayableController(animator, null, clips);
    }

    public AnimationPlayableController(Animator animator, string name, params AnimationClip[] clips)
    {
        //Animationの再生に使用するPlayebleの設定
        if (string.IsNullOrEmpty(name)) AnimationGraph = PlayableGraph.Create();
        else AnimationGraph = PlayableGraph.Create(name + "Graph");
        AnimationGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
        AnimationOutput = AnimationPlayableOutput.Create(AnimationGraph, name + "Output", animator);
        AnimationMixerPlayable = AnimationMixerPlayable.Create(AnimationGraph, 3, false);
        for (int i = 0; i < clips.Length; i++) AnimationMixerPlayable.ConnectInput(i, AnimationClipPlayable.Create(AnimationGraph, clips[i]), 0);
        AnimationOutput.SetSourcePlayable(AnimationMixerPlayable);
        AnimationGraph.Play();
    }

    /// <summary>
    /// アニメーションをはじめから再生する
    /// </summary>
    public void PlayAnimation(int index)
    {
        if (AnimationMixerPlayable.GetInputCount() > index)
        {
            AnimationMixerPlayable.SetInputWeight(index, 1);
            AnimationMixerPlayable.GetInput(index).SetTime(0);
        }
        else AnimationMixerPlayable.SetInputWeight(index, 0);
    }

    /// <summary>
    /// アニメーションをはじめから再生する
    /// </summary>
    public void PlayAnimation(AnimationClip animationClip)
    {
        for (int i = 0; i < AnimationMixerPlayable.GetInputCount(); i++)
        {
            //animationMixerPlayableの入力から同じanimationClipを持ったAnimationClipPlayableを探す
            if (((AnimationClipPlayable)AnimationMixerPlayable.GetInput(i)).GetAnimationClip() == animationClip)
            {
                AnimationMixerPlayable.SetInputWeight(i, 1);
                AnimationMixerPlayable.GetInput(i).SetTime(0);
            }
            else AnimationMixerPlayable.SetInputWeight(i, 0);
        }
    }

    /// <summary>
    /// アニメーションの再生が終了したか
    /// </summary>
    public bool IsFinished(int index)
    {
        return AnimationMixerPlayable.GetInput(index).GetTime() >= ((AnimationClipPlayable)AnimationMixerPlayable.GetInput(index)).GetAnimationClip().length;
    }

    /// <summary>
    /// アニメーションの再生が終了したか
    /// </summary>
    public bool IsFinished(AnimationClip animationClip)
    {
        for (int i = 0; i < AnimationMixerPlayable.GetInputCount(); i++)
        {
            if (((AnimationClipPlayable)AnimationMixerPlayable.GetInput(i)).GetAnimationClip() == animationClip)
                return AnimationMixerPlayable.GetInput(i).GetTime() >= animationClip.length;
        }
        return false;
    }

    /// <summary>
    /// PlayableGraphを開放する
    /// </summary>
    public void Destroy()
    {
        if (AnimationGraph.IsValid()) AnimationGraph.Destroy();
    }
}
