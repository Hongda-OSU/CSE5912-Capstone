using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class CryoIntellection : PlayerSkill
    {
        [SerializeField] private float chancePerLevel = 0.03f;

        protected override string GetBuiltSpecific()
        {
            var value = BuildSpecific("Frozen Chance", chancePerLevel * 100, chancePerLevel * 100, "%", "");
            return value;
        }
        public override bool LevelUp()
        {
            bool result = base.LevelUp();
            if (result)
                PlayerStats.Instance.FrozenBaseChance += chancePerLevel;

            return result;
        }

        public override void ResetLevel()
        {
            base.ResetLevel();
            PlayerStats.Instance.FrozenBaseChance -= chancePerLevel * level;
        }

        public float ChancePerLevel { get { return chancePerLevel; } }
    }
}
