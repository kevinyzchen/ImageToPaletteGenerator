using System;

namespace PercetualColors
{
    /// <summary>
    /// Implementation of https://bottosson.github.io/posts/oklab/ in C#
    /// </summary>
    public static class ColorConversion
    {
        private const double Pi = 3.1415926535897932384626433832795028841971693993751058209749445923078164062f;

        private struct LC
        {
            public double L;
            public double C;
        }

        private struct ST
        {
            public double S;
            public double T;
        }

        private static double Clamp(double x, double min, double max)
        {
            if (x < min)
                return min;
            if (x > max)
                return max;

            return x;
        }

        private static double Sign(double x)
        {
            return Convert.ToSingle(0.0f < x) - Convert.ToSingle(x < 0.0f);
        }

        public static double SrgbTransferFunction(double a)
        {
            return .0031308f >= a ? 12.92f * a : 1.055f * Math.Pow(a, .4166666666666667f) - .055f;
        }

        public static double SrgbTransferFunctionInv(double a)
        {
            return .04045f < a ? Math.Pow((a + .055f) / 1.055f, 2.4f) : a / 12.92f;
        }

        public static Lab LinearSrgbToOklab(sRGB c)
        {
            var l = 0.4122214708f * c.R + 0.5363325363f * c.G + 0.0514459929f * c.B;
            var m = 0.2119034982f * c.R + 0.6806995451f * c.G + 0.1073969566f * c.B;
            var s = 0.0883024619f * c.R + 0.2817188376f * c.G + 0.6299787005f * c.B;

            var l_ = Math.Pow(l, .333333333f);
            var m_ = Math.Pow(m, .333333333f);
            var s_ = Math.Pow(s, .333333333f);

            return new Lab(0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
                1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
                0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_);
        }

        public static sRGB OklabToLinearSrgb(Lab c)
        {
            var l_ = c.L + 0.3963377774f * c.a + 0.2158037573f * c.b;
            var m_ = c.L - 0.1055613458f * c.a - 0.0638541728f * c.b;
            var s_ = c.L - 0.0894841775f * c.a - 1.2914855480f * c.b;

            var l = l_ * l_ * l_;
            var m = m_ * m_ * m_;
            var s = s_ * s_ * s_;

            return new sRGB(
                +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
                -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
                -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s);
        }

        // Finds the maximum saturation possible for a given hue that fits in sRGB
        // Saturation here is defined as S = C/L
        // a and b must be normalized so a^2 + b^2 == 1
        private static double compute_max_saturation(double a, double b)
        {
            // Max saturation will be when one of r, g or b goes below zero.

            // Select different coefficients depending on which component goes below zero first
            double k0, k1, k2, k3, k4, wl, wm, ws;

            if (-1.88170328f * a - 0.80936493f * b > 1)
            {
                // Red component
                k0 = +1.19086277f;
                k1 = +1.76576728f;
                k2 = +0.59662641f;
                k3 = +0.75515197f;
                k4 = +0.56771245f;
                wl = +4.0767416621f;
                wm = -3.3077115913f;
                ws = +0.2309699292f;
            }
            else if (1.81444104f * a - 1.19445276f * b > 1)
            {
                // Green component
                k0 = +0.73956515f;
                k1 = -0.45954404f;
                k2 = +0.08285427f;
                k3 = +0.12541070f;
                k4 = +0.14503204f;
                wl = -1.2684380046f;
                wm = +2.6097574011f;
                ws = -0.3413193965f;
            }
            else
            {
                // Blue component
                k0 = +1.35733652f;
                k1 = -0.00915799f;
                k2 = -1.15130210f;
                k3 = -0.50559606f;
                k4 = +0.00692167f;
                wl = -0.0041960863f;
                wm = -0.7034186147f;
                ws = +1.7076147010f;
            }

            // Approximate max saturation using a polynomial:
            var S = k0 + k1 * a + k2 * b + k3 * a * a + k4 * a * b;

            // Do one step Halley's method to get closer
            // this gives an error less than 10e6, except for some blue hues where the dS/dh is close to infinite
            // this should be sufficient for most applications, otherwise do two/three steps 

            var k_l = +0.3963377774f * a + 0.2158037573f * b;
            var k_m = -0.1055613458f * a - 0.0638541728f * b;
            var k_s = -0.0894841775f * a - 1.2914855480f * b;

            {
                var l_ = 1.0f + S * k_l;
                var m_ = 1.0f + S * k_m;
                var s_ = 1.0f + S * k_s;

                var l = l_ * l_ * l_;
                var m = m_ * m_ * m_;
                var s = s_ * s_ * s_;

                var l_dS = 3.0f * k_l * l_ * l_;
                var m_dS = 3.0f * k_m * m_ * m_;
                var s_dS = 3.0f * k_s * s_ * s_;

                var l_dS2 = 6.0f * k_l * k_l * l_;
                var m_dS2 = 6.0f * k_m * k_m * m_;
                var s_dS2 = 6.0f * k_s * k_s * s_;

                var f = wl * l + wm * m + ws * s;
                var f1 = wl * l_dS + wm * m_dS + ws * s_dS;
                var f2 = wl * l_dS2 + wm * m_dS2 + ws * s_dS2;

                S = S - f * f1 / (f1 * f1 - 0.5f * f * f2);
            }

            return S;
        }

