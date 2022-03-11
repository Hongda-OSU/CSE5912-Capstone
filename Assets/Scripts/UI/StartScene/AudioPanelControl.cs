using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CSE5912.PolyGamers
{
    public class AudioPanelControl : UI
    {
        private class VolumeControl
        {
            public Label label;
            public VisualElement volume;
            public Label magnitude;
            public List<VisualElement> thresholdList = new List<VisualElement>();

            public bool isMouseHovering = false;

            public VolumeControl(VisualElement element)
            {
                label = element.Q<Label>("Label");
                volume = element.Q<VisualElement>("Volume");
                magnitude = element.Q<Label>("Magnitude");

                var thresholds = element.Q<VisualElement>("Thresholds");
                for (int i = 0; i < thresholds.childCount; i++)
                {
                    thresholdList.Add(thresholds.Q<VisualElement>(i.ToString()));
                }

            }

        }

        private VisualElement audioPanel;

        private VolumeControl master;
        private VolumeControl music;
        private VolumeControl effect;
        private void Awake()
        {
            Initialize();

            audioPanel = root.Q<VisualElement>("AudioPanel");

            master = new VolumeControl(audioPanel.Q<VisualElement>("Master"));
            music = new VolumeControl(audioPanel.Q<VisualElement>("Music"));
            effect = new VolumeControl(audioPanel.Q<VisualElement>("Effect"));
        }

        private void Start()
        {
            AssignCallback(master);
            AssignCallback(music);
            AssignCallback(effect);
        }
        private void AssignCallback(VolumeControl volumeControl)
        {
            var thresholdList = volumeControl.thresholdList;
            for (int i = 0; i < thresholdList.Count; i++)
            {
                int magnitude = i;
                thresholdList[i].RegisterCallback<MouseOverEvent>(evt => StartCoroutine(SetVolume(volumeControl, magnitude)));
                thresholdList[i].RegisterCallback<MouseOutEvent>(evt => volumeControl.isMouseHovering = false);
            }
        }
        private IEnumerator SetVolume(VolumeControl volumeControl, int magnitude)
        {
            if (volumeControl.isMouseHovering)
                yield break;

            volumeControl.isMouseHovering = true;

            while (volumeControl.isMouseHovering)
            {
                var volume = volumeControl.volume;
                if (Input.GetMouseButton(0))
                {
                    volume.style.width = magnitude / 10f * volume.parent.resolvedStyle.width;
                }

                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }
}
