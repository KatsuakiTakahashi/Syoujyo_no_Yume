using UnityEngine;

namespace Syoujyo_no_Yume
{
    public class ChaseCamera : MonoBehaviour
    {
        // �ǔ�����Ώۂ��w�肵�܂��B
        [SerializeField]
        private Transform target = null;
        // �ǔ��Ώۂ���̃I�t�Z�b�g�l���w�肵�܂��B
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

        // �^�[�Q�b�g�̈ʒu�Ɉړ����܂��B
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