using System;
using System.Numerics;

namespace Template
{

    class Camera
    {
        public Vector3 pos;
        public Vector3 target;
        public float focalDistance;
        public Vector3 E;
        Vector3 p1, p2, p3, up, right;
        int screenWidth, screenHeight;
        float aspectRatio, lensSize;

        Vector<float> p1x4, p1y4, p1z4, p2x4, p2y4, p2z4, p3x4, p3y4, p3z4;
        Vector<float> screenWidth4, screenHeight4;
        Vector<float> posx4, posy4, posz4, lensSize4;
        Vector<float> upx4, upy4, upz4, rightx4, righty4, rightz4;

        public Camera( int w, int h )
        {
            screenWidth = w;
            screenHeight = h;
            float[] ws = { w, w, w, w };
            float[] hs = { h, h, h, h };
            screenWidth4 = new Vector<float>( ws );
            screenHeight4 = new Vector<float>( hs );

            aspectRatio = (float)w / (float)h;

            lensSize = 0.04f;
            float[] lensSizes = { lensSize, lensSize, lensSize, lensSize };
            lensSize4 = new Vector<float>( lensSizes );

            pos = new Vector3( -0.94f, -0.037f, -3.342f );
            float[] posxs = { pos.X, pos.X, pos.X, pos.X };
            float[] posys = { pos.Y, pos.Y, pos.Y, pos.Y };
            float[] poszs = { pos.Z, pos.Z, pos.Z, pos.Z };
            posx4 = new Vector<float>( posxs );
            posy4 = new Vector<float>( posys );
            posz4 = new Vector<float>( poszs );

            target = new Vector3( -0.418f, -0.026f, -2.435f );
            Update();
        }

