﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace RigelEditor.EGUI
{
    public enum GUIDockPlace
    {
        none,
        center,
        left,
        right,
        top,
        bottom,
    }

    public class GUIDockContentBase
    {
        internal GUIDragState InternalTabBtnDragState = new GUIDragState();
    }


    public abstract class GUIDockBase : IGUIDockObj
    {
        public Vector4 m_size = new Vector4(0, 0, 100, 100);
        public Vector4? m_sizeNext = null;
        public Vector4 m_containerRect;
        public abstract void Draw(Vector4 rect);

        public virtual void LateUpdate()
        {
        }

        public GUIDockGroup m_parent;
    }

    public interface IGUIDockObj
    {
        void Draw(Vector4 rect);
        void LateUpdate();
    }

    public class GUIDockSeparator : IGUIDockObj
    {
        public GUIDockGroup m_parent;
        private bool m_ondrag = false;

        private Vector4 m_rect;
        private Vector4 m_rectab;
        public void Draw(Vector4 rect)
        {

            m_rect = rect;
            m_rectab = GUI.GetRectAbsolute(m_rect);
            if (m_parent.m_orient == GUIDockGroup.GUIDockOrient.Horizontal)
            {
                m_rectab.X -= 2;
                m_rectab.Z += 4;
            }
            else
            {
                m_rectab.Y -= 2;
                m_rectab.W += 4;
            }
            GUI.DrawRect(rect, GUIStyle.Current.DockSepColor);
            CheckDrag();
        }

        public void LateUpdate()
        {

        }

        private void CheckDrag()
        {
            if (GUI.Event.Used) return;
            var e = GUI.Event;
            if (e.EventType == RigelEGUIEventType.MouseDragEnter)
            {
                if (GUIUtility.RectContainsCheck(m_rectab, e.Pointer))
                {
                    m_ondrag = true;
                    e.Use();
                    GUIInternal.SetCursor(m_parent.m_orient == GUIDockGroup.GUIDockOrient.Horizontal ? System.Windows.Forms.Cursors.VSplit : System.Windows.Forms.Cursors.HSplit);
                }
            }
            else if (e.EventType == RigelEGUIEventType.MouseDragLeave)
            {
                if (m_ondrag)
                {
                    e.Use();
                    m_ondrag = false;

                    GUIInternal.SetCursor(System.Windows.Forms.Cursors.Default);
                }
            }
            else if (e.EventType == RigelEGUIEventType.MouseDragUpdate)
            {
                if (m_ondrag)
                {
                    e.Use();
                    m_parent.SeparatorMove(e.DragOffset, this);
                }
            }

        }
    }

    public class GUIDock : GUIDockBase
    {
        public object m_target;
        public List<GUIDockContentBase> m_content = new List<GUIDockContentBase>();
        private GUIDockContentBase m_focus = null;

        private Vector4 m_contentRect;
        private Vector4 m_sizeab;

        private GUIDockPlaceComponent m_guicompPlace;

        public Vector4 SizeAbsolute { get { return m_sizeab; } }


        private string m_debuginfo;

        private EventSlot<Action> m_slotDockPlace = new EventSlot<Action>();


        public GUIDock(string debuginfo = "")
        {
            m_debuginfo = debuginfo;
        }

        public override void Draw(Vector4 rect)
        {
            m_size = rect;
            m_containerRect = m_size;
            m_containerRect = m_containerRect.Padding(1);

            GUI.BeginGroup(m_containerRect, GUIStyle.Current.DockBGColor);
            m_sizeab = GUI.Context.currentGroup.Absolute;
            GUILayout.BeginArea(GUI.Context.currentGroup.Absolute);

            DrawTabBar();

            m_contentRect = GUI.Context.currentGroup.Rect;
            m_contentRect.X = 0;
            m_contentRect.Y = 23;
            m_contentRect.W -= 23;
            GUI.BeginGroup(m_contentRect);
            GUILayout.BeginArea(GUI.Context.currentGroup.Absolute, GUIStyle.Current.DockContentColor);
            GUI.Label(new Vector4(0, 0, 100, 20), "Group");
            GUILayout.Button("Test");
            GUILayout.Text("66666");

            GUILayout.EndArea();

            GUI.EndGroup();
            GUILayout.EndArea();
            GUI.EndGroup();

        }

        public override void LateUpdate()
        {
            m_slotDockPlace.InvokeOnce();
        }

        public void AddDockContent(GUIDockContentBase content)
        {

            m_content.Add(content);
            m_focus = content;
        }


        public void RemoveDockContent(GUIDockContentBase content)
        {
            m_content.Remove(content);
            if (content == m_focus)
            {
                if (m_content.Count != 0)
                {
                    m_focus = m_content[0];
                }
                else
                {
                    m_focus = null;
                }
            }
        }

        private void DrawTabBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.SetLineHeight(20);


            if (GUILayout.Button("+", GUIStyle.Current.TabBtnColorS, GUIOption.Width(20)))
            {
                var win = new GUIWindowTest1();
                AddDockContent(win);

            }

            if (GUILayout.Button("-", GUIStyle.Current.TabBtnColorS, GUIOption.Width(20)))
            {
                if (m_focus != null)
                {
                    RemoveDockContent(m_focus);
                }
            }

            if (m_content.Count == 0)
            {
                GUILayout.Text("None");
            }
            else
            {
                foreach (var c in m_content)
                {
                    GUILayout.Space(1);
                    DrawDockContentButton(c);
                    GUILayout.Space(-1);
                }
            }


            GUILayout.RestoreLineHeight();
            GUILayout.EndHorizontal();
        }

        private void DrawDockContentButton(GUIDockContentBase c)
        {
            var checkrc = GUIOption.Check(GUIOptionCheck.rectContains);
            if (GUILayout.Button("Tab", m_focus == c ? GUIStyle.Current.TabBtnColorActive : GUIStyle.Current.TabBtnColor, checkrc))
            {
                m_focus = c;
            }

            if (c.InternalTabBtnDragState.OnDrag(checkrc.Checked()))
            {
                var root = m_parent;
                while(root.m_parent != null)
                {
                    root = root.m_parent;
                }

                var matchedDock = root.CheckDockMatched(c);
                if(matchedDock == null)
                {
                    //Console.WriteLine("no matched dock");
                }
                else
                {
                    var dockplace = matchedDock.OnDockContentDrag(c);
                    if(dockplace != GUIDockPlace.none)
                    {
                        Console.WriteLine(">>" + dockplace);
                        m_slotDockPlace+=()=> { SetDockPlace(c, matchedDock, dockplace); };
                    }
                }
            }
        }

        private void SetDockPlace(GUIDockContentBase content,GUIDock targetdock,GUIDockPlace place)
        {
            if (place == GUIDockPlace.none) return;

            if(targetdock == this)
            {
                
            }
            else
            {
                this.RemoveDockContent(content);

                switch (place)
                {
                    case GUIDockPlace.center:
                        targetdock.AddDockContent(content);
                        break;
                    case GUIDockPlace.left:
                        if(targetdock.m_parent.m_orient == GUIDockGroup.GUIDockOrient.Horizontal)
                        {
                            var newdock = targetdock.m_parent.InsertDockBefore(targetdock);
                            newdock.AddDockContent(content);
                        }
                        else
                        {

                        }

                        break;
                    case GUIDockPlace.right:
                        if(targetdock.m_parent.m_orient == GUIDockGroup.GUIDockOrient.Horizontal)
                        {
                            var newdock = targetdock.m_parent.InsertDockAfter(targetdock);
                            newdock.AddDockContent(content);
                        }
                        break;
                }

                this.m_parent.RefreshDock();
            }
        }



        public bool ContainsContent(GUIDockContentBase content)
        {
            return m_content.Contains(content);
        }

        public GUIDockPlace OnDockContentDrag(GUIDockContentBase content)
        {
            GUI.BeginGroup(m_sizeab, null, true);

            GUI.BeginDepthLayer(1);

            float rsize = 40;

            var center = m_sizeab.Size() * 0.5f;
            var rectbasic = new Vector4(center - rsize*0.5f* Vector2.One, rsize, rsize);

            bool activeChecked = false;

            GUIDockPlace dockPlace = GUIDockPlace.none;


            //center
            if (!activeChecked && GUIUtility.RectContainsCheck(GUI.GetRectAbsolute(rectbasic), GUI.Event.Pointer))
            {
                GUI.DrawRect(rectbasic, GUIStyle.Current.ColorActive);
                activeChecked = true;
                dockPlace = GUIDockPlace.center;
            }
            else
            {
                GUI.DrawRect(rectbasic, GUIStyle.Current.ColorActiveD);
            }

            //left
            var rect = rectbasic.Move(-35, 0).SetSize(30, rsize);
            if (!activeChecked && GUIUtility.RectContainsCheck(GUI.GetRectAbsolute(rect), GUI.Event.Pointer))
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActive);
                activeChecked = true;
                dockPlace = GUIDockPlace.left;
            }
            else
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActiveD);
            }

            //right
            rect = rectbasic.Move(45, 0).SetSize(30, rsize);
            if (!activeChecked && GUIUtility.RectContainsCheck(GUI.GetRectAbsolute(rect), GUI.Event.Pointer))
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActive);
                activeChecked = true;
                dockPlace = GUIDockPlace.right;
            }
            else
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActiveD);
            }

            //top
            rect = rectbasic.Move(0, -35).SetSize(rsize, 30);
            if (!activeChecked && GUIUtility.RectContainsCheck(GUI.GetRectAbsolute(rect), GUI.Event.Pointer))
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActive);
                activeChecked = true;
                dockPlace = GUIDockPlace.top;
            }
            else
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActiveD);
            }

            //bottom
            rect = rectbasic.Move(0, 45).SetSize(rsize, 30);
            if (!activeChecked && GUIUtility.RectContainsCheck(GUI.GetRectAbsolute(rect), GUI.Event.Pointer))
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActive);
                activeChecked = true;
                dockPlace = GUIDockPlace.bottom;
            }
            else
            {
                GUI.DrawRect(rect, GUIStyle.Current.ColorActiveD);
            }

            GUI.EndDepthLayer();

            GUI.EndGroup();

            if(content.InternalTabBtnDragState.Stage != GUIDrawStateStage.Exit)
            {
                return GUIDockPlace.none;
            }

            return dockPlace;
        }

        private class GUIDockPlaceComponent : IGUIComponent
        {
            private Vector4 m_rect;

            public void Update(Vector4 rect)
            {
                m_rect = rect;
            }

            public override void Draw(RigelEGUIEvent guievent)
            {
                GUI.BeginGroup(m_rect, null, true);

                GUI.BeginDepthLayer(1);
                GUI.Button(new Vector4(100, 100, 20, 20), "=");
                GUI.EndDepthLayer();
                GUI.EndGroup();
            }
        }
    }

    public class GUIDockGroup : GUIDockBase
    {
        private static float SizeMin = 30;
        private string m_debugName;
        private bool m_isroot = false;
        
        internal bool IsRoot { get { return m_isroot; } }

        public enum GUIDockOrient
        {
            Horizontal,
            Vertical
        };

        public List<IGUIDockObj> m_children = new List<IGUIDockObj>();
        public GUIDockOrient m_orient = GUIDockOrient.Horizontal;


        public GUIDockGroup(bool root = false,string debugname = null)
        {
            m_isroot = root;
            m_debugName = debugname;
        }

        public override void Draw(Vector4 size)
        {
            m_size = size;
            GUI.BeginGroup(m_size);
            ReiszeChild();
            GUI.EndGroup();
        }



        private void ReiszeChild()
        {
            float stotal = 0;
            float stepSize = 0;
            foreach (var c in m_children)
            {
                if (c is GUIDockSeparator) continue;

                var dock = c as GUIDockBase;
                if (dock.m_sizeNext != null)
                {
                    dock.m_size = (Vector4)dock.m_sizeNext;
                    dock.m_sizeNext = null;
                }
                if (m_orient == GUIDockOrient.Horizontal)
                {
                    stotal += dock.m_size.Z;
                }
                else
                {
                    stotal += dock.m_size.W;
                }
            }

            Vector4 crect = Vector4.Zero;
            if (m_orient == GUIDockOrient.Horizontal)
            {
                crect.W = m_size.W;
                stepSize = m_size.Z / stotal;
            }
            else
            {
                crect.Z = m_size.Z;
                stepSize = m_size.W / stotal;
            }

            int count = 0;
            int total = m_children.Count;
            foreach (var c in m_children)
            {
                if (c is GUIDockBase)
                {
                    var dock = c as GUIDockBase;

                    if (m_orient == GUIDockOrient.Horizontal)
                    {
                        crect.Z = dock.m_size.Z * stepSize;
                        dock.Draw(crect);
                        crect.X += dock.m_size.Z;
                    }
                    else
                    {
                        crect.W = dock.m_size.W * stepSize;
                        dock.Draw(crect);
                        crect.Y += dock.m_size.W;
                    }
                }
                else
                {
                    //seperator
                    if (count < total - 1)
                    {
                        if (m_orient == GUIDockOrient.Horizontal)
                        {
                            crect.X -= 1;
                            crect.Z = 2;
                            c.Draw(crect);
                            crect.X += 1;
                        }
                        else
                        {
                            crect.Y -= 1;
                            crect.W = 2;
                            c.Draw(crect);
                            crect.Y += 1;
                        }
                    }
                }

                count++;
            }
        }

        public void AddDock(GUIDockBase dock)
        {
            if (m_children.Count != 0)
            {
                var sep = new GUIDockSeparator();
                sep.m_parent = this;
                m_children.Add(sep);
            }
            m_children.Add(dock);
            dock.m_parent = this;
        }

        public GUIDock InsertDockBefore(GUIDockBase target)
        {
            int index = GetDockIndex(target);

            var sep = new GUIDockSeparator();
            sep.m_parent = this;
            m_children.Insert(index, sep);

            var dock = new GUIDock();
            dock.m_parent = this;
            m_children.Insert(index, dock);

            return dock;
        }

        public GUIDock InsertDockAfter(GUIDockBase target)
        {
            int index = GetDockIndex(target);

            var sep = new GUIDockSeparator();
            sep.m_parent = this;
            m_children.Insert(index+1, sep);

            var dock = new GUIDock();
            dock.m_parent = this;
            m_children.Insert(index +2, dock);

            return dock;
        }

        public int GetDockIndex(GUIDockBase dock)
        {
            for(int i = 0; i < m_children.Count; i++)
            {
                if (m_children[i] == dock) return i;
            }
            throw new Exception("Dock Not Found");
        }

        public void SeparatorMove(Vector2 offset, GUIDockSeparator sep)
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                if (m_children[i] == sep)
                {
                    var predock = m_children[i - 1] as GUIDockBase;
                    var nextdock = m_children[i + 1] as GUIDockBase;

                    var rect1 = predock.m_size;
                    var rect2 = nextdock.m_size;
                    if (m_orient == GUIDockOrient.Horizontal)
                    {
                        float off = offset.X;
                        if (rect1.Z + off < GUIDockGroup.SizeMin)
                        {
                            off = GUIDockGroup.SizeMin - rect1.Z;
                        }
                        if (rect2.Z - off < GUIDockGroup.SizeMin)
                        {
                            off = rect2.Z - GUIDockGroup.SizeMin;
                        }

                        rect1.Z += off;
                        rect2.Z -= off;

                        predock.m_sizeNext = rect1;
                        nextdock.m_sizeNext = rect2;
                    }
                    else
                    {
                        float off = offset.Y;
                        if (rect1.W + off < GUIDockGroup.SizeMin)
                        {
                            off = GUIDockGroup.SizeMin - rect1.W;
                        }
                        if (rect2.W - off < GUIDockGroup.SizeMin)
                        {
                            off = rect2.W - GUIDockGroup.SizeMin;
                        }

                        rect1.W += off;
                        rect2.W -= off;

                        predock.m_sizeNext = rect1;
                        nextdock.m_sizeNext = rect2;
                    }
                }
            }
        }


        public GUIDock CheckDockMatched(GUIDockContentBase content)
        {
            foreach(var d in m_children)
            {
                if(d is GUIDock)
                {
                    var dock = d as GUIDock;
                    if (GUIUtility.RectContainsCheck(dock.SizeAbsolute, GUI.Event.Pointer))
                    {
                        return dock;
                    }
                }
                else if(d is GUIDockGroup)
                {
                    var dock = (d as GUIDockGroup).CheckDockMatched(content);
                    if (dock != null) return dock;
                }
            }

            return null;
        }

        public void RefreshDock()
        {
            for(int i= m_children.Count -1; i>=0; i--)
            {
                var c = m_children[i];
                if(c is GUIDock)
                {
                    var dock = c as GUIDock;
                    if(dock.m_content.Count == 0)
                    {
                        m_children.Remove(c);
                    }
                }
                else if(c is GUIDockGroup)
                {
                    var dockgroup = c as GUIDockGroup;
                    if(dockgroup.m_children.Count == 0)
                    {
                        m_children.Remove(c);
                    }
                }

            }

            int dockCount = 0;

            //remove seperator
            bool lastSep = true;
            for(int i = m_children.Count-1; i >= 0; i--)
            {
                var c = m_children[i];
                if(c is GUIDockSeparator)
                {
                    if (lastSep)
                    {
                        m_children.RemoveAt(i);
                    }
                    lastSep = true;
                }
                else
                {
                    lastSep = false;
                    dockCount++;
                }
            }

            if(dockCount == 0)
            {
                m_children.Clear();
                if (IsRoot)
                {
                    AddDock(new GUIDock());
                }
                else
                {
                    if(m_parent != null)
                        m_parent.RefreshDock();
                }
            }
        }

        public override void LateUpdate()
        {
            for(int i = m_children.Count - 1; i >= 0; i--)
            {
                if(i < m_children.Count)
                    m_children[i].LateUpdate();
            }
        }


    }



    public class GUIDockManager
    {
        private GUIDockGroup m_maingroup;

        public GUIDockManager()
        {
            m_maingroup = new GUIDockGroup();
            m_maingroup.AddDock(new GUIDock());

            var group1 = new GUIDockGroup();
            group1.AddDock(new GUIDock());
            group1.AddDock(new GUIDock());

            m_maingroup.AddDock(group1);
        }

        public void Update(Vector4 group)
        {
            GUI.BeginGroup(group, GUIStyle.Current.BorderColor);
            group.X = 0;
            group.Y = 0;
            m_maingroup.Draw(group);
            GUI.EndGroup();
        }

        public void LateUpdate()
        {
            m_maingroup.LateUpdate();
        }
    }
}
