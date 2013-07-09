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
using System.IO;
using Paloma;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TSOClient.Code.UI.Model;
using TSOClient.Code.Utils;
using TSOClient.Code.UI.Framework.Parser;
using System.Threading;

namespace TSOClient.Code.UI.Framework
{
    /// <summary>
    /// Base class for all UIElements. This class has all the common stuff that user interface
    /// components have. This includes:
    /// 
    /// X,Y, ScaleX, ScaleY, Visible, Parent etc.
    /// 
    /// The UIElement has its own coordinate space. You can imagine that when you draw
    /// a texture inside a UIElement you are drawing it at 0,0 within the UIElement. This
    /// is then translated by the engine to screen coordinates.
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// ID of the element, this is not used by any functional code.
        /// It is only used in the debug UI Inspector to help you identify
        /// which component you are looking at.
        /// </summary>
        protected string m_StringID;

        /// <summary>
        /// X position of this UIComponent. This coordinate is relative to this UIElement's
        /// parent component.
        /// </summary>
        protected float _X;
        
        /// <summary>
        /// Y position of this UIComponent. This coordinate is relative to this UIElement's
        /// parent component.
        /// </summary>
        protected float _Y;

        /// <summary>
        /// Scale Factor of the X Axis.
        /// </summary>
        protected float _ScaleX = 1.0f;

        /// <summary>
        /// Scale Factor of the Y Axis.
        /// </summary>
        protected float _ScaleY = 1.0f;

        /// <summary>
        /// Transparency value for this UIElement. Can be between 0.0 and 1.0
        /// </summary>
        protected float _Opacity = 1.0f;

        /// <summary>
        /// When the opacity changes this color will be changed
        /// into a representation of the opacity that can be used
        /// when calling the draw texture methods.
        /// </summary>
        protected Microsoft.Xna.Framework.Color _BlendColor = Microsoft.Xna.Framework.Color.White;

        /// <summary>
        /// Indicates if the opacity has changed and we have not recauclated
        /// the blend colour
        /// </summary>
        protected bool _OpacityDirty = false;

        /// <summary>
        /// Boolean which indicates if the opacity is not 1.0. This is here as a utility
        /// because its faster to compare a boolean than to compare a float (_Opacity != 1.0f)
        /// </summary>
        protected bool _HasOpacity = false;

        /// <summary>
        /// The container which this element is a child of. Can be null if top level UI object.
        /// UIContainer sets this in its Add method. Helps describe the UI Tree.
        /// </summary>
        protected UIContainer _Parent;
        
        /// <summary>
        /// Matrix object which represents the position & scale of this UIElement.
        /// 
        /// Whenever X, Y, ScaleX, ScaleY, Parent (or any of these values on my parent) change
        /// We recalculate this matrix.
        /// 
        /// It is used to convert local coordinates into absolute screen coordinates for drawing.
        /// </summary>
        protected float[] _Mtx = Matrix2D.IDENTITY;

        /// <summary>
        /// This is the absolute scale of this UIElement, Aka it is all the parent
        /// scales multiplied together finally multiplied by this UIElement's scale.
        /// 
        /// Essentially, this is the scale value you would pass into a texture draw method. It is
        /// relative to the screen
        /// </summary>
        protected Vector2 _Scale = Vector2.One;

        /// <summary>
        /// This is the absolute scale of this UIElement, Aka it is all the parent
        /// scales multiplied together.
        /// 
        /// This is used for some specific calculations.
        /// </summary>
        protected Vector2 _ScaleParent = Vector2.One;

        /// <summary>
        /// Indicates if something has changed to make the Matrix invalid, e.g. X,Y has changed or
        /// the component is now inside a different parent object. Also called when the parent decides
        /// its dirty.
        /// </summary>
        protected bool _MtxDirty;

        /// <summary>
        /// Indicates if the component is visible or not. If false the UIElement
        /// should not draw. This must be implemented in the super class. UIElement does nothing
        /// with this variable.
        /// </summary>
        public bool Visible = true;


        /// <summary>
        /// ID of the UIElement. This value is used to help debug
        /// and identify which component is which. The name will display
        /// in the debug UI Inspector panel.
        /// </summary>
        public string ID
        {
            get { return m_StringID; }
            set { m_StringID = value; }
        }


