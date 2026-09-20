using System.Linq;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Helpers.Factory
{
    public class StateConfigFactory
    {
        private readonly GlobalConfig _globalConfig;

        public StateConfigFactory(GlobalConfig globalConfig)
        {
            _globalConfig = globalConfig;
        }

        public StateConfig GetConfig(GameStateType stateType)
        {
            return _globalConfig.StateConfigs.FirstOrDefault(config => config.StateName == stateType);
        }
    }
}