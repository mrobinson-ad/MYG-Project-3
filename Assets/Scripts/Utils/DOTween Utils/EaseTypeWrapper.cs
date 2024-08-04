using UnityEngine;
using DG.Tweening;

namespace FlowerProject
{
    [System.Serializable]
    //Wrapper for DOTWeen Ease that can be changed in editor thanks to Editor\EaseTypeDrawer
    public class EaseTypeWrapper
    {
        public Ease easeType;
    }
}