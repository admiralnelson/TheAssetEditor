﻿using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using View3D.Animation;
using View3D.Components.Gizmo;
using View3D.Components.Rendering;
using View3D.Rendering;
using View3D.Rendering.Geometry;
using View3D.Rendering.RenderItems;
using View3D.Rendering.Shading;
using View3D.Utility;

namespace View3D.SceneNodes
{
    public class Rmv2MeshNode : SceneNode, ITransformable, IEditableGeometry, ISelectable, IUpdateable, IDrawableItem
    {

        Quaternion _orientation = Quaternion.Identity;
        Vector3 _position = Vector3.Zero;
        Vector3 _scale = Vector3.One;

        public Vector3 Position { get { return _position; } set { _position = value; UpdateMatrix(); } }
        public Vector3 Scale { get { return _scale; } set { _scale = value; UpdateMatrix(); } }
        public Quaternion Orientation { get { return _orientation; } set { _orientation = value; UpdateMatrix(); } }

        void UpdateMatrix()
        {
            ModelMatrix = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);
        }


        public AnimationPlayer AnimationPlayer;

        private Rmv2MeshNode()
        { }

        public Rmv2MeshNode(IGeometry geo, string name, AnimationPlayer animationPlayer, PbrShader shader)
        {
            Geometry = geo;
            AnimationPlayer = animationPlayer;

            Name = name;
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Orientation = Quaternion.Identity;

            Effect = shader;
        }

        public Rmv2MeshNode(RmvSubModel rmvSubModel, GraphicsDevice device, ResourceLibary resourceLib, AnimationPlayer animationPlayer)
        {
            Geometry = new Rmv2Geometry(rmvSubModel, device);
            AnimationPlayer = animationPlayer;

            Name = rmvSubModel.Header.ModelName;
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Orientation = Quaternion.Identity;

            Effect = new PbrShader(resourceLib);
            var diffuse = resourceLib.LoadTexture(rmvSubModel.GetTexture(TexureType.Diffuse).Path);
            var specTexture = resourceLib.LoadTexture(rmvSubModel.GetTexture(TexureType.Specular).Path);
            var normalTexture = resourceLib.LoadTexture(rmvSubModel.GetTexture(TexureType.Normal).Path);
            var glossTexture = resourceLib.LoadTexture(rmvSubModel.GetTexture(TexureType.Gloss).Path);

            (Effect as PbrShader).SetTexture(diffuse, TexureType.Diffuse);
            (Effect as PbrShader).SetTexture(specTexture, TexureType.Specular);
            (Effect as PbrShader).SetTexture(normalTexture, TexureType.Normal);
            (Effect as PbrShader).SetTexture(glossTexture, TexureType.Gloss);
        }

        public IShader Effect { get; set; }
        public int LodIndex { get; set; } = -1;
        public IGeometry Geometry { get; set; }

        public void Update(GameTime time)
        {

        }

        public Vector3 GetObjectCenter()
        {
            return MathUtil.GetCenter(Geometry.BoundingBox) + Position;
        }

        public void Render(RenderEngineComponent renderEngine, Matrix parentWorld)
        {
            if (Effect is IShaderAnimation animationEffect)
            {
                Matrix[] data = new Matrix[256];
                for (int i = 0; i < 256; i++)
                    data[i] = Matrix.Identity;

                var player = AnimationPlayer;
                if (player != null)
                {
                    var frame = player.GetCurrentFrame();
                    if (frame != null)
                    {
                        for (int i = 0; i < frame.BoneTransforms.Count(); i++)
                            data[i] = frame.BoneTransforms[i].WorldTransform;
                    }
                }

                animationEffect.SetAnimationParameters(data, 4);
                animationEffect.UseAnimation = AnimationPlayer.IsEnabled;
            }

            if (Effect is IShaderTextures tetureEffect)
            {
                tetureEffect.UseAlpha = false;
            }

            renderEngine.AddRenderItem(RenderBuckedId.Normal, new GeoRenderItem() { Geometry = Geometry, ModelMatrix = ModelMatrix, Shader = Effect });
        }

        public override SceneNode Clone()
        {
            var newItem = new Rmv2MeshNode()
            {
                Geometry = Geometry.Clone(),
                Position = Position,
                Orientation = Orientation,
                Scale = Scale,
                SceneManager = SceneManager,
                IsEditable = IsEditable,
                IsVisible = IsVisible,
                LodIndex = LodIndex,
                Name = Name + " - Clone",
                AnimationPlayer = AnimationPlayer,
            };
            newItem.Effect = Effect.Clone();
            return newItem;
        }
    }
}