        public bool HandleInput()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            target = pos + E;
            bool changed = false;
            if ( keyboard[ OpenTK.Input.Key.A ] )
            { changed = true; pos -= right * 0.1f; target -= right * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.D ] )
            { changed = true; pos += right * 0.1f; target += right * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.W ] )
            { changed = true; pos += E * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.S ] )
            { changed = true; pos -= E * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.R ] )
            { changed = true; pos += up * 0.1f; target += up * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.F ] )
            { changed = true; pos -= up * 0.1f; target -= up * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.Up ] )
            { changed = true; target -= up * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.Down ] )
            { changed = true; target += up * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.Left ] )
            { changed = true; target -= right * 0.1f; }
            if ( keyboard[ OpenTK.Input.Key.Right ] )
            { changed = true; target += right * 0.1f; }
            if ( changed )
            {
                Update();
                return true;
            }
            return false;
        }

        public void Update()
        {
            // construct a look-at matrix
            E = Vector3.Normalize( target - pos );
            up = Vector3.UnitY;
            right = Vector3.Cross( up, E );
            float[] rightxs = { right.X, right.X, right.X, right.X };
            float[] rightys = { right.Y, right.Y, right.Y, right.Y };
            float[] rightzs = { right.Z, right.Z, right.Z, right.Z };
            rightx4 = new Vector<float>( rightxs );
            righty4 = new Vector<float>( rightys );
            rightz4 = new Vector<float>( rightzs );

            up = Vector3.Cross( E, right );
            float[] upxs = { up.X, up.X, up.X, up.X };
            float[] upys = { up.Y, up.Y, up.Y, up.Y };
            float[] upzs = { up.Z, up.Z, up.Z, up.Z };
            upx4 = new Vector<float>( upxs );
            upy4 = new Vector<float>( upys );
            upz4 = new Vector<float>( upzs );

            // calculate focal distance
            Ray ray = new Ray( pos, E, 1e34f );
            Scene.Intersect( ray );
            focalDistance = Math.Min( 20, ray.t );
            // calculate virtual screen corners
            Vector3 C = pos + focalDistance * E;
            p1 = C - 0.5f * focalDistance * aspectRatio * right + 0.5f * focalDistance * up;
            p2 = C + 0.5f * focalDistance * aspectRatio * right + 0.5f * focalDistance * up;
            p3 = C - 0.5f * focalDistance * aspectRatio * right - 0.5f * focalDistance * up;

            float[] p1sX = { p1.X, p1.X, p1.X, p1.X };
            float[] p2sX = { p2.X, p2.X, p2.X, p2.X };
            float[] p3sX = { p3.X, p3.X, p3.X, p3.X };

            float[] p1sY = { p1.Y, p1.Y, p1.Y, p1.Y };
            float[] p2sY = { p2.Y, p2.Y, p2.Y, p2.Y };
            float[] p3sY = { p3.Y, p3.Y, p3.Y, p3.Y };

            float[] p1sZ = { p1.Z, p1.Z, p1.Z, p1.Z };
            float[] p2sZ = { p2.Z, p2.Z, p2.Z, p2.Z };
            float[] p3sZ = { p3.Z, p3.Z, p3.Z, p3.Z };

            p1x4 = new Vector<float>( p1sX );
            p1y4 = new Vector<float>( p1sY );
            p1z4 = new Vector<float>( p1sZ );

            p2x4 = new Vector<float>( p2sX );
            p2y4 = new Vector<float>( p2sY );
            p2z4 = new Vector<float>( p2sZ );

            p3x4 = new Vector<float>( p3sX );
            p3y4 = new Vector<float>( p3sY );
            p3z4 = new Vector<float>( p3sZ );
        }

        public Ray Generate( Random rng, int x, int y )
        {
            float r0 = (float)rng.NextDouble();
            float r1 = (float)rng.NextDouble();
            float r2 = (float)rng.NextDouble() - 0.5f;
            float r3 = (float)rng.NextDouble() - 0.5f;
            // calculate sub-pixel ray target position on screen plane
            float u = ((float)x + r0) / (float)screenWidth;
            float v = ((float)y + r1) / (float)screenHeight;
            Vector3 T = p1 + u * (p2 - p1) + v * (p3 - p1);
            // calculate position on aperture
            Vector3 P = pos + lensSize * (r2 * right + r3 * up);
            // calculate ray direction
            Vector3 D = Vector3.Normalize( T - P );
            // return new primary ray
            return new Ray( P, D, 1e34f );
        }

        public Ray4 Generate4( Random rng, int x, int y )
        {
            float[] r0 = { (float)rng.NextDouble(), (float)rng.NextDouble(),
                           (float)rng.NextDouble(), (float)rng.NextDouble() };
            Vector<float> r0_4 = new Vector<float>( r0 );
            float[] r1 = { (float)rng.NextDouble(), (float)rng.NextDouble(),
                           (float)rng.NextDouble(), (float)rng.NextDouble() };
            Vector<float> r1_4 = new Vector<float>( r1 );
            float[] r2 = { (float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f,
                           (float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f };
            Vector<float> r2_4 = new Vector<float>( r2 );
            float[] r3 = { (float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f,
                           (float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f };
            Vector<float> r3_4 = new Vector<float>( r3 );

            // calculate sub-pixel ray target position on screen plane
            float[] values = { x, x + 1, x + 2, x + 3 };
            Vector<float> x4 = new Vector<float>( values );
            Vector<float> y4 = new Vector<float>( y );
            Vector<float> u4 = (x4 + r0_4) / screenWidth4;
            Vector<float> v4 = (y4 + r1_4) / screenHeight4;

            Vector<float> Tx4 = p1x4 + u4 * (p2x4 - p1x4) + v4 * (p3x4 - p1x4);
            Vector<float> Ty4 = p1y4 + u4 * (p2y4 - p1y4) + v4 * (p3y4 - p1y4);
            Vector<float> Tz4 = p1z4 + u4 * (p2z4 - p1z4) + v4 * (p3z4 - p1z4);

            // calculate position on aperture
            Vector<float> Px4 = posx4 + lensSize4 * (r2_4 * rightx4 + r3_4 * upx4);
            Vector<float> Py4 = posy4 + lensSize4 * (r2_4 * righty4 + r3_4 * upy4);
            Vector<float> Pz4 = posz4 + lensSize4 * (r2_4 * rightz4 + r3_4 * upz4);

            // calculate ray direction
            Vector<float> Dx4 = Tx4 - Px4;
            Vector<float> Dy4 = Ty4 - Py4;
            Vector<float> Dz4 = Tz4 - Pz4;
            Vector<float> len4 = Vector.SquareRoot<float>( Dx4 * Dx4 + Dy4 * Dy4 + Dz4 * Dz4 );
            Dx4 /= len4;
            Dy4 /= len4;
            Dz4 /= len4;

            // return new primary rays
            Ray4 r4 = new Ray4();
            r4.Ox4 = Px4;
            r4.Oy4 = Py4;
            r4.Oz4 = Pz4;
            r4.Dx4 = Dx4;
            r4.Dy4 = Dy4;
            r4.Dz4 = Dz4;
            r4.t4 = new Vector<float>( 1e34f );
            
            return r4;
        }
    }

} // namespace Template
