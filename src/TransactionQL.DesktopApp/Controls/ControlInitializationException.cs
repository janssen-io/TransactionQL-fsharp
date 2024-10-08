﻿using Avalonia.Controls;
using System;
using System.Runtime.Serialization;

namespace TransactionQL.DesktopApp.Controls
{
    [Serializable]
    public class ControlInitializationException : Exception
    {
        public ControlInitializationException()
        {
        }

        public ControlInitializationException(Control control) : this($"Failed to create {control.GetType()}.")
        {
        }

        public ControlInitializationException(Control control, Exception? innerException) : this($"Failed to create {control.GetType()}.", innerException)
        {
        }

        public ControlInitializationException(string? message) : base(message)
        {
        }

        public ControlInitializationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}