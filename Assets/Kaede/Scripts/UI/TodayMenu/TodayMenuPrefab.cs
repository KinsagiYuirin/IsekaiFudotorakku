using System;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Kaede.Scripts.UI.TodayMenu
{
    public class TodayMenuPrefab : MonoBehaviour
    {
        [Title("Reference")] 
        [SerializeField] private TMP_Text foodType;
        [SerializeField] private Image foodImage;
        [SerializeField] private TMP_Text foodName;

        [Title("Animation Setting")] 
        [SerializeField] private Animator coverAnimator;
        [SerializeField] private AnimationClip coverAnimation;

        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationClipPlayable  _clipPlayable;

        private void Start()
        {
            coverAnimator.speed = 0f;
            PrepareAnimation();
        }

        private void PrepareAnimation()
        {
            _graph = PlayableGraph.Create("TodayMenu");
            _output = AnimationPlayableOutput.Create(_graph, "TodayMenu", coverAnimator);
            _clipPlayable = AnimationClipPlayable.Create(_graph, coverAnimation);
            _output.SetSourcePlayable(_clipPlayable);
        }
        
        public void PlayCoverAnimation()
        {
            _graph.Play();
        }

        public void SetFoodDeta(MenuData data)
        {
            foodType.text = data.FoodTypeString; 
            foodImage.sprite = data.menuSprite;
            foodName.text = data.menuName;
        }
    }
}
