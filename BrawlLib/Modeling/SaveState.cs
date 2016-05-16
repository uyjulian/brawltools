﻿using System;
using System.Collections.Generic;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.Modeling
{
    public abstract class SaveState
    {
        public bool _isUndo = true;
    }

    public class VertexState : SaveState
    {
        public int _animFrame;
        public CHR0Node _chr0;
        public List<Vertex3> _vertices = null;
        public List<Vector3> _weightedPositions = null;
        public IModel _targetModel;
    }

    public class BoneState : SaveState
    {
        public bool _updateBindState; //This will update the actual mesh when the bone is moved
        public bool _updateBoneOnly; //This means the bones won't affect the mesh when moved
        public int _frameIndex = 0;
        public CHR0Node _animation;
        public IBoneNode[] _bones;
        public FrameState[] _frameStates;
        public IModel _targetModel;
    }
}
