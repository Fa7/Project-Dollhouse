﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TSOClient.Code.Utils;
using Microsoft.Xna.Framework;

namespace TSOClient.Code.UI.Framework
{
    public class UISpriteBatch : SpriteBatch
    {
        /// <summary>
        /// Creates a UISpriteBatch. Same as spritebatch with some extra functionality
        /// required by the UI system of this game.
        /// 
        /// NumBuffers refers to a number of RenderTarget2D objects to create up front.
        /// These should be used for temp rendering for special effects. E.g. rendering
        /// part of the GUI to a texture and then painting it with opacity onto the main
        /// target.
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="numBuffers">The number of rendering buffers to pre-alloc</param>
        public UISpriteBatch(GraphicsDevice gd, int numBuffers)
            : base(gd)
        {
            for (var i = 0; i < numBuffers; i++)
            {
                Buffers.Add(
                    RenderUtils.CreateRenderTarget(gd, 1, SurfaceFormat.Color, gd.Viewport.Width, gd.Viewport.Height)
                );
            }
        }

        //private SpriteBlendMode _BlendMode;
        private SpriteSortMode _SortMode;
        //private SaveStateMode _SaveStateMode;


        /**
         * SpriteBatches which can be used to render
         * parts of the UI to a texture, then manipulated before
         * being added to the main batch. E.g. to do alpha blending
         */
        private List<RenderTarget2D> Buffers = new List<RenderTarget2D>();


        public void UIBegin(/*SpriteBlendMode blendMode,*/ SpriteSortMode sortMode/*, SaveStateMode stateMode*/)
        {
            /*this._BlendMode = blendMode;
            this._SortMode = sortMode;
            this._SaveStateMode = stateMode;*/

            this.Begin(sortMode, BlendState.AlphaBlend);
        }


        public void Pause()
        {
            this.End();
        }

        public void Resume()
        {
            this.Begin(_SortMode, BlendState.AlphaBlend);
        }


        public RenderTarget2D GetBuffer()
        {
            if (Buffers.Count > 0)
            {
                var item = Buffers[0];
                Buffers.RemoveAt(0);
                return item;
            }
            return null;
        }

        public void FreeBuffer(RenderTarget2D buffer)
        {
            Buffers.Add(buffer);
        }


        public UIRenderPlane WithBuffer(ref Promise<Texture2D> output)
        {
            var promise = new Promise<Texture2D>(x => null);
            output = promise;

            return new UIRenderPlane(
                this,
                promise
            );
        }


    }






    /// <summary>
    /// Temporary rendering target so you can do visual
    /// effects on the rendered output. Bitmap effects,
    /// Alpha blending etc
    /// </summary>
    public class UIRenderPlane : IDisposable
    {
        private GraphicsDevice GD;
        private RenderTarget2D Target;
        private Promise<Texture2D> Texture;
        private Texture2D BackBuffer;
        private UISpriteBatch Batch;

        public UIRenderPlane(UISpriteBatch batch, Promise<Texture2D> texture)
        {
            this.GD = batch.GraphicsDevice;
            this.Target = batch.GetBuffer();
            this.Texture = texture;
            this.Batch = batch;

            /** Switch the render target **/
            Batch.Pause();
            GD.SetRenderTarget(Target);
            GD.Clear(Color.Transparent);
            Batch.Resume();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Batch.Pause();
            
            GD.SetRenderTarget(null);
            Texture.SetValue(Target);
            Batch.Resume();

            /**
                batch.Pause();
                gd.SetRenderTarget(0, (RenderTarget2D)renderTarget);
                batch.FreeBuffer(buffer);

**/

            Batch.FreeBuffer(Target);
        }

        #endregion
    }
}
