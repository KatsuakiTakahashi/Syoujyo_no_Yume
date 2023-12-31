using UnityEngine;

namespace Syoujyo_no_Yume
{
    // 対象を追尾して動くカメラです。
    public class ChaseCamera : MonoBehaviour
    {
        // 追尾する対象を指定します。
        [SerializeField]
        private Transform target = null;
        // 追尾対象からのオフセット値を指定します。
        [SerializeField]
        private Vector2 offset = new(0f, 1.5f);

        [SerializeField]
        private float upCameraPosiY = 10f;

        private void Start()
        {
            TargetChase();
        }

        private void Update()
        {
            TargetChase();
        }

        // ターゲットの位置に移動します。
        private void TargetChase()
        {
            var position = transform.position;
            position.x = target.position.x + offset.x;
            if (target.position.y > upCameraPosiY)
            {
                position.y = upCameraPosiY + offset.y;
            }
            else
            {
                position.y = offset.y;
            }
            transform.position = position;
        }
    }
}