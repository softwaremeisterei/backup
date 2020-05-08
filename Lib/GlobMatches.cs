namespace Softwaremeisterei.Lib
{
    /// <summary>
    /// C# port of C++ version found under https://www.codeproject.com/Articles/5163931/Fast-String-Matching-with-Wildcards-Globs-and-Giti
    /// </summary>
    public class GlobMatches
    {
        // set to true to enable dotglob: *. ?, and [] match a . (dotfile) at the begin or after each /
        const bool DOTGLOB = true;

        // set to true to enable case-insensitive glob matching
        const bool NOCASEGLOB = false;

        // Windows path separator
        const char PATHSEP = '\\';

        public static bool Match(string text, string wild)
        {
            int i = 0;
            int j = 0;
            int n = text.Length;
            int m = wild.Length;
            int text_backup = int.MaxValue;
            int wild_backup = int.MaxValue;

            while (i < n)
            {
                if (j < m && wild[j] == '*')
                {
                    // new star-loop: backup positions in pattern and text
                    text_backup = i;
                    wild_backup = ++j;
                }
                else if (j < m && (wild[j] == '?' || wild[j] == text[i]))
                {
                    // ? matched any character or we matched the current non-NUL character
                    i++;
                    j++;
                }
                else
                {
                    // if no stars we fail to match
                    if (wild_backup == int.MaxValue)
                        return false;
                    // star-loop: backtrack to the last * by restoring the backup positions in the pattern and text
                    i = ++text_backup;
                    j = wild_backup;
                }
            }
            // ignore trailing stars
            while (j < m && wild[j] == '*')
                j++;
            // at end of text means success if nothing else is left to match
            return j >= m;
        }

        // returns TRUE if text string matches glob pattern with * and ?
        public static bool GlobMatch(string text, string glob)
        {
            int i = 0;
            int j = 0;
            int n = text.Length;
            int m = glob.Length;
            int text_backup = int.MaxValue;
            int glob_backup = int.MaxValue;
            bool nodot = !DOTGLOB;
            while (i < n)
            {
                if (j < m)
                {
                    switch (glob[j])
                    {
                        case '*':
                            // match anything except . after /
                            if (nodot && text[i] == '.')
                                break;
                            // new star-loop: backup positions in pattern and text
                            text_backup = i;
                            glob_backup = ++j;
                            continue;
                        case '?':
                            // match anything except . after /
                            if (nodot && text[i] == '.')
                                break;
                            // match any character except /
                            if (text[i] == PATHSEP)
                                break;
                            i++;
                            j++;
                            continue;
                        case '[':
                            {
                                // match anything except . after /
                                if (nodot && text[i] == '.')
                                    break;
                                // match any character in [...] except /
                                if (text[i] == PATHSEP)
                                    break;
                                int lastchr;
                                bool matched = false;
                                bool reverse = j + 1 < m && (glob[j + 1] == '^' || glob[j + 1] == '!');
                                // inverted character class
                                if (reverse)
                                    j++;
                                // match character class
                                for (lastchr = 256; ++j < m && glob[j] != ']'; lastchr = CASE(glob[j]))
                                    if (lastchr < 256 && glob[j] == '-' && j + 1 < m && glob[j + 1] != ']' ?
                                        CASE(text[i]) <= CASE(glob[++j]) && CASE(text[i]) >= lastchr :
                                        CASE(text[i]) == CASE(glob[j]))
                                        matched = true;
                                if (matched == reverse)
                                    break;
                                i++;
                                if (j < m)
                                    j++;
                                continue;
                            }
                        default:
                            if (glob[j] == PATHSEP)
                            {
                                // literal match \-escaped character
                                if (j + 1 < m)
                                    j++;
                            }
                            // match the current non-NUL character
                            if (CASE(glob[j]) != CASE(text[i]) && !(glob[j] == '/' && text[i] == PATHSEP))
                                break;
                            // do not match a . with *, ? [] after /
                            nodot = !DOTGLOB && glob[j] == '/';
                            i++;
                            j++;
                            continue;
                    }
                }
                if (glob_backup == int.MaxValue || text[text_backup] == PATHSEP)
                    return false;
                // star-loop: backtrack to the last * but do not jump over /
                i = ++text_backup;
                j = glob_backup;
            }
            // ignore trailing stars
            while (j < m && glob[j] == '*')
                j++;
            // at end of text means success if nothing else is left to match
            return j >= m;
        }

        /// <summary>
        /// returns TRUE if text string matches gitignore-style glob pattern
        /// </summary>
        /// <param name="text"></param>
        /// <param name="glob"></param>
        /// <returns></returns>
        public static bool GitIgnoreGlobMatch(string text, string glob)
        {
            int i = 0;
            int j = 0;
            int n = text.Length;
            int m = glob.Length;
            int text1_backup = int.MaxValue;
            int glob1_backup = int.MaxValue;
            int text2_backup = int.MaxValue;
            int glob2_backup = int.MaxValue;
            bool nodot = !DOTGLOB;
            // match pathname if glob contains a / otherwise match the basename
            if (j + 1 < m && glob[j] == '/')
            {
                // if pathname starts with ./ then ignore these pairs
                while (i + 1 < n && text[i] == '.' && text[i + 1] == PATHSEP)
                    i += 2;
                // if pathname starts with a / then ignore it
                if (i < n && text[i] == PATHSEP)
                    i++;
                j++;
            }
            else if (glob.IndexOf('/') < 0) // else if (glob.find('/') == int.MaxValue)
            {
                int sep = text.LastIndexOf(PATHSEP);
                if (sep != int.MaxValue)
                    i = sep + 1;
            }
            while (i < n)
            {
                if (j < m)
                {
                    switch (glob[j])
                    {
                        case '*':
                            // match anything except . after /
                            if (nodot && text[i] == '.')
                                break;
                            if (++j < m && glob[j] == '*')
                            {
                                // trailing ** match everything after /
                                if (++j >= m)
                                    return true;
                                // ** followed by a / match zero or more directories
                                if (glob[j] != '/')
                                    return false;
                                // new **-loop, discard *-loop
                                text1_backup = int.MaxValue;
                                glob1_backup = int.MaxValue;
                                text2_backup = i;
                                glob2_backup = ++j;
                                continue;
                            }
                            // trailing * matches everything except /
                            text1_backup = i;
                            glob1_backup = j;
                            continue;
                        case '?':
                            // match anything except . after /
                            if (nodot && text[i] == '.')
                                break;
                            // match any character except /
                            if (text[i] == PATHSEP)
                                break;
                            i++;
                            j++;
                            continue;
                        case '[':
                            {
                                // match anything except . after /
                                if (nodot && text[i] == '.')
                                    break;
                                // match any character in [...] except /
                                if (text[i] == PATHSEP)
                                    break;
                                int lastchr;
                                bool matched = false;
                                bool reverse = j + 1 < m && (glob[j + 1] == '^' || glob[j + 1] == '!');
                                // inverted character class
                                if (reverse)
                                    j++;
                                // match character class
                                for (lastchr = 256; ++j < m && glob[j] != ']'; lastchr = CASE(glob[j]))
                                    if (lastchr < 256 && glob[j] == '-' && j + 1 < m && glob[j + 1] != ']' ?
                                        CASE(text[i]) <= CASE(glob[++j]) && CASE(text[i]) >= lastchr :
                                        CASE(text[i]) == CASE(glob[j]))
                                        matched = true;
                                if (matched == reverse)
                                    break;
                                i++;
                                if (j < m)
                                    j++;
                                continue;
                            }
                        default:
                            if (glob[j] == PATHSEP)
                            {
                                // literal match \-escaped character
                                if (j + 1 < m)
                                    j++;
                            }
                            // match the current non-NUL character
                            if (CASE(glob[j]) != CASE(text[i]) && !(glob[j] == '/' && text[i] == PATHSEP))
                                break;
                            // do not match a . with *, ? [] after /
                            nodot = !DOTGLOB && glob[j] == '/';
                            i++;
                            j++;
                            continue;
                    }
                }
                if (glob1_backup != int.MaxValue && text[text1_backup] != PATHSEP)
                {
                    // *-loop: backtrack to the last * but do not jump over /
                    i = ++text1_backup;
                    j = glob1_backup;
                    continue;
                }
                if (glob2_backup != int.MaxValue)
                {
                    // **-loop: backtrack to the last **
                    i = ++text2_backup;
                    j = glob2_backup;
                    continue;
                }
                return false;
            }
            // ignore trailing stars
            while (j < m && glob[j] == '*')
                j++;
            // at end of text means success if nothing else is left to match
            return j >= m;
        }

        private static char CASE(char c)
        {
            return NOCASEGLOB ? char.ToLower(c) : c;
        }
    }
}
