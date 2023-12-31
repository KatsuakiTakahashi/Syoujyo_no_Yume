using UnityEngine;

namespace Syoujyo_no_Yume
{
    // 対象が正解の状態かどうか判別し、正解なら報酬を出します。
    public class ClockGimmickController : MonoBehaviour
    {
        // 出現させる報酬を設定してください。
        [SerializeField]
        private GameObject clearReward = null;

        // 判定対象１を設定してください。
        [SerializeField]
        private Transform clock1Hand = null;
        // 対象１の正解の状態を設定してください。
        [SerializeField]
        private float clock1CorrectRotation = 240f;

        // 判定対象２を設定してください。
        [SerializeField]
        private Transform clock2Hand = null;
        // 対象２の正解の状態を設定してください。
        [SerializeField]
        private float clock2CorrectRotation = 0f;

        private bool isClear = false;

        private void Start()
        {
            if (!isClear)
            {
                clearReward.SetActive(false);
            }
        }

        // 正解の状態か判定し、正解の場合報酬を出します。
        public void ClearCheck()
        {
            if (clock1CorrectRotation + 20 > clock1Hand.eulerAngles.z && clock1Hand.eulerAngles.z > clock1CorrectRotation - 20)
            {
                if (clock2CorrectRotation + 20 > clock2Hand.eulerAngles.z && clock2Hand.eulerAngles.z > clock2CorrectRotation - 20)
                {
                    clearReward.SetActive(true);
                    isClear = true;
                }
            }
        }
    }
}