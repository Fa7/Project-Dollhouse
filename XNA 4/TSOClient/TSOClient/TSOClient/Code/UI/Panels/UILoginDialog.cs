﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOClient.Code.UI.Controls;
using TSOClient.LUI;
using TSOClient.Code.UI.Framework;
using TSOClient.Code.UI.Screens;

namespace TSOClient.Code.UI.Panels
{
    public class UILoginDialog : UIDialog
    {
        private UITextEdit m_TxtAccName, m_TxtPass;
        private LoginScreen m_LoginScreen;

        public UILoginDialog(LoginScreen loginScreen) : base(UIDialogStyle.Standard, true)
        {
            this.m_LoginScreen = loginScreen;
            this.Caption = GameFacade.Strings.GetString("UIText", "209", "1");

            SetSize(350, 225);


            m_TxtAccName = UITextEdit.CreateTextBox();
            m_TxtAccName.X = 20;
            m_TxtAccName.Y = 72;
            m_TxtAccName.MaxChars = 16;
            m_TxtAccName.SetSize(310, 27);
            m_TxtAccName.CurrentText = "username";
            this.Add(m_TxtAccName);


            m_TxtPass = UITextEdit.CreateTextBox();
            m_TxtPass.X = 20;
            m_TxtPass.Y = 128;
            m_TxtPass.MaxChars = 16;
            m_TxtPass.CurrentText = "password";
            m_TxtPass.SetSize(310, 27);
            this.Add(m_TxtPass);


            /** Login button **/
            var loginBtn = new UIButton {
                X = 116,
                Y = 170,
                Width = 100,
                ID = "LoginButton",
                Caption = GameFacade.Strings.GetString("UIText", "209", "2")
            };
            this.Add(loginBtn);
            loginBtn.OnButtonClick += new ButtonClickDelegate(loginBtn_OnButtonClick);

            var exitBtn = new UIButton
            {
                X = 226,
                Y = 170,
                Width = 100,
                ID = "ExitButton",
                Caption = GameFacade.Strings.GetString("UIText", "209", "3")
            };
            this.Add(exitBtn);
            exitBtn.OnButtonClick += new ButtonClickDelegate(exitBtn_OnButtonClick);


            this.Add(new UILabel
            {
                Caption = GameFacade.Strings.GetString("UIText", "209", "4"),
                X = 24,
                Y = 50
            });

            this.Add(new UILabel
            {
                Caption = GameFacade.Strings.GetString("UIText", "209", "5"),
                X = 24,
                Y = 106
            });

        }



        public string Username
        {
            get
            {
                return m_TxtAccName.CurrentText;
            }
        }

        public string Password
        {
            get
            {
                return m_TxtPass.CurrentText;
            }
        }



        void loginBtn_OnButtonClick(UIElement button)
        {
            m_LoginScreen.Login();
            //GameFacade.Controller.ShowPersonSelection();
        }

        void exitBtn_OnButtonClick(UIElement button)
        {
            //var exitDialog = new UIExitDialog();
            //Parent.Add(exitDialog);
        }
    }
}
