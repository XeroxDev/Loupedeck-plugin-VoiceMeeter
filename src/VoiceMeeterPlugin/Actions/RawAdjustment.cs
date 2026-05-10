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

using Helpers;

using Library.Voicemeeter;

using Services;

using SkiaSharp;

public class RawAdjustment : ActionEditorAdjustment
{
    private VoiceMeeterService VmService { get; }
    private IDisposable _subscription;
    private VoiceMeeterStateManager.RawAdjustmentBinding _binding;

    public RawAdjustment() : base(false)
    {
        base.IsWidget = true;
        this.DisplayName = "Raw Adjustment";
        this.Description = "Adjusts a raw value";

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("name", "Display Name", "Name displayed on the device itself").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("api", "API", "The \"API\" to adjust, example: Strip[0].Gain").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("steps", "Steps", "The steps to adjust the API by").SetRequired().SetRegex(@"^-?\d+(\.\d+)?$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("min", "Min Value", "The minimum value that can be reached").SetRequired().SetFormat(ActionEditorTextboxFormat.Integer).SetRegex(@"^-?\d+$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("max", "Max Value", "The maximum value that can be reached").SetRequired().SetFormat(ActionEditorTextboxFormat.Integer).SetRegex(@"^-?\d+$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("bgcolor", "Background Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("fgcolor", "Foreground Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );

        this.VmService = VoiceMeeterService.Instance;
    }

    protected override Boolean OnUnload()
    {
        if (this._subscription != null)
        {
            this._subscription.Dispose();
            this._subscription = null;
        }

        return base.OnUnload();
    }

    protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight) =>
        this.GetAdjustmentImage(actionParameters, imageWidth, imageHeight);

    protected override BitmapImage GetAdjustmentImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
    {
        var parameters = GetParameters(actionParameters);
        if (parameters == null)
        {
            return null;
        }

        var binding = this.VmService.StateManager.CreateRawAdjustmentBinding(parameters.Item1, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5, parameters.Item6, parameters.Item7);
        this.EnsureRegistered(binding);
        return this.VmService.StateManager.GetRawAdjustmentImage(binding);
    }

    protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
    {
        var parameters = GetParameters(actionParameters);
        if (parameters == null)
        {
            return false;
        }

        var binding = this.VmService.StateManager.CreateRawAdjustmentBinding(parameters.Item1, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5, parameters.Item6, parameters.Item7);
        this.EnsureRegistered(binding);

        try
        {
            var currentValue = this.VmService.StateManager.GetRawAdjustmentValue(binding);

            var newValue = currentValue + diff * binding.Steps;
            if (newValue < binding.MinValue)
            {
                newValue = binding.MinValue;
            }
            else if (newValue > binding.MaxValue)
            {
                newValue = binding.MaxValue;
            }

            Remote.SetParameter(binding.Api, newValue);
            this.VmService.StateManager.UpdateRawAdjustmentState(binding, newValue);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private void EnsureRegistered(VoiceMeeterStateManager.RawAdjustmentBinding binding)
    {
        if (this._binding?.StateKey == binding.StateKey)
        {
            return;
        }

        this._binding = binding;
        this.VmService.StateManager.RegisterRawAdjustmentTarget(binding);

        this._subscription?.Dispose();
        this._subscription = this.VmService.StateManager.Subscribe(binding, () => this.AdjustmentValueChanged());
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
}
