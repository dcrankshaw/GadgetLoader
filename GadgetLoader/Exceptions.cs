﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetLoader
{
    class ConfigurationException : Exception
    {
        public ConfigurationException(String message)
            : base(message)
        {
        }

    }
}
