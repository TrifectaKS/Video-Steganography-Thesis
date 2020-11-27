using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MersenneTwister;

namespace Utilities
{
    public class NonRepeatingRandom
    {
        private Random r;

        public NonRepeatingRandom(int seed)
        {
            r = new Random(seed);
        }
        public IEnumerable<int> GetRandomSequence(int min, int max, int  amount)
        {
            List<int> nums = new List<int>();
 
            for(int i = min; i <= max; i++)
            {
                nums.Add(i);
            }
            
            for(int c = 0; c < amount && nums.Count > 0; c++)
            {
                int index = r.Next(nums.Count);
                yield return nums[index];
                nums.RemoveAt(index);
            }
        }
    }
}
