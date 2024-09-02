/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using UnityEngine;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif
    
    /// <summary>
    /// Used to abstract away the differences between the modern input system and the legacy
    /// input system.
    /// </summary>
    public static class InputUtility
    {
        /*
         * NOTE:
         *
         * There is a discrepancy in the language between the new input system and
         * the legacy system in that the new system makes a distinction between
         * whether a key was pressed or is pressed. The legacy system only
         * determines whether the given key is down or not.
         *
         * This is why there is a difference in tense between TabWasPressed and
         * ShiftIsPressed - the language that best describes the typical usage for
         * the key is used.
         *
         * Despite the difference in tense, when utilizing the legacy input system
         * both functions merely check if the key in question is down.
         */

        /// <summary>
        /// Determines if the tab or tab equivalent was pressed this frame.
        /// </summary>
        /// <returns>True if tab or tab equivalent was pressed this frame.</returns>
        public static bool TabWasPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return null != Keyboard.current && Keyboard.current.tabKey.wasPressedThisFrame;
#else
            return KeyDown(KeyCode.Tab);
#endif
        }

        /// <summary>
        /// Determines if the shift key or shift key equivalent is currently pressed.
        /// </summary>
        /// <returns>True if the shift key or shift key equivalent is currently pressed.</returns>
        public static bool ShiftIsPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return null != Keyboard.current && Keyboard.current.shiftKey.isPressed;
#else
            return KeyDown(KeyCode.RightShift) || KeyDown(KeyCode.LeftShift);
#endif
        }

        /// <summary>
        /// Determines if the enter or numpad enter key or it's equivalent was pressed.
        /// </summary>
        /// <returns>True if the enter or numpad enter key or equivalent is currently pressed.</returns>
        public static bool WasEnterPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return null != Keyboard.current && 
                          (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
            return KeyDown(KeyCode.Return) || KeyDown(KeyCode.KeypadEnter);
#endif
        }

        /// <summary>
        /// Private helper function to determine if a specific key is down - just to limit
        /// the amount of code that interacts with the input system directly.
        /// </summary>
        /// <param name="keycode">Which key to check.</param>
        /// <returns>True if the indicated key is down, false otherwise.</returns>
        private static bool KeyDown(KeyCode keycode)
        {
            return Input.GetKeyDown(keycode);
        }

        /// <summary>
        /// Determines whether or not the Gamepad was used last frame.
        /// </summary>
        /// <returns>True if the game pad is active, false otherwise.</returns>
        public static bool WasGamepadUsedLastFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return (null != Gamepad.current && Gamepad.current.wasUpdatedThisFrame);
#else
            // NOTE: This method of determining gamepad activity is potentially inconclusive
            //       it's possible that the input axis' are both 0.0f with an active gamepad,
            //       but this is not likely.
            return Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f;
#endif
        }

        /// <summary>
        /// Determines whether or not the keyboard was used last frame.
        /// </summary>
        /// <returns>True if the keyboard was used last frame, false otherwise.</returns>
        public static bool WasKeyboardUsedLastFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return null != Keyboard.current && Keyboard.current.wasUpdatedThisFrame;
#else
            return Input.anyKeyDown;
#endif
        }

        /// <summary>
        /// Determines whether or not the mouse was used last frame.
        /// </summary>
        /// <returns>True if the mouse was used last frame, false otherwise.</returns>
        public static bool WasMouseUsedLastFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.wasUpdatedThisFrame;
#else
            // NOTE: This method of determining mouse activity is potentially inconclusive
            //       it's possible that the input axis' are both 0.0f with an active mouse,
            //       but this is not likely.
            return Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f;
#endif
        }
    }
}