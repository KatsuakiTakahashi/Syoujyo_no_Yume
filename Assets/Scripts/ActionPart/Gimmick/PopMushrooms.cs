using UnityEngine;

namespace Syoujyo_no_Yume
{
    public class PopMushrooms : MonoBehaviour
    {
        // �v���C���[��ݒ肵�Ă��������B
        [SerializeField]
        GameObject player = null;
        private Rigidbody2D playerRb;

        // �����͂�ݒ肵�Ă��������B
        [SerializeField]
        float repellentForce = 1f;

        private Animator animator;
        private AudioSource audioSource;

        private void Start()
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // �Փ˂������肪�v���C���[�ł��邩�`�F�b�N
            if (collision.gameObject == player)
            {
                animator.SetTrigger("Active");
                audioSource.Play();

                // �Փ˂�������̖@���x�N�g�����擾
                Vector2 contactNormal = -collision.contacts[0].normal;

                playerRb.AddForce(contactNormal * repellentForce, ForceMode2D.Impulse);
            }
        }
    }
}