        // finds L_cusp and C_cusp for a given hue
        // a and b must be normalized so a^2 + b^2 == 1
        private static LC FindCusp(double a, double b)
        {
            // First, find the maximum saturation (saturation S = C/L)
            var S_cusp = compute_max_saturation(a, b);

            // Convert to linear sRGB to find the first point where at least one of r,g or b >= 1:
            var rgb_at_max = OklabToLinearSrgb(new Lab(1, S_cusp * a, S_cusp * b));
            var L_cusp = Cbrtd(1.0f / Math.Max(Math.Max(rgb_at_max.R, rgb_at_max.G), rgb_at_max.B));
            var C_cusp = L_cusp * S_cusp;

            return new LC { L = L_cusp, C = C_cusp };
        }

        // Finds intersection of the line defined by 
        // L = L0 * (1 - t) + t * L1;
        // C = t * C1;
        // a and b must be normalized so a^2 + b^2 == 1
        private static double FindGamutIntersection(double a, double b, double L1, double C1, double L0, LC cusp)
        {
            // Find the intersection for upper and lower half seprately
            double t;
            if ((L1 - L0) * cusp.C - (cusp.L - L0) * C1 <= 0.0f)
            {
                // Lower half

                t = cusp.C * L0 / (C1 * cusp.L + cusp.C * (L0 - L1));
            }
            else
            {
                // Upper half

                // First intersect with triangle
                t = cusp.C * (L0 - 1.0f) / (C1 * (cusp.L - 1.0f) + cusp.C * (L0 - L1));

                // Then one step Halley's method
                {
                    var dL = L1 - L0;
                    var dC = C1;

                    var k_l = +0.3963377774f * a + 0.2158037573f * b;
                    var k_m = -0.1055613458f * a - 0.0638541728f * b;
                    var k_s = -0.0894841775f * a - 1.2914855480f * b;

                    var l_dt = dL + dC * k_l;
                    var m_dt = dL + dC * k_m;
                    var s_dt = dL + dC * k_s;


                    // If higher accuracy is required, 2 or 3 iterations of the following block can be used:
                    {
                        var L = L0 * (1.0f - t) + t * L1;
                        var C = t * C1;

                        var l_ = L + C * k_l;
                        var m_ = L + C * k_m;
                        var s_ = L + C * k_s;

                        var l = l_ * l_ * l_;
                        var m = m_ * m_ * m_;
                        var s = s_ * s_ * s_;

                        var ldt = 3 * l_dt * l_ * l_;
                        var mdt = 3 * m_dt * m_ * m_;
                        var sdt = 3 * s_dt * s_ * s_;

                        var ldt2 = 6 * l_dt * l_dt * l_;
                        var mdt2 = 6 * m_dt * m_dt * m_;
                        var sdt2 = 6 * s_dt * s_dt * s_;

                        var r0 = 4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s - 1;
                        var r1 = 4.0767416621f * ldt - 3.3077115913f * mdt + 0.2309699292f * sdt;
                        var r2 = 4.0767416621f * ldt2 - 3.3077115913f * mdt2 + 0.2309699292f * sdt2;

                        var u_r = r1 / (r1 * r1 - 0.5f * r0 * r2);
                        var t_r = -r0 * u_r;

                        var g0 = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s - 1;
                        var g1 = -1.2684380046f * ldt + 2.6097574011f * mdt - 0.3413193965f * sdt;
                        var g2 = -1.2684380046f * ldt2 + 2.6097574011f * mdt2 - 0.3413193965f * sdt2;

                        var u_g = g1 / (g1 * g1 - 0.5f * g0 * g2);
                        var t_g = -g0 * u_g;

                        var b0 = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s - 1;
                        var b1 = -0.0041960863f * ldt - 0.7034186147f * mdt + 1.7076147010f * sdt;
                        var b2 = -0.0041960863f * ldt2 - 0.7034186147f * mdt2 + 1.7076147010f * sdt2;

                        var u_b = b1 / (b1 * b1 - 0.5f * b0 * b2);
                        var t_b = -b0 * u_b;

                        t_r = u_r >= 0.0f ? t_r : double.MaxValue;
                        t_g = u_g >= 0.0f ? t_g : double.MaxValue;
                        t_b = u_b >= 0.0f ? t_b : double.MaxValue;

                        t += Math.Min(t_r, Math.Min(t_g, t_b));
                    }
                }
            }

            return t;
        }

