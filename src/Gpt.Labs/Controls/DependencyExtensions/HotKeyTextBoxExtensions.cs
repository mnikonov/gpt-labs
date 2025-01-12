using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.System;
using Windows.UI.Core;

namespace Gpt.Labs.Controls.DependencyExtensions;

public class HotKeyTextBoxExtensions : DependencyObject
{
    #region Fields

    public static readonly DependencyProperty HandleHotKeyProperty = DependencyProperty.Register(
        "HandleHotKey",
        typeof(bool),
        typeof(DependencyObject),
        new PropertyMetadata(false, OnApplyHotKeyHandler));

    #endregion

    #region Public Methods

    public static bool GetHandleHotKey(DependencyObject element)
    {
        return (bool)element.GetValue(HandleHotKeyProperty);
    }

    public static void SetHandleHotKey(DependencyObject element, bool value)
    {
        element.SetValue(HandleHotKeyProperty, value);
    }

    #endregion

    #region Private Methods

    private static void OnApplyHotKeyHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var hotKeyTextBox = (TextBox)d;
        var val = (bool)e.NewValue;

        if (val)
        {
            var pressedKeys = new List<VirtualKey>();

            hotKeyTextBox.KeyDown += (s, e) =>
            {
                e.Handled = true;

                if (!IsModifierDown(e.Key))
                {
                    var r = IsModifierDown(e.Key);
                    Debug.WriteLine($"Info: Modifier NOT PRESSED");
                    return;
                }

                if (!pressedKeys.Contains(e.Key))
                {
                    Debug.WriteLine($"Info: Pressed Key: {e.Key}");

                    if (!IsModifier(e.Key))
                    {
                        var existKey = GetKey(pressedKeys);

                        if (existKey != VirtualKey.None)
                        {
                            pressedKeys.Remove(existKey);
                        }
                    }

                    pressedKeys.Add(e.Key);

                    hotKeyTextBox.Text = GetText(pressedKeys);
                }
            };

            hotKeyTextBox.KeyUp += (s, e) =>
            {
                e.Handled = true;

                if (IsModifier(e.Key) && GetKey(pressedKeys) == VirtualKey.None)
                {
                    pressedKeys.Remove(e.Key);
                }

                hotKeyTextBox.Text = GetText(pressedKeys);
            };

            hotKeyTextBox.TextChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(hotKeyTextBox.Text))
                {
                    pressedKeys.Clear();
                }
            };
        }
    }

    private static string GetText(IEnumerable<VirtualKey> pressedKeys)
    {
        var modifiers = GetMdifiers(pressedKeys).ToList();
        var key = GetKey(pressedKeys);

        Debug.WriteLine($"Warn: Modifiers ->  {string.Join(" + ", modifiers)}, Key: {key}");

        return modifiers.Count != 0
            ? string.Join("+", modifiers) + (key != VirtualKey.None ? "+" + key.ToString() : string.Empty)
            : string.Empty;
    }

    private static bool IsModifier(VirtualKey key)
    {
        return key is VirtualKey.Control or VirtualKey.Menu or VirtualKey.Shift;
    }

    private static VirtualKey GetKey(IEnumerable<VirtualKey> pressedKeys)
    {
        return pressedKeys.FirstOrDefault(p => !IsModifier(p));
    }

    private static IEnumerable<string> GetMdifiers(IEnumerable<VirtualKey> pressedKeys)
    {
        foreach (var key in pressedKeys)
        {
            switch (key)
            {
                case VirtualKey.Control:
                    yield return "Ctrl";
                    break;
                case VirtualKey.Menu:
                    yield return "Alt";
                    break;
                case VirtualKey.Shift:
                    yield return "Shift";
                    break;
            }
        }
    }

    private static bool IsModifierDown(VirtualKey key)
    {
        return IsModifier(key) ||
            (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down ||
            (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down ||
            (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }

    #endregion
}

