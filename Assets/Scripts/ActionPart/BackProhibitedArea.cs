using UnityEngine;

namespace Syoujyo_no_Yume
{
    // プレイヤーの後ろを一定間隔で付いてくる後方立ち入り禁止エリアです。
    public class BackProhibitedArea : MonoBehaviour
    {
        // プレイヤーを設定してください。
        [SerializeField]
        private GameObject player = null;
        Player playerCS;

        // エリアに侵入した際の移動先を設定してください。
        [SerializeField]
        private Vector2 offset = new(-10f, 0f);

        private Animator animator;

        private void Start()
        {
            playerCS = player.GetComponent<Player>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (player.transform.position.x + offset.x > transform.position.x)
            {
                TargetChase();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject == player)
            {
                playerCS.Inactive();
                animator.SetTrigger("Enter");
            }
        }

        // 一定間隔以上離れるとプレイヤーについていきます。
        private void TargetChase()
        {
            var position = transform.position;
            position.x = player.transform.position.x + offset.x;
            transform.position = position;
        }

        public void PlayerBackMove()
        {
            var position = player.transform.position;
            position.x = player.transform.position.x - offset.x / 2;
            player.transform.position = position;

            var localScale = transform.localScale;
            localScale.x = player.transform.localScale.x * -1;
            player.transform.localScale = localScale;
        }

        public void PlayerActive()
        {
            playerCS.Active();
        }
    }
}