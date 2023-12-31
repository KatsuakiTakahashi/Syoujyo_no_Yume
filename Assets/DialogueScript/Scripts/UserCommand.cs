using UnityEngine;
using UnityEngine.Events;

namespace DialogueScript
{
    [System.Serializable]
    public class UserCommand
    {
        // 命令の名前を設定してださい。
        [SerializeField]
        private string commandName;
        public string CommandName { get => commandName; set => commandName = value; }

        // 命令の処理を設定してください。
        [SerializeField]
        private UnityEvent commandEvent;
        public UnityEvent CommandEvent { get => commandEvent; set => commandEvent = value; }
    }
}