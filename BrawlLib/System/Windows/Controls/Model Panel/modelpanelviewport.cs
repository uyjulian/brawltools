﻿using BrawlLib.Imaging;
using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;

namespace System.Windows.Forms
{
    public unsafe class ModelPanelViewport : GLViewport
    {
        public ModelPanelViewport()
        {
            _text = new ScreenTextHandler(this);
            _camera = new GLCamera();
            LightPosition = new Vector4(100.0f, 45.0f, 45.0f, 1.0f);

#if DEBUG
            _textEnabled = true;
#endif
        }

        public ModelPanelViewportInfo GetInfo()
        {
            return new ModelPanelViewportInfo()
            {
                _ambient = _ambient,
                _backColor = (ARGBPixel)base.BackgroundColor,
                _bgImagePath = "",
                _bgType = base.BackgroundImageType,
                _diffuse = _diffuse,
                _emission = _emission,
                _enabled = base.Enabled,
                _lightPosition = _lightPosition,
                _percentages = _percentages,
                _renderAttrib = _renderAttrib,
                _renderFloor = _renderFloor,
                _rotFactor = _rotFactor,
                _specular = _specular,
                _spotCutoff = _spotCutoff,
                _spotExponent = _spotExponent,
                _transFactor = _transFactor,
                _type = _type,
                _viewDistance = _viewDistance,
                _zoomFactor = _zoomFactor,
                _allowSelection = _allowSelection,
                _showCamCoords = _showCamCoords,
                _textEnabled = _textEnabled,

                _defaultRotate = (Vector4)_camera._defaultRotate,
                _defaultScale = (Vector4)_camera._defaultScale,
                _defaultTranslate = (Vector4)_camera._defaultTranslate,
                _farZ = _camera._farZ,
                _fovY = _camera._fovY,
                _nearZ = _camera._nearZ,
                _ortho = _camera._ortho,
                _restrictXRot = _camera._restrictXRot,
                _restrictYRot = _camera._restrictYRot,
                _restrictZRot = _camera._restrictZRot,
            };
        }

        public float _rotFactor = 0.4f;
        public float _transFactor = 0.05f;
        public float _zoomFactor = 2.5f;
        public float _viewDistance = 5.0f;
        public float _spotCutoff = 180.0f;
        public float _spotExponent = 100.0f;

        private Vector4 _posLight, _spotDirLight;
        private Vector4 _lightPosition;

        const float v = 100.0f / 255.0f;
        public Vector4 _ambient = new Vector4(v, v, v, 1.0f);
        public Vector4 _diffuse = new Vector4(v, v, v, 1.0f);
        public Vector4 _specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public Vector4 _emission = new Vector4(v, v, v, 1.0f);

        public ModelRenderAttributes _renderAttrib = new ModelRenderAttributes();
        public bool _renderFloor;

        public float _multiplier = 1.0f;
        public bool _shiftSelecting;
        public Drawing.Point _selStart, _selEnd;

        private ScreenTextHandler _text;

        public bool _textEnabled = false;
        public bool _allowSelection = false;
        public bool _selecting = false;
        public bool _showCamCoords = false;

        public delegate void ZoomDel(float amt, bool invoked);
        public delegate void ScaleDel(float x, float y, float z, bool invoked);
        public delegate void TranslateDel(float x, float y, float z, bool invoked);
        public delegate void PivotDel(float x, float y);
        public delegate void RotateDel(float x, float y, float z);