        /// <summary>
        /// X Coordinate of the UIElement relative to its UIContainer.
        /// </summary>
        public float X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
                _MtxDirty = true;
            }
        }


        /// <summary>
        /// Y Coordinate of the UIElement relative to its UIContainer.
        /// </summary>
        public float Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
                _MtxDirty = true;
            }
        }

        /// <summary>
        /// Horizontal Scale factor for this component.
        /// </summary>
        public float ScaleX
        {
            get { return _ScaleX; }
            set { _ScaleX = value; _MtxDirty = true; }
        }

        /// <summary>
        /// Vertical Scale factor for this component.
        /// </summary>
        public float ScaleY
        {
            get { return _ScaleY; }
            set { _ScaleY = value; _MtxDirty = true; }
        }

        /// <summary>
        /// Transparency value of this UIElement. Value must be between 0.0 and 1.0
        /// </summary>
        public float Opacity
        {
            get {
                return _Opacity;
            }
            set
            {
                _Opacity = value;
                _OpacityDirty = true;
            }
        }

        /// <summary>
        /// Color to blend with while painting. When opacity is set this value
        /// becomes Color(0xFF, 0xFF, 0xFF, Opacity). You can use this as the blend
        /// parameter when drawing a texture
        /// </summary>
        public Microsoft.Xna.Framework.Color BlendColor
        {
            get
            {
                if (_OpacityDirty)
                {
                    CalculateOpacity();
                }
                return _BlendColor;
            }
        }

        /// <summary>
        /// Returns the size of the UIElement. By default this is not implemented as not all UIElement's
        /// have size or a bounding box.
        /// </summary>
        public virtual Vector2 Size
        {
            get
            {
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// The container which this element is a child of. Can be null if top level UI object
        /// </summary>
        public UIContainer Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
                //_MtxDirty = true;
                /** Force a calculate **/
                CalculateMatrix();
            }
        }
        
        /// <summary>
        /// Matrix object which represents the position & scale of this UIElement.
        /// 
        /// Whenever X, Y, ScaleX, ScaleY, Parent (or any of these values on my parent) change
        /// We recalculate this matrix.
        /// 
        /// It is used to convert local coordinates into absolute screen coordinates for drawing.
        /// </summary>
        public float[] Matrix
        {
            get
            {
                return _Mtx;
            }
        }

        /// <summary>
        /// This is the absolute scale of this UIElement, Aka it is all the parent
        /// scales multiplied together finally multiplied by this UIElement's scale.
        /// 
        /// Essentially, this is the scale value you would pass into a texture draw method. It is
        /// relative to the screen
        /// </summary>
        public Vector2 Scale
        {
            get { return _Scale; }
        }


        /// <summary>
        /// When the opacity changes this method is used to calculate
        /// the blend colour.
        /// </summary>
        protected virtual void CalculateOpacity()
        {
            //if (_Parent != null)
            //{
            //    _BlendColor = _Parent.BlendColor;
            //}
            //else
            //{
            //    _BlendColor = Color.White;
            //}

            _BlendColor = Microsoft.Xna.Framework.Color.White;

            //Convert the opacity percentage into a byte (0-255)
            _BlendColor.A = (byte)((((float)_BlendColor.A / 255.0f) * _Opacity) * 255);
            _OpacityDirty = false;
            _HasOpacity = _Opacity != 1.0f;
        }



        /// <summary>
        /// Calculate a matrix which represents this objects position in screen space
        /// </summary>
        protected virtual void CalculateMatrix()
        {
            //If we are a child of another UIElement, start with our container's Matrix.
            //Otherwise, assume our matrix is IDENTITY (aka no scale & positioned at 0,0)
            if (_Parent != null)
            {
                _Mtx = _Parent.Matrix.CloneMatrix();
                _ScaleParent = _Parent.Scale;
            }
            else
            {
                _ScaleParent = Vector2.One;
                _Mtx = Matrix2D.IDENTITY;
            }

            //Translate by our x and y coordinates
            _Mtx.Translate(_X, _Y);

            //Scale by our scaleX and scaleY values
            _Mtx.Scale(_ScaleX, _ScaleY);

            //Work out the absolute scale factor for this UIElement
            _Scale = _Mtx.ExtractScaleVector();

            //Because Matrix has changed, that means the inverted matrix is now invalid.
            //Make this null so that next time its requested it gets recalculated
            _InvertedMtx = null;

            //Matrix is no longer dirty :)
            _MtxDirty = false;

            //We cache mouse target positions, because our coordinates have changed these are no longer valid.
            _HitTestCache.Clear();
        }

        /// <summary>
        /// Utility to force the component to recalculate its matrix
        /// </summary>
        public void InvalidateMatrix()
        {
            _MtxDirty = true;
        }

        /// <summary>
        /// Utility to force the component to recalculate its blend colour
        /// </summary>
        public void InvalidateOpacity()
        {
            _OpacityDirty = true;
        }

        /// <summary>
        /// During the update loop we give each element an absolute depth number.
        /// We can use this to work out which UIElements are visually above others.
        /// This is important for the UIManager.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Standard UIElement update method. All sub-classes should call
        /// this super method.
        /// 
        /// The argument is an UpdateState object, this object contains everything
        /// you may need during update including GameTime, MouseState, KeyboardState etc.
        /// 
        /// This is useful because it means we dont ask for Mouse & Keyboard state in every UIElement
        /// which would be wasteful
        /// </summary>
        /// <param name="statex"></param>
        public virtual void Update(UpdateState state)
        {
            //Set our absolute depth value
            this.Depth = state.Depth++;

            //If our matrix is dirty, recalculate it
            if (_MtxDirty)
            {
                CalculateMatrix();
            }

            //If our blend is dirty, recalculate it
            if (_OpacityDirty)
            {
                CalculateOpacity();
            }


            if (Visible)
            {
                if (m_MouseRefs != null)
                {
                    //Check to see if the mouse is over any of the regions
                    //we have been asked to keep an eye on using ListenForMouse.
                    foreach (var mouseRegion in m_MouseRefs)
                    {
                        if (HitTestArea(state, mouseRegion.Region, true))
                        {
                            state.MouseEvents.Add(mouseRegion);
                        }
                    }
                }

                //Update hooks are callbacks. This lets external code add extra work
                //that executes during the update loop.
                //This is important because things like drag & drop should be calculated in the update loop.
                //See UIUtils.MakeDraggable
                if (UpdateHooks != null)
                {
                    lock (UpdateHooks)
                    {
                        foreach (var hook in UpdateHooks)
                        {
                            hook(state);
                        }
                    }
                }
            }
        }



        /// <summary>
        /// List of callbacks which get invoked every update. Allows external code
        /// to execute during the update loop.
        /// </summary>
        private List<UpdateHookDelegate> UpdateHooks;

        /// <summary>
        /// Adds a callback that will be executed every update loop.
        /// </summary>
        /// <param name="hook"></param>
        public void AddUpdateHook(UpdateHookDelegate hook)
        {
            if (UpdateHooks == null)
            {
                UpdateHooks = new List<UpdateHookDelegate>();
            }
            lock (UpdateHooks)
            {
                UpdateHooks.Add(hook);
            }
        }

        /// <summary>
        /// Removes a previously added update hook.
        /// </summary>
        /// <param name="hook"></param>
        public void RemoveUpdateHook(UpdateHookDelegate hook)
        {
            lock (UpdateHooks)
            {
                UpdateHooks.Remove(hook);
            }
        }


        /// <summary>
        /// May be removed - Called before the draw method.
        /// </summary>
        /// <param name="batch"></param>
        public virtual void PreDraw(UISpriteBatch batch)
        {
        }

        /// <summary>
        /// Basic draw method. Your component should implement this
        /// and add any drawing behavior it needs.
        /// </summary>
        /// <param name="batch"></param>
        public abstract void Draw(UISpriteBatch batch);


        /// <summary>
        /// Converts a rectangle relative to this UIElement into a rectangle
        /// relative to the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public Microsoft.Xna.Framework.Rectangle LocalRect(float x, float y, float w, float h)
        {
            return LocalRect(x, y, w, h, _Mtx);
        }

        /// <summary>
        /// Converts a point relative to this UIElement into a point relative to
        /// the screen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector2 LocalPoint(float x, float y)
        {
            return LocalPoint(new Vector2(x, y));
        }

        /// <summary>
        /// Converts a point relative to this UIElement into a point relative to
        /// the screen
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector2 LocalPoint(Vector2 point)
        {
            return _Mtx.TransformPoint(point);
        }

        /// <summary>
        /// Converts a point relative to the screen into a point relative to
        /// this component
        /// </summary>
        /// <param name="globalPoint"></param>
        /// <returns></returns>
        public Vector2 GlobalPoint(Vector2 globalPoint)
        {
            if (_InvertedMtx == null)
            {
                _InvertedMtx = _Mtx.Invert();
            }
            return _InvertedMtx.TransformPoint(globalPoint);
        }

        /// <summary>
        /// Converts a rectangle relative to this UIElement into a rectangle
        /// relative to the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="mtx"></param>
        /// <returns></returns>
        public Microsoft.Xna.Framework.Rectangle LocalRect(float x, float y, float w, float h, float[] mtx)
        {
            mtx.TransformPoint(ref x, ref y);
            w *= _Scale.X;
            h *= _Scale.Y;

            return new Microsoft.Xna.Framework.Rectangle((int)x, (int)y, (int)w, (int)h);
        }

        /// <summary>
        /// This utility will draw a line of text onto the UIElement.
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw the text onto</param>
        /// <param name="text">The content of the text</param>
        /// <param name="to">The position of the text. Relative to this UIElement.</param>
        /// <param name="style">The text style</param>
        public void DrawLocalString(SpriteBatch batch, string text, Vector2 to, TextStyle style)
        {
            var scale = _Scale;
            if (style.Scale != 1.0f)
            {
                scale = new Vector2(scale.X * style.Scale, scale.Y * style.Scale);
            }
            to.Y += style.BaselineOffset;
            //to.Y += (style.Size + 5) * style.Font.BaselineOffset;
            to.X = (float)Math.Floor(to.X);
            to.Y = (float)Math.Floor(to.Y);
            batch.DrawString(style.SpriteFont, text, LocalPoint(to), style.Color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// This utility will draw a line of text onto the UIElement.
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw the text onto</param>
        /// <param name="text">The content of the text</param>
        /// <param name="to">The position of the text. Relative to this UIElement.</param>
        /// <param name="style">The text style</param>
        /// <param name="bounds">Rectangle relative to this UIElement which the text should be positioned within</param>
        /// <param name="align">Alignment of the text within the bounds box.</param>
        public void DrawLocalString(SpriteBatch batch, string text, Vector2 to, TextStyle style, Microsoft.Xna.Framework.Rectangle bounds, TextAlignment align)
        {
            DrawLocalString(batch, text, to, style, bounds, align, Microsoft.Xna.Framework.Rectangle.Empty);
        }

        /// <summary>
        /// This utility will draw a line of text onto the UIElement.
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw the text onto</param>
        /// <param name="text">The content of the text</param>
        /// <param name="to">The position of the text. Relative to this UIElement.</param>
        /// <param name="style">The text style</param>
        /// <param name="bounds">Rectangle relative to this UIElement which the text should be positioned within</param>
        /// <param name="align">Alignment of the text within the bounds box.</param>
        /// <param name="margin">Margin offset from the bounding box.</param>
        public void DrawLocalString(SpriteBatch batch, string text, Vector2 to, TextStyle style, Microsoft.Xna.Framework.Rectangle bounds, TextAlignment align, Microsoft.Xna.Framework.Rectangle margin)
        {
            DrawLocalString(batch, text, to, style, bounds, align, margin, UIElementState.Normal);
        }

        /// <summary>
        /// This utility will draw a line of text onto the UIElement.
        /// </summary>
        /// <param name="batch">The SpriteBatch to draw the text onto</param>
        /// <param name="text">The content of the text</param>
        /// <param name="to">The position of the text. Relative to this UIElement.</param>
        /// <param name="style">The text style</param>
        /// <param name="bounds">Rectangle relative to this UIElement which the text should be positioned within</param>
        /// <param name="align">Alignment of the text within the bounds box.</param>
        /// <param name="margin">Margin offset from the bounding box.</param>
        /// <param name="state">State of the text, e.g. hover, down, normal</param>
        public void DrawLocalString(SpriteBatch batch, string text, Vector2 to, TextStyle style, Microsoft.Xna.Framework.Rectangle bounds, TextAlignment align, Microsoft.Xna.Framework.Rectangle margin, UIElementState state)
        {
            //TODO: We should find some way to cache this data

            /** 
             * Work out the scale of the vector font.
             * 
             * We need to scale it based on the UIElement's scale factory,
             * but we also need to scale it based on the text styles scale factor.
             * 
             * Aka if the vector font is 12px and we asked for 24px it would be scale of 2.0
             */
            var scale = _Scale;
            if (style.Scale != 1.0f)
            {
                scale = new Vector2(scale.X * style.Scale, scale.Y * style.Scale);
            }

            /** Work out how big the text will be so we can align it **/
            var textSize = style.SpriteFont.MeasureString(text);
            Vector2 size = textSize * style.Scale;

            /** Apply margins **/
            if (margin != Microsoft.Xna.Framework.Rectangle.Empty)
            {
                bounds.X += margin.X;
                bounds.Y += margin.Y;
                bounds.Width -= margin.Right;
                bounds.Height -= margin.Bottom;
            }

            /** Work out X and Y based on alignment & bounding box **/
            var pos = to;
            pos.X += bounds.X;
            pos.Y += bounds.Y;
            
            if ((align & TextAlignment.Right) == TextAlignment.Right)
            {
                pos.X += (bounds.Width - size.X);
            }
            else if ((align & TextAlignment.Center) == TextAlignment.Center)
            {
                pos.X += (bounds.Width - size.X) / 2;
            }

            if ((align & TextAlignment.Middle) == TextAlignment.Middle)
            {
                pos.Y += (bounds.Height - size.Y) / 2;
            }
            else if ((align & TextAlignment.Bottom) == TextAlignment.Bottom)
            {
                pos.Y += (bounds.Height - size.Y);
            }
            //pos.Y += (((style.Size + 5) * style.Scale) * style.Font.BaselineOffset);
            pos.X = (float)Math.Floor(pos.X);
            pos.Y = (float)Math.Floor(pos.Y);


            //DrawLocalTexture(batch, TextureUtils.TextureFromColor(batch.GraphicsDevice, Color.Red), null, pos, size);

            pos.Y += style.BaselineOffset;

            /** Draw the string **/
            pos = LocalPoint(pos);
            batch.DrawString(style.SpriteFont, text, pos, style.GetColor(state), 0, Vector2.Zero, scale, SpriteEffects.None, 0);


        }




        /// <summary>
        /// Draws a texture to the UIElement. This method will deal with
        /// the matrix calculations
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="texture"></param>
        /// <param name="to"></param>
        public void DrawLocalTexture(SpriteBatch batch, Texture2D texture, Vector2 to)
        {
            /**
             * v1.X *= _ScaleParent.X;
             * v1.Y *= _ScaleParent.Y;
             */
            batch.Draw(texture, LocalPoint(to), null, _BlendColor, 0.0f,
                    new Vector2(0.0f, 0.0f), _Scale, SpriteEffects.None, 0.0f);
        }

        /// <summary>
        /// Draws a texture to the UIElement. This method will deal with
        /// the matrix calculations
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="texture"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void DrawLocalTexture(SpriteBatch batch, Texture2D texture, Microsoft.Xna.Framework.Rectangle from, Vector2 to)
        {
            batch.Draw(texture, LocalPoint(to), from, _BlendColor, 0.0f,
                    new Vector2(0.0f, 0.0f), _Scale, SpriteEffects.None, 0.0f);
        }

        /// <summary>
        /// Draws a texture to the UIElement. This method will deal with
        /// the matrix calculations
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="texture"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="scale"></param>
        public void DrawLocalTexture(SpriteBatch batch, Texture2D texture, Nullable<Microsoft.Xna.Framework.Rectangle> from, Vector2 to, Vector2 scale)
        {
            batch.Draw(texture, LocalPoint(to), from, _BlendColor, 0.0f,
                    new Vector2(0.0f, 0.0f), _Scale * scale, SpriteEffects.None, 0.0f);
        }


        private Dictionary<Microsoft.Xna.Framework.Rectangle, Vector4> _HitTestCache = new Dictionary<Microsoft.Xna.Framework.Rectangle, Vector4>();

        /// <summary>
        /// Returns true if the mouse is over the given area
        /// </summary>
        /// <param name="state"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool HitTestArea(UpdateState state, Microsoft.Xna.Framework.Rectangle area, bool cache)
        {
            if (!Visible) { return false; }


            var globalLeft = 0.0f;
            var globalTop = 0.0f;
            var globalRight = 0.0f;
            var globalBottom = 0.0f;

            if (_HitTestCache.ContainsKey(area))
            {
                var positions = _HitTestCache[area];
                globalLeft = positions.X;
                globalTop = positions.Y;
                globalRight = positions.Z;
                globalBottom = positions.W;
            }
            else
            {
                var globalPosition = LocalRect(area.X, area.Y, area.Width, area.Height);

                /*var globalPosition = _Mtx.TransformPoint(area.X, area.Y);//Vector2.Transform(new Vector2(area.X, area.Y), this.Matrix);
                globalLeft = globalPosition.X * _ScaleParent.X;
                globalTop = globalPosition.Y * _ScaleParent.Y;
                globalRight = globalLeft + (area.Width * _Scale.X);
                globalBottom = globalTop + (area.Height * _Scale.Y);*/

                if (cache)
                {
                    _HitTestCache.Add(area, new Vector4(globalPosition.X, globalPosition.Y, globalPosition.Right, globalPosition.Bottom));
                }
            }

            var mx = state.MouseState.X;
            var my = state.MouseState.Y;

            if (mx >= globalLeft && mx <= globalRight &&
                my >= globalTop && my <= globalBottom)
            {
                return true;
            }

            return false;
        }

        private List<UIMouseEventRef> m_MouseRefs;

        /**
         * Mouse utilities
         */
        public UIMouseEventRef ListenForMouse(Microsoft.Xna.Framework.Rectangle region, UIMouseEvent callback)
        {
            var newRegion = new UIMouseEventRef()
            {
                Callback = callback,
                Region = region,
                Element = this
            };
            if (m_MouseRefs == null)
            {
                m_MouseRefs = new List<UIMouseEventRef>();
            }
            m_MouseRefs.Add(newRegion);

            return newRegion;
        }

        private float[] _InvertedMtx;

        /// <summary>
        /// Gets the local mouse coordinates for the given mouse state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Vector2 GetMousePosition(MouseState mouse)
        {
            if (_InvertedMtx == null)
            {
                _InvertedMtx = _Mtx.Invert();
            }

            return _InvertedMtx.TransformPoint(mouse.X, mouse.Y);
        }

        public Texture2D GetTexture(uint id_0, uint id_1)
        {
            ulong ID = (ulong)(((ulong)id_0)<<32 | ((ulong)(id_1 >> 32)));
            return GetTexture(ID);
        }

        public static uint[] MASK_COLORS = new uint[]{
            new Microsoft.Xna.Framework.Color(0xFF, 0x00, 0xFF, 0xFF).PackedValue,
            new Microsoft.Xna.Framework.Color(0xFE, 0x02, 0xFE, 0xFF).PackedValue,
            new Microsoft.Xna.Framework.Color(0xFF, 0x01, 0xFF, 0xFF).PackedValue
        };

        public static Texture2D StoreTexture(ulong id, ContentResource assetData)
        {
            return StoreTexture(id, assetData, true, false);
        }

        public static Texture2D StoreTexture(ulong id, ContentResource assetData, bool mask, bool cacheOnDisk)
        {
            /**
             * This may not be the right way to get the texture to load as ARGB but it works :S
             */
            Texture2D texture = null;
            using (var stream = new MemoryStream())
            {
                var isCached = assetData.FromCache;

                if (!IsTargaImage(assetData.Data))
                {
                    Bitmap BMap = new Bitmap(new MemoryStream(assetData.Data));
                    BMap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
                else
                {
                    Paloma.TargaImage TImg = new Paloma.TargaImage(new MemoryStream(assetData.Data));
                    TImg.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }

                if (mask && !isCached)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    texture = Texture2D.FromStream(GameFacade.GraphicsDevice, stream);

                    TextureUtils.ManualTextureMaskSingleThreaded(ref texture, MASK_COLORS);  
                }
                else
                {
                    texture = Texture2D.FromStream(GameFacade.GraphicsDevice, stream);
                }
                UI_TEXTURE_CACHE.Add(id, texture);

                return texture;
            }
        }

        private static bool IsTargaImage(byte[] AssetData)
        {
            int FooterSignatureOffsetFromEnd = 18;
            int FooterSignatureByteLength = 16;
            string TargaFooterASCIISignature = "TRUEVISION-XFILE";
            MemoryStream MemStream = new MemoryStream(AssetData);
            BinaryReader binReader = new BinaryReader(MemStream);

            // set the cursor at the beginning of the signature string.
            binReader.BaseStream.Seek((FooterSignatureOffsetFromEnd * -1), SeekOrigin.End);

            // read the signature bytes and convert to ascii string
            string Signature = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(FooterSignatureByteLength)).TrimEnd('\0');

            // do we have a proper signature
            if (string.Compare(Signature, TargaFooterASCIISignature) == 0)
            {
                binReader.Close();
                return true;
            }

            binReader.Close();
            return false;
        }

        private static Dictionary<ulong, Texture2D> UI_TEXTURE_CACHE = new Dictionary<ulong, Texture2D>();
        public static Texture2D GetTexture(ulong id)
        {
            try
            {
                if (UI_TEXTURE_CACHE.ContainsKey(id))
                {
                    return UI_TEXTURE_CACHE[id];
                }

                var assetData = ContentManager.GetResourceInfo(id);
                
                return StoreTexture(id, assetData);
            }
            catch (Exception ex)
            {
                    LogThis.Log.LogThis(ex.ToString(), LogThis.eloglevel.error);
            }
            return null;
        }



        /// <summary>
        /// Manually replaces a specified color in a texture with transparent black,
        /// thereby masking it.
        /// </summary>
        /// <param name="Texture">The texture on which to apply the mask.</param>
        /// <param name="ColorFrom">The color to mask away.</param>
        protected void ManualTextureMask(ref Texture2D Texture, Microsoft.Xna.Framework.Color ColorFrom)
        {
            Microsoft.Xna.Framework.Color ColorTo = Microsoft.Xna.Framework.Color.Transparent;

            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[Texture.Width * Texture.Height];
            Texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == ColorFrom)
                    data[i] = ColorTo;
            }

            Texture.SetData(data);
        }

        protected void ManualTextureMask(ref Texture2D Texture, Microsoft.Xna.Framework.Color[] ColorsFrom)
        {
            Microsoft.Xna.Framework.Color ColorTo = Microsoft.Xna.Framework.Color.Transparent;

            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[Texture.Width * Texture.Height];
            Texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                foreach (Microsoft.Xna.Framework.Color Clr in ColorsFrom)
                {
                    if (data[i] == Clr)
                        data[i] = ColorTo;
                }
            }

            Texture.SetData(data);
        }



        public override string ToString()
        {
            var clazzName = this.GetType().Name;

            if (m_StringID == null)
            {
                return clazzName;
            }
            return clazzName + "(" + m_StringID + ")";
        }


        /// <summary>
        /// Gets the bounding box for the component
        /// </summary>
        /// <returns></returns>
        public virtual Microsoft.Xna.Framework.Rectangle GetBounds()
        {
            return Microsoft.Xna.Framework.Rectangle.Empty;
        }







        /**
         * UIScript setters
         */
        [UIAttribute("position")]
        public Vector2 Position
        {
            set
            {
                _X = value.X;
                _Y = value.Y;
                InvalidateMatrix();
            }
            get
            {
                return new Vector2(_X, _Y);
            }
        }





        /// <summary>
        /// Little utility to make it easier to do work outside of the UI thread
        /// </summary>
        public void Async(AsyncHandler handler)
        {
            var t = new Thread(new ThreadStart(handler));
            t.Start();
        }

        public delegate void AsyncHandler();

    }


    public enum UIMouseEventType
    {
        MouseOver,
        MouseOut,
        MouseDown,
        MouseUp
    }

    public delegate void UIMouseEvent(UIMouseEventType type, UpdateState state);


    public class UIMouseEventRef
    {
        public UIMouseEvent Callback;
        public Microsoft.Xna.Framework.Rectangle Region;
        public UIElement Element;
        public UIMouseEventType LastState;
    }
}
