﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using GTA;
using GTA.Native;

namespace RageCoop.Client
{
    internal class Chat
    {
        private readonly Scaleform MainScaleForm;

        public Chat()
        {
            MainScaleForm = new Scaleform("multiplayer_chat");
        }

        public string CurrentInput { get; set; }

        private bool CurrentFocused { get; set; }

        public bool Focused
        {
            get => CurrentFocused;
            set
            {
                if (value && Hidden) Hidden = false;

                MainScaleForm.CallFunction("SET_FOCUS", value ? 2 : 1, 2, "ALL");

                CurrentFocused = value;
            }
        }

        private ulong LastMessageTime { get; set; }

        private bool CurrentHidden { get; set; }

        private bool Hidden
        {
            get => CurrentHidden;
            set
            {
                if (value)
                {
                    if (!CurrentHidden) MainScaleForm.CallFunction("hide");
                }
                else if (CurrentHidden)
                {
                    MainScaleForm.CallFunction("showFeed");
                }

                CurrentHidden = value;
            }
        }

        public void Init()
        {
            MainScaleForm.CallFunction("SET_FOCUS", 2, 2, "ALL");
            MainScaleForm.CallFunction("SET_FOCUS", 1, 2, "ALL");
        }

        public void Clear()
        {
            MainScaleForm.CallFunction("RESET");
        }

        public void Tick()
        {
            if (Util.GetTickCount64() - LastMessageTime > 15000 && !Focused && !Hidden) Hidden = true;

            if (!Hidden) MainScaleForm.Render2D();

            if (!CurrentFocused) return;

            Call(DISABLE_ALL_CONTROL_ACTIONS, 0);
        }

        public void AddMessage(string sender, string msg)
        {
            MainScaleForm.CallFunction("ADD_MESSAGE", sender + ":", msg);
            LastMessageTime = Util.GetTickCount64();
            Hidden = false;
        }

        public void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                Focused = false;
                CurrentInput = "";
                return;
            }

            if (key == Keys.PageUp)
                MainScaleForm.CallFunction("PAGE_UP");
            else if (key == Keys.PageDown) MainScaleForm.CallFunction("PAGE_DOWN");

            var keyChar = GetCharFromKey(key, Game.IsKeyPressed(Keys.ShiftKey), false);

            if (keyChar.Length == 0) return;

            switch (keyChar[0])
            {
                case (char)8:
                    if (CurrentInput?.Length > 0)
                    {
                        CurrentInput = CurrentInput.Remove(CurrentInput.Length - 1);
                        MainScaleForm.CallFunction("DELETE_TEXT");
                    }

                    return;
                case (char)13:
                    MainScaleForm.CallFunction("ADD_TEXT", "ENTER");

                    if (!string.IsNullOrWhiteSpace(CurrentInput)) Networking.SendChatMessage(CurrentInput);

                    Focused = false;
                    CurrentInput = "";
                    return;
                default:
                    CurrentInput += keyChar;
                    MainScaleForm.CallFunction("ADD_TEXT", keyChar);
                    return;
            }
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(uint virtualKeyCode, uint scanCode, byte[] keyboardState,
            [Out] [MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
            StringBuilder receivingBuffer,
            int bufferSize, uint flags, IntPtr kblayout);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        public static string GetCharFromKey(Keys key, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];

            if (shift) keyboardState[(int)Keys.ShiftKey] = 0xff;

            if (altGr)
            {
                keyboardState[(int)Keys.ControlKey] = 0xff;
                keyboardState[(int)Keys.Menu] = 0xff;
            }

            ToUnicodeEx((uint)key, 0, keyboardState, buf, 256, 0, GetKeyboardLayout(0));
            return buf.ToString();
        }
    }
}