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

using Extensions;

using Helpers;

using Library.Voicemeeter;

using Services;

using SkiaSharp;

public class LevelsCommand : ActionEditorCommand
{
    private VoiceMeeterService VmService { get; }
    private Subject<Boolean> OnDestroy { get; } = new();

    public LevelsCommand()
    {
        this.DisplayName = "Level Display";
        this.Description = "Displays specific Levels";

        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("name", "Display Name", "Name displayed on the device itself").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorSlider("channel_number", "Channel Number", "The Channel Number to display").SetRequired().SetValues(0, 100, 0, 1)
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorListbox("channel_type", "Channel Type", "The Channel Type to display").SetRequired()
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("bgcolor", "Background Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );
        this.ActionEditor.AddControlEx(
            new ActionEditorTextbox("fgcolor", "Foreground Color", "The color it should use in hex (#rrggbb example: #FF0000 = red)").SetRegex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
        );

        this.ActionEditor.ListboxItemsRequested += (_, e) =>
        {
            // iterate over LevelType enum values and add them (their name) to the listbox
            foreach (var value in Enum.GetValues(typeof(LevelType)))
            {
                e.Items.Add(new ActionEditorListboxItem("channel_type_" + value, value.ToString(), ""));
            }
        };

        this.VmService = VoiceMeeterService.Instance;
    }

    protected override Boolean OnLoad()
    {
        this.VmService.Levels
            .TakeUntil(this.OnDestroy)
            .Subscribe(_ => this.ActionImageChanged());

        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        this.OnDestroy.OnNext(true);
        return base.OnUnload();
    }

    protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters)
    {
        Tuple<String, Levels.Channel, SKColor, SKColor> parameters;
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

        var (name, channel, bgColor, fgColor) = parameters;

        this.VmService.Levels.AddChannel(channel);

        var currentValue = 0f;

        try
        {
            currentValue = Remote.GetLevel(channel.LevelType, channel.ChannelNumber);
        }
        catch (Exception)
        {
            // ignore
        }
        
        currentValue = (Single)Math.Round(currentValue, 10);
        
        return $"{name} - {currentValue:P0}";
    }

    protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
    {
        Tuple<String, Levels.Channel, SKColor, SKColor> parameters;
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

        var (name, channel, bgColor, fgColor) = parameters;

        this.VmService.Levels.AddChannel(channel);

        var currentValue = 0f;

        try
        {
            currentValue = Remote.GetLevel(channel.LevelType, channel.ChannelNumber);
        }
        catch (Exception)
        {
            // ignore
        }

        currentValue = (Single)Math.Round(currentValue, 10);


        return DrawingHelper.DrawVolumeBar(PluginImageSize.Width60, bgColor.ToBitmapColor(), fgColor.ToBitmapColor(), currentValue, 0, 1, 1, "", name, false);
    }

    private static Tuple<String, Levels.Channel, SKColor, SKColor> GetParameters(ActionEditorActionParameters actionParameters)
    {
        actionParameters.TryGetString("name", out var name);
        actionParameters.TryGetInt32("channel_number", out var channelNumber);
        actionParameters.TryGetString("channel_type", out var channelType);
        actionParameters.TryGetString("bgcolor", out var bgColor);
        actionParameters.TryGetString("fgcolor", out var fgColor);

        // for the channel type, we first have to remove the prefix
        var type = channelType.Replace("channel_type_", "");
        if (!Enum.TryParse<LevelType>(type, out var levelType))
        {
            return null;
        }

        var channel = new Levels.Channel { LevelType = levelType, ChannelNumber = channelNumber };

        return new Tuple<String, Levels.Channel, SKColor, SKColor>(
            String.IsNullOrEmpty(name) ? "Unknown" : name,
            channel,
            SKColor.TryParse(bgColor, out var bg) ? bg : ColorHelper.Inactive,
            SKColor.TryParse(fgColor, out var fg) ? fg : SKColors.White);
    }
}