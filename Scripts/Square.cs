using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Boarshroom.Minesweeper
{
    public class Square : MonoBehaviour
    {
        public bool locked;
        [SerializeField] SpriteRenderer flag;

        private void Awake()
        {
            flag.enabled = false;
            locked = false;
        }

        public int SetFlag()
        {
            flag.enabled = !flag.enabled;
            locked = !locked;

            if(locked) { return -1; }
            else { return 1; }
        }
    }
}
