using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private WeaponsPanelControl weaponsPanelControl;

        [SerializeField] private Firearms[] weapons;
        // mod list


        private void Awake()
        {

        }
        private void Start()
        {
            UpdateAll();
        }

        public void AddWeapon(Firearms weapon)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] == null)
                {
                    weapons[i] = weapon;
                    break;
                }
            }
            UpdateAll();
        }

        private void UpdateAll()
        {
            weaponsPanelControl.UpdateWeaponList(weapons);
        }
    }
}
