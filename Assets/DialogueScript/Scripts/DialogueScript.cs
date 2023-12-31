using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DialogueScript
{
    // adventureパートを作成するスクリプトです。
    public class DialogueScript : MonoBehaviour
    {
        // シナリオファイルのファイル名を指定してください。
        private static readonly string scenarioFileName = "Scenario.txt";

        // シナリオファイルからデータを取り出します
        private static readonly ScenarioFile scenarioFile = new(scenarioFileName);

        // シナリオファイルのデータからコマンドリストを作成します
        private readonly CommandList commandList = new(scenarioFile.scenario, scenarioFile.ScenarioFormat);
        private Func<string, Task> commandMethod = null;

        // 現在の命令番号
        private int currentCommandNum = -1;
        // 終了判定
        private bool isEnd = false;

        // シナリオが正しく動作するか確認出来ます。
        [SerializeField]
        private bool isDebugMode = false;

        public enum DialogueState
        {
            Debug,
            Hide,
            Skip,
            Auto,
            Normal,
        }
        public DialogueState CurrentDialogueState { get; private set; } = DialogueState.Normal;

        [Serializable]
        private struct MessageSpeed
        {
            // 文字の表示間隔
            [SerializeField]
            public int messageInterval;
            [SerializeField]
            public int skipSpeed;
            [SerializeField]
            public int autoSpeed;
        }
        // それぞれのメッセージ速度を設定してください。
        [SerializeField]
        private MessageSpeed messageSpeed;

        // 一つのメッセージの終了判定です
        private bool isMessageEnd = false;
        private CancellationTokenSource messageCancellationSource;

        [SerializeField]
        private GameObject dialogueGameObject = null;
        // メッセージの表示先を指定してください
        [SerializeField]
        private GameObject messageTextBox = null;
        private TextMeshProUGUI messageTextArea = null;

        // 発言者の名前の表示先を指定してください
        [SerializeField]
        private GameObject nameTextBox = null;
        private TextMeshProUGUI nameTextArea = null;

        // メッセージが終了したときに表示されるアイコンです。
        [SerializeField]
        private GameObject messageEndIcon = null;

        private Character[] characters = null;

        // 命令を追加したい場合はここで設定を行ってください。
        [SerializeField]
        private List<UserCommand> userCommands = new();

        // 渡された名前の関数のデリゲート型を作成します。
        public static Func<string, Task> CreateCommand(string commandType)
        {
            MethodInfo methodInfo = typeof(DialogueScript).GetMethod(commandType);

            if (methodInfo == null)
            {
                return null;
            }

            return (Func<string, Task>)Delegate.CreateDelegate(typeof(Func<string, Task>), null, methodInfo);
        }

        private void Awake()
        {
            messageTextArea = messageTextBox.GetComponentInChildren<TextMeshProUGUI>();
            nameTextArea = nameTextBox.GetComponentInChildren<TextMeshProUGUI>();

            characters = FindObjectsOfType<Character>();

            if (messageEndIcon != null && messageEndIcon.activeSelf)
            {
                messageEndIcon.SetActive(false);
            }
        }

        private void Start()
        {
            Debug.Log("シナリオ : \n" + scenarioFile.scenario);

            if (isDebugMode)
            {
                isDebugMode = false;
                CurrentDialogueState = DialogueState.Debug;
            }
            else
            {
                NextCommand();
            }
        }

        private void Update()
        {
            UpdateDialogue();
        }

        // 実行中の処理を行います
        private void UpdateDialogue()
        {
            if (isEnd)
            {
                return;
            }

            switch (CurrentDialogueState)
            {
                case DialogueState.Debug:
                    NextCommand();
                    break;

                case DialogueState.Hide:
                    UpdateHideState();
                    break;

                case DialogueState.Skip:
                    UpdateSkipState();
                    break;

                case DialogueState.Auto:
                    UpdateAutoState();
                    break;

                case DialogueState.Normal:
                    UpdateNormalState();
                    break;
            }
        }

        private void UpdateHideState()
        {
            if (Input.anyKeyDown)
            {
                nameTextBox.SetActive(true);
                messageTextBox.SetActive(true);
                CurrentDialogueState = DialogueState.Normal;
            }
        }

        private void UpdateSkipState()
        {
            if (Input.GetButtonUp("Fire1"))
            {
                CurrentDialogueState = DialogueState.Normal;
            }
            else
            {
                if (isMessageEnd)
                {
                    NextCommand();
                }
            }
        }

        private void UpdateAutoState()
        {
            if (Input.GetButtonUp("Fire2"))
            {
                CurrentDialogueState = DialogueState.Normal;
            }
            else
            {
                if (isMessageEnd)
                {
                    NextCommand();
                }
            }
        }

        private void UpdateNormalState()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                nameTextBox.SetActive(false);
                messageTextBox.SetActive(false);
                CurrentDialogueState = DialogueState.Hide;
            }
            else if (Input.GetButtonDown("Fire1"))
            {
                CurrentDialogueState = DialogueState.Skip;
            }
            else if (Input.GetButtonDown("Fire2"))
            {
                CurrentDialogueState = DialogueState.Auto;
            }
            else if (Input.GetButtonDown("Submit"))
            {
                if (isMessageEnd)
                {
                    NextCommand();
                }
            }
        }

        // 次の命令を実行します。
        public void NextCommand()
        {
            if (isEnd)
            {
                throw new Exception("すべての命令が終わっていますが命令の実行が呼び出されました。");
            }

            // 最後の命令だったら終了判定を有効化します。
            if (currentCommandNum == commandList.commands.Count - 1)
            {
                isEnd = true;
                return;
            }

            // 現在の命令番号を増やします。
            currentCommandNum++;

            // 命令の型を設定します。
            commandMethod = commandList.commands[currentCommandNum].type;
            // 命令の引数を設定して実行します。
            commandMethod.DynamicInvoke(this, new string(commandList.commands[currentCommandNum].argument));

            // 最後の命令だったら終了判定を有効化します。
            if (currentCommandNum == commandList.commands.Count - 1)
            {
                isEnd = true;
            }
            return;
        }

        private void OnDisable()
        {
            TaskCancell();
        }

        // ここからシナリオをチェックできます。
        public Task CheckStart(string none)
        {
            if (CurrentDialogueState == DialogueState.Debug)
            {
                CurrentDialogueState = DialogueState.Normal;
            }

            return Task.CompletedTask;
        }

        // Dialogueスクリプトのアクティブ状態を設定します。
        public Task SetActive(string boolText)
        {
            if (boolText == "true" || boolText == "True")
            {
                dialogueGameObject.SetActive(true);
                enabled = true;
                NextCommand();
            }
            else if (boolText == "false" || boolText == "False")
            {
                dialogueGameObject.SetActive(false);
                TaskCancell();
                enabled = false;
            }
            else
            {
                throw new Exception("<SetActive>に続くテキストは true もしくは false を設定してください。");
            }

            return Task.CompletedTask;
        }

        // テキスト送りのタスクをキャンセルします。
        private void TaskCancell()
        {
            if (messageCancellationSource != null)
            {
                messageCancellationSource.Cancel();
                messageCancellationSource = null;
            }

            if (CurrentDialogueState != DialogueState.Debug)
            {
                CurrentDialogueState = DialogueState.Normal;
            }
        }

        //////////////////////////////以下ダイアログのためのコマンド//////////////////////////////

        // テキストボックスに文字を表示します
        public async Task Message(string messageText)
        {
            if (messageTextBox == null)
            {
                throw new ArgumentNullException("メッセージの表示先を設定してください");
            }

            SetMessageEnd(false);
            await MessageOutput(CurrentDialogueState, messageText);
            SetMessageEnd(true);
        }

        // テキストを出力します。Messageメソッドで使用されます。
        private async Task MessageOutput(DialogueState currentDialogueState, string messageText)
        {
            // テキストボックスを空にします
            messageTextArea.text = "";
            int currentMessageSpeed = 0;

            switch (currentDialogueState)
            {
                case DialogueState.Debug:
                    break;
                case DialogueState.Skip:
                    currentMessageSpeed = messageSpeed.skipSpeed / messageText.Length;
                    break;
                case DialogueState.Auto:
                    currentMessageSpeed = messageSpeed.messageInterval;
                    break;
                case DialogueState.Normal:
                    currentMessageSpeed = messageSpeed.messageInterval;
                    break;
            }

            // もし前回のメッセージ表示がまだ終わっていない場合、キャンセルして待機を中断します
            messageCancellationSource?.Cancel();
            messageCancellationSource = new CancellationTokenSource();

            CancellationToken cancellationToken = messageCancellationSource.Token;

            if (currentMessageSpeed != 0)
            {
                for (int messageTextIndex = 0; messageTextIndex < messageText.Length; messageTextIndex++)
                {
                    // TextMeshProを使用した命令の作動
                    if (messageText[messageTextIndex] == '<')
                    {
                        while (messageText[messageTextIndex] != '>')
                        {
                            messageTextArea.text += messageText[messageTextIndex];
                            messageTextIndex++;
                        }
                        messageTextArea.text += messageText[messageTextIndex];
                    }
                    else
                    {
                        await Task.Delay(currentMessageSpeed);
                        // キャンセル要求が発生した場合はタスクを終了します
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        messageTextArea.text += messageText[messageTextIndex];
                    }
                }
            }
            else
            {
                messageTextArea.text = messageText;
            }

            if (currentDialogueState == DialogueState.Auto)
            {
                await Task.Delay(messageSpeed.autoSpeed);
                // キャンセル要求が発生した場合はタスクを終了します
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        // メッセージ表示の完了非完了を設定します。
        private void SetMessageEnd(bool setBool)
        {
            isMessageEnd = setBool;
            if (messageEndIcon != null && CurrentDialogueState != DialogueState.Hide)
            {
                messageEndIcon.SetActive(setBool);
            }
        }

        // 文字の表示間隔を設定します。
        public Task MessageInterval(string messageIntervalText)
        {
            // 渡された引数が数字以外だったら設定しません。
            if (!int.TryParse(messageIntervalText, out int value))
            {
                throw new Exception("メッセージインターバルの設定値に数字ではないものが渡されました。 : " + messageIntervalText);
            }

            messageSpeed.messageInterval = value;

            NextCommand();

            return Task.CompletedTask;
        }

        // スキップ時の速度を設定します。
        public Task SkipSpeed(string skipSpeedText)
        {
            // 渡された引数が数字以外だったら設定しません。
            if (!int.TryParse(skipSpeedText, out int value))
            {
                throw new Exception("スキップ速度の設定値に数字ではないものが渡されました。 : " + skipSpeedText);
            }

            messageSpeed.skipSpeed = value;

            NextCommand();

            return Task.CompletedTask;
        }

        // オート時の速度を設定します。
        public Task AutoSpeed(string autoSpeedText)
        {
            // 渡された引数が数字以外だったら設定しません。
            if (!int.TryParse(autoSpeedText, out int value))
            {
                throw new Exception("オート速度の設定値に数字ではないものが渡されました。 : " + autoSpeedText);
            }

            messageSpeed.autoSpeed = value;

            NextCommand();

            return Task.CompletedTask;
        }

        // 発言者の名前を表示します。
        public Task Name(string nameText)
        {
            if (nameTextBox == null)
            {
                throw new ArgumentNullException("発言者の名前の表示先を設定してください");
            }

            nameTextArea.text = nameText;

            NextCommand();

            return Task.CompletedTask;
        }

        // メッセージテキストボックスの表示非表示を設定します。
        public Task MessageTextBox(string boolText)
        {
            if (boolText == "true" || boolText == "True")
            {
                messageTextBox.SetActive(true);
            }
            else if (boolText == "false" || boolText == "False")
            {
                messageTextBox.SetActive(false);
            }
            else
            {
                throw new Exception("<MessageTextBox>に続くテキストは true もしくは false を設定してください。");
            }

            NextCommand();

            return Task.CompletedTask;
        }

        // メッセージテキストボックスのフォントサイズを変更します。
        public Task MessageFontSize(string fontSizeText)
        {
            // 渡された引数が数字以外だったら設定しません。
            if (!int.TryParse(fontSizeText, out int value))
            {
                throw new Exception("メッセージフォントサイズの設定値に数字ではないものが渡されました。 : " + fontSizeText);
            }

            messageTextArea.fontSize = value;
            NextCommand();
            return Task.CompletedTask;
        }

        // メッセージテキストボックスのフォントカラーを変更します。
        public Task MessageFontColor(string colorText)
        {
            if(!ColorUtility.TryParseHtmlString(colorText, out var color))
            {
                throw new Exception("メッセージフォントカラーの設定は＃000000のように行って下さい。 : " + colorText);
            }

            messageTextArea.color = color;
            NextCommand();
            return Task.CompletedTask;
        }

        // ネームテキストボックスの表示非表示を設定します。
        public Task NameTextBox(string boolText)
        {
            if (boolText == "true" || boolText == "True")
            {
                nameTextBox.SetActive(true);
            }
            else if (boolText == "false" || boolText == "False")
            {
                nameTextBox.SetActive(false);
            }
            else
            {
                throw new Exception("<MessageTextBox>に続くテキストは true もしくは false を設定してください。");
            }

            NextCommand();

            return Task.CompletedTask;
        }

        // ネームテキストボックスのフォントサイズを変更します。
        public Task NameFontSize(string fontSizeText)
        {
            // 渡された引数が数字以外だったら設定しません。
            if (!int.TryParse(fontSizeText, out int value))
            {
                throw new Exception("メッセージフォントサイズの設定値に数字ではないものが渡されました。 : " + fontSizeText);
            }

            nameTextArea.fontSize = value;
            NextCommand();
            return Task.CompletedTask;
        }

        // ネームテキストボックスのフォントカラーを変更します。
        public Task NameFontColor(string colorText)
        {
            if (!ColorUtility.TryParseHtmlString(colorText, out var color))
            {
                throw new Exception("ネームフォントカラーの設定は＃000000のように行って下さい。 : " + colorText);
            }

            nameTextArea.color = color;
            NextCommand();
            return Task.CompletedTask;
        }

        // キャラクターの状態を変化させます。
        public Task CharacterMode(string characterNameAndModeNameText)
        {
            bool isCharacterName = true;
            string characterName = "";
            string modeName = "";

            for (int textIndex = 0; textIndex < characterNameAndModeNameText.Length; textIndex++)
            {
                if ((characterNameAndModeNameText[textIndex] == '_' || characterNameAndModeNameText[textIndex] == '＿') && isCharacterName)
                {
                    textIndex++;
                    isCharacterName = false;
                }

                if (isCharacterName)
                {
                    characterName += characterNameAndModeNameText[textIndex];
                }
                else
                {
                    modeName += characterNameAndModeNameText[textIndex];
                }
            }

            if (characterName == "")
            {
                characterName = nameTextArea.text;
            }

            for (int characterIndex = 0; characterIndex < characters.Length; characterIndex++)
            {
                if (characters[characterIndex].CharacterName == characterName)
                {
                    characters[characterIndex].SetCharacterMode(modeName);

                    NextCommand();

                    return Task.CompletedTask;
                }
            }

            throw new ArgumentNullException("存在しないキャラクターもしくは存在しないモードです。 : " + characterNameAndModeNameText);
        }

        // キャラクターの表示非表示を設定します。
        public Task CharacterSetActive(string characterNameAndBoolText)
        {
            bool isCharacterName = true;
            string characterName = "";
            string boolText = "";

            for (int textIndex = 0; textIndex < characterNameAndBoolText.Length; textIndex++)
            {
                if ((characterNameAndBoolText[textIndex] == '_' || characterNameAndBoolText[textIndex] == '＿') && isCharacterName)
                {
                    textIndex++;
                    isCharacterName = false;
                }

                if (isCharacterName)
                {
                    characterName += characterNameAndBoolText[textIndex];
                }
                else
                {
                    boolText += characterNameAndBoolText[textIndex];
                }
            }

            if (characterName == "")
            {
                characterName = nameTextArea.text;
            }

            for (int characterIndex = 0; characterIndex < characters.Length; characterIndex++)
            {
                if (characters[characterIndex].CharacterName == characterName)
                {
                    if (boolText == "true" || boolText == "True")
                    {
                        characters[characterIndex].gameObject.SetActive(true);
                    }
                    else if (boolText == "false" || boolText == "False")
                    {
                        characters[characterIndex].gameObject.SetActive(false);
                    }
                    else
                    {
                        throw new Exception("<MessageTextBox>に続くテキストは true もしくは false を設定してください。");
                    }

                    NextCommand();

                    return Task.CompletedTask;
                }
            }

            throw new ArgumentNullException("存在しないキャラクターもしくは存在しないモードです。 : " + characterNameAndBoolText);
        }

        // シーンを移動します。
        public Task LoadScene(string sceneNameText)
        {
            SceneManager.LoadScene(sceneNameText);

            return Task.CompletedTask;
        }

        // ユーザーが作成した命令です。
        public Task UserCommand(string commandNameText)
        {
            // 命令の名称から要素番号を設定します。
            int index = userCommands.FindIndex(item => item.CommandName == commandNameText);

            if (index == -1)
            {
                throw new Exception("作成されてない命令か間違った名称が渡されました。 : " + commandNameText);
            }

            userCommands[index].CommandEvent.Invoke();

            NextCommand();

            return Task.CompletedTask;
        }
    }
}