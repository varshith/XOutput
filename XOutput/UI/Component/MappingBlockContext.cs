﻿using XOutput.Devices;
using XOutput.Devices.XInput;
using XOutput.Tools;

namespace XOutput.UI.Component
{
    public class MappingBlockContext
    {
        public XInputTypes InputType { get; set; }

        public GameController Controller { get; set; }
    }
}