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
        this.Description = "Toggle a Voicemeeter parameter or execute a raw Voicemeeter script";

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("name", "Display Name", "Name displayed on the device itself").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("api", "API", "Enter a parameter name to toggle, or a script such as Strip[0].Mute = %toggle%; Bus[0].Mono = 1. Use %toggle% to invert the current value of a readable parameter.").SetRequired()
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
        (String Name, String Api, SKColor OnColor, SKColor OffColor) parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return "Unknown";
        }

        var (name, _, _, _) = parameters;

        return $"{name} - {(stateIndex == 0 ? "Off" : "On")}";
    }

    protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 stateIndex, Int32 imageWidth, Int32 imageHeight)
    {
        (String Name, String Api, SKColor OnColor, SKColor OffColor) parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return null;
        }

        var (name, api, onColor, offColor) = parameters;

        var currentValue = false;

        try
        {
            currentValue = TryReadBooleanState(api, out var stateTarget) && stateTarget;
        }
        catch (Exception)
        {
            // ignore
        }


        return DrawingHelper.DrawDefaultImage(name, "", currentValue ? onColor : offColor);
    }

    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        (String Name, String Api, SKColor OnColor, SKColor OffColor) parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return false;
        }

        var (_, api, _, _) = parameters;

        try
        {
            if (IsScript(api))
            {
                var script = BuildScript(api);
                Remote.SetParameters(script);

                if (TryReadBooleanState(GetPrimaryTarget(api), out var stateTarget))
                {
                    this.SetCurrentState(actionParameters, stateTarget ? 1 : 0);
                }
            }
            else
            {
                if (TryReadBooleanState(api, out var currentValue))
                {
                    Remote.SetParameter(api, currentValue ? 0 : 1);
                    this.SetCurrentState(actionParameters, currentValue ? 0 : 1);
                }
                else
                {
                    Remote.SetParameter(api, 1);
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private static (String Name, String Api, SKColor OnColor, SKColor OffColor) GetParameters(ActionEditorActionParameters actionParameters)
    {
        actionParameters.TryGetString("name", out var name);
        actionParameters.TryGetString("api", out var api);
        actionParameters.TryGetString("oncolor", out var onColor);
        actionParameters.TryGetString("offcolor", out var offColor);

        return (
            String.IsNullOrEmpty(name) ? "Unknown" : name,
            String.IsNullOrEmpty(api) ? "Strip[1].Gain" : api,
            SKColor.TryParse(onColor, out var on) ? on : ColorHelper.Active,
            SKColor.TryParse(offColor, out var off) ? off : ColorHelper.Inactive);
    }

    private static Boolean IsScript(String api) =>
        api?.Contains("=") == true || api?.Contains(";") == true;

    private static String BuildScript(String api)
    {
        var commands = api.Split(';');
        for (var i = 0; i < commands.Length; i++)
        {
            commands[i] = BuildCommand(commands[i]);
        }

        return String.Join(";", commands);
    }

    private static String BuildCommand(String command)
    {
        var trimmedCommand = command.Trim();
        if (String.IsNullOrWhiteSpace(trimmedCommand))
        {
            return trimmedCommand;
        }

        if (!trimmedCommand.Contains("%toggle%"))
        {
            return trimmedCommand;
        }

        if (!TryReadBooleanState(GetCommandTarget(trimmedCommand), out var currentValue))
        {
            throw new InvalidOperationException($"The command '{trimmedCommand}' cannot be toggled.");
        }

        return trimmedCommand.Replace("%toggle%", currentValue ? "0" : "1");
    }

    private static Boolean TryReadBooleanState(String api, out Boolean value)
    {
        value = false;

        if (String.IsNullOrWhiteSpace(api))
        {
            return false;
        }

        var probe = 0f;
        var result = RemoteWrapper.GetParameter(api, ref probe);
        if (result != 0)
        {
            return false;
        }

        value = (Int32)probe == 1;
        return true;
    }

    private static String GetPrimaryTarget(String api) =>
        GetCommandTarget(api?.Split(';')[0]);

    private static String GetCommandTarget(String command)
    {
        if (String.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        var assignmentIndex = command.IndexOf('=');
        return assignmentIndex >= 0
            ? command.Substring(0, assignmentIndex).Trim()
            : command.Trim();
    }
}
