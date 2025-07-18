using UnityEngine;
using System.Diagnostics;

namespace Weapon
{
    public class WeaponSMG : WeaponBase
    {
        private bool isPlayLoopingSFX = false;

        private void LateUpdate()
        {
            if (!isFiring && isPlayLoopingSFX)
            {
                isPlayLoopingSFX = false;
                SoundManager.Instance.StopLoopingSFX();
            }
        }
        protected override void PlayVfx()
        {
            if (!isPlayLoopingSFX)
            {
                //UnityEngine.Debug.Log("PlayVfx: " + shootVfx);
                isPlayLoopingSFX = true;
                SoundManager.Instance.PlayLoopingSFX(shootVfx, 0.3f);
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
