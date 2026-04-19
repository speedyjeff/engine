using System;

namespace engine.Common.Entities3D
{
    public class Humanoid3D : ComboElement3D
    {
        public Humanoid3D()
        {
            ShirtColor = new RGBA() { R = 96, G = 108, B = 92, A = 255 };
            PantsColor = new RGBA() { R = 52, G = 60, B = 72, A = 255 };
            BootColor = new RGBA() { R = 86, G = 62, B = 42, A = 255 };
            SkinColor = new RGBA() { R = 244, G = 214, B = 58, A = 255 };
            BackpackColor = new RGBA() { R = 82, G = 95, B = 74, A = 255 };
            WeaponColor = new RGBA() { R = 64, G = 68, B = 78, A = 255 };

            Head = new Sphere() { Wireframe = false, DisableShading = true };
            Torso = new Cylinder() { Wireframe = false, DisableShading = true };
            Hips = new Cube() { Wireframe = false, DisableShading = true };
            LeftArm = new Cylinder() { Wireframe = false, DisableShading = true };
            RightArm = new Cylinder() { Wireframe = false, DisableShading = true };
            LeftLeg = new Cylinder() { Wireframe = false, DisableShading = true };
            RightLeg = new Cylinder() { Wireframe = false, DisableShading = true };
            LeftBoot = new Cube() { Wireframe = false, DisableShading = false };
            RightBoot = new Cube() { Wireframe = false, DisableShading = false };
            Backpack = new Cube() { Wireframe = false, DisableShading = false };
            HandItem = new Cube() { Wireframe = false, DisableShading = false };

            AddInner(Head);
            AddInner(Torso);
            AddInner(Hips);
            AddInner(LeftArm);
            AddInner(RightArm);
            AddInner(LeftLeg);
            AddInner(RightLeg);
            AddInner(LeftBoot);
            AddInner(RightBoot);
            AddInner(Backpack);
            AddInner(HandItem);

            ApplyAppearance();
        }

        public RGBA ShirtColor { get; set; }
        public RGBA PantsColor { get; set; }
        public RGBA BootColor { get; set; }
        public RGBA SkinColor { get; set; }
        public RGBA BackpackColor { get; set; }
        public RGBA WeaponColor { get; set; }
        public float WalkPhase { get; set; }

        public void ApplyAppearance()
        {
            Head.UniformColor = SkinColor;
            Torso.UniformColor = ShirtColor;
            Hips.UniformColor = PantsColor;
            LeftArm.UniformColor = ShirtColor;
            RightArm.UniformColor = ShirtColor;
            LeftLeg.UniformColor = PantsColor;
            RightLeg.UniformColor = PantsColor;
            LeftBoot.UniformColor = BootColor;
            RightBoot.UniformColor = BootColor;
            Backpack.UniformColor = BackpackColor;
            HandItem.UniformColor = WeaponColor;
        }

        public void UpdatePose()
        {
            var bob = (float)Math.Sin(WalkPhase) * (Height * 0.025f);
            var sway = (float)Math.Sin(WalkPhase) * (Width * 0.05f);
            var leftLift = (float)Math.Abs(Math.Sin(WalkPhase)) * (Height * 0.035f);
            var rightLift = (float)Math.Abs(Math.Cos(WalkPhase)) * (Height * 0.035f);

            Head.Width = Width * 0.32f;
            Head.Height = Height * 0.32f;
            Head.Depth = Depth * 0.32f;
            Head.X = X;
            Head.Y = Y - (Height * 0.48f) + bob;
            Head.Z = Z;

            Torso.Width = Width * 0.38f;
            Torso.Height = Height * 0.52f;
            Torso.Depth = Depth * 0.28f;
            Torso.X = X;
            Torso.Y = Y - (Height * 0.08f) + (bob * 0.5f);
            Torso.Z = Z;

            Hips.Width = Width * 0.42f;
            Hips.Height = Height * 0.16f;
            Hips.Depth = Depth * 0.30f;
            Hips.X = X;
            Hips.Y = Y + (Height * 0.16f) + (bob * 0.25f);
            Hips.Z = Z;

            LeftArm.Width = Width * 0.12f;
            LeftArm.Height = Height * 0.38f;
            LeftArm.Depth = Depth * 0.12f;
            LeftArm.X = X - (Width * 0.28f) + sway;
            LeftArm.Y = Y + (Height * 0.02f) + bob;
            LeftArm.Z = Z;

            RightArm.Width = Width * 0.12f;
            RightArm.Height = Height * 0.38f;
            RightArm.Depth = Depth * 0.12f;
            RightArm.X = X + (Width * 0.28f) - sway;
            RightArm.Y = Y + (Height * 0.02f) + bob;
            RightArm.Z = Z;

            LeftLeg.Width = Width * 0.14f;
            LeftLeg.Height = Height * 0.34f;
            LeftLeg.Depth = Depth * 0.14f;
            LeftLeg.X = X - (Width * 0.12f);
            LeftLeg.Y = Y + (Height * 0.38f) - leftLift;
            LeftLeg.Z = Z;

            RightLeg.Width = Width * 0.14f;
            RightLeg.Height = Height * 0.34f;
            RightLeg.Depth = Depth * 0.14f;
            RightLeg.X = X + (Width * 0.12f);
            RightLeg.Y = Y + (Height * 0.38f) - rightLift;
            RightLeg.Z = Z;

            LeftBoot.Width = Width * 0.15f;
            LeftBoot.Height = Height * 0.08f;
            LeftBoot.Depth = Depth * 0.20f;
            LeftBoot.X = LeftLeg.X;
            LeftBoot.Y = Y + (Height * 0.56f) - leftLift;
            LeftBoot.Z = Z + (Depth * 0.03f);

            RightBoot.Width = Width * 0.15f;
            RightBoot.Height = Height * 0.08f;
            RightBoot.Depth = Depth * 0.20f;
            RightBoot.X = RightLeg.X;
            RightBoot.Y = Y + (Height * 0.56f) - rightLift;
            RightBoot.Z = Z + (Depth * 0.03f);

            Backpack.Width = Width * 0.22f;
            Backpack.Height = Height * 0.26f;
            Backpack.Depth = Depth * 0.12f;
            Backpack.X = X;
            Backpack.Y = Y - (Height * 0.05f) + (bob * 0.4f);
            Backpack.Z = Z + (Depth * 0.12f);

            HandItem.Width = Width * 0.10f;
            HandItem.Height = Height * 0.07f;
            HandItem.Depth = Depth * 0.22f;
            HandItem.X = RightArm.X + (Width * 0.08f);
            HandItem.Y = RightArm.Y + (Height * 0.12f);
            HandItem.Z = Z - (Depth * 0.10f);
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            base.Move(xDelta, yDelta, zDelta);

            var movement = Math.Abs(xDelta) + Math.Abs(yDelta) + Math.Abs(zDelta);
            if (movement > 0.001f) WalkPhase += Math.Min(0.45f, movement * 0.18f);

            UpdatePose();
        }

        #region private
        private readonly Sphere Head;
        private readonly Cylinder Torso;
        private readonly Cube Hips;
        private readonly Cylinder LeftArm;
        private readonly Cylinder RightArm;
        private readonly Cylinder LeftLeg;
        private readonly Cylinder RightLeg;
        private readonly Cube LeftBoot;
        private readonly Cube RightBoot;
        private readonly Cube Backpack;
        private readonly Cube HandItem;
        #endregion
    }
}
