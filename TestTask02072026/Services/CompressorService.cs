using System.Text;

//Block-scoped namespace (traditional) as it lasted till C# 9
namespace Services
{
    public static class CompressorService
    {
        //The class throws as input validation is not it's concern
        public static string Compress(string input)
        {
            //null check
            if(string.IsNullOrWhiteSpace(input))
                throw new ArgumentException(
                    "Input cannot be null or empty.", 
                    nameof(input));
            //latin and lowercase check of the first index
            if (input[0] < 'a' || input[0] > 'z')
                throw new ArgumentException(
                    "Input must contain only lowercase Latin letters.", 
                    nameof(input));
                
            //I chose stringbuilder as it
            //reduces intermediate string allocations in for loop iterations
            //which matters because input size is unknown
            var result = new StringBuilder();
                
            char current = input[0];
            int count = 1;

            for(int i = 1; i < input.Length; i++)
            {
                char c = input[i];

                if (c < 'a' || c > 'z')
                {
                    //check each compare index
                    throw new ArgumentException(
                        "Input must contain only lowercase Latin letters.", 
                        nameof(input));
                }
                if(c == current)
                {
                    count++;
                }
                
                else
                {
                    AppendGroup(result, current, count);

                    current = c;
                    count = 1;
                }
            }

            AppendGroup(result, current, count);

            return result.ToString();
        }
        
        //Little helper method that appends a symbol to string builder
        //and a number after that symbol if it's not 1
        private static void AppendGroup(StringBuilder result, char symbol, int count)
        {
            result.Append(symbol);

            if (count > 1)
                result.Append(count);
        }

        //Also a decompression method
    }

}