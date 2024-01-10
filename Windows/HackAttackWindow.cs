using System.Collections.Generic;
using System.Linq;
using TerminalDesktopMod.Extentions;
using TerminalDesktopMod.Sync;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TerminalDesktopMod
{
    public class HackAttackWindow : DesktopWindowBase
    {
        public TextMeshProUGUI CodeText;
        public Image TimeTimage;
        private string Answer { get; set; }
        private List<string> TextCodes { get; set; } = new List<string>();
        private List<string> SaveTextCodes { get; set; } = new List<string>();
        private List<string> TextAnswers { get; set; } = new List<string>();
        private List<string> SaveTextAnswers { get; set; } = new List<string>();
        private float AttackTime { get; set; }
        private float CurrentAttackTime { get; set; } = -1;
        private int DifficultyLevel { get; set; } = -1;

        private void FixedUpdate()
        {
            if (DifficultyLevel == -1)
                return;
            if (CurrentAttackTime <= -1)
                return;
            CurrentAttackTime -= Time.fixedDeltaTime;
            var scaleX = Mathf.Clamp(CurrentAttackTime / AttackTime,0 ,1);
            TimeTimage.transform.localScale = new Vector3(scaleX, 1, 1);
            if (CurrentAttackTime < 0 && TerminalDesktopManager.Instance.IsServer)
            {
                CurrentAttackTime = -1;
                ReferencesStorage.Terminal.ChangeCredits(-10 * DifficultyLevel);
                TerminalDesktopManager.Instance.CloseWindow(this);
            }
        }

        public virtual void Init(int difficultyLevel)
        {
            DifficultyLevel = difficultyLevel;
            GenerateCode();
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomInt = true,
                CustomInt = difficultyLevel,
                SyncCustomString = true,
                CustomString = $"{string.Join(',',TextCodes)};{string.Join(',',TextAnswers)};{AttackTime.ToString()}"
            });
        }

        protected virtual void GenerateCode()
        {
            var codeLenght = Random.Range(3 + DifficultyLevel, 8 + DifficultyLevel);
            AttackTime = codeLenght * Random.Range(1f, 1.8f) + 1f;
            if (DifficultyLevel >= 4)
                AttackTime = codeLenght * Random.Range(3f, 4f) + 2f;
            
            for (int i = 0; i < codeLenght; i++)
            {
                var answer = Random.Range(1, 9);

                if (DifficultyLevel >= 2)
                    answer += Random.Range(0, 20);
                
                if (DifficultyLevel >= 3)
                    answer += Random.Range(10, 65);
                if (answer % 10 == 0)
                    answer++;
                var stringAnswer = answer.ToString();
                if (DifficultyLevel == 4 && Random.Range(0, 2) == 0)
                {
                    var splitAnswer = Random.Range(5, answer);
                    stringAnswer = $"({answer - splitAnswer} + {splitAnswer})";
                }
                if (DifficultyLevel >= 5 && Random.Range(0, 1) == 0)
                {
                    var splitAnswer = Random.Range(5, answer);
                    if (Random.Range(0, 1) == 0)
                        stringAnswer = $"({answer - splitAnswer} + {splitAnswer})";
                    else 
                        stringAnswer = $"({answer + splitAnswer} - {splitAnswer})";
                }
                TextCodes.Add(stringAnswer);
                SaveTextCodes.Add(stringAnswer);
                TextAnswers.Add(answer.ToString());
                SaveTextAnswers.Add(answer.ToString());
            }
            CurrentAttackTime = AttackTime;
        }
        public virtual void NumberClick(string number)
        {
            Answer += number;
            var currentAnswer = TextAnswers.First();
            if (!currentAnswer.Contains(Answer))
            {
                TextAnswers = new List<string>(SaveTextAnswers);
                TextCodes = new List<string>(SaveTextCodes);
                CodeText.text = string.Join(' ', TextCodes);
                Answer = "";
                return;
            }

            if (Answer == currentAnswer)
            {
                TextAnswers.RemoveAt(0);
                TextCodes.RemoveAt(0);
                CodeText.text = string.Join(' ', TextCodes);
                Answer = "";
            }

            if (TextCodes.Count == 0)
            {
                TerminalDesktopManager.Instance.CloseWindow(this);
            }
        }

        public override void WindowSync(WindowSync windowSync)
        {
            base.WindowSync(windowSync);
            if (windowSync.SyncCustomInt)
                DifficultyLevel = windowSync.CustomInt;
            if (windowSync.SyncCustomString)
            {
                var data = windowSync.CustomString.Split(';');
                var textCodes = data[0];
                var textAnswers = data[1];
                var attackTime = data[2];
                TextCodes = new List<string>(textCodes.Split(','));
                SaveTextCodes = new List<string>(TextCodes);
                TextAnswers = new List<string>(textAnswers.Split(','));
                SaveTextAnswers = new List<string>(TextAnswers);
                AttackTime = float.Parse(attackTime);
                CurrentAttackTime = AttackTime;
                CodeText.text = string.Join(' ', TextCodes);
            }
        }

        public override WindowSync GetFullWindowSync()
        {
            return new WindowSync()
            {
                ChangeCollapsed = true,
                IsCollapsed = WindowCanvasGroup.alpha == 0,
                SyncPosition = true,
                Position = transform.localPosition,
                SyncScale = true,
                Scale = WindowContainer.sizeDelta,
                SyncCustomInt = true,
                CustomInt = DifficultyLevel,
                SyncCustomString = true,
                CustomString = $"{string.Join(',',TextCodes)};{string.Join(',',TextAnswers)};{AttackTime.ToString()}"
            };
        }
    }
}