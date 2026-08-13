using Runtime.Core;
using Runtime.Level.Config;
namespace Runtime.Services
{
    public class LevelService : IService
    {
        private readonly LevelData[] _levels;
        private int _currentLevelNumber;
        
        public LevelData Current => _levels[_currentLevelNumber - 1];
        
        public LevelService(LevelData[] levels, int currentLevelNumber)
        {
            _levels = levels;
            _currentLevelNumber = currentLevelNumber;
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        public void Next()
        {
            _currentLevelNumber = _currentLevelNumber % _levels.Length + 1; // Easy level loop
        }
    }
}