        public event ZoomDel Zoomed;
        public event ScaleDel Scaled;
        public event TranslateDel Translated;
        public event PivotDel Pivoted;
        public event RotateDel Rotated;

        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selecting { get { return _selecting; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScreenTextHandler ScreenText { get { return _text; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Drawing.Point SelectionStart { get { return _selStart; } set { _selStart = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Drawing.Point SelectionEnd { get { return _selEnd; } set { _selEnd = value; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowSelection { get { return _allowSelection; } set { _allowSelection = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TextOverlaysEnabled { get { return _textEnabled; } set { _textEnabled = value; Invalidate(); } }

        public float RotationScale { get { return _rotFactor; } set { _rotFactor = value; } }
        public float TranslationScale { get { return _transFactor; } set { _transFactor = value; } }
        public float ZoomScale { get { return _zoomFactor; } set { _zoomFactor = value; } }

        public int Width { get { return Region.Width; } }
        public int Height { get { return Region.Height; } }
        public Rectangle RegionNoBorder { get { return new Rectangle(Region.X + 1, Region.Y + 1, Region.Width - 2, Region.Height - 2); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 DefaultTranslate { get { return Camera._defaultTranslate; } set { Camera._defaultTranslate = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector2StringConverter))]
        public Vector3 DefaultRotate { get { return Camera._defaultRotate; } set { Camera._defaultRotate = value; } }

        #endregion

        #region Viewport Render Attributes
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Emission
        {
            get { return _emission; }
            set
            {
                _emission = value;
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Ambient
        {
            get { return _ambient; }
            set
            {
                _ambient = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 LightPosition
        {
            get { return _lightPosition; }
            set
            {
                _lightPosition = value;

                float r = _lightPosition._x;
                float azimuth = _lightPosition._y * Maths._deg2radf;
                float elevation = 360.0f - (_lightPosition._z * Maths._deg2radf);

                float
                    cosElev = (float)Math.Cos(elevation),
                    sinElev = (float)Math.Sin(elevation),
                    cosAzi = (float)Math.Cos(azimuth),
                    sinAzi = (float)Math.Sin(azimuth);

                _posLight = new Vector4(r * cosAzi * sinElev, r * cosElev, r * sinAzi * sinElev, 1.0f);
                _spotDirLight = new Vector4(-cosAzi * sinElev, -cosElev, -sinAzi * sinElev, 1.0f);

                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Diffuse
        {
            get { return _diffuse; }
            set
            {
                _diffuse = value;
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(Vector4StringConverter))]
        public Vector4 Specular
        {
            get { return _specular; }
            set
            {
                _specular = value;
                Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderFloor
        {
            get { return _renderFloor; }
            set
            {
                _renderFloor = value;
                Invalidate();
            }
        }
        #endregion

        #region Model Render Attributes
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderBones
        {
            get { return _renderAttrib._renderBones; }
            set
            {
                _renderAttrib._renderBones = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderVertices
        {
            get { return _renderAttrib._renderVertices; }
            set
            {
                _renderAttrib._renderVertices = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderNormals
        {
            get { return _renderAttrib._renderNormals; }
            set
            {
                _renderAttrib._renderNormals = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderPolygons
        {
            get { return _renderAttrib._renderPolygons; }
            set
            {
                _renderAttrib._renderPolygons = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderWireframe
        {
            get { return _renderAttrib._renderWireframe; }
            set
            {
                _renderAttrib._renderWireframe = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderModelBox
        {
            get { return _renderAttrib._renderModelBox; }
            set
            {
                _renderAttrib._renderModelBox = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderObjectBox
        {
            get { return _renderAttrib._renderObjectBoxes; }
            set
            {
                _renderAttrib._renderObjectBoxes = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RenderVisBoneBox
        {
            get { return _renderAttrib._renderBoneBoxes; }
            set
            {
                _renderAttrib._renderBoneBoxes = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseBindStateBoxes
        {
            get { return _renderAttrib._useBindStateBoxes; }
            set
            {
                _renderAttrib._useBindStateBoxes = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DontRenderOffscreen
        {
            get { return _renderAttrib._dontRenderOffscreen; }
            set
            {
                _renderAttrib._dontRenderOffscreen = value;
                Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ApplyBillboardBones
        {
            get { return _renderAttrib._applyBillboardBones; }
            set
            {
                _renderAttrib._applyBillboardBones = value;
                Invalidate();
            }
        }
        #endregion

        #region Render Functions

        public void RenderBackground()
        {
            //Apply image
            if (BackgroundImage != null)
                RenderBackgroundImage();
            else if (_updateImage && _bgImage != null)
            {
                _bgImage.Delete();
                _bgImage = null;
                _updateImage = false;
            }

            //Apply color
            Vector3 v = (Vector3)BackgroundColor;
            GL.ClearColor(v._x, v._y, v._z, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void RenderForeground(bool current, bool only)
        {
            if (_showCamCoords)
            {
                Vector3 point = Camera.GetPoint().Round(3);
                Vector3 rot = Camera._rotation.Round(3);
                _text[String.Format("Position\nX: {0}\nY: {1}\nZ: {2}\n\nRotation\nX: {3}\nY: {4}\nZ: {5}", point._x, point._y, point._z, rot._x, rot._y, rot._z)] = new Vector3(5.0f, 5.0f, 0.5f);
            }

            //Render selection overlay and/or text overlays
            if ((_selecting && _allowSelection) || (_text.Count != 0 && _textEnabled) || !only)
            {
                GL.PushAttrib(AttribMask.AllAttribBits);
                {
                    GL.Disable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.Lighting);
                    GL.Disable(EnableCap.CullFace);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PushMatrix();
                    {
                        GL.LoadIdentity();
                        Matrix p = Matrix.OrthographicMatrix(0, Width, 0, Height, -1, 1);
                        GL.LoadMatrix((float*)&p);

                        GL.MatrixMode(MatrixMode.Modelview);
                        GL.PushMatrix();
                        {
                            GL.LoadIdentity();

                            if (!only)
                            {
                                GL.Color4(current ? Color.DarkOrange : Color.Gray);
                                GL.Begin(PrimitiveType.LineLoop);
                                GL.Vertex2(0, 0);
                                GL.Vertex2(0, Height);
                                GL.Vertex2(Width, Height);
                                GL.Vertex2(Width, 0);
                                GL.Vertex2(0, 0);
                                GL.End();
                            }

                            GL.Color4(Color.White);
                            if (_text.Count != 0 && _textEnabled)
                                _text.Draw();

                            if (_selecting && _allowSelection)
                                RenderSelection();
                        }
                        GL.PopMatrix();
                    }
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PopMatrix();
                }
                GL.PopAttrib();

                //Clear text values
                //This will be filled until the next render
                _text.Clear();
            }

        }

        public void RenderSelection()
        {
            if (_selecting)
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(1, 0x0F0F);
                GL.Color4(Color.Blue);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(_selStart.X, _selStart.Y);
                GL.Vertex2(_selEnd.X, _selStart.Y);
                GL.Vertex2(_selEnd.X, _selEnd.Y);
                GL.Vertex2(_selStart.X, _selEnd.Y);
                GL.End();
                GL.Disable(EnableCap.LineStipple);
            }
        }

        private void RenderBackgroundImage()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.CullFace);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            {
                GL.LoadIdentity();
                Matrix p = Matrix.OrthographicMatrix(0, Region.Width, 0, Region.Height, -1, 1);
                GL.LoadMatrix((float*)&p);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                {
                    GL.LoadIdentity();

                    GL.Color4(Color.White);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                    GL.Enable(EnableCap.Texture2D);

                    if (_updateImage)
                    {
                        if (_bgImage != null)
                        {
                            _bgImage.Delete();
                            _bgImage = null;
                        }

                        GL.ClearColor(Color.Black);

                        Bitmap bmp = BackgroundImage as Bitmap;

                        _bgImage = new GLTexture(bmp);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
                        _bgImage.Bind();

                        _updateImage = false;
                    }
                    else
                        GL.BindTexture(TextureTarget.Texture2D, _bgImage._texId);

                    float* points = stackalloc float[8];
                    float tAspect = (float)_bgImage.Width / (float)_bgImage.Height;
                    float wAspect = (float)Width / (float)Height;

                    switch (_bgType)
                    {
                        case BGImageType.Stretch:

                            points[0] = points[1] = points[3] = points[6] = 0.0f;
                            points[2] = points[4] = Width;
                            points[5] = points[7] = Height;

                            break;

                        case BGImageType.Center:

                            if (tAspect > wAspect)
                            {
                                points[1] = points[3] = 0.0f;
                                points[5] = points[7] = Height;

                                points[0] = points[6] = Width * ((Width - ((float)Height / _bgImage.Height * _bgImage.Width)) / Width / 2.0f);
                                points[2] = points[4] = Width - points[0];
                            }
                            else
                            {
                                points[0] = points[6] = 0.0f;
                                points[2] = points[4] = Width;

                                points[1] = points[3] = Height * (((Height - ((float)Width / _bgImage.Width * _bgImage.Height))) / Height / 2.0f);
                                points[5] = points[7] = Height - points[1];
                            }
                            break;

                        case BGImageType.ResizeWithBars:

                            if (tAspect > wAspect)
                            {
                                points[0] = points[6] = 0.0f;
                                points[2] = points[4] = Width;

                                points[1] = points[3] = Height * (((Height - ((float)Width / _bgImage.Width * _bgImage.Height))) / Height / 2.0f);
                                points[5] = points[7] = Height - points[1];
                            }
                            else
                            {
                                points[1] = points[3] = 0.0f;
                                points[5] = points[7] = Height;

                                points[0] = points[6] = Width * ((Width - ((float)Height / _bgImage.Height * _bgImage.Width)) / Width / 2.0f);
                                points[2] = points[4] = Width - points[0];
                            }

                            break;
                    }

                    GL.Begin(PrimitiveType.Quads);

                    GL.TexCoord2(0.0f, 0.0f);
                    GL.Vertex2(&points[0]);
                    GL.TexCoord2(1.0f, 0.0f);
                    GL.Vertex2(&points[2]);
                    GL.TexCoord2(1.0f, 1.0f);
                    GL.Vertex2(&points[4]);
                    GL.TexCoord2(0.0f, 1.0f);
                    GL.Vertex2(&points[6]);

                    GL.End();

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MatTextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MatTextureMagFilter.Nearest);

                    GL.Disable(EnableCap.Texture2D);
                    GL.Enable(EnableCap.DepthTest);
                    GL.Enable(EnableCap.Lighting);
                }
                GL.PopMatrix();
            }
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        //Call this every time the scene is rendered
        //Otherwise the light will move with the camera
        //(Which makes sense, since the camera isn't moving and the scene is)
        public void RecalcLight()
        {
            GL.Light(LightName.Light0, LightParameter.SpotCutoff, _spotCutoff);
            GL.Light(LightName.Light0, LightParameter.SpotExponent, _spotExponent);

            fixed (Vector4* pos = &_posLight)
                GL.Light(LightName.Light0, LightParameter.Position, (float*)pos);
            fixed (Vector4* pos = &_spotDirLight)
                GL.Light(LightName.Light0, LightParameter.SpotDirection, (float*)pos);

            fixed (Vector4* pos = &_ambient)
            {
                GL.Light(LightName.Light0, LightParameter.Ambient, (float*)pos);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, (float*)pos);
            }
            fixed (Vector4* pos = &_diffuse)
            {
                GL.Light(LightName.Light0, LightParameter.Diffuse, (float*)pos);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, (float*)pos);
            }
            fixed (Vector4* pos = &_specular)
            {
                GL.Light(LightName.Light0, LightParameter.Specular, (float*)pos);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, (float*)pos);
            }
            fixed (Vector4* pos = &_emission)
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, (float*)pos);
            }

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
        }

        #endregion

        #region Mouse/Keyboard Functions

        public void HandleLeftMouseDown(MouseEventArgs e, Keys mod)
        {
            if (_allowSelection && !_selecting)
            {
                _selecting = true;
                _selStart = new Drawing.Point(e.X - Region.X, WorldToLocalY(e.Y));
                _selEnd = _selStart;
                _shiftSelecting = mod == Keys.ShiftKey || mod == Keys.Shift;
            }
            else if (_selecting && _shiftSelecting)
            {
                _selecting = false;
                _selEnd = new Drawing.Point(e.X - Region.X, WorldToLocalY(e.Y));
                _shiftSelecting = false;
            }
        }

        public void HandleLeftMouseUp(MouseEventArgs e, Keys ModifierKeys)
        {
            if (_selecting && !(ModifierKeys == Keys.ShiftKey || ModifierKeys == Keys.Shift || _shiftSelecting))
            {
                _selEnd = new Drawing.Point(e.X - Region.X, WorldToLocalY(e.Y));
                _selecting = false;
            }
        }

        public void HandleMouseMove(TKContext ctx, MouseEventArgs e, Keys keys)
        {
            if (_selecting)
                _selEnd = new Drawing.Point(e.X - Region.X, WorldToLocalY(e.Y));

            int x = e.X - Region.X;
            int y = e.Y - Region.Y;

            if (ctx != null && (_grabbing || _scrolling))
                lock (ctx)
                {
                    int xDiff = x - _lastX;
                    int yDiff = _lastY - y;

                    Keys mod = Control.ModifierKeys;
                    bool ctrl = (mod & Keys.Control) != 0;
                    bool shift = (mod & Keys.Shift) != 0;
                    bool alt = (mod & Keys.Alt) != 0;

                    if (shift)
                    {
                        xDiff *= 16;
                        yDiff *= 16;
                    }

                    if (_scrolling)
                        Translate(0, 0, (float)yDiff * 0.01f);
                    else if (ctrl)
                        if (alt)
                            Rotate(0, 0, -yDiff * RotationScale);
                        else
                            Pivot(yDiff * RotationScale, -xDiff * RotationScale);
                    else
                        Translate(-xDiff * TranslationScale, -yDiff * TranslationScale, 0.0f);
                }

            _lastX = x;
            _lastY = y;

            if (_selecting)
                Invalidate();
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if ((e.KeyData == Keys.ShiftKey || e.KeyData == Keys.Shift) && _shiftSelecting)
            {
                _selecting = false;
                _shiftSelecting = false;
                Invalidate();
            }
        }

        internal bool ProcessKeys(Keys keys, Keys mod)
        {
            bool ctrl = (mod & Keys.Control) != 0;
            bool shift = (mod & Keys.Shift) != 0;
            bool alt = (mod & Keys.Alt) != 0;
            switch (keys)
            {
                case Keys.Shift:
                case Keys.ShiftKey:
                    if (_selecting)
                        _shiftSelecting = true;
                    break;

                case Keys.NumPad8:
                case Keys.Up:
                    {
                        if (alt)
                            break;
                        if (ctrl)
                            Pivot(-RotationScale * (shift ? 32 : 4), 0.0f);
                        else
                            Translate(0.0f, TranslationScale * (shift ? 128 : 8), 0.0f);
                        return true;
                    }
                case Keys.NumPad2:
                case Keys.Down:
                    {
                        if (alt)
                            break;
                        if (ctrl)
                            Pivot(RotationScale * (shift ? 32 : 4), 0.0f);
                        else
                            Translate(0.0f, -TranslationScale * (shift ? 128 : 8), 0.0f);
                        return true;
                    }
                case Keys.NumPad6:
                case Keys.Right:
                    {
                        if (alt)
                            break;
                        if (ctrl)
                            Pivot(0.0f, RotationScale * (shift ? 32 : 4));
                        else
                            Translate(TranslationScale * (shift ? 128 : 8), 0.0f, 0.0f);
                        return true;
                    }
                case Keys.NumPad4:
                case Keys.Left:
                    {
                        if (alt)
                            break;
                        if (ctrl)
                            Pivot(0.0f, -RotationScale * (shift ? 32 : 4));
                        else
                            Translate(-TranslationScale * (shift ? 128 : 8), 0.0f, 0.0f);
                        return true;
                    }
                case Keys.Add:
                case Keys.Oemplus:
                    {
                        if (alt) break;
                        Zoom(-ZoomScale * (shift ? 32 : 2));
                        return true;
                    }
                case Keys.Subtract:
                case Keys.OemMinus:
                    {
                        if (alt) break;
                        Zoom(ZoomScale * (shift ? 32 : 2));
                        return true;
                    }
            }
            return false;
        }

        public void Zoom(float amt, bool invoked = false)
        {
            _scrolling = true;
            Camera.Zoom(amt);
            _scrolling = false;

            if (!invoked)
            {
                if (Zoomed != null)
                    Zoomed(amt, true);

                Invalidate();
            }
        }
        public void Scale(float x, float y, float z, bool invoked = false)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            Camera.Scale(x, y, z);

            if (!invoked)
            {
                if (Scaled != null)
                    Scaled(x, y, z, true);

                Invalidate();
            }
        }
        public void Translate(float x, float y, float z, bool invoked = false)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            Camera.Translate(x, y, z);

            if (!invoked)
            {
                if (Translated != null)
                    Translated(x, y, z, true);

                Invalidate();
            }
        }
        public void Pivot(float x, float y)
        {
            x *= _multiplier;
            y *= _multiplier;

            Camera.Pivot(_viewDistance, x, y);

            if (Pivoted != null)
                Pivoted(x, y);
            else
                Invalidate();
        }
        public void Rotate(float x, float y, float z)
        {
            x *= _multiplier;
            y *= _multiplier;
            z *= _multiplier;

            Camera.Rotate(x, y, z);

            if (Rotated != null)
                Rotated(x, y, z);
            else
                Invalidate();
        }

        public unsafe Vector3 ProjectCameraSphere(Vector2 mousePoint, Vector3 center, float radius, bool clamp)
        {
            return Camera.ProjectCameraSphere(new Vector2(mousePoint._x - Region.X, WorldToLocalYf(mousePoint._y)), center, radius, clamp);
        }

        #endregion

        #region Default Viewports
        public static ModelPanelViewport DefaultPerspective
        {
            get
            {
                return new ModelPanelViewport()
                {
                    _type = ViewportProjection.Perspective,
                    _camera = new GLCamera(),
                    _percentages = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                };
            }
        }
        private static ModelPanelViewport BaseOrtho
        {
            get
            {
                return new ModelPanelViewport()
                {
                    _type = ViewportProjection.Orthographic,
                    _camera = new GLCamera()
                    {
                        _ortho = true,
                        _nearZ = -10000.0f,
                        _farZ = 10000.0f,
                        _defaultScale = new Vector3(0.035f, 0.035f, 0.035f),
                    },
                    _percentages = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                };
            }
        }
        public static ModelPanelViewport DefaultOrtho
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Orthographic;
                p.Camera.Reset();
                return p;
            }
        }
        public static ModelPanelViewport DefaultFront
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Front;
                p.Camera._restrictXRot = true;
                p.Camera._restrictYRot = true;
                p.Camera.Reset();
                return p;
            }
        }
        public static ModelPanelViewport DefaultBack
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Back;
                p.Camera._restrictXRot = true;
                p.Camera._restrictYRot = true;
                p.Camera._defaultRotate = new Vector3(0.0f, 180.0f, 0.0f);
                p.Camera.Reset();
                return p;
            }
        }
        public static ModelPanelViewport DefaultLeft
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Left;
                p.Camera._restrictXRot = true;
                p.Camera._restrictYRot = true;
                p.Camera._defaultRotate = new Vector3(0.0f, 90.0f, 0.0f);
                p.Camera.Reset();
                return p;
            }
        }
        public static ModelPanelViewport DefaultRight
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Right;
                p.Camera._restrictXRot = true;
                p.Camera._restrictYRot = true;
                p.Camera._defaultRotate = new Vector3(0.0f, -90.0f, 0.0f);
                p.Camera.Reset();
                return p;
            }
        }
        public static ModelPanelViewport DefaultTop
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Top;
                p.Camera._restrictXRot = true;
                p.Camera._restrictYRot = true;
                p.Camera._defaultRotate = new Vector3(-90.0f, 0.0f, 0.0f);
                p.Camera.Reset();
                return p;
            }
        }
        public static ModelPanelViewport DefaultBottom
        {
            get
            {
                ModelPanelViewport p = BaseOrtho;
                p._type = ViewportProjection.Top;
                p.Camera._restrictXRot = true;
                p.Camera._restrictYRot = true;
                p.Camera._defaultRotate = new Vector3(90.0f, 0.0f, 0.0f);
                p.Camera.Reset();
                return p;
            }
        }
        public override Vector3 GetDefaultScale()
        {
            float f = _camera._ortho ? 0.035f : 1.0f;
            return new Vector3(f);
        }
        #endregion
    }

    [Serializable]
    public class ModelPanelViewportInfo : ISerializable
    {
        public ModelPanelViewportInfo() { }
        public ModelPanelViewportInfo(SerializationInfo info, StreamingContext ctxt)
        {
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                f.SetValue(this, info.GetValue(f.Name, t));
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                info.AddValue(f.Name, f.GetValue(this));
            }
        }

        #region Camera
        public bool _ortho, _restrictXRot, _restrictYRot, _restrictZRot;
        public float _fovY = 45.0f, _nearZ = 1.0f, _farZ = 200000.0f;
        public Vector4 _defaultTranslate;
        public Vector4 _defaultRotate;
        public Vector4 _defaultScale = new Vector4(1);
        #endregion

        #region GLPanelViewport
        public bool _enabled = true;
        public Vector4 _percentages = new Vector4(0, 0, 1, 1);
        public BGImageType _bgType = BGImageType.Stretch;
        public ViewportProjection _type = ViewportProjection.Perspective;
        public string _bgImagePath;
        public ARGBPixel _backColor;
        #endregion

        #region ModelPanelViewport
        public float _rotFactor = 0.4f;
        public float _transFactor = 0.05f;
        public float _zoomFactor = 2.5f;
        public float _viewDistance = 5.0f;
        public float _spotCutoff = 180.0f;
        public float _spotExponent = 100.0f;
        const float v = 100.0f / 255.0f;
        public Vector4 _ambient = new Vector4(v, v, v, 1.0f);
        public Vector4 _diffuse = new Vector4(v, v, v, 1.0f);
        public Vector4 _specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public Vector4 _emission = new Vector4(v, v, v, 1.0f);
        public Vector4 _lightPosition = new Vector4(100.0f, 45.0f, 45.0f, 1.0f);
        public ModelRenderAttributes _renderAttrib = new ModelRenderAttributes();
        public bool _renderFloor;
        public bool _textEnabled = false;
        public bool _allowSelection = false;
        public bool _showCamCoords = false;
        #endregion

        public ModelPanelViewport AsViewport()
        {
            ModelPanelViewport v = ModelPanelViewport.DefaultPerspective;
            v.Camera = new GLCamera(1, 1, (Vector3)_defaultTranslate, (Vector3)_defaultRotate, (Vector3)_defaultScale)
            {
                _farZ = _farZ,
                _fovY = _fovY,
                _nearZ = _nearZ,
                _ortho = _ortho,
                _restrictXRot = _restrictXRot,
                _restrictYRot = _restrictYRot,
                _restrictZRot = _restrictZRot,
            };
            v.SetPercentages(_percentages);
            v.LightPosition = _lightPosition;
            v.Enabled = _enabled;
            v.BackgroundColor = (Color)_backColor;
            v.BackgroundImageType = _bgType;
            v._allowSelection = _allowSelection;
            v._showCamCoords = _showCamCoords;
            v._textEnabled = _textEnabled;
            v._type = _type;
            v._diffuse = _diffuse;
            v._ambient = _ambient;
            v._emission = _emission;
            v._renderAttrib = _renderAttrib;
            v._renderFloor = _renderFloor;
            v._rotFactor = _rotFactor;
            v._specular = _specular;
            v._spotCutoff = _spotCutoff;
            v._spotExponent = _spotExponent;
            v._transFactor = _transFactor;
            v._viewDistance = _viewDistance;
            v._zoomFactor = _zoomFactor;
            return v;
        }
    }
}