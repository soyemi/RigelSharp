﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace RigelEditor.EGUI
{
    public static class RigelEGUI
    {
        internal static RigelEGUICtx s_currentCtx = null;
        internal static RigelEGUIWindow s_currentWindow = null;


        public static void DrawRect(Vector4 rect,Vector4 color)
        {
            s_currentCtx.BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X, rect.Y, 0, 1),
                Color = color,
                UV = Vector2.Zero
            });
            s_currentCtx.BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X, rect.Y + rect.W, 0, 1),
                Color = color,
                UV = Vector2.Zero
            });
            s_currentCtx.BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X +rect.Z, rect.Y + rect.W, 0, 1),
                Color = color,
                UV = Vector2.Zero
            });
            s_currentCtx.BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X + rect.Z, rect.Y, 0, 1),
                Color = color,
                UV = Vector2.Zero
            });
        }

        public static void DrawText(Vector4 rect,string content)
        {

        }




        #region utility

        internal static bool RectContainsCheck(Vector2 pos,Vector2 size,Vector2 point)
        {
            if (point.X < pos.X || point.X > pos.X + size.X) return false;
            if (point.Y < pos.Y || point.Y > pos.Y + size.Y) return false;
            return true;
        }


        #endregion
    }


    

    


}
