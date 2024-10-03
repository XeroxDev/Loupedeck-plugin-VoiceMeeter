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

using System.Reactive.Linq;
using System.Reactive.Subjects;

using Helpers;

using Library.Voicemeeter;

using Services;

using SkiaSharp;

public class RawCommand : MultistateActionEditorCommand
{
    private VoiceMeeterService VmService { get; }
    private Subject<Boolean> OnDestroy { get; } = new();

    public RawCommand()
    {
        this.DisplayName = "Raw Command";
        this.Description = "Toggle or execute an action";

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("name", "Display Name", "Name displayed on the device itself").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("api", "API", "The \"API\" to adjust, example: Strip[0].Gain").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("oncolor", "On Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("offcolor", "Off Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );

        this.VmService = VoiceMeeterService.Instance;
        this.AddState("Off", "If the action is off");
        this.AddState("On", "If the action is on");
    }

    protected override Boolean OnLoad()
    {
        this.VmService.Parameters
            .TakeUntil(this.OnDestroy)
            .Subscribe(_ => this.ActionImageChanged());

        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        this.OnDestroy.OnNext(true);
        return base.OnUnload();
    }

    protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters, Int32 stateIndex)
    {
        Tuple<String, String, SKColor, SKColor> parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return "Unknown";
        }

        if (parameters == null)
        {
            return "Unknown";
        }

        var (name, _, _, _) = parameters;

        return $"{name} - {(stateIndex == 0 ? "Off" : "On")}";
    }

    protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 stateIndex, Int32 imageWidth, Int32 imageHeight)
    {
        Tuple<String, String, SKColor, SKColor> parameters;
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

        var (name, api, onColor, offColor) = parameters;

        var currentValue = false;

        try
        {
            currentValue = (Int32)Remote.GetParameter(api) == 1;
        }
        catch (Exception)
        {
            // ignore
        }


        return DrawingHelper.DrawDefaultImage(name, "", currentValue ? onColor : offColor);
    }

    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        Tuple<String, String, SKColor, SKColor> parameters;
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

        var (_, api, _, _) = parameters;

        try
        {
            var currentValue = (Int32)Remote.GetParameter(api) == 1;
            Remote.SetParameter(api, currentValue ? 0 : 1);
            this.SetCurrentState(actionParameters, currentValue ? 0 : 1);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private static Tuple<String, String, SKColor, SKColor> GetParameters(ActionEditorActionParameters actionParameters)
    {
        actionParameters.TryGetString("name", out var name);
        actionParameters.TryGetString("api", out var api);
        actionParameters.TryGetString("oncolor", out var onColor);
        actionParameters.TryGetString("offcolor", out var offColor);

        return new Tuple<String, String, SKColor, SKColor>(
            String.IsNullOrEmpty(name) ? "Unknown" : name,
            String.IsNullOrEmpty(api) ? "Strip[1].Gain" : api,
            SKColor.TryParse(onColor, out var on) ? on : ColorHelper.Active,
            SKColor.TryParse(offColor, out var off) ? off : ColorHelper.Inactive);
    }
}