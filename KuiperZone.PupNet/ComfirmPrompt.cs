// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-25
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/PupNet
//
// PupNet is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// PupNet is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along
// with PupNet. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

namespace KuiperZone.PupNet;

/// <summary>
/// Prompts for yes or no.
/// </summary>
public class ConfirmPrompt
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ConfirmPrompt(bool multi = false)
        : this(null, multi)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ConfirmPrompt(string? question, bool multi = false)
    {
        question = question?.Trim();

        if (string.IsNullOrEmpty(question))
        {
            question = "Continue?";
        }

        if (multi)
        {
            IsMultiple = true;
            PromptText += question + " [N/y] or ESC aborts: ";
        }
        else
        {
            PromptText = question + " [N/y]: ";
        }
    }

    /// <summary>
    /// Gets the prompt text.
    /// </summary>
    public string PromptText { get; }

    /// <summary>
    /// Multiple prompts (adds Escape option).
    /// </summary>
    public bool IsMultiple { get; }

    /// <summary>
    /// Gets user response.
    /// </summary>
    public bool Wait()
    {
        Console.Write(PromptText);

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            Console.WriteLine();
            throw new InvalidOperationException("Aborted by user");
        }

        if (key == ConsoleKey.Y)
        {
            Console.WriteLine("Y");
            return true;
        }

        Console.WriteLine("N");
        return false;
    }

}