        private static double FindGamutIntersection(double a, double b, double L1, double C1, double L0)
        {
            // Find the cusp of the gamut triangle
            var cusp = FindCusp(a, b);

            return FindGamutIntersection(a, b, L1, C1, L0, cusp);
        }

        private static sRGB GamutClipPreserveChroma(sRGB rgb)
        {
            if (rgb.R < 1 && rgb.G < 1 && rgb.B < 1 && rgb.R > 0 && rgb.G > 0 && rgb.B > 0)
                return rgb;

            var lab = LinearSrgbToOklab(rgb);

            var L = lab.L;
            var eps = 0.00001f;
            var C = Math.Max(eps, Math.Sqrt(lab.a * lab.a + lab.b * lab.b));
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            var L0 = Clamp(L, 0d, 1d);

            var t = FindGamutIntersection(a_, b_, L, C, L0);
            var L_clipped = L0 * (1 - t) + t * L;
            var C_clipped = t * C;

            return OklabToLinearSrgb(new Lab(L_clipped, C_clipped * a_, C_clipped * b_));
        }

        private static sRGB GamutClipProjectGamutClipProjectTo05(sRGB rgb)
        {
            if (rgb.R < 1 && rgb.G < 1 && rgb.B < 1 && rgb.R > 0 && rgb.G > 0 && rgb.B > 0)
                return rgb;

            var lab = LinearSrgbToOklab(rgb);

            var L = lab.L;
            var eps = 0.00001f;
            var C = Math.Max(eps, Math.Sqrt(lab.a * lab.a + lab.b * lab.b));
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            var L0 = 0.5f;

            var t = FindGamutIntersection(a_, b_, L, C, L0);
            var L_clipped = L0 * (1 - t) + t * L;
            var C_clipped = t * C;

            return OklabToLinearSrgb(new Lab(L_clipped, C_clipped * a_, C_clipped * b_));
        }

        private static sRGB gamut_clip_project_to_L_cusp(sRGB rgb)
        {
            if (rgb.R < 1 && rgb.G < 1 && rgb.B < 1 && rgb.R > 0 && rgb.G > 0 && rgb.B > 0)
                return rgb;

            var lab = LinearSrgbToOklab(rgb);

            var L = lab.L;
            var eps = 0.00001f;
            var C = Math.Max(eps, Math.Sqrt(lab.a * lab.a + lab.b * lab.b));
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            // The cusp is computed here and in find_gamut_intersection, an optimized solution would only compute it once.
            var cusp = FindCusp(a_, b_);

            var L0 = cusp.L;

            var t = FindGamutIntersection(a_, b_, L, C, L0);

            var L_clipped = L0 * (1 - t) + t * L;
            var C_clipped = t * C;

            return OklabToLinearSrgb(new Lab(L_clipped, C_clipped * a_, C_clipped * b_));
        }

