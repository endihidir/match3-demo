using System.Collections.Generic;
using Game.Grid.Contexts;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IBoosterActionBuildHandler
    {
        void Build(BoosterObject booster, List<BoosterActionContext> output);
        void BuildCombo(BoosterObject source, BoosterObject target, List<BoosterActionContext> output);
    }
}