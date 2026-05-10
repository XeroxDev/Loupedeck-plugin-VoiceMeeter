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
using System.Text.RegularExpressions;

using Helpers;

using Library.Voicemeeter;

using Services;

using SkiaSharp;

public class RawCommand : MultistateActionEditorCommand
{
    private VoiceMeeterService VmService { get; }
    private IDisposable _subscription;
    private VoiceMeeterStateManager.RawCommandBinding _binding;
    private ActionEditorActionParameters _lastActionParameters;

    public RawCommand()
    {
        base.IsWidget = true;
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

    protected override Boolean OnUnload()
    {
        if (this._subscription != null)
        {
            this._subscription.Dispose();
            this._subscription = null;
        }

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
        this._lastActionParameters = actionParameters;

        (String Name, String Api, SKColor OnColor, SKColor OffColor) parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return null;
        }

        var binding = this.VmService.StateManager.CreateRawCommandBinding(parameters.Name, parameters.Api, parameters.OnColor, parameters.OffColor);
        this.EnsureRegistered(binding);
        return this.VmService.StateManager.GetRawCommandImage(binding);
    }

    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        this._lastActionParameters = actionParameters;

        (String Name, String Api, SKColor OnColor, SKColor OffColor) parameters;
        try
        {
            parameters = GetParameters(actionParameters);
        }
        catch (Exception)
        {
            return false;
        }

        var binding = this.VmService.StateManager.CreateRawCommandBinding(parameters.Name, parameters.Api, parameters.OnColor, parameters.OffColor);
        this.EnsureRegistered(binding);

        try
        {
            if (IsScript(binding.Api))
            {
                var script = BuildScript(binding.Api);
                Remote.SetParameters(script);

                if (TryReadBooleanState(binding.Target, out var stateTarget))
                {
                    this.VmService.StateManager.UpdateRawCommandState(binding, stateTarget);
                    this.SetCurrentState(actionParameters, stateTarget ? 1 : 0);
                }
            }
            else
            {
                if (TryReadBooleanState(binding.Api, out var currentValue))
                {
                    var newState = !currentValue;
                    Remote.SetParameter(binding.Api, newState ? 1 : 0);
                    this.VmService.StateManager.UpdateRawCommandState(binding, newState);
                    this.SetCurrentState(actionParameters, newState ? 1 : 0);
                }
                else
                {
                    Remote.SetParameter(binding.Api, 1);
                    this.VmService.StateManager.UpdateRawCommandState(binding, true);
                    this.SetCurrentState(actionParameters, 1);
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private void EnsureRegistered(VoiceMeeterStateManager.RawCommandBinding binding)
    {
        if (this._binding?.StateKey == binding.StateKey)
        {
            return;
        }

        this._binding = binding;
        this.VmService.StateManager.RegisterRawCommandTarget(binding);

        this._subscription?.Dispose();
        this._subscription = this.VmService.StateManager.Subscribe(binding, () =>
        {
            var state = this.VmService.StateManager.GetRawCommandState(binding);
            if (this._lastActionParameters != null)
            {
                this.SetCurrentState(this._lastActionParameters, state ? 1 : 0);
            }

            this.ActionImageChanged();
        });
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
        var commands = Regex.Split(api, @"[;\r\n,]");
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
        GetCommandTarget(Regex.Split(api ?? String.Empty, @"[;\r\n,]")[0]);

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
