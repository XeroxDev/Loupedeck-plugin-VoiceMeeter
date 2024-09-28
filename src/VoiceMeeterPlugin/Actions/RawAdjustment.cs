// This file is part of the VoiceMeeterPlugin project.
// 
// Copyright (c) 2024 Dominic Ris
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Loupedeck.VoiceMeeterPlugin.Actions;

using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Extensions;

using Helpers;

using Library.Voicemeeter;

using Services;

using SkiaSharp;

public class RawAdjustment : ActionEditorAdjustment
{
    private VoiceMeeterService VmService { get; }
    private Subject<Boolean> OnDestroy { get; } = new();

    public RawAdjustment() : base(false)
    {
        this.DisplayName = "Raw Adjustment";
        this.Description = "Adjusts a raw value";

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("name", "Display Name", "Name displayed on the device itself").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("api", "API", "The \"API\" to adjust, example: Strip[0].Gain").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("steps", "Steps", "The steps to adjust the API by").SetRequired().SetFormat(ActionEditorTextboxFormat.Integer).SetRegex(@"^-?\d+(\.\d+)?$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("min", "Min Value", "The minimum value that can be reached").SetRequired().SetFormat(ActionEditorTextboxFormat.Integer).SetRegex(@"^-?\d+(\.\d+)?$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("max", "Max Value", "The maximum value that can be reached").SetRequired().SetFormat(ActionEditorTextboxFormat.Integer).SetRegex(@"^-?\d+(\.\d+)?$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("bgcolor", "Background Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("fgcolor", "Foreground Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );

        this.VmService = VoiceMeeterService.Instance;
    }

    protected override Boolean OnLoad()
    {
        this.VmService.Parameters
            .TakeUntil(this.OnDestroy)
            .Subscribe(_ => this.AdjustmentValueChanged());

        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        this.OnDestroy.OnNext(true);
        return base.OnUnload();
    }

    protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight) =>
        this.GetAdjustmentImage(actionParameters, imageWidth, imageHeight);

    protected override BitmapImage GetAdjustmentImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
    {
        Tuple<String, String, Single, Int32, Int32, SKColor, SKColor> parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return null;
        }

        if (parameters == null)
        {
            return null;
        }

        var (name, api, steps, min, max, bgColor, fgColor) = parameters;

        var currentValue = -9999f;

        try
        {
            currentValue = Remote.GetParameter(api);
        }
        catch (Exception)
        {
            // ignore
        }
        
        var decimalPlaces = GetDecimalPlaces(steps);
        currentValue = (Single)Math.Round(currentValue, decimalPlaces);
        

        return DrawingHelper.DrawVolumeBar(PluginImageSize.Width60, bgColor.ToBitmapColor(), fgColor.ToBitmapColor(), currentValue, min, max, 1, "", name);
    }

    protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
    {
        Tuple<String, String, Single, Int32, Int32, SKColor, SKColor> parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return false;
        }

        if (parameters == null)
        {
            return false;
        }

        var (_, api, steps, min, max, _, _) = parameters;

        try
        {
            var currentValue = Remote.GetParameter(api);
            var newValue = currentValue + diff * steps;
            if (newValue < min)
            {
                newValue = min;
            }
            else if (newValue > max)
            {
                newValue = max;
            }
            Remote.SetParameter(api, newValue);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private static Tuple<String, String, Single, Int32, Int32, SKColor, SKColor> GetParameters(ActionEditorActionParameters actionParameters)
    {
        actionParameters.TryGetString("name", out var name);
        actionParameters.TryGetString("api", out var api);
        actionParameters.TryGetString("steps", out var value);
        actionParameters.TryGetString("min", out var min);
        actionParameters.TryGetString("max", out var max);
        actionParameters.TryGetString("bgcolor", out var bgColor);
        actionParameters.TryGetString("fgcolor", out var fgColor);

        return new Tuple<String, String, Single, Int32, Int32, SKColor, SKColor>(
            String.IsNullOrEmpty(name) ? "Unknown" : name,
            String.IsNullOrEmpty(api) ? "Strip[1].Gain" : api,
            Single.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 1,
            Int32.TryParse(min, NumberStyles.Any, CultureInfo.InvariantCulture, out var minValue) ? minValue : 0,
            Int32.TryParse(max, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxValue) ? maxValue : 100,
            SKColor.TryParse(bgColor, out var bg) ? bg : ColorHelper.Inactive,
            SKColor.TryParse(fgColor, out var fg) ? fg : SKColors.White);
    }
    
    private static Int32 GetDecimalPlaces(Single steps)
    {
        var str = steps.ToString(CultureInfo.InvariantCulture);
        var index = str.IndexOf('.');
        return index == -1 ? 0 : str.Length - index - 1;
    }
}