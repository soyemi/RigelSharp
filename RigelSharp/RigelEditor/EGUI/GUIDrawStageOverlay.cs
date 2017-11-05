﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RigelEditor.EGUI
{
    public class GUIDrawStageOverlay : GUIDrawStage
    {

        private bool m_needSyncBuffer = false;


        private int m_lastBufferSizeRect = 0;
        private int m_lastBufferSizeText = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stagename"></param>
        /// <param name="order"> near(0) ------ far(500) </param>
        public GUIDrawStageOverlay(string stagename, int order = 0): base(stagename, order)
        {

        }

        public override void Draw(RigelEGUIEvent guievent)
        {
            base.Draw(guievent);

            //do draw
            m_needSyncBuffer = false;
            var comp = GUI.Context.componentStack;
            if (comp.Count == 0) return;

            //every draw the last one
            IGUIComponent curcomp = comp.Peek();
            if (curcomp.InitDrawed)
            {
                m_drawTarget.bufferRect.RemoveRange(curcomp.BufferRectStart,curcomp.BufferRectCount);
                m_drawTarget.bufferText.RemoveRange(curcomp.BufferTextStart, curcomp.BufferTextCount);
            }

            curcomp.BufferRectStart = m_drawTarget.bufferRect.Count;
            curcomp.BufferTextStart = m_drawTarget.bufferText.Count;

            curcomp.Draw(guievent);
            guievent.Use();

            curcomp.BufferRectEnd = m_drawTarget.bufferRect.Count;
            curcomp.BufferTextEnd = m_drawTarget.bufferText.Count;

            curcomp.InitDrawed = true;
            m_needSyncBuffer = true;
        }

        public override void SyncBuffer(RigelEGUICtx eguictx)
        {
            if (!m_needSyncBuffer) return;


            var bind = eguictx.GraphicsBind;
            {
                var rectCount = m_drawTarget.bufferRect.Count;
                if(rectCount != 0)
                {
                    var cursize = bind.BufferMainRect.BufferDataCount;
                    if (cursize != m_lastBufferSizeRect) bind.NeedRebuildCommandList = true;
                    var newsize = cursize + rectCount;
                    bind.BufferMainRect.CheckAndExtendsWithSize(newsize);
                    m_drawTarget.bufferRect.CopyTo(0, bind.BufferMainRect.BufferData, cursize, rectCount);
                    bind.BufferMainRect.InternalSetBufferDataCount(newsize);

                    m_lastBufferSizeRect = newsize;
                }
            }

            {
                var textCount = m_drawTarget.bufferText.Count;
                if(textCount != 0)
                {
                    var cursize = bind.BufferMainText.BufferDataCount;
                    if (cursize != m_lastBufferSizeText) bind.NeedRebuildCommandList = true;
                    var newsize = cursize + textCount;
                    bind.BufferMainText.CheckAndExtendsWithSize(newsize);
                    m_drawTarget.bufferText.CopyTo(0, bind.BufferMainText.BufferData, cursize, textCount);
                    bind.BufferMainText.InternalSetBufferDataCount(newsize);

                    m_lastBufferSizeText = newsize;
                }
            }

            //remove bufferdata
            var comp = GUI.Context.componentStack.Peek();
            if (comp.Distroy)
            {
                m_drawTarget.bufferRect.RemoveRange(comp.BufferRectStart, comp.BufferRectCount);
                m_drawTarget.bufferText.RemoveRange(comp.BufferTextStart, comp.BufferTextCount);
                GUI.Context.componentStack.Pop();
            }
        }
    }
}