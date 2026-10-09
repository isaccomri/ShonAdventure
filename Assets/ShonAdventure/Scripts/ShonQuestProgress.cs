using System;
using UnityEngine;

namespace ShonAdventure
{
    /// <summary>
    /// Standalone prototype quest tracker. Attach to one GameObject.
    /// Integrate with Adventure Creator ActionLists in later iterations.
    /// </summary>
    public sealed class ShonQuestProgress : MonoBehaviour
    {
        public const int QuestCount = 20;
        private const string ProgressKey = "ShonAdventure.Progress.v1";

        [SerializeField, Range(1, QuestCount)]
        private int currentQuest = 1;

        public int CurrentQuest => currentQuest;
        public bool Finished { get; private set; }
        public event Action<int> OnQuestChanged;

        private void Awake() => Load();

        public void CompleteQuest()
        {
            if (Finished) return;
            if (currentQuest == QuestCount)
            {
                Finished = true;
                PlayerPrefs.SetInt(ProgressKey, QuestCount + 1);
            }
            else
            {
                currentQuest++;
                PlayerPrefs.SetInt(ProgressKey, currentQuest);
            }
            PlayerPrefs.Save();
            OnQuestChanged?.Invoke(Finished ? QuestCount + 1 : currentQuest);
        }

        public void Load()
        {
            int saved = PlayerPrefs.GetInt(ProgressKey, 1);
            Finished = saved > QuestCount;
            currentQuest = Mathf.Clamp(saved, 1, QuestCount);
            OnQuestChanged?.Invoke(Finished ? QuestCount + 1 : currentQuest);
        }

        public void ResetQuests()
        {
            Finished = false;
            currentQuest = 1;
            PlayerPrefs.SetInt(ProgressKey, 1);
            PlayerPrefs.Save();
            OnQuestChanged?.Invoke(currentQuest);
        }
    }
}
