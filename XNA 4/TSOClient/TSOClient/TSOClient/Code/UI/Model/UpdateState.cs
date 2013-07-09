﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TSOClient.Code.UI.Framework;

namespace TSOClient.Code.UI.Model
{
    /// <summary>
    /// Contains common information used in the update loop
    /// </summary>
    public class UpdateState
    {
        public GameTime Time;
        public MouseState MouseState;
        public KeyboardState KeyboardState;
        public InputManager InputManager;

        public KeyboardState PreviousKeyboardState;

        /** A Place to keep shared variables, clears every update cycle **/
        public Dictionary<string, object> SharedData = new Dictionary<string, object>();
        public List<UIMouseEventRef> MouseEvents = new List<UIMouseEventRef>();

        private Dictionary<Keys, long> KeyDownTime = new Dictionary<Keys, long>();
        private List<Keys> KeyInRepeatMode = new List<Keys>();

        public List<Keys> NewKeys = new List<Keys>();
        public int Depth;

        /// <summary>
        /// </summary>
        public void Update()
        {
            NewKeys.Clear();
            Depth = 0;

            /**
             * If a key has been held down for X duration, treat it as if it is newly
             * pressed
             */
            for(var i=0; i < KeyInRepeatMode.Count; i++){

                if (!KeyboardState.IsKeyDown(KeyInRepeatMode[i]))
                {
                    KeyInRepeatMode.RemoveAt(i);
                    i--;
                }
            }

            var now = Time.TotalGameTime.Ticks;
            var keys = KeyboardState.GetPressedKeys();

            foreach (var key in keys)
            {
                var newPress = PreviousKeyboardState.IsKeyUp(key);
                if (newPress)
                {
                    KeyDownTime[key] = now;
                    NewKeys.Add(key);
                }
                else
                {
                    if (KeyInRepeatMode.Contains(key))
                    {

                        /** How long has it been down? **/
                        if (now - KeyDownTime[key] > 400000)
                        {
                            /** Its been down long enough, consider it a new key **/
                            KeyDownTime[key] = now;
                            NewKeys.Add(key);
                        }
                    }
                    else
                    {
                        /** How long has it been down? **/
                        if (now - KeyDownTime[key] > 9000000)
                        {
                            /** Its been down long enough, consider it in repeat mode **/
                            KeyDownTime[key] = now;
                            KeyInRepeatMode.Add(key);
                        }
                    }
                }
            }
        }
    }
}
