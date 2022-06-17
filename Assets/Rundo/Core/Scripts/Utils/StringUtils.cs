namespace Rundo.Core.Utils
{
    public static class StringUtils
    {
        public static string ToMaxLength(string value, int length)
        {
            if (value.Length >= length)
                value = value.Substring(0, length - 3) + "...";
            return value;
        }
        
        public static string ToPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            
            var onlyAlphaNumeric = "";
            var upperCaseNext = true;
            for (var i = 0; i < value.Length; ++i)
            {
                var c = value[i];
                if (char.IsLetter(c) || char.IsNumber(c))
                {
                    if (upperCaseNext)
                    {
                        upperCaseNext = false;
                        c = char.ToUpper(c);
                    }
                    onlyAlphaNumeric += c;
                }
                else
                {
                    upperCaseNext = true;
                }
            }
            
            // add space between lower and upper chars
            string pascalCase = "";
            for (var i = 0; i < onlyAlphaNumeric.Length; ++i)
            {
                if (i > 0)
                    if (char.IsLower(onlyAlphaNumeric[i - 1]) &&
                        char.IsUpper(onlyAlphaNumeric[i]))
                    {
                        pascalCase += " ";
                    }

                pascalCase += onlyAlphaNumeric[i];
            }

            // first char is upper
            if (value.Length > 0)
            {
                var c = char.ToUpper(value[0]);
                value.Remove(0, 1);
                value.Insert(0, c.ToString());

            }

            return pascalCase;
        }
    }
}