        private static sRGB GamutClipAdaptiveL005(sRGB rgb, double alpha = 0.05d)
        {
            if (rgb.R < 1 && rgb.G < 1 && rgb.B < 1 && rgb.R > 0 && rgb.G > 0 && rgb.B > 0)
                return rgb;

            var lab = LinearSrgbToOklab(rgb);

            var L = lab.L;
            var eps = 0.00001f;
            var C = Math.Max(eps, Math.Sqrt(lab.a * lab.a + lab.b * lab.b));
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            var Ld = L - 0.5f;
            var e1 = 0.5f + Math.Abs(Ld) + alpha * C;
            var L0 = 0.5f * (1.0f + Sign(Ld) * (e1 - Math.Sqrt(e1 * e1 - 2.0f * Math.Abs(Ld))));

            var t = FindGamutIntersection(a_, b_, L, C, L0);
            var L_clipped = L0 * (1.0f - t) + t * L;
            var C_clipped = t * C;

            return OklabToLinearSrgb(new Lab(L_clipped, C_clipped * a_, C_clipped * b_));
        }

        private static sRGB GamutClipAdaptiveL0LCusp(sRGB rgb, double alpha = 0.05f)
        {
            if (rgb.R < 1 && rgb.G < 1 && rgb.B < 1 && rgb.R > 0 && rgb.G > 0 && rgb.B > 0)
                return rgb;

            var lab = LinearSrgbToOklab(rgb);

            var L = lab.L;
            var eps = 0.00001d;
            var C = Math.Max(eps, Math.Sqrt(lab.a * lab.a + lab.b * lab.b));
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            // The cusp is computed here and in find_gamut_intersection, an optimized solution would only compute it once.
            var cusp = FindCusp(a_, b_);

            var Ld = L - cusp.L;
            var k = 2.0f * (Ld > 0 ? 1.0f - cusp.L : cusp.L);

            var e1 = 0.5f * k + Math.Abs(Ld) + alpha * C / k;
            var L0 = cusp.L + 0.5f * (Sign(Ld) * (e1 - Math.Sqrt(e1 * e1 - 2.0f * k * Math.Abs(Ld))));

            var t = FindGamutIntersection(a_, b_, L, C, L0);
            var L_clipped = L0 * (1.0f - t) + t * L;
            var C_clipped = t * C;

            return OklabToLinearSrgb(new Lab(L_clipped, C_clipped * a_, C_clipped * b_));
        }

        private static double Toe(double x)
        {
            const double k_1 = 0.206f;
            const double k_2 = 0.03f;
            const double k_3 = (1.0f + k_1) / (1.0f + k_2);
            return 0.5f * (k_3 * x - k_1 + Math.Sqrt((k_3 * x - k_1) * (k_3 * x - k_1) + 4 * k_2 * k_3 * x));
        }

        private static double ToeInv(double x)
        {
            const double k_1 = 0.206f;
            const double k_2 = 0.03f;
            const double k_3 = (1.0f + k_1) / (1.0f + k_2);
            return (x * x + k_1 * x) / (k_3 * (x + k_2));
        }

        private static ST ToSt(LC cusp)
        {
            var L = cusp.L;
            var C = cusp.C;
            return new ST { S = C / L, T = C / (1.0f + double.Epsilon - L) };
        }

        // Returns a smooth approximation of the location of the cusp
        // This polynomial was created by an optimization process
        // It has been designed so that S_mid < S_max and T_mid < T_max
        private static ST GETStMid(double a_, double b_)
        {
            var S = 0.11516993f + 1.0f / (
                +7.44778970f + 4.15901240f * b_
                             + a_ * (-2.19557347f + 1.75198401f * b_
                                                  + a_ * (-2.13704948f - 10.02301043f * b_
                                                          + a_ * (-4.24894561f + 5.38770819f * b_ + 4.69891013f * a_
                                                          )))
            );

            var T = 0.11239642f + 1.0f / (
                +1.61320320f - 0.68124379f * b_
                + a_ * (+0.40370612f + 0.90148123f * b_
                                     + a_ * (-0.27087943f + 0.61223990f * b_
                                                          + a_ * (+0.00299215f - 0.45399568f * b_ - 0.14661872f * a_
                                                          )))
            );

            return new ST { S = S, T = T };
        }

        private struct Cs
        {
            public double C_0;
            public double C_mid;
            public double C_max;
        }

