using System.Collections.Generic;
using System.Linq;
using Kaede.Scripts.GamePlay;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Kaede.Scripts.Managers
{
    public class ScoreManager
    {
        
        public List<List<float>> StepScoresPerMenu { get; } = new();
        public List<float> MenuScores { get; } = new();
        public float GrandTotalScore { get; private set; }
        
        public int TotalRedoSteps { get; set; }
        
        private readonly List<float> _currentStepScores = new();
        public List<float> CurrentStepScores => new List<float>(_currentStepScores);
        private float _pendingStepScore;

        #region Score Methods
        public void SetPendingStepScore(float score)
        {
            _pendingStepScore = score;
        }

        public void CommitPendingStepScore()
        {
            _currentStepScores.Add(_pendingStepScore);
            _pendingStepScore = 0f;
        }

        public void AddPendingStepScore(float score)
        {
            _pendingStepScore += score;
        }
        
        public void ResetPendingStepScore()
        {
            _pendingStepScore = 0f;
        }
        
        public void FinalizeCurrentMenuScore()
        {
            var menuScore = _currentStepScores.Sum();
            MenuScores.Add(menuScore);
            StepScoresPerMenu.Add(new List<float>(_currentStepScores));
            _currentStepScores.Clear();
            GrandTotalScore = MenuScores.Sum();
        }
        
        public void ResetAllScores()
        {
            StepScoresPerMenu.Clear();
            MenuScores.Clear();
            GrandTotalScore = 0f;
            _currentStepScores.Clear();
            ResetPendingStepScore();
        }
        #endregion

        #region Redo Methods
        public void ResetRedoCount()
        {
            TotalRedoSteps = 0;
        }
        
        public void AddRedoCount()
        {
            TotalRedoSteps++;
        }
        #endregion
    }
}
