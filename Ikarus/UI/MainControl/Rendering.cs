﻿using BrawlLib.OpenGL;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using Ikarus.MovesetFile;
using Ikarus.ModelViewer;

namespace Ikarus.UI
{
    public partial class MainControl : ModelEditorBase
    {
        protected override void ModelPanel_PreRender(ModelPanelViewport sender)
        {
            base.ModelPanel_PreRender(sender);
            Attributes.PreRender();
        }

        protected override void ModelPanel_PostRender(ModelPanelViewport sender)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            Attributes.PostRender();

            //Render hurtboxes
            if (chkHurtboxes.Checked)
                for (int i = 0; i < listPanel.lstHurtboxes.Items.Count; i++)
                    if (listPanel.lstHurtboxes.GetItemChecked(i))
                        ((MiscHurtBox)listPanel.lstHurtboxes.Items[i]).Render(SelectedHurtbox != null && SelectedHurtbox.Index == i, Scriptor._hurtBoxType);

            //Render hitboxes
            if (chkHitboxes.Checked && Manager.Moveset != null)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GLDisplayList c = TKContext.GetRingList();
                GLDisplayList s = TKContext.GetSphereList();

                foreach (HitBox e in RunTime._hitBoxes)
                    e.Render(modelPanel.Camera.GetPoint());
            }

            base.ModelPanel_PostRender(sender);
        }
    }
}
