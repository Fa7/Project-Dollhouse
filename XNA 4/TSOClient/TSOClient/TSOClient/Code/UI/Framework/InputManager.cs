﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using TSOClient.Code.UI.Model;
using System.Runtime.InteropServices;

namespace TSOClient.Code.UI.Framework
{
    


    public class InputManager
    {
        private IFocusableUI LastFocus;

        public void SetFocus(IFocusableUI ui)
        {
            /** No change **/
            if (ui == LastFocus) { return; }

            if (LastFocus != null)
            {
                LastFocus.OnFocusChanged(FocusEvent.FocusOut);
            }

            LastFocus = ui;
            if (ui != null)
            {
                LastFocus.OnFocusChanged(FocusEvent.FocusIn);
            }
        }


        [DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);

        /// <summary>
        /// Utility to apply the result of pressing keys against a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="keys"></param>
        public KeyboardInputResult ApplyKeyboardInput(StringBuilder m_SBuilder, UpdateState state, int cursorIndex, int cursorEndIndex, bool allowInput)
        {
            var PressedKeys = state.KeyboardState.GetPressedKeys();
            if (PressedKeys.Length == 0) { return null; }

            var didChange = false;
            var result = new KeyboardInputResult();


            var m_CurrentKeyState = state.KeyboardState;
            var m_OldKeyState = state.PreviousKeyboardState;

            result.ShiftDown = PressedKeys.Contains(Keys.LeftShift) || PressedKeys.Contains(Keys.RightShift);
            result.CapsDown = System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
            result.NumLockDown = System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.NumLock);
            result.CtrlDown = PressedKeys.Contains(Keys.LeftControl) || PressedKeys.Contains(Keys.RightControl);





            for (int j = 0; j < state.NewKeys.Count; j++)
            {
                var key = state.NewKeys[j];

                if (key == Keys.Back || key == Keys.Delete)
                {
                    if (m_SBuilder.Length > 0)
                    {
                        /**
                         * Delete previous character or delete selection
                         */
                        if (cursorEndIndex == -1 && result.CtrlDown)
                        {
                            /** Delete up until the previous whitespace char **/
                            int newEndIndex = cursorIndex;
                            if (newEndIndex == -1)
                            {
                                newEndIndex = m_SBuilder.Length - 1;
                            }
                            while (newEndIndex >= 0)
                            {
                                if (Char.IsWhiteSpace(m_SBuilder[newEndIndex]))
                                {
                                    /** Keep the whitespace char **/
                                    newEndIndex++;
                                    break;
                                }
                                newEndIndex--;
                            }
                            cursorEndIndex = newEndIndex;
                        }

                        if (cursorEndIndex == -1)
                        {
                            /** Previous character **/
                            var index = cursorIndex == -1 ? m_SBuilder.Length : cursorIndex;
                            var numToDelete = 1;
                            if (index > 1 && index < m_SBuilder.Length && m_SBuilder[index] == '\n' && m_SBuilder[index - 1] == '\r')
                            {
                                numToDelete = 2;
                            }

                            m_SBuilder.Remove(index - numToDelete, numToDelete);
                            result.NumDeletes += numToDelete;

                            if (cursorIndex != -1)
                            {
                                cursorIndex -= numToDelete;
                            }
                        }
                        else
                        {
                            DeleteSelectedText(m_SBuilder, ref cursorIndex, ref cursorEndIndex, ref didChange, result);
                        }
                        result.SelectionChanged = true;
                        didChange = true;
                    }
                }
                else if (key == Keys.Enter)
                {
                    if (allowInput)
                    {
                        /** Line break **/
                        if (cursorEndIndex != -1)
                        {
                            /** Delete selected text **/
                            DeleteSelectedText(m_SBuilder, ref cursorIndex, ref cursorEndIndex, ref didChange, result);
                        }

                        if (cursorIndex == -1)
                        {
                            m_SBuilder.Append("\r\n");
                        }
                        else
                        {
                            m_SBuilder.Insert(cursorIndex, "\r\n");
                            cursorIndex += 2;
                        }
                        result.NumInsertions += 2;
                        didChange = true;
                    }
                }
                else
                {
                    if (result.CtrlDown)
                    {
                        switch (key)
                        {
                            case Keys.A:
                                /** Select all **/
                                cursorIndex = 0;
                                cursorEndIndex = m_SBuilder.Length;
                                result.SelectionChanged = true;
                                break;

                            case Keys.C:
                            case Keys.X:
                                /** Copy text to clipboard **/
                                if (cursorEndIndex != -1)
                                {
                                    var selectionStart = cursorIndex;
                                    var selectionEnd = cursorEndIndex;
                                    GetSelectionRange(ref selectionStart, ref selectionEnd);

                                    var str = m_SBuilder.ToString().Substring(selectionStart, selectionEnd - selectionStart);
                                    System.Windows.Forms.Clipboard.SetText(str);

                                    if (key == Keys.X)
                                    {
                                        DeleteSelectedText(m_SBuilder, ref cursorIndex, ref cursorEndIndex, ref didChange, result);
                                    }
                                }
                                break;

                            case Keys.V:
                                /** Paste text in **/
                                var clipboardText = System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.Text);
                                if (clipboardText != null)
                                {
                                    /** TODO: Cleanup the clipboard text to make sure its valid **/

                                    /** If i have selection, delete it **/
                                    if (cursorEndIndex != -1)
                                    {
                                        DeleteSelectedText(m_SBuilder, ref cursorIndex, ref cursorEndIndex, ref didChange, result);
                                    }

                                    /** Paste **/
                                    if (cursorIndex == -1)
                                    {
                                        m_SBuilder.Append(clipboardText);
                                    }
                                    else
                                    {
                                        m_SBuilder.Insert(cursorIndex, clipboardText);
                                        cursorIndex += clipboardText.Length;
                                    }
                                }
                                break;
                        }
                        continue;
                    }


                    char value = TranslateChar(key, result.ShiftDown, result.CapsDown, result.NumLockDown);
                    /** For now we dont support tabs in text **/
                    if (value != '\0' && value != '\t')
                    {
                        if (allowInput)
                        {
                            if (cursorEndIndex != -1)
                            {
                                /** Delete selected text **/
                                DeleteSelectedText(m_SBuilder, ref cursorIndex, ref cursorEndIndex, ref didChange, result);
                            }




                            if (cursorIndex == -1)
                            {
                                m_SBuilder.Append(value);
                            }
                            else
                            {
                                m_SBuilder.Insert(cursorIndex, value);
                                cursorIndex++;
                            }
                            result.NumInsertions++;
                            didChange = true;
                        }
                    }
                    else
                    {
                        result.UnhandledKeys.Add(key);
                    }
                }

            }