        private static Cs GETCs(double L, double a_, double b_)
        {
            var cusp = FindCusp(a_, b_);

            var C_max = FindGamutIntersection(a_, b_, L, 1, L, cusp);
            var ST_max = ToSt(cusp);

            // Scale factor to compensate for the curved part of gamut shape:
            var k = C_max / Math.Min(L * ST_max.S, (1 - L) * ST_max.T);

            double C_mid;
            {
                var ST_mid = GETStMid(a_, b_);

                // Use a soft minimum function, instead of a sharp triangle shape to get a smooth value for chroma.
                var C_a = L * ST_mid.S;
                var C_b = (1.0f - L) * ST_mid.T;
                C_mid = 0.9f * k *
                        Math.Sqrt(
                            Math.Sqrt(1.0f / (1.0f / (C_a * C_a * C_a * C_a) + 1.0f / (C_b * C_b * C_b * C_b))));
            }

            double C_0;
            {
                // for C_0, the shape is independent of hue, so ST are constant. Values picked to roughly be the average values of ST.
                var C_a = L * 0.4f;
                var C_b = (1.0f - L) * 0.8f;

                // Use a soft minimum function, instead of a sharp triangle shape to get a smooth value for chroma.
                C_0 = Math.Sqrt(1.0f / (1.0f / (C_a * C_a) + 1.0f / (C_b * C_b)));
            }

            return new Cs { C_0 = C_0, C_mid = C_mid, C_max = C_max };
        }

        public static Lab OkhslToLab(HSL hsl)
        {
            var h = hsl.H;
            var s = hsl.S;
            var l = hsl.L;

            if (l == 1.0f)
                return new Lab(1f, 0f, 0f);

            if (l == 0.0f) return new Lab(0f, 0f, 0f);

            var a_ = Math.Cos(2.0f * Pi * h);
            var b_ = Math.Sin(2.0f * Pi * h);
            var L = ToeInv(l);

            var cs = GETCs(L, a_, b_);
            var C_0 = cs.C_0;
            var C_mid = cs.C_mid;
            var C_max = cs.C_max;

            var mid = 0.8f;
            var mid_inv = 1.25f;

            double C, t, k_0, k_1, k_2;

            if (s < mid)
            {
                t = mid_inv * s;

                k_1 = mid * C_0;
                k_2 = 1.0f - k_1 / C_mid;

                C = t * k_1 / (1.0f - k_2 * t);
            }
            else
            {
                t = (s - mid) / (1 - mid);

                k_0 = C_mid;
                k_1 = (1.0f - mid) * C_mid * C_mid * mid_inv * mid_inv / C_0;
                k_2 = 1.0f - k_1 / (C_max - C_mid);

                C = k_0 + t * k_1 / (1.0f - k_2 * t);
            }

            return new Lab(L, C * a_, C * b_);
        }

        public static sRGB OkhslToSrgb(HSL hsl)
        {
            var rgb = OklabToLinearSrgb(OkhslToLab(hsl));
            return new sRGB(rgb.R, rgb.G, rgb.B);
        }

        public static HSL LabToHSL(Lab lab)
        {
            var C = Math.Sqrt(lab.a * lab.a + lab.b * lab.b);
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            var L = lab.L;
            var h = 0.5f + 0.5f * Math.Atan2(-lab.b, -lab.a) / Pi;

            var cs = GETCs(L, a_, b_);
            var C_0 = cs.C_0;
            var C_mid = cs.C_mid;
            var C_max = cs.C_max;

            // Inverse of the interpolation in okhsl_to_srgb:

            var mid = 0.8f;
            var mid_inv = 1.25f;

            double s;
            if (C < C_mid)
            {
                var k_1 = mid * C_0;
                var k_2 = 1.0f - k_1 / C_mid;

                var t = C / (k_1 + k_2 * C);
                s = t * mid;
            }
            else
            {
                var k_0 = C_mid;
                var k_1 = (1.0f - mid) * C_mid * C_mid * mid_inv * mid_inv / C_0;
                var k_2 = 1.0f - k_1 / (C_max - C_mid);

                var t = (C - k_0) / (k_1 + k_2 * (C - k_0));
                s = mid + (1.0f - mid) * t;
            }

            var l = Toe(L);
            return new HSL(h, s, l);
        }


