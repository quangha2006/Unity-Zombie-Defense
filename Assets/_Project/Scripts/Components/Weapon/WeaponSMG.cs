using UnityEngine;
using System.Diagnostics;

namespace Weapon
{
    public class WeaponSMG : WeaponBase
    {
        private bool isPlayLoopingSFX = false;


        protected override void PlayVfx()
        {
            if (!isPlayLoopingSFX)
            {
                UnityEngine.Debug.Log("PlayVfx: " + shootVfx);
                isPlayLoopingSFX = true;
                SoundManager.Instance.PlayLoopingSFX(shootVfx, 0.3f);
            }
        }
        public override void StopFire()
        {
            if (isPlayLoopingSFX)
            {
                isPlayLoopingSFX = false;
                SoundManager.Instance.StopLoopingSFX();
            }
        }
        private void OnDisable()
        {
            if (isPlayLoopingSFX)
            {
                SoundManager.Instance.StopLoopingSFX();
            }
        }
    }
}
