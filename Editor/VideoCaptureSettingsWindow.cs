using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo.Playables;

public class VideoCaptureSettingsWindow : EditorWindow
{
    PlayableDirector m_director = null;
    bool m_frameAccurate = false;

    List<MediaPlayerControlAsset> m_mediaClipAssets = new List<MediaPlayerControlAsset>();

    // Add menu named "My Window" to the Window menu
    [MenuItem("Capture/VideoCapture")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        VideoCaptureSettingsWindow window = (VideoCaptureSettingsWindow)EditorWindow.GetWindow(typeof(VideoCaptureSettingsWindow));
        window.Show();
    }

    static void SearchDirectorForMediaClips(ref PlayableDirector director, ref List<MediaPlayerControlAsset> clips)
    {
        var asset = director.playableAsset as TimelineAsset;
        if (!asset) return;

        foreach (var track in asset.GetOutputTracks())
        {
            foreach (var clip in track.GetClips())
            {
                MediaPlayerControlAsset control = clip.asset as MediaPlayerControlAsset;
                if (control)
                {
                    clips.Add(control);
                    continue;
                }

                ControlPlayableAsset cpa = clip.asset as ControlPlayableAsset;
                if(cpa)
                {
                    GameObject source = cpa.sourceGameObject.Resolve(director);
                    PlayableDirector newDir = source.GetComponent<PlayableDirector>();
                    if(newDir)
                    {
                        SearchDirectorForMediaClips(ref newDir, ref clips);
                    }
                }

            }
        }
    }

    void OnGUI()
    {
        var prev = m_director;
        m_director = EditorGUILayout.ObjectField(m_director, typeof(PlayableDirector), true) as PlayableDirector;
        if (prev != m_director && m_director != null)
        {
            m_mediaClipAssets.Clear();
            SearchDirectorForMediaClips(ref m_director, ref m_mediaClipAssets);
        }

        if (GUILayout.Button("Toggle Frame Accurate Playback"))
        {
            foreach(var control in m_mediaClipAssets)
            {
                control.frameAccurateSeek = m_frameAccurate;
                EditorUtility.SetDirty(control);
            }
            m_frameAccurate = !m_frameAccurate;
        }

        foreach(var control in m_mediaClipAssets)
        {
            EditorGUILayout.LabelField(control.name + " : " + (control.frameAccurateSeek ? "true" : "false"));
        }
    }
}
