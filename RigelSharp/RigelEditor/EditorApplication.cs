﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

using RigelCore;
using RigelEditor.EGUI;

namespace RigelEditor
{
    public class RigelEditorApp :Singleton<RigelEditorApp>
    {

        private RenderForm m_windowForm;
        private EditorGraphicsManager m_graphics;
        private EditorGUICtx m_EGUI;

        public EditorGUICtx EditorGUI { get { return m_EGUI; } }


        public RigelEditorApp()
        {
            m_windowForm = new RenderForm("Rigel");
            m_windowForm.AllowUserResizing = true;

            m_graphics = new EditorGraphicsManager();
            m_graphics.CreateWithSwapChain(m_windowForm);

            m_EGUI = new EditorGUICtx(m_windowForm,m_graphics);

            //test
            //var testwind = RigelEGUIWindow.GetWindow<RigelEditorAboutPage>();
            //var consolewin = RigelEGUIWindow.GetWindow<RigelEditorConsoleWindow>();
        }

        public void EnterRunloop() {

            RenderLoop.Run(m_windowForm, () =>
            {
                m_graphics.Render(()=> {

                    m_EGUI.Render(m_graphics);

                });
            });

            m_EGUI.Dispose();


            m_graphics.Dispose();
            m_windowForm.Dispose();
        }

    }
}
