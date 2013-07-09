﻿/*The contents of this file are subject to the Mozilla Public License Version 1.1
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

The Original Code is the TSOClient.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using TSOClient.Code;
using Microsoft.Xna.Framework.Graphics;
using TSOClient.Code.Rendering;
using TSOClient.Code.Utils;

namespace TSOClient.ThreeD
{
    /// <summary>
    /// A renderable three dimensional scene that is rendered
    /// separately from UIScenes. 3D objects cannot be rendered
    /// between a call to SpriteBatch.Begin() and SpriteBatch.End(),
    /// as that will cause the objects to be transparent.
    /// </summary>
    public class ThreeDScene
    {
        private SceneManager m_SceneMgr;
        private List<ThreeDElement> m_Elements = new List<ThreeDElement>();

        public Camera Camera;
        public string ID;


        public SceneManager SceneMgr
        {
            get { return m_SceneMgr; }
        }

        public ThreeDScene()
        {
            m_SceneMgr = GameFacade.Scenes;
            Camera = new Camera(Vector3.Backward * 17, Vector3.Zero, Vector3.Right);
        }


        public List<ThreeDElement> GetElements()
        {
            return m_Elements;
        }


        public void Update(GameTime Time)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                m_Elements[i].Update(Time);
            }
        }



        public ThreeDScene(SceneManager SceneMgr)
        {
            m_SceneMgr = SceneMgr;
        }

        /// <summary>
        /// Creates a UI3DView instance that can be used to render 3D elements
        /// (sims) in this scene on top of UI elements.
        /// </summary>
        /// <param name="Width">The width of the rendering surface for this UI3DView instance.</param>
        /// <param name="Height">The height of the rendering surface for this UI3DView instance.</param>
        /// <param name="SingleRenderer">Will this UI3DView be used to render a single or multiple sims?</param>
        /// <param name="StrID">The string ID of this UI3DView instance.</param>
        /// <returns>A UI3DView instance.</returns>
        //public UI3DView Create3DView(int Width, int Height, bool SingleRenderer, string StrID)
        //{
        //    UI3DView ThreeDView = new UI3DView(Width, Height, SingleRenderer, this, StrID);
        //    m_Elements.Add(ThreeDView);

        //    return ThreeDView;
        //}

        public void Add(ThreeDElement item)
        {
            m_Elements.Add(item);
            item.Scene = this;
        }

        public void Draw(GraphicsDevice device)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                    m_Elements[i].Draw(device, this);
            }

            if (Camera.DrawCamera)
            {
                Camera.Draw(device);
            }
        }




        public override string ToString()
        {
            if (ID != null)
            {
                return ID;
            }

            return base.ToString();
        }
    }
}