        public static HSL SrgbToOkhsl(sRGB rgb)
        {
            var lab = LinearSrgbToOklab(new sRGB(rgb.R, rgb.G, rgb.B));

            var C = Math.Sqrt(lab.a * lab.a + lab.b * lab.b);
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            var L = lab.L;
            var h = 0.5f + 0.5f * Math.Atan2(-lab.b, -lab.a) / Pi;

            var cs = GETCs(L, a_, b_);
            var C_0 = cs.C_0;
            var C_mid = cs.C_mid;
            var C_max = cs.C_max;

            // Inverse of the interpolation in okhsl_to_srgb:

            var mid = 0.8f;
            var mid_inv = 1.25f;

            double s;
            if (C < C_mid)
            {
                var k_1 = mid * C_0;
                var k_2 = 1.0f - k_1 / C_mid;

                var t = C / (k_1 + k_2 * C);
                s = t * mid;
            }
            else
            {
                var k_0 = C_mid;
                var k_1 = (1.0f - mid) * C_mid * C_mid * mid_inv * mid_inv / C_0;
                var k_2 = 1.0f - k_1 / (C_max - C_mid);

                var t = (C - k_0) / (k_1 + k_2 * (C - k_0));
                s = mid + (1.0f - mid) * t;
            }

            var l = Toe(L);
            return new HSL(h, s, l);
        }


        public static sRGB OkhsvToSrgb(HSV hsv)
        {
            var h = hsv.H;
            var s = hsv.S;
            var v = hsv.V;

            var a_ = Math.Cos(2.0f * Pi * h);
            var b_ = Math.Sin(2.0f * Pi * h);


            var cusp = FindCusp(a_, b_);

            var ST_max = ToSt(cusp);
            var S_max = ST_max.S;
            var T_max = ST_max.T;
            var S_0 = 0.5f;
            var k = 1f - S_0 / S_max;

            // first we compute L and V as if the gamut is a perfect triangle:

            // L, C when v==1:
            var L_v = 1 - s * S_0 / (S_0 + T_max - T_max * k * s);
            var C_v = s * T_max * S_0 / (S_0 + T_max - T_max * k * s);


            var L = v * L_v;
            var C = v * C_v;

            // then we compensate for both toe and the curved top part of the triangle:
            var L_vt = ToeInv(L_v);
            var C_vt = C_v * L_vt / L_v;

            var L_new = ToeInv(L);
            C = C * L_new / L;
            L = L_new;

            var rgb_scale = OklabToLinearSrgb(new Lab(L_vt, a_ * C_vt, b_ * C_vt));
            var scale_L = Cbrtd(1.0d / Math.Max(Math.Max(rgb_scale.R, rgb_scale.G), Math.Max(rgb_scale.B, 0.0d)));

            L = L * scale_L;
            C = C * scale_L;

            var rgb = OklabToLinearSrgb(new Lab(L, C * a_, C * b_));
            return new sRGB(rgb.R, rgb.G, rgb.B);
        }

        private static double Cbrtd(double number)
        {
            return Math.Pow(number, 1d / 3d);
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static HSV SrgbToOkhsv(sRGB rgb)
        {
            var lab = LinearSrgbToOklab(new sRGB(rgb.R, rgb.G, rgb.B));

            var C = Math.Sqrt(lab.a * lab.a + lab.b * lab.b);
            var a_ = lab.a / C;
            var b_ = lab.b / C;

            var L = lab.L;
            var h = 0.5f + 0.5f * Math.Atan2(-lab.b, -lab.a) / Pi;

            var cusp = FindCusp(a_, b_);
            var ST_max = ToSt(cusp);
            var S_max = ST_max.S;
            var T_max = ST_max.T;
            var S_0 = 0.5f;
            var k = 1 - S_0 / S_max;

            // first we find L_v, C_v, L_vt and C_vt

            var t = T_max / (C + L * T_max);
            var L_v = t * L;
            var C_v = t * C;

            var L_vt = ToeInv(L_v);
            var C_vt = C_v * L_vt / L_v;

            // we can then use these to invert the step that compensates for the toe and the curved top part of the triangle:
            var rgb_scale = OklabToLinearSrgb(new Lab(L = L_vt, a_ * C_vt, b_ * C_vt));
            var scale_L = Cbrtd(1.0f / Math.Max(Math.Max(rgb_scale.R, rgb_scale.G), Math.Max(rgb_scale.B, 0.0f)));

            L = L / scale_L;
            C = C / scale_L;

            C = C * Toe(L) / L;
            L = Toe(L);

            // we can now compute v and s:

            var v = L / L_v;
            var s = (S_0 + T_max) * C_v / (T_max * S_0 + T_max * k * C_v);

            return new HSV(h, s, v);
        }
    }
}