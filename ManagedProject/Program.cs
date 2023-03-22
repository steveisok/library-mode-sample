using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ManagedProject
{
    class Program
    {
		[UnmanagedCallersOnly(EntryPoint = nameof(SayHello))]
		public static void SayHello()
		{
			Console.WriteLine("Called from native!  Hello!");
		}

		public static void Main(string[] args)
		{

		}
	}
        
}
