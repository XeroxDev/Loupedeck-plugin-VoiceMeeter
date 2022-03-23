namespace Loupedeck.VoiceMeeterPlugin.Commands.Bases
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using Extensions;

    using Helper;

    using Library.Voicemeeter;

    using Services;

    public class BooleanBaseCommand : PluginDynamicCommand
    {
        private Dictionary<Int32, Boolean[]> Actions { get; } = new();
        private VoiceMeeterService VmService { get; }
        private Boolean IsMultiAction { get; set; }
        private String Command { get; set; }
        private Subject<Boolean> OnDestroy { get; } = new();
        public Boolean IsRealClass { get; set; }
        private Boolean IsStrip { get; }
        private Int32 Offset { get; set; }
        protected Boolean Loaded { get; set; }

        public BooleanBaseCommand(Boolean isRealClass, Boolean isStrip)
        {
            this.IsRealClass = isRealClass;
            this.IsStrip = isStrip;
            if (!this.IsRealClass)
            {
                return;
            }

            this.VmService = VoiceMeeterService.Instance;
        }

        public async Task CreateCommands(Int32 stripCount, String cmd, Int32 specialCount, Int32 offset,
            String displayName = null)
        {
            this.IsMultiAction = true;
            this.Command = cmd;
            this.Offset = offset;
            this.DisplayName = displayName ?? cmd;
            while (!this.VmService.Connected)
            {
                await Task.Delay(1000);
            }

            for (var hi = 0; hi < stripCount; hi++)
            {
                this.Actions.Add(hi, new Boolean[specialCount]);
                for (var special = 1; special <= specialCount; special++)
                {
                    var name = Remote.GetTextParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{hi + this.Offset}].Label");
                    var groupName = String.IsNullOrEmpty(name) ? this.IsStrip ? "Strip" : "Bus" : name;
                    this.AddParameter(
                        $"VM-Strip{hi}-{cmd}{special}",
                        $"{this.DisplayName}{special}",
                        $"{groupName} ({hi + 1 + this.Offset})",
                        "Input"
                    );
                }
            }

            this.GetNewSettings();
        }

        public async Task CreateCommands(Int32 stripCount, String cmd, Int32 offset, String displayName = null)
        {
            this.IsMultiAction = false;
            this.Command = cmd;
            this.Offset = offset;
            this.DisplayName = displayName ?? cmd;
            while (!this.VmService.Connected)
            {
                await Task.Delay(1000);
            }

            this.Actions.Add(0, new Boolean[stripCount]);
            for (var hi = 0; hi < stripCount; hi++)
            {
                var name = Remote.GetTextParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{hi + this.Offset}].Label");
                var groupName = String.IsNullOrEmpty(name) ? this.IsStrip ? "Strip" : "Bus" : name;
                this.AddParameter(
                    $"VM-Strip{hi}-{cmd}2147483647",
                    this.DisplayName,
                    $"{groupName} ({hi + 1 + this.Offset})",
                    "Input"
                );
            }

            this.GetNewSettings();
        }

        private void GetNewSettings()
        {
            if (this.IsMultiAction)
            {
                for (var hiIndex = 0; hiIndex < this.Actions.Count; hiIndex++)
                {
                    var hi = this.Actions[hiIndex];
                    for (var index = 1; index <= hi.Length; index++)
                    {
                        hi[index - 1] =
                            (Int32)Remote.GetParameter(
                                $"{(this.IsStrip ? "Strip" : "Bus")}[{hiIndex + this.Offset}].{this.Command}{index}") ==
                            1;
                    }
                }
            }
            else
            {
                var hiArray = this.Actions[0];
                for (var hiIndex = 0; hiIndex < hiArray.Length; hiIndex++)
                {
                    hiArray[hiIndex] =
                        (Int32)Remote.GetParameter(
                            $"{(this.IsStrip ? "Strip" : "Bus")}[{hiIndex + this.Offset}].{this.Command}") == 1;
                }
            }

            if (this.Loaded)
            {
            	this.ActionImageChanged();
            }
        }

        protected override Boolean OnLoad()
        {
        	this.Loaded = true;
            if (!this.IsRealClass)
            {
                return base.OnLoad();
            }

            this.VmService.Parameters
                .TakeUntil(this.OnDestroy)
                .Subscribe(_ => this.GetNewSettings());

            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            if (!this.IsRealClass)
            {
                return base.OnLoad();
            }

            this.OnDestroy.OnNext(true);
            return base.OnUnload();
        }

        protected override void RunCommand(String actionParameter)
        {
            if (!this.IsRealClass)
            {
                return;
            }

            this.GetButton(actionParameter, out var mainIndex, out var action, out var actionIndex);

            if (mainIndex == -1 || action == -1)
            {
                return;
            }

            Remote.SetParameter(
                this.IsMultiAction
                    ? $"{(this.IsStrip ? "Strip" : "Bus")}[{mainIndex + this.Offset}].{this.Command}{action}"
                    : $"{(this.IsStrip ? "Strip" : "Bus")}[{action + this.Offset}].{this.Command}",
                this.Actions[mainIndex][actionIndex] ? 0 : 1);

            this.ActionImageChanged(actionParameter);
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            if (!this.IsRealClass || actionParameter == null)
            {
                return null;
            }

            this.GetButton(actionParameter, out var mainIndex, out var action, out var actionIndex);

            if (mainIndex == -1 || action == -1)
            {
                return null;
            }

            return $"{this.DisplayName}\n" + (this.Actions[mainIndex][actionIndex] ? "On" : "Off");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!this.IsRealClass)
            {
                return null;
            }

            this.GetButton(actionParameter, out var mainIndex, out var action, out var actionIndex);

            if (mainIndex == -1 || action == -1)
            {
                return null;
            }

            var enabled = this.Actions[mainIndex][actionIndex];
            
            var actionString = this.IsMultiAction
                ? $"{(this.IsStrip ? "Strip" : "Bus")}[{mainIndex + this.Offset}]"
                : $"{(this.IsStrip ? "Strip" : "Bus")}[{action + this.Offset}]";
            var name = Remote.GetTextParameter($"{actionString}.Label");
            if (String.IsNullOrEmpty(name))
            {
                name = $"{(this.IsStrip ? "Strip" : "Bus")} {mainIndex + 1 + this.Offset}";
            }

            return DrawingHelper.DrawDefaultImage(this.IsMultiAction ? $"{this.DisplayName}{action}" : this.DisplayName, name, enabled ? Color.GreenYellow : Color.DimGray);
        }

        private void GetButton(String actionParameter, out Int32 mainIndex, out Int32 action, out Int32 actionIndex)
        {
            var splitted = actionParameter.Replace("VM-Strip", "").Replace(this.Command, "").Split('-');

            var firstSplitTruthy = Int32.TryParse(splitted[0], out var first);
            var secondSplitTruthy = Int32.TryParse(splitted[1], out var second);
            if (!firstSplitTruthy || !secondSplitTruthy)
            {
                mainIndex = -1;
                action = -1;
                actionIndex = -1;
            }

            if (second != 2147483647)
            {
                mainIndex = first;
                action = second;
                actionIndex = second - 1;
            }
            else
            {
                mainIndex = 0;
                action = first;
                actionIndex = first;
            }
        }
    }
}