            result.SelectionStart = cursorIndex;
            result.SelectionEnd = cursorEndIndex;


            result.ContentChanged = didChange;
            return result;
        }


        private void DeleteSelectedText(StringBuilder m_SBuilder, ref int cursorIndex, ref int cursorEndIndex, ref bool didChange, KeyboardInputResult result)
        {
            /** Remove selected text **/
            var index = cursorIndex == -1 ? m_SBuilder.Length : cursorIndex;
            var end = cursorEndIndex;
            if (end < index)
            {
                var temp = index;
                index = end;
                end = temp;
            }
            m_SBuilder.Remove(index, end - index);

            cursorIndex = index;
            if (cursorIndex >= m_SBuilder.Length)
            {
                cursorIndex = -1;
            }
            cursorEndIndex = -1;
            result.SelectionChanged = true;
            didChange = true;
        }






        public void GetSelectionRange(ref int start, ref int end)
        {
            if (end < start)
            {
                var temp = start;
                start = end;
                end = temp;
            }
        }


        public static char TranslateChar(Keys key, bool shift, bool capsLock, bool numLock)
        {

            switch (key)
            {

                case Keys.A: return TranslateAlphabetic('a', shift, capsLock);

                case Keys.B: return TranslateAlphabetic('b', shift, capsLock);

                case Keys.C: return TranslateAlphabetic('c', shift, capsLock);

                case Keys.D: return TranslateAlphabetic('d', shift, capsLock);

                case Keys.E: return TranslateAlphabetic('e', shift, capsLock);

                case Keys.F: return TranslateAlphabetic('f', shift, capsLock);

                case Keys.G: return TranslateAlphabetic('g', shift, capsLock);

                case Keys.H: return TranslateAlphabetic('h', shift, capsLock);

                case Keys.I: return TranslateAlphabetic('i', shift, capsLock);

                case Keys.J: return TranslateAlphabetic('j', shift, capsLock);

                case Keys.K: return TranslateAlphabetic('k', shift, capsLock);

                case Keys.L: return TranslateAlphabetic('l', shift, capsLock);

                case Keys.M: return TranslateAlphabetic('m', shift, capsLock);

                case Keys.N: return TranslateAlphabetic('n', shift, capsLock);

                case Keys.O: return TranslateAlphabetic('o', shift, capsLock);

                case Keys.P: return TranslateAlphabetic('p', shift, capsLock);

                case Keys.Q: return TranslateAlphabetic('q', shift, capsLock);

                case Keys.R: return TranslateAlphabetic('r', shift, capsLock);

                case Keys.S: return TranslateAlphabetic('s', shift, capsLock);

                case Keys.T: return TranslateAlphabetic('t', shift, capsLock);

                case Keys.U: return TranslateAlphabetic('u', shift, capsLock);

                case Keys.V: return TranslateAlphabetic('v', shift, capsLock);

                case Keys.W: return TranslateAlphabetic('w', shift, capsLock);

                case Keys.X: return TranslateAlphabetic('x', shift, capsLock);

                case Keys.Y: return TranslateAlphabetic('y', shift, capsLock);

                case Keys.Z: return TranslateAlphabetic('z', shift, capsLock);



                case Keys.D0: return (shift) ? ')' : '0';

                case Keys.D1: return (shift) ? '!' : '1';

                case Keys.D2: return (shift) ? '@' : '2';

                case Keys.D3: return (shift) ? '#' : '3';

                case Keys.D4: return (shift) ? '$' : '4';

                case Keys.D5: return (shift) ? '%' : '5';

                case Keys.D6: return (shift) ? '^' : '6';

                case Keys.D7: return (shift) ? '&' : '7';

                case Keys.D8: return (shift) ? '*' : '8';

                case Keys.D9: return (shift) ? '(' : '9';



                case Keys.Add: return '+';

                case Keys.Divide: return '/';

                case Keys.Multiply: return '*';

                case Keys.Subtract: return '-';



                case Keys.Space: return ' ';

                case Keys.Tab: return '\t';



                case Keys.Decimal: if (numLock && !shift) return '.'; break;

                case Keys.NumPad0: if (numLock && !shift) return '0'; break;

                case Keys.NumPad1: if (numLock && !shift) return '1'; break;

                case Keys.NumPad2: if (numLock && !shift) return '2'; break;

                case Keys.NumPad3: if (numLock && !shift) return '3'; break;

                case Keys.NumPad4: if (numLock && !shift) return '4'; break;

                case Keys.NumPad5: if (numLock && !shift) return '5'; break;

                case Keys.NumPad6: if (numLock && !shift) return '6'; break;

                case Keys.NumPad7: if (numLock && !shift) return '7'; break;

                case Keys.NumPad8: if (numLock && !shift) return '8'; break;

                case Keys.NumPad9: if (numLock && !shift) return '9'; break;



                case Keys.OemBackslash: return shift ? '|' : '\\';

                case Keys.OemCloseBrackets: return shift ? '}' : ']';

                case Keys.OemComma: return shift ? '<' : ',';

                case Keys.OemMinus: return shift ? '_' : '-';

                case Keys.OemOpenBrackets: return shift ? '{' : '[';

                case Keys.OemPeriod: return shift ? '>' : '.';

                case Keys.OemPipe: return shift ? '|' : '\\';

                case Keys.OemPlus: return shift ? '+' : '=';

                case Keys.OemQuestion: return shift ? '?' : '/';

                case Keys.OemQuotes: return shift ? '"' : '\'';

                case Keys.OemSemicolon: return shift ? ':' : ';';

                case Keys.OemTilde: return shift ? '~' : '`';

            }



            return (char)0;

        }



        public static char TranslateAlphabetic(char baseChar, bool shift, bool capsLock)
        {
            return (capsLock ^ shift) ? char.ToUpper(baseChar) : baseChar;
        }





        /// <summary>
        /// Mouse event code, ensures depth is considered for mouse events
        /// </summary>
        

        private UIMouseEventRef LastMouseOver;
        private UIMouseEventRef LastMouseDown;
        private bool LastMouseDownState = false;


        public void HandleMouseEvents(TSOClient.Code.UI.Model.UpdateState state)
        {
            var mouseBtnDown = state.MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            var mouseDif = mouseBtnDown != LastMouseDownState;
            LastMouseDownState = mouseBtnDown;

            if (mouseDif)
            {
                if (mouseBtnDown)
                {
                    if (LastMouseDown != null)
                    {
                        /** We already have mouse down on an object **/
                        return;
                    }
                    if (LastMouseOver != null)
                    {
                        LastMouseDown = LastMouseOver;
                        LastMouseDown.Callback(UIMouseEventType.MouseDown, state);
                    }
                }
                else
                {
                    if (LastMouseDown != null)
                    {
                        LastMouseDown.Callback(UIMouseEventType.MouseUp, state);
                        LastMouseDown = null;
                    }
                }
            }

            if (state.MouseEvents.Count > 0)
            {
                var topMost =
                    state.MouseEvents.OrderByDescending(x => x.Element.Depth).First();


                /** Same element **/
                if (LastMouseOver == topMost)
                {
                    return;
                }

                if (LastMouseOver != null)
                {
                    LastMouseOver.Callback(UIMouseEventType.MouseOut, state);
                }

                topMost.Callback(UIMouseEventType.MouseOver, state);
                LastMouseOver = topMost;
            }
            else
            {
                if (LastMouseOver != null)
                {
                    LastMouseOver.Callback(UIMouseEventType.MouseOut, state);
                    LastMouseOver = null;
                }
            }

        }

    }


    public class KeyboardInputResult
    {
        public List<Keys> UnhandledKeys = new List<Keys>();
        public bool ContentChanged;
        public bool ShiftDown;
        public bool CapsDown;
        public bool NumLockDown;
        public bool CtrlDown;

        public int NumDeletes;
        public int NumInsertions;

        public int SelectionStart;
        public int SelectionEnd;
        public bool SelectionChanged;
    }
}
