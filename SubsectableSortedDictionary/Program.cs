#if SSSD_BENCHMARKING

using System.Diagnostics;
using System.Security.Cryptography;
using Emperor.SubsectableSortedDictionary;

int iterations = 10000;
Console.WriteLine("Enter number of test items (blank = 10000)");
string? input = Console.ReadLine();

if (!string.IsNullOrEmpty(input))
    iterations = Convert.ToInt32(input);

List<int> testNums = new List<int>(iterations);
List<string> testStrings = new List<string>(iterations);

for (int i = 0; i < iterations; i++)
{
    //testNums.Add(Random.Shared.Next(1, iterations + 1));
    testNums.Add(i);
    
    testStrings.Add(SHA512.Create().ComputeHash(new byte[1] { (byte)testNums[i] }).ToString());
}

SubsectableBinarySearchTree<int, string> bst = new SubsectableBinarySearchTree<int, string>();
SortedDictionary<int, string> sd = new SortedDictionary<int, string>();

var timesBST = new List<long>(iterations);
var timesSD = new List<long>(iterations);
var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    sw.Reset();
    sw.Start();
    bst[testNums[i]] = testStrings[i];
    sw.Stop();
    timesBST.Add(sw.ElapsedTicks);
    
    sw.Reset();
    sw.Start();
    sd[testNums[i]] = testStrings[i];
    sw.Stop();
    timesSD.Add(sw.ElapsedTicks);
}

var timesBSTd = new List<long>(iterations);
var timesSDd = new List<long>(iterations);

for (int i = 0; i < iterations; i++)
{
    sw.Reset();
    sw.Start();
    bst.Remove(testNums[i]);
    sw.Stop();
    timesBSTd.Add(sw.ElapsedTicks);
    
    sw.Reset();
    sw.Start();
    sd.Remove(testNums[i]);
    sw.Stop();
    timesSDd.Add(sw.ElapsedTicks);
}

timesBST.Sort();
timesSD.Sort();
timesBSTd.Sort();
timesSDd.Sort();

Console.WriteLine($"Stopwatch TPS = {Stopwatch.Frequency}");
Console.WriteLine($"--- ADD ---");
Console.WriteLine($"Custom implementation (random array of size {iterations}):");
Console.WriteLine($"min: {timesBST[0]} ticks, max: {timesBST[^1]} ticks, median: {timesBST[iterations/2]} ticks, avg: {timesBST.Average()} ticks");
Console.WriteLine("Built-in C# SortedDictionary object:");
Console.WriteLine($"min: {timesSD[0]} ticks, max: {timesSD[^1]} ticks, median: {timesSD[iterations/2]} ticks, avg: {timesSD.Average()} ticks");
Console.WriteLine($"Speedup (avgs): {timesSD.Average() / timesBST.Average():F1}x, medians: {(float)timesSD[iterations/2] / timesBST[iterations/2]:F1}x");
Console.WriteLine($"--- DELETE ---");
Console.WriteLine($"Custom implementation (random array of size {iterations}):");
Console.WriteLine($"min: {timesBSTd[0]} ticks, max: {timesBSTd[^1]} ticks, median: {timesBSTd[iterations/2]} ticks, avg: {timesBSTd.Average()} ticks");
Console.WriteLine("Built-in C# SortedDictionary object:");
Console.WriteLine($"min: {timesSDd[0]} ticks, max: {timesSDd[^1]} ticks, median: {timesSDd[iterations/2]} ticks, avg: {timesSDd.Average()} ticks");
Console.WriteLine($"Speedup (avgs): {timesSDd.Average() / timesBSTd.Average():F1}x, medians: {(float)timesSDd[iterations/2] / timesBSTd[iterations/2]:F1}x");

#endif

