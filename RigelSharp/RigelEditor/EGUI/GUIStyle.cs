﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace RigelEditor.EGUI
{
    public class GUIStyle
    {
        public static GUIStyle Current { get { return m_currentStyle; } }
        private static GUIStyle m_currentStyle = new GUIStyle();

        public Vector4 WinBGColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        public Vector4 WinBGColorFocused = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
        public Vector4 WinHeaderColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        public Vector4 WinHeaderColorFocused = RigelColor.RGBA(0, 122, 204, 255);
        public float WinHeaderHeight = 25f;

        public Vector4 MainMenuBGColor = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
        public Vector4 MainStatusBarColor = RigelColor.RGBA(0, 122, 204, 255);

        public Vector4 ButtonColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);

        public Vector4 DockBGColor =RigelColor.RGBA(30, 30, 30,255);
        public Vector4 DockContentColor = RigelColor.RGBA(50, 50, 50, 255);
        public Vector4 DockSepColor = RigelColor.RGBA(30, 30, 30, 255);
        public Vector4 BorderColor = RigelColor.RGBA(63, 63, 70, 255);

        public Vector4 TabBtnColor = RigelColor.RGBA(30, 30, 30, 255);
        public Vector4 TabBtnColorActive = RigelColor.RGBA(50, 50, 50, 255);


    }
}
