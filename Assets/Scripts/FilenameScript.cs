using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FilenameScript
{
    public static bool IsValidFilename(string testName)
    {
        System.Text.RegularExpressions.Regex containsABadCharacter = new System.Text.RegularExpressions.Regex(
            "[" + System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");

        if (containsABadCharacter.IsMatch(testName))
            return false;

        return true;
    }
    public static bool FilenameExists(string saveLoadPath, string filename)
    {
        return File.Exists(saveLoadPath + "/" + filename.ToLower() + ".obj");
    }
}
