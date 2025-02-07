using engine.Common;
using engine.Common.Entities3D;
namespace engine.Common.Entities3D
{
    public class Icosphere : Element3D
    {
        public Icosphere()
        {
            var multiplier = 15f;
            Width = 1.902116f * multiplier;
            Height = 2f * multiplier;
            Depth = 2f * multiplier;
            UniformColor = new RGBA() { R = 255, G = 0, B = 255, A = 255 };
            Polygons = new Point[][]
            {
                new Point[] { new Point() { X = 0f, Y = -0.5f, Z = 0f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = 0.1545055f }, new Point() { X = -0.08540805f, Y = -0.425327f, Z = 0.2499975f } },
                new Point[] { new Point() { X = 0.38042215f, Y = -0.22361f, Z = 0.2628625f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = 0.1545055f }, new Point() { X = 0.4472114f, Y = -0.262868f, Z = 0f } },
                new Point[] { new Point() { X = 0f, Y = -0.5f, Z = 0f }, new Point() { X = -0.08540805f, Y = -0.425327f, Z = 0.2499975f }, new Point() { X = -0.2763922f, Y = -0.425326f, Z = 0f } },
                new Point[] { new Point() { X = 0f, Y = -0.5f, Z = 0f }, new Point() { X = -0.2763922f, Y = -0.425326f, Z = 0f }, new Point() { X = -0.08540805f, Y = -0.425327f, Z = -0.2499975f } },
                new Point[] { new Point() { X = 0f, Y = -0.5f, Z = 0f }, new Point() { X = -0.08540805f, Y = -0.425327f, Z = -0.2499975f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = -0.1545055f } },
                new Point[] { new Point() { X = 0.38042215f, Y = -0.22361f, Z = 0.2628625f }, new Point() { X = 0.4472114f, Y = -0.262868f, Z = 0f }, new Point() { X = 0.5f, Y = 0f, Z = 0.1545065f } },
                new Point[] { new Point() { X = -0.14530554f, Y = -0.22361f, Z = 0.4253245f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = 0.404506f }, new Point() { X = 0f, Y = 0f, Z = 0.5f } },
                new Point[] { new Point() { X = -0.47022685f, Y = -0.223608f, Z = 0f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = 0.2499985f }, new Point() { X = -0.5f, Y = 0f, Z = 0.1545065f } },
                new Point[] { new Point() { X = -0.14530554f, Y = -0.22361f, Z = -0.4253245f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = -0.2499985f }, new Point() { X = -0.3090169f, Y = 0f, Z = -0.4045085f } },
                new Point[] { new Point() { X = 0.38042215f, Y = -0.22361f, Z = -0.2628625f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = -0.404506f }, new Point() { X = 0.3090169f, Y = 0f, Z = -0.4045085f } },
                new Point[] { new Point() { X = 0.38042215f, Y = -0.22361f, Z = 0.2628625f }, new Point() { X = 0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = 0.3090169f, Y = 0f, Z = 0.4045085f } },
                new Point[] { new Point() { X = -0.14530554f, Y = -0.22361f, Z = 0.4253245f }, new Point() { X = 0f, Y = 0f, Z = 0.5f }, new Point() { X = -0.3090169f, Y = 0f, Z = 0.4045085f } },
                new Point[] { new Point() { X = -0.47022685f, Y = -0.223608f, Z = 0f }, new Point() { X = -0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = -0.5f, Y = 0f, Z = -0.1545065f } },
                new Point[] { new Point() { X = -0.14530554f, Y = -0.22361f, Z = -0.4253245f }, new Point() { X = -0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = 0f, Y = 0f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.38042215f, Y = -0.22361f, Z = -0.2628625f }, new Point() { X = 0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = 0.5f, Y = 0f, Z = -0.1545065f } },
                new Point[] { new Point() { X = 0.14530554f, Y = 0.22361f, Z = 0.4253245f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = 0.2499985f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = 0.2499975f } },
                new Point[] { new Point() { X = -0.38042215f, Y = 0.22361f, Z = 0.2628625f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = 0.404506f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = 0.1545055f } },
                new Point[] { new Point() { X = -0.38042215f, Y = 0.22361f, Z = -0.2628625f }, new Point() { X = -0.4472114f, Y = 0.262868f, Z = 0f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = -0.1545055f } },
                new Point[] { new Point() { X = 0.14530554f, Y = 0.22361f, Z = -0.4253245f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = -0.404506f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = -0.2499975f } },
                new Point[] { new Point() { X = 0.47022685f, Y = 0.223608f, Z = 0f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = -0.2499985f }, new Point() { X = 0.2763922f, Y = 0.425326f, Z = 0f } },
                new Point[] { new Point() { X = 0.2763922f, Y = 0.425326f, Z = 0f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = -0.2499975f }, new Point() { X = 0f, Y = 0.5f, Z = 0f } },
                new Point[] { new Point() { X = 0.2763922f, Y = 0.425326f, Z = 0f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = -0.2499985f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = -0.2499975f } },
                new Point[] { new Point() { X = 0.36180183f, Y = 0.262868f, Z = -0.2499985f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = -0.4253245f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = -0.2499975f } },
                new Point[] { new Point() { X = 0.08540805f, Y = 0.425327f, Z = -0.2499975f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = -0.1545055f }, new Point() { X = 0f, Y = 0.5f, Z = 0f } },
                new Point[] { new Point() { X = 0.08540805f, Y = 0.425327f, Z = -0.2499975f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = -0.404506f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = -0.1545055f } },
                new Point[] { new Point() { X = -0.1381982f, Y = 0.262869f, Z = -0.404506f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = -0.2628625f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = -0.1545055f } },
                new Point[] { new Point() { X = -0.2236052f, Y = 0.425327f, Z = -0.1545055f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = 0.1545055f }, new Point() { X = 0f, Y = 0.5f, Z = 0f } },
                new Point[] { new Point() { X = -0.2236052f, Y = 0.425327f, Z = -0.1545055f }, new Point() { X = -0.4472114f, Y = 0.262868f, Z = 0f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = 0.1545055f } },
                new Point[] { new Point() { X = -0.4472114f, Y = 0.262868f, Z = 0f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = 0.2628625f }, new Point() { X = -0.2236052f, Y = 0.425327f, Z = 0.1545055f } },
                new Point[] { new Point() { X = -0.2236052f, Y = 0.425327f, Z = 0.1545055f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = 0.2499975f }, new Point() { X = 0f, Y = 0.5f, Z = 0f } },
                new Point[] { new Point() { X = -0.2236052f, Y = 0.425327f, Z = 0.1545055f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = 0.404506f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = 0.2499975f } },
                new Point[] { new Point() { X = -0.1381982f, Y = 0.262869f, Z = 0.404506f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = 0.4253245f }, new Point() { X = 0.08540805f, Y = 0.425327f, Z = 0.2499975f } },
                new Point[] { new Point() { X = 0.08540805f, Y = 0.425327f, Z = 0.2499975f }, new Point() { X = 0.2763922f, Y = 0.425326f, Z = 0f }, new Point() { X = 0f, Y = 0.5f, Z = 0f } },
                new Point[] { new Point() { X = 0.08540805f, Y = 0.425327f, Z = 0.2499975f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = 0.2499985f }, new Point() { X = 0.2763922f, Y = 0.425326f, Z = 0f } },
                new Point[] { new Point() { X = 0.36180183f, Y = 0.262868f, Z = 0.2499985f }, new Point() { X = 0.47022685f, Y = 0.223608f, Z = 0f }, new Point() { X = 0.2763922f, Y = 0.425326f, Z = 0f } },
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = -0.1545065f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = -0.2499985f }, new Point() { X = 0.47022685f, Y = 0.223608f, Z = 0f } },
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = -0.1545065f }, new Point() { X = 0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = -0.2499985f } },
                new Point[] { new Point() { X = 0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = -0.4253245f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = -0.2499985f } },
                new Point[] { new Point() { X = 0f, Y = 0f, Z = -0.5f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = -0.404506f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = -0.4253245f } },
                new Point[] { new Point() { X = 0f, Y = 0f, Z = -0.5f }, new Point() { X = -0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = -0.404506f } },
                new Point[] { new Point() { X = -0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = -0.2628625f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = -0.404506f } },
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = -0.1545065f }, new Point() { X = -0.4472114f, Y = 0.262868f, Z = 0f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = -0.2628625f } },
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = -0.1545065f }, new Point() { X = -0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = -0.4472114f, Y = 0.262868f, Z = 0f } },
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = 0.2628625f }, new Point() { X = -0.4472114f, Y = 0.262868f, Z = 0f } },
                new Point[] { new Point() { X = -0.3090169f, Y = 0f, Z = 0.4045085f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = 0.404506f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = 0.2628625f } },
                new Point[] { new Point() { X = -0.3090169f, Y = 0f, Z = 0.4045085f }, new Point() { X = 0f, Y = 0f, Z = 0.5f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = 0.404506f } },
                new Point[] { new Point() { X = 0f, Y = 0f, Z = 0.5f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = 0.4253245f }, new Point() { X = -0.1381982f, Y = 0.262869f, Z = 0.404506f } },
                new Point[] { new Point() { X = 0.3090169f, Y = 0f, Z = 0.4045085f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = 0.2499985f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = 0.4253245f } },
                new Point[] { new Point() { X = 0.3090169f, Y = 0f, Z = 0.4045085f }, new Point() { X = 0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = 0.2499985f } },
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = 0.47022685f, Y = 0.223608f, Z = 0f }, new Point() { X = 0.36180183f, Y = 0.262868f, Z = 0.2499985f } },
                new Point[] { new Point() { X = 0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = 0f, Y = 0f, Z = -0.5f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = -0.4253245f } },
                new Point[] { new Point() { X = 0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = -0.404506f }, new Point() { X = 0f, Y = 0f, Z = -0.5f } },
                new Point[] { new Point() { X = 0.1381982f, Y = -0.262869f, Z = -0.404506f }, new Point() { X = -0.14530554f, Y = -0.22361f, Z = -0.4253245f }, new Point() { X = 0f, Y = 0f, Z = -0.5f } },
                new Point[] { new Point() { X = -0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = -0.5f, Y = 0f, Z = -0.1545065f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = -0.2628625f } },
                new Point[] { new Point() { X = -0.3090169f, Y = 0f, Z = -0.4045085f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = -0.2499985f }, new Point() { X = -0.5f, Y = 0f, Z = -0.1545065f } },
                new Point[] { new Point() { X = -0.36180183f, Y = -0.262868f, Z = -0.2499985f }, new Point() { X = -0.47022685f, Y = -0.223608f, Z = 0f }, new Point() { X = -0.5f, Y = 0f, Z = -0.1545065f } },
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = -0.3090169f, Y = 0f, Z = 0.4045085f }, new Point() { X = -0.38042215f, Y = 0.22361f, Z = 0.2628625f } },
                new Point[] { new Point() { X = -0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = 0.2499985f }, new Point() { X = -0.3090169f, Y = 0f, Z = 0.4045085f } },
                new Point[] { new Point() { X = -0.36180183f, Y = -0.262868f, Z = 0.2499985f }, new Point() { X = -0.14530554f, Y = -0.22361f, Z = 0.4253245f }, new Point() { X = -0.3090169f, Y = 0f, Z = 0.4045085f } },
                new Point[] { new Point() { X = 0f, Y = 0f, Z = 0.5f }, new Point() { X = 0.3090169f, Y = 0f, Z = 0.4045085f }, new Point() { X = 0.14530554f, Y = 0.22361f, Z = 0.4253245f } },
                new Point[] { new Point() { X = 0f, Y = 0f, Z = 0.5f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = 0.404506f }, new Point() { X = 0.3090169f, Y = 0f, Z = 0.4045085f } },
                new Point[] { new Point() { X = 0.1381982f, Y = -0.262869f, Z = 0.404506f }, new Point() { X = 0.38042215f, Y = -0.22361f, Z = 0.2628625f }, new Point() { X = 0.3090169f, Y = 0f, Z = 0.4045085f } },
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = 0.5f, Y = 0f, Z = -0.1545065f }, new Point() { X = 0.47022685f, Y = 0.223608f, Z = 0f } },
                new Point[] { new Point() { X = 0.5f, Y = 0f, Z = 0.1545065f }, new Point() { X = 0.4472114f, Y = -0.262868f, Z = 0f }, new Point() { X = 0.5f, Y = 0f, Z = -0.1545065f } },
                new Point[] { new Point() { X = 0.4472114f, Y = -0.262868f, Z = 0f }, new Point() { X = 0.38042215f, Y = -0.22361f, Z = -0.2628625f }, new Point() { X = 0.5f, Y = 0f, Z = -0.1545065f } },
                new Point[] { new Point() { X = 0.2236052f, Y = -0.425327f, Z = -0.1545055f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = -0.404506f }, new Point() { X = 0.38042215f, Y = -0.22361f, Z = -0.2628625f } },
                new Point[] { new Point() { X = 0.2236052f, Y = -0.425327f, Z = -0.1545055f }, new Point() { X = -0.08540805f, Y = -0.425327f, Z = -0.2499975f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = -0.404506f } },
                new Point[] { new Point() { X = -0.08540805f, Y = -0.425327f, Z = -0.2499975f }, new Point() { X = -0.14530554f, Y = -0.22361f, Z = -0.4253245f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = -0.404506f } },
                new Point[] { new Point() { X = -0.08540805f, Y = -0.425327f, Z = -0.2499975f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = -0.2499985f }, new Point() { X = -0.14530554f, Y = -0.22361f, Z = -0.4253245f } },
                new Point[] { new Point() { X = -0.08540805f, Y = -0.425327f, Z = -0.2499975f }, new Point() { X = -0.2763922f, Y = -0.425326f, Z = 0f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = -0.2499985f } },
                new Point[] { new Point() { X = -0.2763922f, Y = -0.425326f, Z = 0f }, new Point() { X = -0.47022685f, Y = -0.223608f, Z = 0f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = -0.2499985f } },
                new Point[] { new Point() { X = -0.2763922f, Y = -0.425326f, Z = 0f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = 0.2499985f }, new Point() { X = -0.47022685f, Y = -0.223608f, Z = 0f } },
                new Point[] { new Point() { X = -0.2763922f, Y = -0.425326f, Z = 0f }, new Point() { X = -0.08540805f, Y = -0.425327f, Z = 0.2499975f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = 0.2499985f } },
                new Point[] { new Point() { X = -0.08540805f, Y = -0.425327f, Z = 0.2499975f }, new Point() { X = -0.14530554f, Y = -0.22361f, Z = 0.4253245f }, new Point() { X = -0.36180183f, Y = -0.262868f, Z = 0.2499985f } },
                new Point[] { new Point() { X = 0.4472114f, Y = -0.262868f, Z = 0f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = -0.1545055f }, new Point() { X = 0.38042215f, Y = -0.22361f, Z = -0.2628625f } },
                new Point[] { new Point() { X = 0.4472114f, Y = -0.262868f, Z = 0f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = 0.1545055f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = -0.1545055f } },
                new Point[] { new Point() { X = 0.2236052f, Y = -0.425327f, Z = 0.1545055f }, new Point() { X = 0f, Y = -0.5f, Z = 0f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = -0.1545055f } },
                new Point[] { new Point() { X = -0.08540805f, Y = -0.425327f, Z = 0.2499975f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = 0.404506f }, new Point() { X = -0.14530554f, Y = -0.22361f, Z = 0.4253245f } },
                new Point[] { new Point() { X = -0.08540805f, Y = -0.425327f, Z = 0.2499975f }, new Point() { X = 0.2236052f, Y = -0.425327f, Z = 0.1545055f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = 0.404506f } },
                new Point[] { new Point() { X = 0.2236052f, Y = -0.425327f, Z = 0.1545055f }, new Point() { X = 0.38042215f, Y = -0.22361f, Z = 0.2628625f }, new Point() { X = 0.1381982f, Y = -0.262869f, Z = 0.404506f } },
            };
        }
    }
}
