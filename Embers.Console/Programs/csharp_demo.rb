# Embers C# Interop Demo
# Demonstrates common .NET integration patterns

puts "=== C# Interop Demo ==="
puts ""

# 1. Working with DateTime
puts "1. DateTime operations:"
dt = System.DateTime.Now
puts "Current time: #{dt}"
puts "Year: #{dt.Year}"
puts "Is weekend: #{dt.DayOfWeek == System.DayOfWeek::Saturday || dt.DayOfWeek == System.DayOfWeek::Sunday}"
puts ""

# 2. File System operations
puts "2. File System:"
currentDir = System.IO.Directory.GetCurrentDirectory()
puts "Current directory: #{currentDir}"
files = System.IO.Directory.GetFiles(currentDir)
puts "Files in directory: #{files.Length}"
puts ""

# 3. String manipulation with .NET
puts "3. String operations:"
text = "Hello, Embers!"
puts "Original: #{text}"
puts "Upper: #{text.ToUpper()}"
puts "Lower: #{text.ToLower()}"
puts "Length: #{text.Length}"
puts "Contains 'Embers': #{text.Contains('Embers')}"
puts ""

# 4. Collections
puts "4. .NET Collections:"
list = System.Collections.Generic.List.new(System.Int32)
list.Add(10)
list.Add(20)
list.Add(30)
puts "List count: #{list.Count}"
puts "First item: #{list[0]}"
puts ""

# 5. Math operations
puts "5. Math operations:"
pi = System.Math::PI
puts "PI: #{pi}"
puts "Square root of 16: #{System.Math.Sqrt(16)}"
puts "Round PI to 2 decimals: #{System.Math.Round(pi, 2)}"
puts "Max of 42 and 100: #{System.Math.Max(42, 100)}"
puts ""

# 6. Random numbers
puts "6. Random numbers:"
rnd = System.Random.new
puts "Random number: #{rnd.Next(1, 100)}"
puts ""

# 7. Environment info
puts "7. Environment:"
puts "OS Version: #{System.Environment.OSVersion}"
puts "Machine Name: #{System.Environment.MachineName}"
puts "Processor Count: #{System.Environment.ProcessorCount}"
puts ""

# 8. Combining Embers stdlib with C#
puts "8. Hybrid operations (Embers + C#):"
value = 42
puts "Value: #{value}"
puts "As float (Embers): #{value.to_f}"
puts "As string (Embers): #{value.to_s}"
puts "Converted to double (C#): #{System.Convert.ToDouble(value)}"
puts ""

puts "=== Demo Complete ==="
