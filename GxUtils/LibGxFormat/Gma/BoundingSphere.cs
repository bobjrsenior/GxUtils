using OpenTK;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LibGxFormat
{
    /// <summary>
    /// Class to calculate the bounding sphere that contains a set of points.
    /// Based on http://stackoverflow.com/a/24818473, by Andres Hernandez.
    /// </summary>
    public class BoundingSphere
    {
        /// <summary>
        /// The center of the bounding sphere.
        /// </summary>
        public Vector3 Center { get; private set; }
        /// <summary>
        /// The radius of the bounding sphere.
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Create a new bounding sphere from its center and radius.
        /// </summary>
        /// <param name="center">The center of the bounding sphere.</param>
        /// <param name="radius">The radius of the bounding sphere.</param>
        public BoundingSphere(Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Find the bounding sphere containing the given set of points.
        /// </summary>
        /// <param name="points">The points that the bounding sphere should contain.</param>
        /// <returns>The bounding sphere containing the given set of points.</returns>
        public static BoundingSphere FromPoints(IEnumerable<Vector3> points)
        {
            Vector3 center = points.First();
            float radius = 0.0001f;

            for (int i = 0; i < 2; i++)
            {
                foreach (Vector3 pos in points)
                {
                    Vector3 diff = pos - center;
                    float len = diff.Length;
                    if (len > radius)
                    {
                        float alpha = len / radius;
                        float alphaSq = alpha * alpha;
                        radius = 0.5f * (alpha + 1 / alpha) * radius;
                        center = 0.5f * ((1 + 1 / alphaSq) * center + (1 - 1 / alphaSq) * pos);
                    }
                }
            }

            foreach (Vector3 pos in points)
            {
                Vector3 diff = pos - center;
                float len = diff.Length;
                if (len > radius)
                {
                    radius = (radius + len) / 2.0f;
                    center = center + ((len - radius) / len * diff);
                }
            }

            return new BoundingSphere(center, radius);
        }
    }
}
