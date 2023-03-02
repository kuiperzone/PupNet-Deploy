// -----------------------------------------------------------------------------
// PROJECT   : Pubpak
// COPYRIGHT : Andy Thomas (C) 2022-23
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/Pubpak
//
// Pubpak is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// Pubpak is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with Pubpak. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

namespace KuiperZone.Pubpak;

/// <summary>
/// Prompts for yes or no.
/// </summary>
public class ConfirmPrompt
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ConfirmPrompt(string? question = null)
    {
        question = question?.Trim();

        if (string.IsNullOrEmpty(question))
        {
            question = "Continue?";
        }

        PromptText = question + " [N/y] or ESC aborts? ";
    }

    /// <summary>
    /// Gets the prompt text.
    /// </summary>
    public string PromptText { get; }

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
            Console.WriteLine("YES");
            return true;
        }

        Console.WriteLine("NO");
        return false;
    }

}

