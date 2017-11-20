﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RigelEditor.EGUI
{
    public abstract class GUIWindow : GUIDockContentBase
    {
    }


    public class GUIWindowTest1 : GUIWindow
    {
        public override void OnGUI()
        {
            GUILayout.Button("WindowTest1");
        }
    }

    public class GUIWindowTest2: GUIWindow
    {
        public override void OnGUI()
        {
            GUILayout.Button("WindowTest2");
        }
    }
}
