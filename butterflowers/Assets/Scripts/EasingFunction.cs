﻿/*
 * Created by C.J. Kimberlin
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2019
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * 
 * TERMS OF USE - EASING EQUATIONS
 * Open source under the BSD License.
 * Copyright (c)2001 Robert Penner
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 *
 * 
 * for visualizations: https://easings.net/en
 */

using UnityEngine;

public static class EasingFunction
{

    private const float NATURAL_LOG_OF_2 = 0.693147181f;

    //
    // Easing functions
    //

    public static float Linear(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value);
    }

    public static float Spring(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    public static float EaseInQuad(float start, float end, float value)
    {
        end -= start;
        return end * value * value + start;
    }

    public static float EaseOutQuad(float start, float end, float value)
    {
        end -= start;
        return -end * value * (value - 2) + start;
    }

    public static float EaseInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value + start;
        value--;
        return -end * 0.5f * (value * (value - 2) - 1) + start;
    }

    public static float EaseInCubic(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value + start;
    }

    public static float EaseOutCubic(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }

    public static float EaseInOutCubic(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value + 2) + start;
    }

    public static float EaseInQuart(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value + start;
    }

    public static float EaseOutQuart(float start, float end, float value)
    {
        value--;
        end -= start;
        return -end * (value * value * value * value - 1) + start;
    }

    public static float EaseInOutQuart(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value + start;
        value -= 2;
        return -end * 0.5f * (value * value * value * value - 2) + start;
    }

    public static float EaseInQuint(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value * value + start;
    }

    public static float EaseOutQuint(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value * value * value + 1) + start;
    }

    public static float EaseInOutQuint(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value * value * value + 2) + start;
    }

    public static float EaseInSine(float start, float end, float value)
    {
        end -= start;
        return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
    }

    public static float EaseOutSine(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
    }

    public static float EaseInOutSine(float start, float end, float value)
    {
        end -= start;
        return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
    }

    public static float EaseInExpo(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Pow(2, 10 * (value - 1)) + start;
    }

    public static float EaseOutExpo(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
    }

    public static float EaseInOutExpo(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }

    public static float EaseInCirc(float start, float end, float value)
    {
        end -= start;
        return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
    }

    public static float EaseOutCirc(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * Mathf.Sqrt(1 - value * value) + start;
    }

    public static float EaseInOutCirc(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
        value -= 2;
        return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
    }

    public static float EaseInBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        return end - EaseOutBounce(0, end, d - value) + start;
    }

    public static float EaseOutBounce(float start, float end, float value)
    {
        value /= 1f;
        end -= start;
        if (value < (1 / 2.75f))
        {
            return end * (7.5625f * value * value) + start;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return end * (7.5625f * (value) * value + .75f) + start;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return end * (7.5625f * (value) * value + .9375f) + start;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }
    }

    public static float EaseInOutBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        if (value < d * 0.5f) return EaseInBounce(0, end, value * 2) * 0.5f + start;
        else return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
    }

    public static float EaseInBack(float start, float end, float value)
    {
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }

    public static float EaseOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }

    public static float EaseInOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
        }
        value -= 2;
        s *= (1.525f);
        return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
    }

    public static float EaseInElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
    }

    public static float EaseOutElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p * 0.25f;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
    }

    public static float EaseInOutElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d * 0.5f) == 2) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
    }

    //
    // These are derived functions that the motor can use to get the speed at a specific time.
    //
    // The easing functions all work with a normalized time (0 to 1) and the returned value here
    // reflects that. Values returned here should be divided by the actual time.
    //
    // TODO: These functions have not had the testing they deserve. If there is odd behavior around
    //       dash speeds then this would be the first place I'd look.

    public static float LinearD(float start, float end, float value)
    {
        return end - start;
    }

    public static float EaseInQuadD(float start, float end, float value)
    {
        return 2f * (end - start) * value;
    }

    public static float EaseOutQuadD(float start, float end, float value)
    {
        end -= start;
        return -end * value - end * (value - 2);
    }

    public static float EaseInOutQuadD(float start, float end, float value)
    {
        value /= .5f;
        end -= start;

        if (value < 1)
        {
            return end * value;
        }

        value--;

        return end * (1 - value);
    }

    public static float EaseInCubicD(float start, float end, float value)
    {
        return 3f * (end - start) * value * value;
    }

    public static float EaseOutCubicD(float start, float end, float value)
    {
        value--;
        end -= start;
        return 3f * end * value * value;
    }

    public static float EaseInOutCubicD(float start, float end, float value)
    {
        value /= .5f;
        end -= start;

        if (value < 1)
        {
            return (3f / 2f) * end * value * value;
        }

        value -= 2;

        return (3f / 2f) * end * value * value;
    }

    public static float EaseInQuartD(float start, float end, float value)
    {
        return 4f * (end - start) * value * value * value;
    }

    public static float EaseOutQuartD(float start, float end, float value)
    {
        value--;
        end -= start;
        return -4f * end * value * value * value;
    }

    public static float EaseInOutQuartD(float start, float end, float value)
    {
        value /= .5f;
        end -= start;

        if (value < 1)
        {
            return 2f * end * value * value * value;
        }

        value -= 2;

        return -2f * end * value * value * value;
    }

    public static float EaseInQuintD(float start, float end, float value)
    {
        return 5f * (end - start) * value * value * value * value;
    }

    public static float EaseOutQuintD(float start, float end, float value)
    {
        value--;
        end -= start;
        return 5f * end * value * value * value * value;
    }

    public static float EaseInOutQuintD(float start, float end, float value)
    {
        value /= .5f;
        end -= start;

        if (value < 1)
        {
            return (5f / 2f) * end * value * value * value * value;
        }

        value -= 2;

        return (5f / 2f) * end * value * value * value * value;
    }

    public static float EaseInSineD(float start, float end, float value)
    {
        return (end - start) * 0.5f * Mathf.PI * Mathf.Sin(0.5f * Mathf.PI * value);
    }

    public static float EaseOutSineD(float start, float end, float value)
    {
        end -= start;
        return (Mathf.PI * 0.5f) * end * Mathf.Cos(value * (Mathf.PI * 0.5f));
    }

    public static float EaseInOutSineD(float start, float end, float value)
    {
        end -= start;
        return end * 0.5f * Mathf.PI * Mathf.Sin(Mathf.PI * value);
    }
    public static float EaseInExpoD(float start, float end, float value)
    {
        return (10f * NATURAL_LOG_OF_2 * (end - start) * Mathf.Pow(2f, 10f * (value - 1)));
    }

    public static float EaseOutExpoD(float start, float end, float value)
    {
        end -= start;
        return 5f * NATURAL_LOG_OF_2 * end * Mathf.Pow(2f, 1f - 10f * value);
    }

    public static float EaseInOutExpoD(float start, float end, float value)
    {
        value /= .5f;
        end -= start;

        if (value < 1)
        {
            return 5f * NATURAL_LOG_OF_2 * end * Mathf.Pow(2f, 10f * (value - 1));
        }

        value--;

        return (5f * NATURAL_LOG_OF_2 * end) / (Mathf.Pow(2f, 10f * value));
    }

    public static float EaseInCircD(float start, float end, float value)
    {
        return ((end - start) * value) / Mathf.Sqrt(1f - value * value);
    }

    public static float EaseOutCircD(float start, float end, float value)
    {
        value--;
        end -= start;
        return (-end * value) / Mathf.Sqrt(1f - value * value);
    }

    public static float EaseInOutCircD(float start, float end, float value)
    {
        value /= .5f;
        end -= start;

        if (value < 1)
        {
            return (end * value) / (2f * Mathf.Sqrt(1f - value * value));
        }

        value -= 2;

        return (-end * value) / (2f * Mathf.Sqrt(1f - value * value));
    }

    public static float EaseInBounceD(float start, float end, float value)
    {
        end -= start;
        float d = 1f;

        return EaseOutBounceD(0, end, d - value);
    }

    public static float EaseOutBounceD(float start, float end, float value)
    {
        value /= 1f;
        end -= start;

        if (value < (1 / 2.75f))
        {
            return 2f * end * 7.5625f * value;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return 2f * end * 7.5625f * value;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return 2f * end * 7.5625f * value;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return 2f * end * 7.5625f * value;
        }
    }

    public static float EaseInOutBounceD(float start, float end, float value)
    {
        end -= start;
        float d = 1f;

        if (value < d * 0.5f)
        {
            return EaseInBounceD(0, end, value * 2) * 0.5f;
        }
        else
        {
            return EaseOutBounceD(0, end, value * 2 - d) * 0.5f;
        }
    }

    public static float EaseInBackD(float start, float end, float value)
    {
        float s = 1.70158f;

        return 3f * (s + 1f) * (end - start) * value * value - 2f * s * (end - start) * value;
    }

    public static float EaseOutBackD(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;

        return end * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
    }

    public static float EaseInOutBackD(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value /= .5f;

        if ((value) < 1)
        {
            s *= (1.525f);
            return 0.5f * end * (s + 1) * value * value + end * value * ((s + 1f) * value - s);
        }

        value -= 2;
        s *= (1.525f);
        return 0.5f * end * ((s + 1) * value * value + 2f * value * ((s + 1f) * value + s));
    }

    public static float EaseInElasticD(float start, float end, float value)
    {
        return EaseOutElasticD(start, end, 1f - value);
    }

    public static float EaseOutElasticD(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p * 0.25f;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.PI * d * Mathf.Pow(2f, 1f - 10f * value) *
            Mathf.Cos((2f * Mathf.PI * (d * value - s)) / p)) / p - 5f * NATURAL_LOG_OF_2 * a *
            Mathf.Pow(2f, 1f - 10f * value) * Mathf.Sin((2f * Mathf.PI * (d * value - s)) / p);
    }

    public static float EaseInOutElasticD(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1)
        {
            value -= 1;

            return -5f * NATURAL_LOG_OF_2 * a * Mathf.Pow(2f, 10f * value) * Mathf.Sin(2 * Mathf.PI * (d * value - 2f) / p) -
                a * Mathf.PI * d * Mathf.Pow(2f, 10f * value) * Mathf.Cos(2 * Mathf.PI * (d * value - s) / p) / p;
        }

        value -= 1;

        return a * Mathf.PI * d * Mathf.Cos(2f * Mathf.PI * (d * value - s) / p) / (p * Mathf.Pow(2f, 10f * value)) -
            5f * NATURAL_LOG_OF_2 * a * Mathf.Sin(2f * Mathf.PI * (d * value - s) / p) / (Mathf.Pow(2f, 10f * value));
    }

    public static float SpringD(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        end -= start;

        // Damn... Thanks http://www.derivative-calculator.net/
        // TODO: And it's a little bit wrong
        return end * (6f * (1f - value) / 5f + 1f) * (-2.2f * Mathf.Pow(1f - value, 1.2f) *
            Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + Mathf.Pow(1f - value, 2.2f) *
            (Mathf.PI * (2.5f * value * value * value + 0.2f) + 7.5f * Mathf.PI * value * value * value) *
            Mathf.Cos(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + 1f) -
            6f * end * (Mathf.Pow(1 - value, 2.2f) * Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + value
            / 5f);

    }
}