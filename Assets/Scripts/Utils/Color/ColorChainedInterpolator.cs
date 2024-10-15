using UnityEngine;
using System;

/// <summary>
/// 
/// @Unity Color spectrum reference points (UPPER RIGHT CORNER)
/// a (0.5, 1, 0)
/// b (1, 0.75, 0)
/// c (1, 0, 0)
/// d (1, 0, 0.75)
/// e (0.5, 0, 1)
/// f (0, 0.25, 1)
/// g (0, 1, 1)
/// h (0, 1, 0.25)
/// 
/// @transitons between chain links (ColorNode)
/// a -> b [r+0.5, g-0.25]
/// b -> c [g-0.75]
/// c -> d [b+0.75]
/// d -> e [r-0.5, b+0.25]
/// e -> f [r-0.5, g+0.25]
/// f -> g [g+0.75]
/// g -> h [b-0.75]
/// h -> a [r+0.5,b-0.25]
/// 
/// @Spectrum Navigation Alg
/// 1) Finding the closest ColorNode to startColor
///     - Using the transitions between the [Color] of the ColorNodes, we can determine
///     the proximity of [startColor] by interating over the chain and substracting 
///     [startColor.r/.g/.b] to the [Color] of the ColorNode and seeing if the values
///     returned have the same sign has [transition.Red/.Green/.Blue], the last ColorNode
///     with the same signs will be the closest one
///         - example: 
///         startColor -> (0f, 0.40f, 1f)
///         firstColorNode.Color -> (0.5f, 1f, 0f)
///         (0f, 0.40f, 1f) - (0.5f, 1f, 0f) => (-0.5f, -0.60f, 1f)
///         transition of ColorNode to ColorNode.Next -> ((+)0.5, (-)0.25, (+)0)
///         ((-)0.5f, (-)0.60f, (+)1f) != ((+)0.5, (-)0.25, (+)0) <-- not the closest Color, because the sigs aren't the same 
///         
/// 
/// </summary>
public class ColorChainedInterpolator
{

    private ColorNode firstColorNode;
    public delegate void OnIteration(ColorNode colorNode);

    public ColorChainedInterpolator()
    {
        this.SetColorNodeChain(
            new Color(0.5f, 1f, 0f),
            new Color(1f, 0.75f, 0f),
            new Color(1f, 0f, 0f),
            new Color(1f, 0f, 0.75f),
            new Color(0.5f, 0f, 1f),
            new Color(0f, 0.25f, 1f),
            new Color(0f, 1f, 1f),
            new Color(0f, 1f, 0.25f)
            );
    }

    public ColorChainedInterpolator(params Color[] colors) 
    {
        if (colors.Length >= 8)
            this.SetColorNodeChain(colors);
        else throw new Exception();
    }

    private void SetColorNodeChain(params Color[] colors)
    {
        this.firstColorNode = new ColorNode(colors[0]);
        ColorNode currentColorNode = this.firstColorNode;
        for (int elem = 1; elem < colors.Length; elem++) 
        { 
            ColorNode nColorNode = new ColorNode(colors[elem]);
            currentColorNode.SetNext(nColorNode);
            nColorNode.Previous = currentColorNode;
            currentColorNode = nColorNode;
        }
        currentColorNode.SetNext(this.firstColorNode);
        this.firstColorNode.Previous = currentColorNode;
    }

    private void Iterate(OnIteration callback)
    {
        ColorNode tempColorNode = this.firstColorNode;
        bool overFirstColorNode = true;
        while (tempColorNode != this.firstColorNode || overFirstColorNode)
        {
            callback(tempColorNode);
            tempColorNode = tempColorNode.Next;
            overFirstColorNode = false;
        }
    }

    public int GetLength()
    {
        int len = 0;
        this.Iterate(colorNode => len++);
        return len;
    }

    private bool ColorComponentIsClose(float differential, float transition)
    {
        return ((transition >= 0) && (differential >= 0)) || ((transition < 0) && (differential < 0));
    }

    /// <summary>
    /// NEEDS TO BE FIXED [(1, 0.41, 0) start]
    /// </summary>
    private ColorNode GetClosestColorNode(Color color)
    {
        ColorNode closestColorNode = null;
        this.Iterate(colorNode =>
        {
            if (ColorComponentIsClose(color.r - colorNode.Color.r, colorNode.transition.Red) 
            && ColorComponentIsClose(color.g - colorNode.Color.g, colorNode.transition.Green) 
            && ColorComponentIsClose(color.b - colorNode.Color.b, colorNode.transition.Blue))
                closestColorNode = colorNode;
        });
        return closestColorNode;
    }

    /// <summary>
    /// Sets the percentages for each ColorNode from any ColorNode
    /// </summary>
    private void SetSpectrumPercentages(ColorNode startingColorNode)
    {
        float percentagePart = 100f / this.GetLength();
        float currentPercentage = 0f;
        this.Iterate(colorNode => 
        { 
            startingColorNode.spectrumPercentage = currentPercentage;
            currentPercentage += percentagePart;
            startingColorNode = startingColorNode.Next;
        });
    }

    private float GetColorComponentPercentage(float transition, float colorComponent, float nextColorComponent, float spectrumPercentage)
    {
        float percentage;
        if (transition >= 0 && (colorComponent != nextColorComponent))
            percentage = (colorComponent * spectrumPercentage) / transition;
        else if (transition < 0 && (colorComponent != nextColorComponent))
            percentage = (colorComponent * spectrumPercentage) / (colorComponent - nextColorComponent);
        else percentage = 0f;
        return percentage;
    }

    /// <summary>
    /// - After each colorNode percentages haves been set, get the exact percentage represtentation
    ///   of the color
    /// - Each component of the color (red, green, blue) travels differently on the spectrum (have a different transition, clockwise)
    /// - Only one component changes at a time as we travel
    /// - The exact percentage is determined by adding the percentage of all components (since only one changes at a time between the
    ///   different colorNodes)
    ///
    /// </summary>
    private float GetExactSpectrumPercentage(Color color, ColorNode closestColorNode)
    {
        float percentage = this.GetColorComponentPercentage(closestColorNode.transition.Red, color.r, closestColorNode.Next.Color.r, closestColorNode.Next.spectrumPercentage)
            + this.GetColorComponentPercentage(closestColorNode.transition.Green, color.g, closestColorNode.Next.Color.g, closestColorNode.Next.spectrumPercentage)
            + this.GetColorComponentPercentage(closestColorNode.transition.Blue, color.b, closestColorNode.Next.Color.b, closestColorNode.Next.spectrumPercentage);
        if (percentage == 0f) return closestColorNode.spectrumPercentage;
        else return percentage;
    }
    public Color[] GetColorSpectrum(int spectrumLength, Color startColor, Color endColor)
    {
        Color[] colors = new Color[spectrumLength];
        ColorNode closestStartColorNode = this.GetClosestColorNode(startColor);
        ColorNode closestEndColorNode = this.GetClosestColorNode(endColor);
        this.SetSpectrumPercentages(closestStartColorNode);
        float exactStartSpectrumPercentage = this.GetExactSpectrumPercentage(startColor, closestStartColorNode);
        float exactEndSpectrumPercentage = this.GetExactSpectrumPercentage(endColor, closestEndColorNode);
        Debug.Log(closestStartColorNode.Color);
        Debug.Log(exactEndSpectrumPercentage);
        float subSpectrumPercentageIncrement = (exactEndSpectrumPercentage - exactStartSpectrumPercentage) / spectrumLength;
        float subSpectrumPercentage = 0f;

        return null;
    }
    
}