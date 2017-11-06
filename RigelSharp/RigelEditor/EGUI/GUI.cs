﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace RigelEditor.EGUI
{
    public class GUICtx
    {
        public Vector4 color = RigelColor.White;
        public Vector4 backgroundColor = RigelColor.RGBA(128, 128, 128, 255);

        public Stack<Vector4> groupStack = new Stack<Vector4>();
        public Vector4 baseRect;
        public Vector4 currentRect;

        public Stack<Vector4> areaStack = new Stack<Vector4>();
        public Stack<GUILayoutInfo> layoutStack = new Stack<GUILayoutInfo>();
        public Vector4 currentArea;
        public GUILayoutInfo currentLayout;
        public Vector4 GetNextDrawPos()
        {
            var rect = currentArea;
            rect.X += currentLayout.Offset.X;
            rect.Y += currentLayout.Offset.Y;

            return rect;
        }


        

        public float s_depthStep = 0.0001f;

        public RigelFont Font { get; set; }

        //component
        public Stack<IGUIComponent> componentStack = new Stack<IGUIComponent>();


        public void Frame(RigelEGUIEvent guievent,int width,int height)
        {
            RigelUtility.Assert(groupStack.Count == 0);
            RigelUtility.Assert(areaStack.Count == 0);
            RigelUtility.Assert(layoutStack.Count == 0);

            GUI.Event = guievent;
            baseRect = new Vector4(0, 0, width, height);
            currentRect = baseRect;

            //layout
            currentLayout.Offset = Vector2.Zero;
            currentLayout.SizeMax = Vector2.Zero;
            currentLayout.Verticle = true;

            currentArea = baseRect;
        }

    }

    public class GUIDrawTarget
    {
        public List<RigelEGUIVertex> bufferRect;
        public List<RigelEGUIVertex> bufferText;
        public float depth;

        public GUIDrawTarget(float depth)
        {
            this.depth = depth;
            bufferRect = new List<RigelEGUIVertex>();
            bufferText = new List<RigelEGUIVertex>();
        }
    }

    public struct GUILayoutInfo
    {
        public bool Verticle;
        public Vector2 Offset;
        public Vector2 SizeMax;

        public int LastDrawHeight;
        public int LastDrawWidth;
    }


    public class GUIStackValue<T>
    {
        private Stack<T> m_stack = new Stack<T>();
        public T Value { get; private set; }


        public GUIStackValue(T defaultval) {
            Value = defaultval;
            m_stack.Push(Value);
        }

        public void Set(T v)
        {
            m_stack.Push(Value);
            Value = v;
        }
        public void Restore()
        {
            Value = m_stack.Pop();
        }
    }

    public static class GUI
    {
        public static GUICtx s_ctx;
        public static GUICtx Context {
            get { return s_ctx; }
            set { s_ctx = value; GUILayout.s_ctx = s_ctx; }
        }
        public static GUIDrawTarget DrawTarget { get; private set; }

        public static RigelEGUIEvent Event { get; set; }

        internal static List<RigelEGUIVertex> BufferRect { get { return DrawTarget.bufferRect; } }
        internal static List<RigelEGUIVertex> BufferText { get { return DrawTarget.bufferText; } }

        private static float s_depthz;
        public static float Depth { get { return s_depthz; } }

        public static void BeginGroup(Vector4 rect,Vector4? color = null, bool absulate = false)
        {
           

            var groupStack = s_ctx.groupStack;
            if (absulate)
            {
                if (color != null) DrawRect(rect, (Vector4)color,true);
                s_ctx.currentRect = rect;
                groupStack.Push(rect);
            }
            else
            {
                Vector4 root = s_ctx.currentRect;
                
                rect.X = MathUtil.Clamp(rect.X, 0, root.Z);
                rect.Y = MathUtil.Clamp(rect.Y, 0, root.W);

                rect.Z = MathUtil.Clamp(rect.Z, 0, root.Z - rect.X);
                rect.W = MathUtil.Clamp(rect.W, 0, root.W - rect.Y);

                if (color != null) DrawRect(rect, (Vector4)color);
                s_ctx.currentRect = rect;
                groupStack.Push(rect);
            }
        }

        public static void EndGroup()
        {
            var groupStack = s_ctx.groupStack;
            RigelUtility.Assert(groupStack.Count > 0);
            groupStack.Pop();
            s_ctx.currentRect = groupStack.Count == 0? s_ctx.baseRect: groupStack.Peek();
        }

        public static bool Button(Vector4 rect, string label)
        {
            return Button(rect, label, Context.color, Context.backgroundColor);
        }

        public static bool Button(Vector4 rect,string label,Vector4 color,Vector4 texcolor)
        {
            DrawRect(rect, color);
            DrawText(rect, label, texcolor);
            if (Event.Used) return false;
            if (Event.EventType != RigelEGUIEventType.MouseClick) return false;

            rect.X += s_ctx.currentRect.X;
            rect.Y += s_ctx.currentRect.Y;
            if (RectContainsCheck(rect, Event.Pointer))
            {
                Event.Use();
                return true;
            }
            return false;
        }

        public static void Label(Vector4 position,string text)
        {
            DrawText(position, text, Context.color);
        }

        public static int DrawText(Vector4 rect, string content, Vector4 color)
        {
            RectClip(ref rect, s_ctx.currentRect);
            var w = 0;
            foreach (var c in content)
            {
                int xoff = DrawChar(rect, c, color);
                w += xoff;
                rect.X += xoff;
                if (w > rect.Z) break;
            }

            return w;
        }

        public static int DrawChar(Vector4 rect,uint c,Vector4 color)
        {
            var glyph = Context.Font.GetGlyphInfo(c);
            if (glyph == null) return (int)Context.Font.FontPixelSize;

            float x1 = rect.X + glyph.LineOffsetX;
            float y1 = rect.Y + glyph.LineOffsetY;
            float x2 = x1 + glyph.PixelWidth;
            float y2 = y1 + glyph.PixelHeight;

            BufferText.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(x1, y1, s_depthz, 1),
                Color = color,
                UV = glyph.UV[0]
            });
            BufferText.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(x1, y2, s_depthz, 1),
                Color = color,
                UV = glyph.UV[1]
            });
            BufferText.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(x2, y2, s_depthz, 1),
                Color = color,
                UV = glyph.UV[2]
            });
            BufferText.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(x2, y1, s_depthz, 1),
                Color = color,
                UV = glyph.UV[3]
            });

            DepthIncrease();

            return glyph.AdvancedX;
        }

        public static bool Toggle(Vector4 rect,bool value,string label)
        {
            return value;
        }

        public static void DrawRect(Vector4 rect)
        {
            DrawRect(rect, Context.backgroundColor);
        }

        public static void DrawRect(Vector4 rect,Vector4 color,bool absulate = false)
        {
            var valid = RectClip(ref rect, absulate? s_ctx.baseRect: s_ctx.currentRect);
            if (!valid) return;

            BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X, rect.Y, s_depthz, 1),
                Color = color,
                UV = Vector2.Zero
            });
            BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X, rect.Y + rect.W, s_depthz, 1),
                Color = color,
                UV = Vector2.Zero
            });
            BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X + rect.Z, rect.Y + rect.W, s_depthz, 1),
                Color = color,
                UV = Vector2.Zero
            });
            BufferRect.Add(new RigelEGUIVertex()
            {
                Position = new Vector4(rect.X + rect.Z, rect.Y, s_depthz, 1),
                Color = color,
                UV = Vector2.Zero
            });

            DepthIncrease();
        }

        public static void DrawTexture(Vector4 rect,uint textureid)
        {

        }

        public static string TextField(Vector4 rect,string content)
        {
            return null;
        }

        public static Vector2 BeginScrollView(Vector4 rect,Vector2 pos)
        {
            return pos;
        }

        public static void EndScrollView()
        {

        }

        public static void BeginWindow()
        {

        }

        public static void EndWindow()
        {

        }


        public static void DrawComponent(IGUIComponent component)
        {
            component.InitDrawed = false;
            component.Distroy = false;
            Context.componentStack.Push(component);
            Event.Use();
        }

        #region utility
        internal static void SetDrawTarget(GUIDrawTarget target)
        {
            DrawTarget = target;
            s_depthz = target.depth;
        }
        internal static void DepthIncrease()
        {
            s_depthz -= Context.s_depthStep;
        }

        internal static bool RectClip(ref Vector4 rect, Vector4 group)
        {
            rect.X = MathUtil.Clamp(rect.X, 0, group.Z);
            rect.Y = MathUtil.Clamp(rect.Y, 0, group.W);

            rect.Z = MathUtil.Clamp(rect.Z, 0, group.Z - rect.X);
            rect.W = MathUtil.Clamp(rect.W, 0, group.W - rect.Y);

            rect.X += group.X;
            rect.Y += group.Y;

            if (rect.Z < 1.0f || rect.W < 1.0f) return false;
            return true;
        }

        internal static bool RectContainsCheck(Vector2 pos, Vector2 size, Vector2 point)
        {
            if (point.X < pos.X || point.X > pos.X + size.X) return false;
            if (point.Y < pos.Y || point.Y > pos.Y + size.Y) return false;
            return true;
        }

        internal static bool RectContainsCheck(Vector4 rect, Vector2 point)
        {
            if (point.X < rect.X || point.X > rect.X + rect.Z) return false;
            if (point.Y < rect.Y || point.Y > rect.Y + rect.W) return false;
            return true;
        }
        internal static void SetDepth(float depth)
        {
            s_depthz = depth;
        }

        #endregion
    }

    public static class GUILayout
    {
        //reference to GUI.s_ctx
        public static GUICtx s_ctx;


        private static Stack<GUILayoutInfo> layoutStack { get { return s_ctx.layoutStack; } }

        public static GUIStackValue<int> s_svLineHeight = new GUIStackValue<int>(25);
        public static GUIStackValue<int> s_svLineIndent = new GUIStackValue<int>(5);
        public static int layoutOffX = 1;
        public static int layoutOffY = 1;

        public static void BeginArea(Vector4 rect,Vector4? color = null)
        {
            if(color != null)
            {
                GUI.DrawRect(rect, (Vector4)color);
            }
            s_ctx.areaStack.Push(s_ctx.currentArea);
            s_ctx.currentArea = rect;

            var curlayout = s_ctx.currentLayout;
            layoutStack.Push(curlayout);

            curlayout.Verticle = true;
            curlayout.SizeMax = Vector2.Zero;
            curlayout.Offset = Vector2.Zero;

            s_ctx.currentLayout = curlayout;
        }

        public static void EndArea()
        {
            s_ctx.currentArea = s_ctx.areaStack.Pop();
            s_ctx.currentLayout = s_ctx.layoutStack.Pop();            
        }

        public static void BeginHorizontal()
        {
            s_ctx.currentLayout.Verticle = false;
            s_ctx.layoutStack.Push(s_ctx.currentLayout);
            s_ctx.currentLayout.SizeMax.Y = 0;
        }

        public static void EndHorizontal()
        {
            var playout = s_ctx.layoutStack.Pop();
            if(layoutStack.Count == 0)
            {
                s_ctx.currentLayout.Verticle = true;
            }
            else
            {
                s_ctx.currentLayout.Verticle = layoutStack.Peek().Verticle;
            }
            

            var lastOffset = playout.Offset;

            var off = s_ctx.currentLayout.Offset - lastOffset;
            s_ctx.currentLayout.SizeMax.X = Math.Max(off.X, s_ctx.currentLayout.SizeMax.X);
            s_ctx.currentLayout.Offset.Y += s_ctx.currentLayout.SizeMax.Y > s_svLineHeight.Value ? s_ctx.currentLayout.SizeMax.Y : s_svLineHeight.Value;
            s_ctx.currentLayout.Offset.X = lastOffset.X;
        }

        public static void BeginVertical()
        {
            s_ctx.currentLayout.Verticle = true;
            layoutStack.Push(s_ctx.currentLayout);
            s_ctx.currentLayout.SizeMax.X = 0;
        }

        public static void EndVertical()
        {
            var playout = layoutStack.Pop();
            s_ctx.currentLayout.Verticle = layoutStack.Peek().Verticle;
            var lastOffset = playout.Offset;

            var off = s_ctx.currentLayout.Offset - lastOffset;

            s_ctx.currentLayout.SizeMax.Y = Math.Max(off.Y, s_ctx.currentLayout.SizeMax.Y);

            s_ctx.currentLayout.Offset.X += s_ctx.currentLayout.SizeMax.X > 5f ? s_ctx.currentLayout.SizeMax.X : 5f;
            s_ctx.currentLayout.Offset.Y = lastOffset.Y;
        }

        public static void Space()
        {
            s_ctx.currentLayout.Offset.Y += s_svLineHeight.Value;
        }
        public static void Space(int height)
        {
            s_ctx.currentLayout.Offset.Y += height;
        }

        public static void Indent()
        {
            s_ctx.currentLayout.Offset.X += s_svLineIndent.Value;
        }
        public static void Indent(int width)
        {
            s_ctx.currentLayout.Offset.X += width;
        }


        internal static void AutoCaculateOffset(int w, int h)
        {
            var layout = s_ctx.currentLayout;
            if (layout.Verticle)
            {
                layout.Offset.Y += h + layoutOffY;
                layout.SizeMax.X = Math.Max(layout.SizeMax.X, w);
            }
            else
            {
                layout.Offset.X += w + layoutOffX;
                layout.SizeMax.Y = Math.Max(layout.SizeMax.Y, h);
            }
            layout.LastDrawWidth = w;
            layout.LastDrawHeight = h;


            s_ctx.currentLayout = layout;
        }
        internal static void AutoCaculateOffsetW(int w)
        {
            AutoCaculateOffset(w, s_svLineHeight.Value);
        }
        internal static void AutoCaculateOffsetH(int h)
        {
            AutoCaculateOffset(s_svLineIndent.Value, h);
        }

        public static bool Button(string label)
        {
            var curarea = s_ctx.currentArea;
            var rect = new Vector4(s_ctx.currentLayout.Offset, 50f, s_svLineHeight.Value);
            rect.X += curarea.X;
            rect.Y += curarea.Y;

            var ret = GUI.Button(rect, label);
            AutoCaculateOffsetW(50);

            return ret;
        }


        public static void Text(string content)
        {
            var rect = new Vector4(s_ctx.currentLayout.Offset, 400, s_svLineHeight.Value);
            rect.X += s_ctx.currentArea.X;
            rect.Y += s_ctx.currentArea.Y;
            var width = GUI.DrawText(rect, content, s_ctx.color);

            AutoCaculateOffsetW(width);
        }

        public static void BeginToolBar(int height)
        {
            SetLineHeight(height);
            var rect = new Vector4(0, 0, s_ctx.currentArea.Z, height);
            GUI.DrawRect(rect, GUIStyle.Current.MainMenuBGColor);
            BeginHorizontal();

        }

        public static void EndToolBar()
        {
            EndHorizontal();
            RestoreLineHeight();
        }

        public static void SetLineHeight(int height)
        {
            s_svLineHeight.Set(height);
        }
        public static void RestoreLineHeight()
        {
            s_svLineHeight.Restore();
        }

        public static void DrawMenuList(GUIMenuList menulist)
        {
            var pos = s_ctx.GetNextDrawPos();
            if (Button(menulist.Label))
            {
                pos.Y += s_ctx.currentLayout.LastDrawHeight+1;
                menulist.InternalSetStartPos(pos);
                GUI.DrawComponent(menulist);
            }
        }

        public static void DrawRect(Vector4 rect,Vector4 color)
        {
            GUI.RectClip(ref rect, s_ctx.currentArea);
            GUI.DrawRect(rect, color);
        }

    }
}
